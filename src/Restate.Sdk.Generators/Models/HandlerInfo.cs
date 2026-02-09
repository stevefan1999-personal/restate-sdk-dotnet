namespace Restate.Sdk.Generators.Models;

internal sealed class HandlerInfo : IEquatable<HandlerInfo>
{
    public HandlerInfo(
        string name,
        string methodName,
        bool isShared,
        string? inputTypeFullName,
        string? outputTypeFullName,
        string returnTypeFullName,
        string contextParameterType,
        bool hasCancellationToken,
        string? inactivityTimeout = null,
        string? abortTimeout = null,
        string? idempotencyRetention = null,
        string? journalRetention = null,
        bool ingressPrivate = false)
    {
        Name = name;
        MethodName = methodName;
        IsShared = isShared;
        InputTypeFullName = inputTypeFullName;
        OutputTypeFullName = outputTypeFullName;
        ReturnTypeFullName = returnTypeFullName;
        ContextParameterType = contextParameterType;
        HasCancellationToken = hasCancellationToken;
        InactivityTimeout = inactivityTimeout;
        AbortTimeout = abortTimeout;
        IdempotencyRetention = idempotencyRetention;
        JournalRetention = journalRetention;
        IngressPrivate = ingressPrivate;
    }

    public string Name { get; }
    public string MethodName { get; }
    public bool IsShared { get; }
    public string? InputTypeFullName { get; }
    public string? OutputTypeFullName { get; }
    public string ReturnTypeFullName { get; }
    public string ContextParameterType { get; }
    public bool HasCancellationToken { get; }
    public string? InactivityTimeout { get; }
    public string? AbortTimeout { get; }
    public string? IdempotencyRetention { get; }
    public string? JournalRetention { get; }
    public bool IngressPrivate { get; }

    public bool Equals(HandlerInfo? other)
    {
        if (other is null) return false;
        return Name == other.Name
               && MethodName == other.MethodName
               && IsShared == other.IsShared
               && InputTypeFullName == other.InputTypeFullName
               && OutputTypeFullName == other.OutputTypeFullName
               && ReturnTypeFullName == other.ReturnTypeFullName
               && ContextParameterType == other.ContextParameterType
               && HasCancellationToken == other.HasCancellationToken
               && InactivityTimeout == other.InactivityTimeout
               && AbortTimeout == other.AbortTimeout
               && IdempotencyRetention == other.IdempotencyRetention
               && JournalRetention == other.JournalRetention
               && IngressPrivate == other.IngressPrivate;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HandlerInfo);
    }

    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (Name?.GetHashCode() ?? 0);
            hash = hash * 31 + (MethodName?.GetHashCode() ?? 0);
            hash = hash * 31 + IsShared.GetHashCode();
            hash = hash * 31 + (InputTypeFullName?.GetHashCode() ?? 0);
            hash = hash * 31 + (OutputTypeFullName?.GetHashCode() ?? 0);
            hash = hash * 31 + (ReturnTypeFullName?.GetHashCode() ?? 0);
            hash = hash * 31 + (ContextParameterType?.GetHashCode() ?? 0);
            hash = hash * 31 + HasCancellationToken.GetHashCode();
            hash = hash * 31 + (InactivityTimeout?.GetHashCode() ?? 0);
            hash = hash * 31 + (AbortTimeout?.GetHashCode() ?? 0);
            hash = hash * 31 + (IdempotencyRetention?.GetHashCode() ?? 0);
            hash = hash * 31 + (JournalRetention?.GetHashCode() ?? 0);
            hash = hash * 31 + IngressPrivate.GetHashCode();
            return hash;
        }
#else
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(MethodName);
        hash.Add(IsShared);
        hash.Add(InputTypeFullName);
        hash.Add(OutputTypeFullName);
        hash.Add(ReturnTypeFullName);
        hash.Add(ContextParameterType);
        hash.Add(HasCancellationToken);
        hash.Add(InactivityTimeout);
        hash.Add(AbortTimeout);
        hash.Add(IdempotencyRetention);
        hash.Add(JournalRetention);
        hash.Add(IngressPrivate);
        return hash.ToHashCode();
#endif
    }
}
