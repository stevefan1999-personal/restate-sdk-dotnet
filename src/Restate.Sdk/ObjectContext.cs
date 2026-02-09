namespace Restate.Sdk;

/// <summary>
///     Context for exclusive handlers on virtual objects. Provides read-write state access.
///     Only one exclusive handler runs at a time per key.
/// </summary>
public abstract class ObjectContext : SharedObjectContext
{
    /// <summary>Sets the value for the given state key.</summary>
    public abstract void Set<T>(StateKey<T> key, T value);

    /// <summary>Clears a single state key.</summary>
    public abstract void Clear(string key);

    /// <summary>Clears all state keys for this virtual object instance.</summary>
    public abstract void ClearAll();
}