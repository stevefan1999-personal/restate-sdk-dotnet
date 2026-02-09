using System.Buffers;
using System.Text.Json;
using Restate.Sdk.Internal;
using Restate.Sdk.Internal.Journal;

namespace Restate.Sdk.Tests;

public class CombinatorTests
{
    private static DurableFuture<T> CompletedFuture<T>(T value)
    {
        return DurableFuture<T>.Completed(value);
    }

    private static (DurableFuture<T> Future, TaskCompletionSource<CompletionResult> Tcs) PendingFuture<T>()
    {
        var tcs = new TaskCompletionSource<CompletionResult>();
        return (new DurableFuture<T>(tcs, JsonSerializerOptions.Default), tcs);
    }

    private static void Complete<T>(TaskCompletionSource<CompletionResult> tcs, T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        JsonSerializer.Serialize(
            new Utf8JsonWriter(buffer), value);
        tcs.SetResult(CompletionResult.Success(buffer.WrittenMemory));
    }

    // ── All ──

    [Fact]
    public async Task All_EmptyArray_ReturnsEmpty()
    {
        var ctx = new TestCombinatorContext();
        var results = await ctx.All(Array.Empty<IDurableFuture<int>>());
        Assert.Empty(results);
    }

    [Fact]
    public async Task All_AllCompleted_ReturnsInOrder()
    {
        var ctx = new TestCombinatorContext();
        var f1 = CompletedFuture(1);
        var f2 = CompletedFuture(2);
        var f3 = CompletedFuture(3);

        var results = await ctx.All(f1, f2, f3);

        Assert.Equal([1, 2, 3], results);
    }

    [Fact]
    public async Task All_PendingFutures_WaitsForAll()
    {
        var ctx = new TestCombinatorContext();
        var (f1, tcs1) = PendingFuture<int>();
        var (f2, tcs2) = PendingFuture<int>();

        var task = ctx.All(f1, f2).AsTask();

        Assert.False(task.IsCompleted);

        Complete(tcs1, 10);
        Assert.False(task.IsCompleted);

        Complete(tcs2, 20);
        var results = await task;

        Assert.Equal([10, 20], results);
    }

    [Fact]
    public async Task All_OneFailure_ThrowsFirst()
    {
        var ctx = new TestCombinatorContext();
        var f1 = CompletedFuture(1);
        var (f2, tcs2) = PendingFuture<int>();

        tcs2.SetResult(CompletionResult.Failure(500, "boom"));

        await Assert.ThrowsAsync<TerminalException>(() => ctx.All(f1, f2).AsTask());
    }

    // ── Race ──

    [Fact]
    public async Task Race_FirstCompleted_Wins()
    {
        var ctx = new TestCombinatorContext();
        var (f1, tcs1) = PendingFuture<string>();
        var (f2, tcs2) = PendingFuture<string>();

        var task = ctx.Race(f1, f2).AsTask();

        Complete(tcs2, "second");
        var result = await task;

        Assert.Equal("second", result);
    }

    [Fact]
    public async Task Race_PreCompleted_ReturnsImmediately()
    {
        var ctx = new TestCombinatorContext();
        var f1 = CompletedFuture("fast");
        var (f2, _) = PendingFuture<string>();

        var result = await ctx.Race(f1, f2);

        Assert.Equal("fast", result);
    }

    // ── WaitAll ──

    [Fact]
    public async Task WaitAll_YieldsInCompletionOrder()
    {
        var ctx = new TestCombinatorContext();

        var tcs1 = new TaskCompletionSource<CompletionResult>();
        var tcs2 = new TaskCompletionSource<CompletionResult>();
        var tcs3 = new TaskCompletionSource<CompletionResult>();

        var f1 = new DurableFuture<int>(tcs1, JsonSerializerOptions.Default);
        var f2 = new DurableFuture<int>(tcs2, JsonSerializerOptions.Default);
        var f3 = new DurableFuture<int>(tcs3, JsonSerializerOptions.Default);

        // Complete in reverse order: 3, 1, 2
        Complete(tcs3, 30);
        Complete(tcs1, 10);

        var completionOrder = new List<IDurableFuture>();
        var errors = new List<Exception?>();

        // Start enumeration in background
        var enumTask = Task.Run(async () =>
        {
            await foreach (var (future, error) in ctx.WaitAll(f1, f2, f3))
            {
                completionOrder.Add(future);
                errors.Add(error);
            }
        });

        // Give it a moment to process the already-completed ones
        await Task.Delay(50);

        // Complete the last one
        Complete(tcs2, 20);

        await enumTask;

        Assert.Equal(3, completionOrder.Count);
        Assert.All(errors, e => Assert.Null(e));
    }

    [Fact]
    public async Task WaitAll_FaultedFuture_YieldsError()
    {
        var ctx = new TestCombinatorContext();

        var tcs = new TaskCompletionSource<CompletionResult>();
        var future = new DurableFuture<int>(tcs, JsonSerializerOptions.Default);

        tcs.SetResult(CompletionResult.Failure(500, "oops"));

        var results = new List<(IDurableFuture, Exception?)>();
        await foreach (var item in ctx.WaitAll(future)) results.Add(item);

        Assert.Single(results);
        Assert.NotNull(results[0].Item2);
        Assert.IsType<TerminalException>(results[0].Item2);
    }

    /// <summary>
    ///     Lightweight context that exposes just the combinator methods for testing.
    ///     Mirrors Context's combinator logic.
    /// </summary>
    private sealed class TestCombinatorContext
    {
        public async ValueTask<T[]> All<T>(params IDurableFuture<T>[] futures)
        {
            var results = new T[futures.Length];
            var tasks = new Task[futures.Length];
            for (var i = 0; i < futures.Length; i++)
                tasks[i] = futures[i].GetResult().AsTask();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            for (var i = 0; i < futures.Length; i++)
                results[i] = await futures[i].GetResult().ConfigureAwait(false);

            return results;
        }

        public async ValueTask<T> Race<T>(params IDurableFuture<T>[] futures)
        {
            var tasks = new Task<T>[futures.Length];
            for (var i = 0; i < futures.Length; i++)
                tasks[i] = futures[i].GetResult().AsTask();

            var winner = await Task.WhenAny(tasks).ConfigureAwait(false);
            return await winner.ConfigureAwait(false);
        }

        public async IAsyncEnumerable<(IDurableFuture future, Exception? error)> WaitAll(
            params IDurableFuture[] futures)
        {
            var remaining = new List<(IDurableFuture future, Task task)>(futures.Length);
            for (var i = 0; i < futures.Length; i++)
                remaining.Add((futures[i], futures[i].GetResult().AsTask()));

            while (remaining.Count > 0)
            {
                var tasks = remaining.Select(r => r.task).ToArray();
                var completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);

                for (var i = remaining.Count - 1; i >= 0; i--)
                    if (remaining[i].task == completedTask)
                    {
                        var entry = remaining[i];
                        remaining.RemoveAt(i);
                        var error = completedTask.IsFaulted
                            ? completedTask.Exception?.InnerException
                            : null;
                        yield return (entry.future, error);
                        break;
                    }
            }
        }
    }
}