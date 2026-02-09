namespace Restate.Sdk.Testing;

/// <summary>
///     A mock <see cref="SharedObjectContext" /> for unit testing shared virtual object handlers.
///     Provides read-only in-memory state. Side effects execute inline. Calls and sends are recorded.
/// </summary>
public sealed class MockSharedObjectContext : SharedObjectContext
{
    private readonly MockContext _mock;
    private readonly MockStateStore _stateStore = new();

    /// <summary>Creates a new mock shared object context with the given key.</summary>
    public MockSharedObjectContext(string key = "test-key", string? invocationId = null)
    {
        Key = key;
        _mock = new MockContext(invocationId);
        BaseContext = _mock;
    }

    /// <inheritdoc />
    public override string Key { get; }

    /// <summary>All recorded Call invocations.</summary>
    public IReadOnlyList<RecordedCall> Calls => _mock.Calls;

    /// <summary>All recorded Send invocations.</summary>
    public IReadOnlyList<RecordedSend> Sends => _mock.Sends;

    /// <summary>All recorded Sleep invocations with their requested durations.</summary>
    public IReadOnlyList<RecordedSleep> Sleeps => _mock.Sleeps;

    /// <summary>Configures the return value for a Call to the given service/handler.</summary>
    public void SetupCall<T>(string service, string handler, T result)
    {
        _mock.SetupCall(service, handler, result);
    }

    /// <summary>Configures the return value for a Call to the given service/key/handler.</summary>
    public void SetupCall<T>(string service, string key, string handler, T result)
    {
        _mock.SetupCall(service, key, handler, result);
    }

    /// <summary>
    ///     Enqueues a value to be returned by the next <see cref="Context.Awakeable{T}" /> call.
    ///     Values are consumed in FIFO order.
    /// </summary>
    public void SetupAwakeable<T>(T result)
    {
        _mock.SetupAwakeable(result);
    }

    /// <summary>Pre-populates a state key for testing.</summary>
    public void SetupState<T>(StateKey<T> key, T value)
    {
        _stateStore.SetupState(key, value);
    }

    /// <summary>Pre-populates a state key by name.</summary>
    public void SetupState<T>(string key, T value)
    {
        _stateStore.SetupState(key, value);
    }

    /// <summary>Gets the current value of a state key for verification.</summary>
    public T? GetStateValue<T>(string key)
    {
        return _stateStore.GetStateValue<T>(key);
    }

    /// <summary>Verifies whether a state key exists.</summary>
    public bool HasState(string key)
    {
        return _stateStore.HasState(key);
    }

    // SharedObjectContext — unique abstract overrides
    /// <inheritdoc />
    public override ValueTask<T?> Get<T>(StateKey<T> key) where T : default
    {
        return _stateStore.Get<T>(key);
    }

    /// <inheritdoc />
    public override ValueTask<string[]> StateKeys()
    {
        return _stateStore.StateKeys();
    }

    // Client methods — override to throw (mocks don't support generated clients)
    /// <inheritdoc />
    public override TClient ServiceClient<TClient>()
    {
        throw new NotSupportedException(MockHelpers.TypedClientNotSupported);
    }

    /// <inheritdoc />
    public override TClient ObjectClient<TClient>(string key)
    {
        throw new NotSupportedException(MockHelpers.TypedClientNotSupported);
    }

    /// <inheritdoc />
    public override TClient WorkflowClient<TClient>(string key)
    {
        throw new NotSupportedException(MockHelpers.TypedClientNotSupported);
    }

    /// <inheritdoc />
    public override TClient ServiceSendClient<TClient>(SendOptions? options = null)
    {
        throw new NotSupportedException(MockHelpers.TypedSendClientNotSupported);
    }

    /// <inheritdoc />
    public override TClient ObjectSendClient<TClient>(string key, SendOptions? options = null)
    {
        throw new NotSupportedException(MockHelpers.TypedSendClientNotSupported);
    }

    /// <inheritdoc />
    public override TClient WorkflowSendClient<TClient>(string key, SendOptions? options = null)
    {
        throw new NotSupportedException(MockHelpers.TypedSendClientNotSupported);
    }
}