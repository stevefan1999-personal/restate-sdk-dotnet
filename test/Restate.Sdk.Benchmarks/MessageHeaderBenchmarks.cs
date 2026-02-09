using System.Buffers;
using System.Buffers.Binary;
using BenchmarkDotNet.Attributes;
using Restate.Sdk.Internal.Protocol;

namespace Restate.Sdk.Benchmarks;

/// <summary>
///     Benchmarks for 8-byte protocol frame header encode/decode.
///     This is called per message — every request and response goes through header parsing.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class MessageHeaderBenchmarks
{
    private byte[] _headerBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _headerBytes = new byte[MessageHeader.Size];
        BinaryPrimitives.WriteUInt16BigEndian(_headerBytes, (ushort)MessageType.CallCommand);
        BinaryPrimitives.WriteUInt16BigEndian(_headerBytes.AsSpan(2), (ushort)MessageFlags.RequiresAck);
        BinaryPrimitives.WriteUInt32BigEndian(_headerBytes.AsSpan(4), 1024);
    }

    [Benchmark]
    public uint Read()
    {
        return MessageHeader.Read(_headerBytes).Length;
    }

    [Benchmark]
    public bool TryRead()
    {
        return MessageHeader.TryRead(_headerBytes, out _);
    }

    [Benchmark]
    public void Write()
    {
        Span<byte> buf = stackalloc byte[MessageHeader.Size];
        var header = MessageHeader.Create(MessageType.CallCommand, MessageFlags.RequiresAck, 1024);
        header.Write(buf);
    }

    [Benchmark]
    public void WriteTo_BufferWriter()
    {
        var writer = new ArrayBufferWriter<byte>(MessageHeader.Size);
        var header = MessageHeader.Create(MessageType.SetStateCommand, 512);
        header.WriteTo(writer);
    }

    [Benchmark]
    public uint RoundTrip()
    {
        Span<byte> buf = stackalloc byte[MessageHeader.Size];
        var header = MessageHeader.Create(MessageType.RunCommand, MessageFlags.None, 256);
        header.Write(buf);
        return MessageHeader.Read(buf).Length;
    }
}