using System.Buffers;

namespace Restate.Sdk;

/// <summary>
///     Strongly-typed serialization/deserialization interface for Restate handler payloads and state.
///     Named "Serde" for consistency with other Restate SDKs (TypeScript, Java, Python, Rust).
/// </summary>
public interface ISerde<T>
{
    /// <summary>Gets the MIME content type used by this serde (e.g. <c>"application/json"</c>).</summary>
    string ContentType { get; }

    /// <summary>Serializes <paramref name="value" /> to the specified <paramref name="writer" />.</summary>
    /// <param name="writer">The buffer writer to write serialized bytes to.</param>
    /// <param name="value">The value to serialize.</param>
    void Serialize(IBufferWriter<byte> writer, T value);

    /// <summary>Deserializes a value of type <typeparamref name="T" /> from the specified <paramref name="data" />.</summary>
    /// <param name="data">The byte sequence to deserialize from.</param>
    /// <returns>The deserialized value.</returns>
    T Deserialize(ReadOnlySequence<byte> data);
}