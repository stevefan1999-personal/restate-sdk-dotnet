namespace Restate.Sdk.Internal.StateMachine;

internal enum InvocationState
{
    WaitingStart,
    Replaying,
    Processing,
    Closed
}