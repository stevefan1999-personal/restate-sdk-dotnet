using Restate.Sdk.Internal.Journal;

namespace Restate.Sdk.Tests.Journal;

public class CompletionManagerTests
{
    [Fact]
    public void Register_CreatesCompletionSource()
    {
        var manager = new CompletionManager();
        var tcs = manager.Register(0);

        Assert.NotNull(tcs);
        Assert.False(tcs.Task.IsCompleted);
    }

    [Fact]
    public void Register_ThrowsOnDuplicate()
    {
        var manager = new CompletionManager();
        manager.Register(0);

        Assert.Throws<InvalidOperationException>(() => manager.Register(0));
    }

    [Fact]
    public void GetOrRegister_ReturnsSameForDuplicate()
    {
        var manager = new CompletionManager();
        var tcs1 = manager.GetOrRegister(0);
        var tcs2 = manager.GetOrRegister(0);

        Assert.Same(tcs1, tcs2);
    }

    [Fact]
    public async Task TryComplete_ResolvesTask()
    {
        var manager = new CompletionManager();
        var tcs = manager.Register(0);

        var result = CompletionResult.Success(new byte[] { 1, 2, 3 });
        Assert.True(manager.TryComplete(0, result));

        var completed = await tcs.Task;
        Assert.True(completed.IsSuccess);
        Assert.Equal(new byte[] { 1, 2, 3 }, completed.Value.ToArray());
    }

    [Fact]
    public void TryComplete_StoresEarlyCompletion()
    {
        var manager = new CompletionManager();
        // Completion arrives before any registration — should store it for later.
        Assert.True(manager.TryComplete(99, CompletionResult.Success(new byte[] { 42 })));
    }

    [Fact]
    public async Task TryComplete_EarlyCompletionDeliveredOnRegister()
    {
        var manager = new CompletionManager();
        // Completion arrives before registration.
        manager.TryComplete(0, CompletionResult.Success(new byte[] { 7, 8 }));

        // Now register — the TCS should already be resolved.
        var tcs = manager.Register(0);
        Assert.True(tcs.Task.IsCompleted);
        var completed = await tcs.Task;
        Assert.True(completed.IsSuccess);
        Assert.Equal(new byte[] { 7, 8 }, completed.Value.ToArray());
    }

    [Fact]
    public async Task TryComplete_EarlyCompletionDeliveredOnGetOrRegister()
    {
        var manager = new CompletionManager();
        // Completion arrives before registration.
        manager.TryComplete(0, CompletionResult.Success(new byte[] { 9 }));

        // GetOrRegister should detect and deliver the early completion.
        var tcs = manager.GetOrRegister(0);
        Assert.True(tcs.Task.IsCompleted);
        var completed = await tcs.Task;
        Assert.True(completed.IsSuccess);
        Assert.Equal(new byte[] { 9 }, completed.Value.ToArray());
    }

    [Fact]
    public async Task TryFail_EarlyFailureDeliveredOnRegister()
    {
        var manager = new CompletionManager();
        // Failure arrives before registration.
        manager.TryFail(0, 409, "Conflict");

        var tcs = manager.Register(0);
        Assert.True(tcs.Task.IsFaulted);
        var ex = await Assert.ThrowsAsync<TerminalException>(() => tcs.Task);
        Assert.Equal("Conflict", ex.Message);
        Assert.Equal(409, ex.Code);
    }

    [Fact]
    public async Task TryFail_SetsTerminalException()
    {
        var manager = new CompletionManager();
        var tcs = manager.Register(0);

        Assert.True(manager.TryFail(0, 409, "Conflict"));

        var ex = await Assert.ThrowsAsync<TerminalException>(() => tcs.Task);
        Assert.Equal("Conflict", ex.Message);
        Assert.Equal(409, ex.Code);
    }

    [Fact]
    public async Task CancelAll_CancelsAllPending()
    {
        var manager = new CompletionManager();
        var tcs1 = manager.Register(0);
        var tcs2 = manager.Register(1);

        manager.CancelAll();

        await Assert.ThrowsAsync<TaskCanceledException>(() => tcs1.Task);
        await Assert.ThrowsAsync<TaskCanceledException>(() => tcs2.Task);
    }

    [Fact]
    public void CancelAll_IsIdempotent()
    {
        var manager = new CompletionManager();
        manager.Register(0);
        manager.CancelAll();
        manager.CancelAll();
    }
}