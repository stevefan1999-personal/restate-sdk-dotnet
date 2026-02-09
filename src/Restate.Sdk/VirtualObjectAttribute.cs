namespace Restate.Sdk;

/// <summary>
///     Marks a class as a Restate virtual object — a keyed stateful entity.
///     Each key has isolated K/V state and exclusive handler execution.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VirtualObjectAttribute : Attribute
{
    /// <summary>
    ///     Override the service name used in discovery and invocation routing.
    ///     Defaults to the class name.
    /// </summary>
    public string? Name { get; set; }
}