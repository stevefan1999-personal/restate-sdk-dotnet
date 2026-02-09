using System.Collections.Frozen;
using Restate.Sdk.Endpoint;

namespace Restate.Sdk.Internal;

internal sealed class ServiceRegistry
{
    private FrozenDictionary<(string Service, string Handler), HandlerDefinition>? _frozenHandlers;

    private FrozenDictionary<string, ServiceDefinition>? _frozenServices;
    private Dictionary<(string Service, string Handler), HandlerDefinition>? _handlers = new();

    private bool _isFrozen;
    private Dictionary<string, ServiceDefinition>? _services = new();

    public IEnumerable<ServiceDefinition> Services =>
        _frozenServices is not null ? _frozenServices.Values : _services!.Values;

    /// <summary>
    ///     Builds a frozen registry from a list of service types.
    ///     Resolves each type via <see cref="ServiceDefinitionRegistry" />.
    /// </summary>
    public static ServiceRegistry FromTypes(IEnumerable<Type> serviceTypes)
    {
        var registry = new ServiceRegistry();
        foreach (var type in serviceTypes)
        {
            var def = ServiceDefinitionRegistry.TryGet(type)
                      ?? throw new InvalidOperationException(
                          $"No generated service definition for '{type.Name}'. " +
                          "Ensure the Restate source generator is enabled.");
            registry.Register(def);
        }

        registry.Freeze();
        return registry;
    }

    public void Register(ServiceDefinition service)
    {
        if (_isFrozen)
            throw new InvalidOperationException("Cannot register services after the registry is frozen");

        if (!_services!.TryAdd(service.Name, service))
            throw new InvalidOperationException($"Service '{service.Name}' is already registered");

        foreach (var handler in service.Handlers)
            if (!_handlers!.TryAdd((service.Name, handler.Name), handler))
                throw new InvalidOperationException(
                    $"Handler '{handler.Name}' is already registered on service '{service.Name}'");
    }

    public void Freeze()
    {
        if (_isFrozen) return;

        _frozenServices = _services!.ToFrozenDictionary();
        _frozenHandlers = _handlers!.ToFrozenDictionary();

        _services = null;
        _handlers = null;
        _isFrozen = true;
    }

    public bool TryGetService(string name, out ServiceDefinition? service)
    {
        if (_frozenServices is not null)
            return _frozenServices.TryGetValue(name, out service);
        return _services!.TryGetValue(name, out service);
    }

    public bool TryGetHandler(string serviceName, string handlerName, out HandlerDefinition? handler)
    {
        if (_frozenHandlers is not null)
            return _frozenHandlers.TryGetValue((serviceName, handlerName), out handler);
        return _handlers!.TryGetValue((serviceName, handlerName), out handler);
    }
}