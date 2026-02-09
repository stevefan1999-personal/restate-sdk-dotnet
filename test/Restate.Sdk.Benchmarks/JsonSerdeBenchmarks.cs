using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using Restate.Sdk.Internal.Serde;

namespace Restate.Sdk.Benchmarks;

/// <summary>
///     Benchmarks for JSON serialization/deserialization — used for all user payloads
///     (request bodies, responses, state values, awakeable/promise payloads).
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class JsonSerdeBenchmarks
{
    private ArrayBufferWriter<byte> _largeBuffer = null!;
    private ReadOnlySequence<byte> _largeJson;
    private JsonSerde<LargeDto> _largeReflectionSerde = null!;
    private JsonSerde<LargeDto> _largeSourceGenSerde = null!;
    private JsonSerde<SmallDto> _reflectionSerde = null!;
    private ArrayBufferWriter<byte> _smallBuffer = null!;
    private ReadOnlySequence<byte> _smallJson;
    private JsonSerde<SmallDto> _sourceGenSerde = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Default);
        _reflectionSerde = new JsonSerde<SmallDto>(options);
        _sourceGenSerde = new JsonSerde<SmallDto>(BenchmarkJsonContext.Default.SmallDto);
        _largeReflectionSerde = new JsonSerde<LargeDto>(options);
        _largeSourceGenSerde = new JsonSerde<LargeDto>(BenchmarkJsonContext.Default.LargeDto);

        _smallJson = new ReadOnlySequence<byte>(
            JsonSerializer.SerializeToUtf8Bytes(SmallDto.Sample));
        _largeJson = new ReadOnlySequence<byte>(
            JsonSerializer.SerializeToUtf8Bytes(LargeDto.Sample));

        _smallBuffer = new ArrayBufferWriter<byte>(128);
        _largeBuffer = new ArrayBufferWriter<byte>(2048);
    }

    [Benchmark(Baseline = true)]
    public void Serialize_Small_Reflection()
    {
        _smallBuffer.ResetWrittenCount();
        _reflectionSerde.Serialize(_smallBuffer, SmallDto.Sample);
    }

    [Benchmark]
    public void Serialize_Small_SourceGen()
    {
        _smallBuffer.ResetWrittenCount();
        _sourceGenSerde.Serialize(_smallBuffer, SmallDto.Sample);
    }

    [Benchmark]
    public SmallDto Deserialize_Small_Reflection()
    {
        return _reflectionSerde.Deserialize(_smallJson);
    }

    [Benchmark]
    public SmallDto Deserialize_Small_SourceGen()
    {
        return _sourceGenSerde.Deserialize(_smallJson);
    }

    [Benchmark]
    public void Serialize_Large_Reflection()
    {
        _largeBuffer.ResetWrittenCount();
        _largeReflectionSerde.Serialize(_largeBuffer, LargeDto.Sample);
    }

    [Benchmark]
    public void Serialize_Large_SourceGen()
    {
        _largeBuffer.ResetWrittenCount();
        _largeSourceGenSerde.Serialize(_largeBuffer, LargeDto.Sample);
    }

    [Benchmark]
    public LargeDto Deserialize_Large_Reflection()
    {
        return _largeReflectionSerde.Deserialize(_largeJson);
    }

    [Benchmark]
    public LargeDto Deserialize_Large_SourceGen()
    {
        return _largeSourceGenSerde.Deserialize(_largeJson);
    }
}

public record SmallDto(string Name, int Count)
{
    public static readonly SmallDto Sample = new("Alice", 42);
}

public record LargeDto(string Id, string Name, string Email, string[] Tags, Dictionary<string, string> Metadata)
{
    public static readonly LargeDto Sample = new(
        "user-123",
        "Alice Johnson",
        "alice@example.com",
        ["admin", "reviewer", "contributor", "premium"],
        new Dictionary<string, string>
        {
            ["region"] = "us-east-1",
            ["tier"] = "enterprise",
            ["signupDate"] = "2025-01-15",
            ["lastLogin"] = "2025-12-01",
            ["preferences"] = "dark-mode,notifications-on,weekly-digest"
        });
}

[JsonSerializable(typeof(SmallDto))]
[JsonSerializable(typeof(LargeDto))]
internal partial class BenchmarkJsonContext : JsonSerializerContext;