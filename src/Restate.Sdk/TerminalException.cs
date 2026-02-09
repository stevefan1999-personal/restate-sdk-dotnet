namespace Restate.Sdk;

/// <summary>
///     An error that will not be retried by the Restate runtime.
///     Use this for business logic failures or validation errors where retrying would not help.
/// </summary>
public sealed class TerminalException : Exception
{
    /// <summary>Creates a terminal exception with the specified message and error code.</summary>
    public TerminalException(string message, ushort code = 500)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Creates a terminal exception with the specified message, inner exception, and error code.</summary>
    public TerminalException(string message, Exception innerException, ushort code = 500)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>The HTTP-like error code for this terminal error.</summary>
    public ushort Code { get; }
}