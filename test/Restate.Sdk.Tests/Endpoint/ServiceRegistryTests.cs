using Restate.Sdk.Endpoint;
using Restate.Sdk.Internal;

namespace Restate.Sdk.Tests.Endpoint;

public class ServiceRegistryTests
{
    [Fact]
    public void Register_AndLookup_Works()
    {
        var registry = new ServiceRegistry();
        var def = ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!;
        registry.Register(def);

        Assert.True(registry.TryGetService("GreeterService", out var found));
        Assert.Equal("GreeterService", found!.Name);
    }

    [Fact]
    public void TryGetHandler_FindsHandler()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);

        Assert.True(registry.TryGetHandler("GreeterService", "Greet", out var handler));
        Assert.Equal("Greet", handler!.Name);
    }

    [Fact]
    public void TryGetHandler_ReturnsFalseForMissing()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);

        Assert.False(registry.TryGetHandler("GreeterService", "NonExistent", out _));
        Assert.False(registry.TryGetHandler("NonExistent", "Greet", out _));
    }

    [Fact]
    public void Register_ThrowsOnDuplicate()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!));
    }

    [Fact]
    public void Freeze_EnablesFrozenLookup()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);
        registry.Freeze();

        Assert.True(registry.TryGetService("GreeterService", out _));
        Assert.True(registry.TryGetService("CounterObject", out _));
        Assert.True(registry.TryGetHandler("GreeterService", "Greet", out _));
        Assert.True(registry.TryGetHandler("CounterObject", "Add", out _));
    }

    [Fact]
    public void Freeze_PreventsRegistration()
    {
        var registry = new ServiceRegistry();
        registry.Freeze();

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!));
    }

    [Fact]
    public void Freeze_IsIdempotent()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Freeze();
        registry.Freeze();

        Assert.True(registry.TryGetService("GreeterService", out _));
    }

    [Fact]
    public void Services_EnumeratesAll()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);

        Assert.Equal(2, registry.Services.Count());
    }

    [Fact]
    public void TryGetService_ReturnsFalseForUnknown()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);

        Assert.False(registry.TryGetService("NonExistentService", out var service));
        Assert.Null(service);
    }

    [Fact]
    public void TryGetService_ReturnsFalseForUnknown_WhenFrozen()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Freeze();

        Assert.False(registry.TryGetService("NonExistentService", out var service));
        Assert.Null(service);
    }

    [Fact]
    public void TryGetHandler_ReturnsFalseForUnknownService_WhenFrozen()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Freeze();

        Assert.False(registry.TryGetHandler("NonExistent", "Greet", out var handler));
        Assert.Null(handler);
    }

    [Fact]
    public void TryGetHandler_ReturnsFalseForUnknownHandler_WhenFrozen()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Freeze();

        Assert.False(registry.TryGetHandler("GreeterService", "NonExistentHandler", out var handler));
        Assert.Null(handler);
    }

    [Fact]
    public void Register_AfterFreeze_Throws()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Freeze();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!));
        Assert.Contains("frozen", ex.Message);
    }

    [Fact]
    public void Services_EnumeratesAll_BeforeFreeze()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);

        // Before freeze, should still enumerate from the mutable dictionary
        var names = registry.Services.Select(s => s.Name).OrderBy(n => n).ToArray();
        Assert.Equal(["CounterObject", "GreeterService"], names);
    }

    [Fact]
    public void Services_EnumeratesAll_AfterFreeze()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(OrderWorkflow))!);
        registry.Freeze();

        var names = registry.Services.Select(s => s.Name).OrderBy(n => n).ToArray();
        Assert.Equal(["CounterObject", "GreeterService", "OrderWorkflow"], names);
    }

    [Fact]
    public void Freeze_AllHandlersAccessible()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(GreeterService))!);
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);
        registry.Freeze();

        // Verify all handlers from both services are accessible after freeze
        Assert.True(registry.TryGetHandler("GreeterService", "Greet", out _));
        Assert.True(registry.TryGetHandler("GreeterService", "SayHello", out _));
        Assert.True(registry.TryGetHandler("CounterObject", "Add", out _));
        Assert.True(registry.TryGetHandler("CounterObject", "Get", out _));
    }

    [Fact]
    public void TryGetHandler_ReturnsCorrectDefinition()
    {
        var registry = new ServiceRegistry();
        registry.Register(ServiceDefinitionRegistry.TryGet(typeof(CounterObject))!);

        Assert.True(registry.TryGetHandler("CounterObject", "Add", out var addHandler));
        Assert.False(addHandler!.IsShared);

        Assert.True(registry.TryGetHandler("CounterObject", "Get", out var getHandler));
        Assert.True(getHandler!.IsShared);
    }
}