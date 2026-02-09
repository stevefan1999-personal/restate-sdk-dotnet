namespace Restate.Sdk;

/// <summary>
///     Marks a class as a Restate workflow — a virtual object with a run handler
///     that executes exactly once per workflow ID, plus shared handlers for queries and signals.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class WorkflowAttribute : Attribute
{
    /// <summary>
    ///     Override the service name used in discovery and invocation routing.
    ///     Defaults to the class name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Duration to retain the workflow execution data after the workflow completes.
    ///     Format: TimeSpan string (e.g. "1.00:00:00" for 24 hours).
    /// </summary>
    public string? WorkflowRetention { get; set; }
}
