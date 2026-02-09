using Restate.Sdk.Hosting;

namespace Restate.Sdk.Tests;

public class OptionsTests
{
    // ── SendOptions ──

    [Fact]
    public void SendOptions_Defaults_AreNull()
    {
        var opts = new SendOptions();
        Assert.Null(opts.Delay);
        Assert.Null(opts.IdempotencyKey);
    }

    [Fact]
    public void SendOptions_AfterDelay_SetsDelay()
    {
        var opts = SendOptions.AfterDelay(TimeSpan.FromSeconds(5));
        Assert.Equal(TimeSpan.FromSeconds(5), opts.Delay);
        Assert.Null(opts.IdempotencyKey);
    }

    [Fact]
    public void SendOptions_WithIdempotencyKey_SetsKey()
    {
        var opts = SendOptions.WithIdempotencyKey("my-key");
        Assert.Equal("my-key", opts.IdempotencyKey);
        Assert.Null(opts.Delay);
    }

    [Fact]
    public void SendOptions_WithBothDelayAndKey()
    {
        var opts = new SendOptions
        {
            Delay = TimeSpan.FromSeconds(3),
            IdempotencyKey = "key-123"
        };
        Assert.Equal(TimeSpan.FromSeconds(3), opts.Delay);
        Assert.Equal("key-123", opts.IdempotencyKey);
    }

    // ── InvocationHandle ──

    [Fact]
    public void InvocationHandle_StoresId()
    {
        var handle = new InvocationHandle("inv-abc-123");
        Assert.Equal("inv-abc-123", handle.InvocationId);
    }

    // ── RestateOptions ──

    [Fact]
    public void RestateOptions_AddService_AddsType()
    {
        var opts = new RestateOptions();
        opts.AddService<OptionsTests>();

        Assert.Single(opts.ServiceTypes);
        Assert.Equal(typeof(OptionsTests), opts.ServiceTypes[0]);
    }

    [Fact]
    public void RestateOptions_MultipleAdd_Accumulates()
    {
        var opts = new RestateOptions();
        opts.AddService<OptionsTests>();
        opts.AddService<TerminalException>();

        Assert.Equal(2, opts.ServiceTypes.Count);
        Assert.Contains(typeof(OptionsTests), opts.ServiceTypes);
        Assert.Contains(typeof(TerminalException), opts.ServiceTypes);
    }

    [Fact]
    public void RestateOptions_AddService_ReturnsSelf_ForChaining()
    {
        var opts = new RestateOptions();
        var returned = opts.AddService<OptionsTests>();

        Assert.Same(opts, returned);
    }

    [Fact]
    public void RestateOptions_ChainedAdds()
    {
        var opts = new RestateOptions()
            .AddService<OptionsTests>()
            .AddService<TerminalException>()
            .AddService<TerminalException>();

        Assert.Equal(3, opts.ServiceTypes.Count);
    }

    [Fact]
    public void RestateOptions_EmptyByDefault()
    {
        var opts = new RestateOptions();
        Assert.Empty(opts.ServiceTypes);
    }
}
