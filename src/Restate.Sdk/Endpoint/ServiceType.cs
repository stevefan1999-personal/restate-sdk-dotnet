using System.ComponentModel;

namespace Restate.Sdk.Endpoint;

/// <summary>
///     Specifies the type of a Restate service.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public enum ServiceType
{
    /// <summary>A stateless service whose handlers can run concurrently.</summary>
    Service,

    /// <summary>A keyed virtual object with exclusive per-key state and handler access.</summary>
    VirtualObject,

    /// <summary>A keyed workflow with a single run handler and durable promise support.</summary>
    Workflow
}