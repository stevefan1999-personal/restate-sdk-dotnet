namespace Restate.Sdk;

/// <summary>
///     A handle to a running invocation, returned by send operations.
/// </summary>
public readonly record struct InvocationHandle(string InvocationId);