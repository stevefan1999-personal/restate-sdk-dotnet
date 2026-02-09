using Restate.Sdk.Endpoint;

namespace Restate.Sdk.Tests.Endpoint;

public class ServiceDefinitionTests
{
    [Fact]
    public void FromType_Service_DetectsCorrectly()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!;

        Assert.Equal("GreeterService", def.Name);
        Assert.Equal(ServiceType.Service, def.Type);
        Assert.NotNull(def.Factory);
        Assert.Equal(2, def.Handlers.Count);
    }

    [Fact]
    public void FromType_VirtualObject_DetectsCorrectly()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!;

        Assert.Equal("CounterObject", def.Name);
        Assert.Equal(ServiceType.VirtualObject, def.Type);
        Assert.Equal(2, def.Handlers.Count);
    }

    [Fact]
    public void FromType_Workflow_DetectsCorrectly()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(OrderWorkflow))!;

        Assert.Equal("OrderWorkflow", def.Name);
        Assert.Equal(ServiceType.Workflow, def.Type);
        Assert.Equal(2, def.Handlers.Count);
    }

    [Fact]
    public void FromType_UsesCustomName()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(NamedService))!;

        Assert.Equal("CustomName", def.Name);
        Assert.Single(def.Handlers);
        Assert.Equal("CustomHandler", def.Handlers[0].Name);
    }

    [Fact]
    public void TryGet_ReturnsNullForNonService()
    {
        Assert.Null(ServiceDefinitionRegistry.TryGet(typeof(NotAService)));
    }

    [Fact]
    public void TryGet_ReturnsNullForEmptyService()
    {
        // EmptyService has [Service] but no handlers.
        // The source generator won't register it, so TryGet returns null.
        Assert.Null(ServiceDefinitionRegistry.TryGet(typeof(EmptyService)));
    }

    [Fact]
    public void Handler_DetectsInputAndOutputTypes()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!;
        var greet = def.Handlers.First(h => h.Name == "Greet");

        Assert.True(greet.HasInput);
        Assert.True(greet.HasOutput);
        Assert.False(greet.IsShared);
    }

    [Fact]
    public void Handler_DetectsVoidOutput()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!;
        var sayHello = def.Handlers.First(h => h.Name == "SayHello");

        Assert.False(sayHello.HasInput);
        Assert.False(sayHello.HasOutput);
    }

    [Fact]
    public void SharedHandler_IsDetected()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!;
        var get = def.Handlers.First(h => h.Name == "Get");

        Assert.True(get.IsShared);
    }

    [Fact]
    public void Handler_DetectsValueTaskReturn()
    {
        var def = ServiceDefinitionRegistry.TryGet(typeof(ValueTaskService))!;

        var getNumber = def.Handlers.First(h => h.Name == "GetNumber");
        Assert.True(getNumber.HasOutput);

        var doWork = def.Handlers.First(h => h.Name == "DoWork");
        Assert.False(doWork.HasOutput);
    }
}