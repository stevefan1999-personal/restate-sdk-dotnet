using Restate.Sdk.Testing;

namespace Restate.Sdk.Tests.Handlers;

// Inline handler for testing — demonstrates ctx.Run() and ctx.Sleep()
[Service]
file class GreeterService
{
    [Handler]
    public async Task<string> Greet(Context ctx, string name)
    {
        var greeting = await ctx.Run("generate-greeting",
            () => $"Hello, {name}!");

        await ctx.Sleep(TimeSpan.FromSeconds(1));

        return greeting;
    }

    [Handler]
    public async Task<string> GreetWithCancellation(Context ctx, string name)
    {
        var greeting = await ctx.Run("generate-greeting", runCtx =>
        {
            runCtx.CancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Hi, {name}!");
        });

        return greeting;
    }
}

public class GreeterHandlerTests
{
    [Fact]
    public async Task Greet_ReturnsGreeting()
    {
        var ctx = new MockContext();

        var service = new GreeterService();
        var result = await service.Greet(ctx, "Alice");

        Assert.Equal("Hello, Alice!", result);
    }

    [Fact]
    public async Task Greet_RecordsSleep()
    {
        var ctx = new MockContext();

        var service = new GreeterService();
        await service.Greet(ctx, "Bob");

        Assert.Single(ctx.Sleeps);
        Assert.Equal(TimeSpan.FromSeconds(1), ctx.Sleeps[0].Duration);
    }

    [Fact]
    public async Task Greet_RunsSideEffectAndReturnsResult()
    {
        var ctx = new MockContext();

        var service = new GreeterService();
        var result = await service.Greet(ctx, "Charlie");

        // The side effect (ctx.Run) executes inline in MockContext
        Assert.Contains("Charlie", result);
    }

    [Fact]
    public async Task GreetWithCancellation_ExecutesWithRunContext()
    {
        var ctx = new MockContext();

        var service = new GreeterService();
        var result = await service.GreetWithCancellation(ctx, "Dana");

        Assert.Equal("Hi, Dana!", result);
    }
}