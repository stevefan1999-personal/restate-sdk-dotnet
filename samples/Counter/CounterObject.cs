using Restate.Sdk;

namespace Counter;

/// <summary>
///     A Virtual Object that maintains a durable counter per key.
///     Each key gets its own isolated counter with exclusive access for writes.
///     Virtual Objects are addressed by key — e.g., calling "counter-1" and "counter-2"
///     creates two independent counters. Exclusive handlers (Add, Reset) run one at a time
///     per key, while shared handlers (Get, GetKeys) can run concurrently.
/// </summary>
[VirtualObject]
public sealed class CounterObject
{
    private static readonly StateKey<int> Count = new("count");

    /// <summary>
    ///     Adds a delta to the counter and returns the new value.
    ///     This is an exclusive handler — only one Add/Reset can run at a time per key.
    /// </summary>
    [Handler]
    public async Task<int> Add(ObjectContext ctx, int delta)
    {
        // State is loaded from Restate's durable store (returns 0 if not set)
        var current = await ctx.Get(Count);
        var next = current + delta;

        // State is persisted durably — survives crashes and restarts
        ctx.Set(Count, next);

        return next;
    }

    /// <summary>
    ///     Demonstrates a non-retryable error. TerminalException stops Restate from
    ///     retrying the invocation — useful for validation errors or business rule violations.
    /// </summary>
    [Handler]
    public Task AddThenFail(ObjectContext ctx)
    {
        throw new TerminalException("This operation intentionally fails and will not be retried", 400);
    }

    /// <summary>
    ///     Resets the counter by clearing all state for this key.
    ///     ClearAll() removes every state key associated with this virtual object key.
    /// </summary>
    [Handler]
    public Task Reset(ObjectContext ctx)
    {
        ctx.ClearAll();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Returns the current counter value. This is a shared handler — multiple
    ///     reads can execute concurrently without blocking exclusive handlers.
    /// </summary>
    [SharedHandler]
    public async Task<int> Get(SharedObjectContext ctx)
    {
        return await ctx.Get(Count);
    }

    /// <summary>
    ///     Lists all state keys for this virtual object. Useful for debugging
    ///     or introspecting what state is stored.
    /// </summary>
    [SharedHandler]
    public async Task<string[]> GetKeys(SharedObjectContext ctx)
    {
        return await ctx.StateKeys();
    }
}