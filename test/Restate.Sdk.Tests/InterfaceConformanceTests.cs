using Restate.Sdk.Testing;

namespace Restate.Sdk.Tests;

public class InterfaceConformanceTests
{
    // ──────────────────────────────────────────────
    // 1. Interface hierarchy relationships
    // ──────────────────────────────────────────────

    [Fact]
    public void ISharedObjectContext_Is_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(ISharedObjectContext)));
    }

    [Fact]
    public void IObjectContext_Is_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(IObjectContext)));
    }

    [Fact]
    public void IObjectContext_Is_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(IObjectContext)));
    }

    [Fact]
    public void ISharedWorkflowContext_Is_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(ISharedWorkflowContext)));
    }

    [Fact]
    public void ISharedWorkflowContext_Is_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(ISharedWorkflowContext)));
    }

    [Fact]
    public void IWorkflowContext_Is_IObjectContext()
    {
        Assert.True(typeof(IObjectContext).IsAssignableFrom(typeof(IWorkflowContext)));
    }

    [Fact]
    public void IWorkflowContext_Is_ISharedWorkflowContext()
    {
        Assert.True(typeof(ISharedWorkflowContext).IsAssignableFrom(typeof(IWorkflowContext)));
    }

    [Fact]
    public void IWorkflowContext_Is_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(IWorkflowContext)));
    }

    [Fact]
    public void IWorkflowContext_Is_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(IWorkflowContext)));
    }

    // ──────────────────────────────────────────────
    // 2. Abstract classes implement expected interfaces
    // ──────────────────────────────────────────────

    [Fact]
    public void Context_Implements_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(Context)));
    }

    [Fact]
    public void SharedObjectContext_Implements_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(SharedObjectContext)));
    }

    [Fact]
    public void SharedObjectContext_Implements_IContext()
    {
        Assert.True(typeof(IContext).IsAssignableFrom(typeof(SharedObjectContext)));
    }

    [Fact]
    public void ObjectContext_Implements_IObjectContext()
    {
        Assert.True(typeof(IObjectContext).IsAssignableFrom(typeof(ObjectContext)));
    }

    [Fact]
    public void ObjectContext_Implements_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(ObjectContext)));
    }

    [Fact]
    public void SharedWorkflowContext_Implements_ISharedWorkflowContext()
    {
        Assert.True(typeof(ISharedWorkflowContext).IsAssignableFrom(typeof(SharedWorkflowContext)));
    }

    [Fact]
    public void SharedWorkflowContext_Implements_ISharedObjectContext()
    {
        Assert.True(typeof(ISharedObjectContext).IsAssignableFrom(typeof(SharedWorkflowContext)));
    }

    [Fact]
    public void WorkflowContext_Implements_IWorkflowContext()
    {
        Assert.True(typeof(IWorkflowContext).IsAssignableFrom(typeof(WorkflowContext)));
    }

    [Fact]
    public void WorkflowContext_Implements_IObjectContext()
    {
        Assert.True(typeof(IObjectContext).IsAssignableFrom(typeof(WorkflowContext)));
    }

    [Fact]
    public void WorkflowContext_Implements_ISharedWorkflowContext()
    {
        Assert.True(typeof(ISharedWorkflowContext).IsAssignableFrom(typeof(WorkflowContext)));
    }

    // ──────────────────────────────────────────────
    // 3. Mock contexts implement expected interfaces (runtime instances)
    // ──────────────────────────────────────────────

    [Fact]
    public void MockContext_Is_IContext()
    {
        var ctx = new MockContext();
        Assert.IsAssignableFrom<IContext>(ctx);
    }

    [Fact]
    public void MockObjectContext_Is_IObjectContext()
    {
        var ctx = new MockObjectContext();
        Assert.IsAssignableFrom<IObjectContext>(ctx);
    }

    [Fact]
    public void MockObjectContext_Is_ISharedObjectContext()
    {
        var ctx = new MockObjectContext();
        Assert.IsAssignableFrom<ISharedObjectContext>(ctx);
    }

    [Fact]
    public void MockObjectContext_Is_IContext()
    {
        var ctx = new MockObjectContext();
        Assert.IsAssignableFrom<IContext>(ctx);
    }

    [Fact]
    public void MockSharedObjectContext_Is_ISharedObjectContext()
    {
        var ctx = new MockSharedObjectContext();
        Assert.IsAssignableFrom<ISharedObjectContext>(ctx);
    }

    [Fact]
    public void MockSharedObjectContext_Is_IContext()
    {
        var ctx = new MockSharedObjectContext();
        Assert.IsAssignableFrom<IContext>(ctx);
    }

    [Fact]
    public void MockWorkflowContext_Is_IWorkflowContext()
    {
        var ctx = new MockWorkflowContext();
        Assert.IsAssignableFrom<IWorkflowContext>(ctx);
    }

    [Fact]
    public void MockWorkflowContext_Is_IObjectContext()
    {
        var ctx = new MockWorkflowContext();
        Assert.IsAssignableFrom<IObjectContext>(ctx);
    }

    [Fact]
    public void MockWorkflowContext_Is_ISharedWorkflowContext()
    {
        var ctx = new MockWorkflowContext();
        Assert.IsAssignableFrom<ISharedWorkflowContext>(ctx);
    }

    [Fact]
    public void MockSharedWorkflowContext_Is_ISharedWorkflowContext()
    {
        var ctx = new MockSharedWorkflowContext();
        Assert.IsAssignableFrom<ISharedWorkflowContext>(ctx);
    }

    [Fact]
    public void MockSharedWorkflowContext_Is_ISharedObjectContext()
    {
        var ctx = new MockSharedWorkflowContext();
        Assert.IsAssignableFrom<ISharedObjectContext>(ctx);
    }

    [Fact]
    public void MockSharedWorkflowContext_Is_IContext()
    {
        var ctx = new MockSharedWorkflowContext();
        Assert.IsAssignableFrom<IContext>(ctx);
    }

    // ──────────────────────────────────────────────
    // 4. Mock contexts can be passed to methods accepting interfaces
    // ──────────────────────────────────────────────

    [Fact]
    public async Task MockContext_CanBePassedAsIContext()
    {
        var mock = new MockContext();
        var result = await AcceptIContext(mock);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MockObjectContext_CanBePassedAsIObjectContext()
    {
        var mock = new MockObjectContext();
        var result = await AcceptIObjectContext(mock);
        Assert.Equal("test-key", result);
    }

    [Fact]
    public async Task MockWorkflowContext_CanBePassedAsIObjectContext()
    {
        var mock = new MockWorkflowContext();
        var result = await AcceptIObjectContext(mock);
        Assert.Equal("test-key", result);
    }

    [Fact]
    public async Task MockWorkflowContext_CanBePassedAsISharedWorkflowContext()
    {
        var mock = new MockWorkflowContext();
        var result = await AcceptISharedWorkflowContext(mock);
        Assert.Equal("test-key", result);
    }

    // Helper methods simulating handler signatures that accept interfaces

    private static async Task<string> AcceptIContext(IContext ctx)
    {
        await ctx.Sleep(TimeSpan.FromMilliseconds(1));
        return ctx.InvocationId;
    }

    private static Task<string> AcceptIObjectContext(IObjectContext ctx)
    {
        return Task.FromResult(ctx.Key);
    }

    private static Task<string> AcceptISharedWorkflowContext(ISharedWorkflowContext ctx)
    {
        return Task.FromResult(ctx.Key);
    }
}
