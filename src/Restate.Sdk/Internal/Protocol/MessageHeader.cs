using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Restate.Sdk.Internal.Protocol;

/// <summary>
///     8-byte protocol frame header.
///     Layout (big-endian): Type(16) | Flags(16) | Length(32)
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = Size)]
[SkipLocalsInit]
internal readonly struct MessageHeader
{
    public const int Size = 8;

    public MessageType Type { get; }
    public MessageFlags Flags { get; }
    public uint Length { get; }

    private MessageHeader(MessageType type, MessageFlags flags, uint length)
    {
        Type = type;
        Flags = flags;
        Length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MessageHeader Create(MessageType type, MessageFlags flags, uint length)
    {
        return new MessageHeader(type, flags, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MessageHeader Create(MessageType type, uint length)
    {
        return new MessageHeader(type, MessageFlags.None, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MessageHeader Read(ReadOnlySpan<byte> source)
    {
        var type = (MessageType)BinaryPrimitives.ReadUInt16BigEndian(source);
        var flags = (MessageFlags)BinaryPrimitives.ReadUInt16BigEndian(source[2..]);
        var length = BinaryPrimitives.ReadUInt32BigEndian(source[4..]);
        return new MessageHeader(type, flags, length);
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out MessageHeader header)
    {
        if (source.Length < Size)
        {
            header = default;
            return false;
        }

        header = Read(source);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> destination)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(destination[2..], (ushort)Flags);
        BinaryPrimitives.WriteUInt32BigEndian(destination[4..], Length);
    }

    public bool TryWrite(Span<byte> destination)
    {
        if (destination.Length < Size)
            return false;
        Write(destination);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTo(IBufferWriter<byte> writer)
    {
        var span = writer.GetSpan(Size);
        Write(span);
        writer.Advance(Size);
    }

    public MessageHeader WithLength(uint length)
    {
        return new MessageHeader(Type, Flags, length);
    }

    public MessageHeader WithFlags(MessageFlags flags)
    {
        return new MessageHeader(Type, flags, Length);
    }

    public override string ToString()
    {
        return $"[{Type} Flags={Flags} Length={Length}]";
    }
}