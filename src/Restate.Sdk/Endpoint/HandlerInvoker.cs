using System.ComponentModel;

namespace Restate.Sdk.Endpoint;

/// <summary>
///     Delegate for invoking a handler method on a service instance.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate Task<object?> HandlerInvoker(
    object instance, Context context, object? input, CancellationToken ct);