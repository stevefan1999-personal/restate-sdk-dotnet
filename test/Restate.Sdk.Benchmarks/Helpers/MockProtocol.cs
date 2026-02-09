using System.Buffers;
using System.IO.Pipelines;
using Restate.Sdk.Internal.Protocol;

namespace Restate.Sdk.Benchmarks.Helpers;

/// <summary>
///     In-memory protocol infrastructure for benchmarking state machine operations
///     without network I/O. Uses System.IO.Pipelines Pipe for zero-copy message passing.
/// </summary>
internal sealed class MockProtocol : IDisposable
{
    private readonly Pipe _inbound = new();
    private readonly Pipe _outbound = new();

    /// <summary>Reader that the state machine reads inbound messages from.</summary>
    public ProtocolReader Reader { get; }

    /// <summary>Writer that the state machine writes outbound messages to.</summary>
    public ProtocolWriter Writer { get; }

    /// <summary>Write side of the inbound pipe — feed pre-built messages here.</summary>
    public PipeWriter InboundWriter => _inbound.Writer;

    /// <summary>Read side of the outbound pipe — drain messages the state machine wrote.</summary>
    public PipeReader OutboundReader => _outbound.Reader;

    public MockProtocol()
    {
        Reader = new ProtocolReader(_inbound.Reader);
        Writer = new ProtocolWriter(_outbound.Writer);
    }

    /// <summary>
    ///     Writes a complete protocol message (header + payload) to the inbound pipe.
    /// </summary>
    public void WriteInboundMessage(MessageType type, MessageFlags flags, ReadOnlySpan<byte> payload)
    {
        var span = InboundWriter.GetSpan(MessageHeader.Size + payload.Length);
        var header = MessageHeader.Create(type, flags, (uint)payload.Length);
        header.Write(span);
        payload.CopyTo(span[MessageHeader.Size..]);
        InboundWriter.Advance(MessageHeader.Size + payload.Length);
    }

    /// <summary>
    ///     Writes a complete protocol message (header + payload) to the inbound pipe.
    /// </summary>
    public void WriteInboundMessage(MessageType type, ReadOnlySpan<byte> payload)
    {
        WriteInboundMessage(type, MessageFlags.None, payload);
    }

    /// <summary>
    ///     Flushes the inbound writer so messages become available to the reader.
    /// </summary>
    public ValueTask<FlushResult> FlushInbound() => InboundWriter.FlushAsync();

    /// <summary>
    ///     Completes the inbound writer, signaling end of stream.
    /// </summary>
    public void CompleteInbound() => InboundWriter.Complete();

    /// <summary>
    ///     Drains and discards all outbound messages (from the state machine).
    /// </summary>
    public async Task DrainOutbound()
    {
        _outbound.Writer.Complete();
        while (true)
        {
            var result = await OutboundReader.ReadAsync();
            OutboundReader.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted) break;
        }
    }

    public void Dispose()
    {
        _inbound.Writer.Complete();
        _inbound.Reader.Complete();
        _outbound.Writer.Complete();
        _outbound.Reader.Complete();
    }
}
