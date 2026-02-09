using System.Buffers;
using Restate.Sdk.Internal.Serde;

namespace Restate.Sdk.Tests.Serde;

public class JsonSerdeTests
{
    private static ReadOnlyMemory<byte> SerializeToMemory<T>(ISerde<T> serde, T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        serde.Serialize(buffer, value);
        return buffer.WrittenMemory;
    }

    [Fact]
    public void SerializeDeserialize_Int()
    {
        var serde = JsonSerde<int>.Default;
        var bytes = SerializeToMemory(serde, 42);
        var result = serde.Deserialize(new ReadOnlySequence<byte>(bytes));
        Assert.Equal(42, result);
    }

    [Fact]
    public void SerializeDeserialize_String()
    {
        var serde = JsonSerde<string>.Default;
        var bytes = SerializeToMemory(serde, "hello");
        var result = serde.Deserialize(new ReadOnlySequence<byte>(bytes));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void SerializeDeserialize_Object()
    {
        var serde = JsonSerde<TestPayload>.Default;
        var original = new TestPayload { Name = "test", Value = 99 };
        var bytes = SerializeToMemory(serde, original);
        var result = serde.Deserialize(new ReadOnlySequence<byte>(bytes));

        Assert.Equal("test", result.Name);
        Assert.Equal(99, result.Value);
    }

    [Fact]
    public void Deserialize_EmptyData_ReturnsDefault()
    {
        var serde = JsonSerde<int>.Default;
        var result = serde.Deserialize(ReadOnlySequence<byte>.Empty);
        Assert.Equal(0, result);
    }

    [Fact]
    public void ContentType_IsJson()
    {
        Assert.Equal("application/json", JsonSerde<int>.Default.ContentType);
    }

    private class TestPayload
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}