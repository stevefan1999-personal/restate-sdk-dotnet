using System.Buffers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

// ReSharper disable once CheckNamespace — public API referenced by source-generated code as global::Restate.Sdk.JsonSerde
namespace Restate.Sdk
{
    /// <summary>
    ///     Static utility providing the default <see cref="JsonSerializerOptions" /> for Restate serialization.
    ///     Source-generated code calls <see cref="Configure" /> at module init to wire in a
    ///     <c>JsonSerializerContext</c> for AOT-compatible serialization.
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Default used only as fallback; source-generated context replaces it in AOT scenarios.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Default used only as fallback; source-generated context replaces it in AOT scenarios.")]
    public static class JsonSerde
    {
        private static JsonSerializerOptions _options = JsonSerializerOptions.Default;

        /// <summary>Returns the configured <see cref="JsonSerializerOptions" /> used by the SDK.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static JsonSerializerOptions SerializerOptions => Volatile.Read(ref _options);

        /// <summary>
        ///     Configures the default serializer options. Called by generated code
        ///     to wire in source-generated <c>JsonSerializerContext</c> for AOT compatibility.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Configure(JsonSerializerOptions options)
        {
            Volatile.Write(ref _options, options);
        }
    }
}

namespace Restate.Sdk.Internal.Serde
{
    /// <summary>
    ///     Strongly-typed JSON serde using <see cref="System.Text.Json" />.
    ///     Supports both <see cref="JsonSerializerOptions" /> and <see cref="JsonTypeInfo{T}" /> for AOT.
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection path is fallback only; AOT users provide JsonTypeInfo<T>.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Reflection path is fallback only; AOT users provide JsonTypeInfo<T>.")]
    internal sealed class JsonSerde<T> : ISerde<T>
    {
        public static readonly JsonSerde<T> Default = new(JsonSerializerOptions.Default);

        private static readonly JsonWriterOptions WriterOptions = new() { SkipValidation = true };

        private readonly JsonSerializerOptions? _options;
        private readonly JsonTypeInfo<T>? _typeInfo;

        public JsonSerde(JsonSerializerOptions options)
        {
            _options = options;
        }

        public JsonSerde(JsonTypeInfo<T> typeInfo)
        {
            _typeInfo = typeInfo;
        }

        public string ContentType => "application/json";

        public void Serialize(IBufferWriter<byte> writer, T value)
        {
            using var jsonWriter = new Utf8JsonWriter(writer, WriterOptions);
            if (_typeInfo is not null)
                JsonSerializer.Serialize(jsonWriter, value, _typeInfo);
            else
                JsonSerializer.Serialize(jsonWriter, value, _options ?? JsonSerializerOptions.Default);
        }

        public T Deserialize(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
                return default!;
            var reader = new Utf8JsonReader(data);
            if (_typeInfo is not null)
                return JsonSerializer.Deserialize(ref reader, _typeInfo)!;
            return JsonSerializer.Deserialize<T>(ref reader, _options ?? JsonSerializerOptions.Default)!;
        }
    }
}
