namespace Restate.Sdk.Tests;

public class TerminalExceptionTests
{
    [Fact]
    public void DefaultCode_Is500()
    {
        var err = new TerminalException("something broke");
        Assert.Equal(500, err.Code);
        Assert.Equal("something broke", err.Message);
    }

    [Fact]
    public void CustomCode()
    {
        var err = new TerminalException("not found", 404);
        Assert.Equal(404, err.Code);
    }

    [Fact]
    public void WithInnerException()
    {
        var inner = new InvalidOperationException("bad state");
        var err = new TerminalException("wrapped", inner, 409);
        Assert.Equal(409, err.Code);
        Assert.Same(inner, err.InnerException);
    }

    [Fact]
    public void Message_IsPreserved()
    {
        var err = new TerminalException("detailed error message");
        Assert.Equal("detailed error message", err.Message);
    }

    [Fact]
    public void InnerException_WithDefaultCode()
    {
        var inner = new ArgumentException("bad arg");
        var err = new TerminalException("outer", inner);
        Assert.Equal(500, err.Code);
        Assert.Same(inner, err.InnerException);
        Assert.Equal("outer", err.Message);
    }

    [Fact]
    public void IsException()
    {
        var err = new TerminalException("test");
        Assert.IsAssignableFrom<Exception>(err);
    }

    [Fact]
    public void Code_ZeroIsValid()
    {
        var err = new TerminalException("zero code", 0);
        Assert.Equal(0, err.Code);
    }

    [Fact]
    public void Code_MaxUshortIsValid()
    {
        var err = new TerminalException("max code", ushort.MaxValue);
        Assert.Equal(ushort.MaxValue, err.Code);
    }

    [Fact]
    public void CanBeCaughtAsException()
    {
        try
        {
            throw new TerminalException("test throw", 422);
        }
        catch (Exception ex)
        {
            Assert.IsType<TerminalException>(ex);
            var terminal = (TerminalException)ex;
            Assert.Equal(422, terminal.Code);
            Assert.Equal("test throw", terminal.Message);
        }
    }
}