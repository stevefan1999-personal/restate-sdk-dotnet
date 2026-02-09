using System.Text;
using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Restate.Sdk.Internal.Protocol;
using Gen = Restate.Sdk.Internal.Protocol.Generated;

namespace Restate.Sdk.Benchmarks;

/// <summary>
///     Benchmarks for protobuf message building and parsing using Google.Protobuf generated classes.
///     Covers realistic command message construction and StartMessage/completion parsing.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ProtobufMessageBenchmarks
{
    private byte[] _completionPayload = null!;
    private byte[] _inputCommandPayload = null!;
    private byte[] _smallPayload = null!;
    private byte[] _startMessagePayload = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallPayload = """{"name":"Alice"}"""u8.ToArray();

        // Build a realistic StartMessage payload using generated classes
        var startMsg = new Gen.StartMessage
        {
            Id = ByteString.CopyFromUtf8("inv_1234abcd"),
            DebugId = "inv_1234abcd",
            KnownEntries = 5,
            Key = "my-key",
            RandomSeed = 12345678UL
        };
        _startMessagePayload = startMsg.ToByteArray();

        // Build a completion notification with a value
        var compMsg = new Gen.NotificationTemplate
        {
            CompletionId = 3,
            Value = new Gen.Value { Content = ByteString.CopyFrom(_smallPayload) }
        };
        _completionPayload = compMsg.ToByteArray();

        // Build an InputCommand payload
        var inputMsg = new Gen.InputCommandMessage
        {
            Value = new Gen.Value { Content = ByteString.CopyFrom(_smallPayload) }
        };
        _inputCommandPayload = inputMsg.ToByteArray();
    }

    [Benchmark]
    public string Parse_StartMessage()
    {
        return ProtobufCodec.ParseStartMessage(_startMessagePayload).InvocationId;
    }

    [Benchmark]
    public uint Parse_CompletionNotification()
    {
        return ProtobufCodec.ParseCompletionNotification(_completionPayload).CompletionId;
    }

    [Benchmark]
    public (ReadOnlyMemory<byte> Input, Dictionary<string, string>? Headers) Parse_InputCommand()
    {
        return ProtobufCodec.ParseInputCommand(_inputCommandPayload);
    }

    [Benchmark]
    public int Build_CallCommand()
    {
        var msg = ProtobufCodec.CreateCallCommand(
            "GreeterService", "Greet", "my-key", _smallPayload, 1, 0);
        return msg.CalculateSize();
    }

    [Benchmark]
    public int Build_SetStateCommand()
    {
        var msg = ProtobufCodec.CreateSetStateCommand("count", _smallPayload);
        return msg.CalculateSize();
    }

    [Benchmark]
    public int Build_RunCommand()
    {
        var msg = ProtobufCodec.CreateRunCommand("generate-greeting", 1);
        return msg.CalculateSize();
    }

    [Benchmark]
    public int Build_OutputCommand()
    {
        var msg = ProtobufCodec.CreateOutputCommand(_smallPayload);
        return msg.CalculateSize();
    }

    [Benchmark]
    public int Build_SleepCommand()
    {
        var msg = ProtobufCodec.CreateSleepCommand(1234567890UL, 5);
        return msg.CalculateSize();
    }

    [Benchmark]
    public int Build_ErrorMessage()
    {
        var msg = ProtobufCodec.CreateErrorMessage(400, "Ticket is already reserved");
        return msg.CalculateSize();
    }
}
