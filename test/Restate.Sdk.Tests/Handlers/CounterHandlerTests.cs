using Restate.Sdk.Testing;

namespace Restate.Sdk.Tests.Handlers;

// Inline handler for testing — demonstrates virtual object state operations
[VirtualObject]
file class CounterObject
{
    private static readonly StateKey<int> Count = new("count");

    [Handler]
    public async Task<int> Add(ObjectContext ctx, int delta)
    {
        var current = await ctx.Get(Count);
        var next = current + delta;
        ctx.Set(Count, next);
        return next;
    }

    [SharedHandler]
    public async Task<int> Get(SharedObjectContext ctx)
    {
        return await ctx.Get(Count);
    }

    [Handler]
    public Task Reset(ObjectContext ctx)
    {
        ctx.ClearAll();
        return Task.CompletedTask;
    }
}

public class CounterHandlerTests
{
    [Fact]
    public async Task Add_IncrementsByDelta()
    {
        var ctx = new MockObjectContext("my-counter");
        ctx.SetupState(new StateKey<int>("count"), 5);

        var counter = new CounterObject();
        var result = await counter.Add(ctx, 3);

        Assert.Equal(8, result);
        Assert.Equal(8, ctx.GetStateValue<int>("count"));
    }

    [Fact]
    public async Task Add_DefaultsToZeroWhenNoState()
    {
        var ctx = new MockObjectContext("new-counter");

        var counter = new CounterObject();
        var result = await counter.Add(ctx, 1);

        Assert.Equal(1, result);
        Assert.Equal(1, ctx.GetStateValue<int>("count"));
    }

    [Fact]
    public async Task Add_NegativeDelta_Decrements()
    {
        var ctx = new MockObjectContext("counter");
        ctx.SetupState(new StateKey<int>("count"), 10);

        var counter = new CounterObject();
        var result = await counter.Add(ctx, -3);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task Get_ReturnsCurrentCount()
    {
        var ctx = new MockSharedObjectContext("counter");
        ctx.SetupState(new StateKey<int>("count"), 42);

        var counter = new CounterObject();
        var result = await counter.Get(ctx);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Get_ReturnsZeroWhenNoState()
    {
        var ctx = new MockSharedObjectContext("empty");

        var counter = new CounterObject();
        var result = await counter.Get(ctx);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Reset_ClearsAllState()
    {
        var ctx = new MockObjectContext("counter");
        ctx.SetupState(new StateKey<int>("count"), 100);

        var counter = new CounterObject();
        await counter.Reset(ctx);

        Assert.False(ctx.HasState("count"));
    }
}