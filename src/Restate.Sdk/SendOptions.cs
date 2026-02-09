namespace Restate.Sdk;

/// <summary>
///     Options for fire-and-forget send operations.
/// </summary>
public readonly record struct SendOptions
{
    /// <summary>Delay before the invocation is executed.</summary>
    public TimeSpan? Delay { get; init; }

    /// <summary>Idempotency key to deduplicate send operations.</summary>
    public string? IdempotencyKey { get; init; }

    /// <inheritdoc cref="Delay" />
    public static SendOptions AfterDelay(TimeSpan delay)
    {
        return new SendOptions { Delay = delay };
    }

    /// <inheritdoc cref="IdempotencyKey" />
    public static SendOptions WithIdempotencyKey(string key)
    {
        return new SendOptions { IdempotencyKey = key };
    }
}