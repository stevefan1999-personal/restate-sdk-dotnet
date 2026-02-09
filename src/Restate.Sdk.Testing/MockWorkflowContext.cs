namespace Restate.Sdk.Testing;

/// <summary>
///     A mock <see cref="WorkflowContext" /> for unit testing workflow run handlers.
///     State is stored in-memory. Durable promises are stored in-memory for get/resolve/reject.
///     Side effects execute inline. Calls and sends are recorded.
/// </summary>
public sealed class MockWorkflowContext : WorkflowContext
{
    private readonly MockContext _mock;
    private readonly MockPromiseStore _promiseStore = new();
    private readonly MockStateStore _stateStore = new();

    /// <summary>Creates a new mock workflow context with the given key.</summary>
    public MockWorkflowContext(string key = "test-key", string? invocationId = null)
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

    /// <summary>
    ///     Pre-resolves a promise so that <see cref="Promise{T}" /> returns immediately.
    /// </summary>
    public void SetupPromise<T>(string name, T value)
    {
        _promiseStore.SetupPromise(name, value);
    }

    /// <summary>
    ///     Checks whether a promise has been resolved (via <see cref="ResolvePromise{T}" /> or <see cref="SetupPromise{T}" />
    ///     ).
    /// </summary>
    public bool IsPromiseResolved(string name)
    {
        return _promiseStore.IsPromiseResolved(name);
    }

    /// <summary>
    ///     Checks whether a promise has been rejected.
    /// </summary>
    public bool IsPromiseRejected(string name)
    {
        return _promiseStore.IsPromiseRejected(name);
    }

    /// <summary>
    ///     Gets the resolved value of a promise for verification.
    /// </summary>
    public T? GetPromiseValue<T>(string name)
    {
        return _promiseStore.GetPromiseValue<T>(name);
    }

    /// <summary>
    ///     Gets the rejection reason of a promise for verification.
    /// </summary>
    public string? GetPromiseRejection(string name)
    {
        return _promiseStore.GetPromiseRejection(name);
    }

    // WorkflowContext — unique abstract overrides
    /// <inheritdoc />
    public override ValueTask<T> Promise<T>(string name)
    {
        return _promiseStore.Promise<T>(name);
    }

    /// <inheritdoc />
    public override ValueTask<T?> PeekPromise<T>(string name) where T : default
    {
        return _promiseStore.PeekPromise<T>(name);
    }

    /// <inheritdoc />
    public override void ResolvePromise<T>(string name, T payload)
    {
        _promiseStore.ResolvePromise(name, payload);
    }

    /// <inheritdoc />
    public override void RejectPromise(string name, string reason)
    {
        _promiseStore.RejectPromise(name, reason);
    }

    // ObjectContext — unique abstract overrides
    /// <inheritdoc />
    public override void Set<T>(StateKey<T> key, T value)
    {
        _stateStore.Set(key, value);
    }

    /// <inheritdoc />
    public override void Clear(string key)
    {
        _stateStore.Clear(key);
    }

    /// <inheritdoc />
    public override void ClearAll()
    {
        _stateStore.ClearAll();
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