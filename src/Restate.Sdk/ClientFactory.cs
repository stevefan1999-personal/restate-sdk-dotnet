using System.ComponentModel;

namespace Restate.Sdk;

/// <summary>
///     Registry for generated typed client factories.
///     Source generators register their factory at module initialization time.
///     Thread-safety: single-writer (module initializer) / multiple-reader (invocation contexts)
///     via Volatile.Read/Write. Only one generator can register; if multiple register, last wins.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ClientFactory
{
    private static Func<Context, Type, string?, SendOptions?, object?>? _factory;

    /// <summary>
    ///     Registers a factory that creates typed clients for the given interface type.
    ///     Called by generated code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Register(Func<Context, Type, string?, SendOptions?, object?> factory)
    {
        Volatile.Write(ref _factory, factory);
    }

    internal static TClient Create<TClient>(Context context, string? key = null, SendOptions? options = null)
        where TClient : class
    {
        var factory = Volatile.Read(ref _factory);
        if (factory is not null)
        {
            var client = factory(context, typeof(TClient), key, options);
            if (client is TClient typed)
                return typed;
        }

        throw new NotSupportedException(
            $"No typed client registered for {typeof(TClient).Name}. " +
            "Ensure the source generator is enabled.");
    }
}