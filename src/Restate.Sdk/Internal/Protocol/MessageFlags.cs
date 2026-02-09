namespace Restate.Sdk.Internal.Protocol;

[Flags]
internal enum MessageFlags : ushort
{
    None = 0x0000,
    Completed = 0x0001,
    RequiresAck = 0x8000
}

internal static class MessageFlagsExtensions
{
    public static bool IsCompleted(this MessageFlags flags)
    {
        return (flags & MessageFlags.Completed) != 0;
    }

    public static bool HasRequiresAck(this MessageFlags flags)
    {
        return (flags & MessageFlags.RequiresAck) != 0;
    }
}