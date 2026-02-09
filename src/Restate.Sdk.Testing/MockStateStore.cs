namespace Restate.Sdk.Testing;

/// <summary>
///     In-memory state store shared by mock contexts that support virtual object state.
/// </summary>
internal sealed class MockStateStore
{
    private readonly Dictionary<string, object?> _state = [];

    public void SetupState<T>(StateKey<T> key, T value)
    {
        _state[key.Name] = value;
    }

    public void SetupState<T>(string key, T value)
    {
        _state[key] = value;
    }

    public T? GetStateValue<T>(string key)
    {
        return _state.TryGetValue(key, out var val) ? (T?)val : default;
    }

    public bool HasState(string key)
    {
        return _state.ContainsKey(key);
    }

    public ValueTask<T?> Get<T>(StateKey<T> key)
    {
        return _state.TryGetValue(key.Name, out var val) ? new ValueTask<T?>((T?)val) : new ValueTask<T?>(default(T));
    }

    public ValueTask<string[]> StateKeys()
    {
        return new ValueTask<string[]>(_state.Keys.ToArray());
    }

    public void Set<T>(StateKey<T> key, T value)
    {
        _state[key.Name] = value;
    }

    public void Clear(string key)
    {
        _state.Remove(key);
    }

    public void ClearAll()
    {
        _state.Clear();
    }
}