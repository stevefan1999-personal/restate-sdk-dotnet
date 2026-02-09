using System.Runtime.CompilerServices;

namespace Restate.Sdk;

/// <summary>
///     A replay-aware console that suppresses output during journal replay.
///     Checks the live replay state on each call so output resumes after replay completes.
/// </summary>
public sealed class DurableConsole
{
    // Internal: accessed by ReplayAwareInterpolatedStringHandler (same assembly, public ref struct).
    internal readonly Func<bool> _isReplaying;

    internal DurableConsole(Func<bool> isReplaying)
    {
        _isReplaying = isReplaying;
    }

    /// <summary>Writes a message to the console, suppressed during replay.</summary>
    public void Log(string message)
    {
        if (!_isReplaying())
            Console.WriteLine(message);
    }

    /// <summary>Writes an interpolated string to the console, suppressed during replay. Avoids allocations when suppressed.</summary>
    public void Log([InterpolatedStringHandlerArgument("")] ref ReplayAwareInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
            Console.WriteLine(handler.ToStringAndClear());
    }
}

/// <summary>
///     Interpolated string handler that skips formatting entirely during replay.
///     Avoids boxing and string allocation when the console output would be suppressed.
/// </summary>
[InterpolatedStringHandler]
public ref struct ReplayAwareInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _inner;

    internal bool IsEnabled { get; }

    /// <summary>Called by the compiler for interpolated string arguments. Skips formatting when replaying.</summary>
    public ReplayAwareInterpolatedStringHandler(
        int literalLength, int formattedCount, DurableConsole console, out bool handlerIsValid)
    {
        IsEnabled = !console._isReplaying();
        handlerIsValid = IsEnabled;
        if (IsEnabled)
            _inner = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
    }

    /// <summary>Appends a literal string to the handler.</summary>
    public void AppendLiteral(string value)
    {
        _inner.AppendLiteral(value);
    }

    /// <summary>Appends a formatted value to the handler.</summary>
    public void AppendFormatted<T>(T value)
    {
        _inner.AppendFormatted(value);
    }

    /// <summary>Appends a formatted value with a format string to the handler.</summary>
    public void AppendFormatted<T>(T value, string? format)
    {
        _inner.AppendFormatted(value, format);
    }

    internal string ToStringAndClear()
    {
        return _inner.ToStringAndClear();
    }
}