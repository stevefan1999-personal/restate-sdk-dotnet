using Restate.Sdk;

namespace Restate.Sdk.ThroughputBenchmark;

[VirtualObject]
public sealed class CounterObject
{
    private static readonly StateKey<int> Count = new("count");

    [Handler]
    public async Task<int> GetAndAdd(ObjectContext ctx, int delta)
    {
        var current = await ctx.Get(Count);
        var newValue = current + delta;
        ctx.Set(Count, newValue);
        return newValue;
    }
}
