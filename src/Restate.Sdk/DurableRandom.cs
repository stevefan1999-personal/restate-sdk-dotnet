namespace Restate.Sdk;

/// <summary>
///     Deterministic random number generator seeded by the invocation ID.
///     Produces the same sequence on every replay, making it safe for durable execution.
/// </summary>
public sealed class DurableRandom
{
    private readonly Random _random;

    internal DurableRandom(ulong seed)
    {
        _random = new Random(unchecked((int)(seed ^ (seed >> 32))));
    }

    /// <summary>Returns a non-negative random integer.</summary>
    public int Next()
    {
        return _random.Next();
    }

    /// <summary>Returns a non-negative random integer less than <paramref name="maxExclusive" />.</summary>
    public int Next(int maxExclusive)
    {
        return _random.Next(maxExclusive);
    }

    /// <summary>Returns a random integer within the specified range.</summary>
    public int Next(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }

    /// <summary>Returns a random double between 0.0 and 1.0.</summary>
    public double NextDouble()
    {
        return _random.NextDouble();
    }

    /// <summary>Returns a deterministic version-4 GUID (RFC 4122).</summary>
    public Guid NextGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        _random.NextBytes(bytes);

        // RFC 4122 version 4 — set version and variant bits.
        // Use big-endian constructor so byte layout matches UUID string directly:
        // bytes[6] high nibble = version (4), bytes[8] high bits = variant (10xx)
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes, true);
    }
}