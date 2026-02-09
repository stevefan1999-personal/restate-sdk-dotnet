using Restate.Sdk.Internal.Protocol;

namespace Restate.Sdk.Tests.Protocol;

public class MessageHeaderTests
{
    [Fact]
    public void Create_StoresComponents()
    {
        var header = MessageHeader.Create(MessageType.Start, MessageFlags.Completed, 42);
        Assert.Equal(MessageType.Start, header.Type);
        Assert.Equal(MessageFlags.Completed, header.Flags);
        Assert.Equal(42u, header.Length);
    }

    [Fact]
    public void Roundtrip_ThroughSpan()
    {
        var original = MessageHeader.Create(MessageType.CallCommand, MessageFlags.RequiresAck, 1024);
        Span<byte> buf = stackalloc byte[MessageHeader.Size];
        original.Write(buf);

        var restored = MessageHeader.Read(buf);
        Assert.Equal(original.Type, restored.Type);
        Assert.Equal(original.Flags, restored.Flags);
        Assert.Equal(original.Length, restored.Length);
    }

    [Fact]
    public void BigEndian_Layout()
    {
        var header = MessageHeader.Create(MessageType.Start, MessageFlags.None, 256);
        Span<byte> buf = stackalloc byte[MessageHeader.Size];
        header.Write(buf);

        // Type=0x0000 big-endian
        Assert.Equal(0x00, buf[0]);
        Assert.Equal(0x00, buf[1]);
        // Flags=0x0000
        Assert.Equal(0x00, buf[2]);
        Assert.Equal(0x00, buf[3]);
        // Length=256=0x00000100 big-endian
        Assert.Equal(0x00, buf[4]);
        Assert.Equal(0x00, buf[5]);
        Assert.Equal(0x01, buf[6]);
        Assert.Equal(0x00, buf[7]);
    }

    [Fact]
    public void TryRead_TooSmall_ReturnsFalse()
    {
        Span<byte> buf = stackalloc byte[4];
        Assert.False(MessageHeader.TryRead(buf, out _));
    }

    [Fact]
    public void WithLength_CreatesNewHeader()
    {
        var header = MessageHeader.Create(MessageType.End, 0);
        var updated = header.WithLength(99);
        Assert.Equal(99u, updated.Length);
        Assert.Equal(MessageType.End, updated.Type);
    }

    [Fact]
    public void MessageType_Categories()
    {
        Assert.True(MessageType.Start.IsControlMessage());
        Assert.False(MessageType.Start.IsCommand());
        Assert.False(MessageType.Start.IsNotification());

        Assert.True(MessageType.CallCommand.IsCommand());
        Assert.False(MessageType.CallCommand.IsNotification());
        Assert.False(MessageType.CallCommand.IsControlMessage());

        Assert.True(MessageType.CallCompletion.IsNotification());
        Assert.False(MessageType.CallCompletion.IsCommand());
    }
}