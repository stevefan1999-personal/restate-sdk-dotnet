using System.ComponentModel;

namespace Restate.Sdk.Endpoint;

/// <summary>
///     Registry for generated service definitions.
///     Source generators register service definitions at module initialization time.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ServiceDefinitionRegistry
{
    private static readonly Lock SyncRoot = new();
    private static Dictionary<Type, ServiceDefinition>? _definitions;

    /// <summary>
    ///     Registers a pre-built service definition for a given implementation type.
    ///     Called by generated code.
    /// </summary>
    public static void Register<TService>(ServiceDefinition definition)
    {
        lock (SyncRoot)
        {
            _definitions ??= [];
            _definitions[typeof(TService)] = definition;
        }
    }

    internal static ServiceDefinition? TryGet(Type implementationType)
    {
        lock (SyncRoot)
        {
            if (_definitions is null) return null;
            _definitions.TryGetValue(implementationType, out var def);
            return def;
        }
    }
}