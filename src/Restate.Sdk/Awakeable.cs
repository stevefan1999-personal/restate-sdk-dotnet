namespace Restate.Sdk;

/// <summary>
///     A durable promise that can be completed from outside the current invocation.
/// </summary>
public readonly record struct Awakeable<T>
{
    /// <summary>
    ///     The unique identifier for this awakeable. Pass this to an external system
    ///     so it can resolve the awakeable when ready.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     The value that will be available once the awakeable is resolved.
    /// </summary>
    /// <remarks>This value must only be awaited once.</remarks>
    public required ValueTask<T> Value { get; init; }
}