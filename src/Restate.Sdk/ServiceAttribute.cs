namespace Restate.Sdk;

/// <summary>
///     Marks a class as a Restate service — a stateless handler group with durable execution.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceAttribute : Attribute
{
    /// <summary>
    ///     Override the service name used in discovery and invocation routing.
    ///     Defaults to the class name.
    /// </summary>
    public string? Name { get; set; }
}