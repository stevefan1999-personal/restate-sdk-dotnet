using System.Collections.Immutable;

namespace Restate.Sdk.Generators.Models;

internal sealed class ServiceInfo : IEquatable<ServiceInfo>
{
    public ServiceInfo(
        string @namespace,
        string className,
        string serviceName,
        ServiceKind kind,
        ImmutableArray<HandlerInfo> handlers,
        string? workflowRetention = null)
    {
        Namespace = @namespace;
        ClassName = className;
        ServiceName = serviceName;
        Kind = kind;
        Handlers = handlers;
        WorkflowRetention = workflowRetention;
    }

    public string Namespace { get; }
    public string ClassName { get; }
    public string ServiceName { get; }
    public ServiceKind Kind { get; }
    public ImmutableArray<HandlerInfo> Handlers { get; }
    public string? WorkflowRetention { get; }

    public bool Equals(ServiceInfo? other)
    {
        if (other is null) return false;
        if (Namespace != other.Namespace || ClassName != other.ClassName
                                         || ServiceName != other.ServiceName || Kind != other.Kind
                                         || WorkflowRetention != other.WorkflowRetention)
            return false;
        if (Handlers.Length != other.Handlers.Length)
            return false;
        for (var i = 0; i < Handlers.Length; i++)
            if (!Handlers[i].Equals(other.Handlers[i]))
                return false;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ServiceInfo);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
            hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
            hash = hash * 31 + (ServiceName?.GetHashCode() ?? 0);
            hash = hash * 31 + Kind.GetHashCode();
            hash = hash * 31 + (WorkflowRetention?.GetHashCode() ?? 0);
            foreach (var h in Handlers)
                hash = hash * 31 + h.GetHashCode();
            return hash;
        }
    }
}
