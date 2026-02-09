namespace Restate.Sdk.Testing;

internal static class MockHelpers
{
    internal const string TypedClientNotSupported =
        "Typed clients are not supported in mock contexts. Use Call/Send directly.";

    internal const string TypedSendClientNotSupported =
        "Typed clients are not supported in mock contexts. Use Send directly.";
}