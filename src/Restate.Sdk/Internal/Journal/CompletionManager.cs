using System.Collections.Concurrent;

namespace Restate.Sdk.Internal.Journal;

// ConcurrentDictionary is required here: the handler thread calls GetOrRegister while
// ProcessIncomingMessagesAsync (running on a separate Task) calls TryComplete/TryFail.
// Early completions are stored so notifications arriving before registration are not lost.
internal sealed class CompletionManager
{
    private readonly ConcurrentDictionary<int, object> _slots = new();

    public TaskCompletionSource<CompletionResult> Register(int entryIndex)
    {
        var tcs = new TaskCompletionSource<CompletionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (_slots.TryAdd(entryIndex, tcs))
            return tcs;

        // Slot already occupied — either an early completion or a duplicate registration.
        if (_slots.TryRemove(entryIndex, out var slot))
        {
            if (slot is TaskCompletionSource<CompletionResult>)
                throw new InvalidOperationException($"Entry {entryIndex} already registered");

            // Early completion arrived before registration — resolve the TCS immediately.
            if (slot is CompletionResult result)
                tcs.SetResult(result);
            else if (slot is TerminalException ex)
                tcs.SetException(ex);

            _slots.TryAdd(entryIndex, tcs);
        }

        return tcs;
    }

    public TaskCompletionSource<CompletionResult> GetOrRegister(int entryIndex)
    {
        var slot = _slots.GetOrAdd(entryIndex,
            static _ => new TaskCompletionSource<CompletionResult>(TaskCreationOptions.RunContinuationsAsynchronously));

        if (slot is TaskCompletionSource<CompletionResult> tcs)
            return tcs;

        // An early completion arrived before we registered — create a pre-resolved TCS.
        var earlyTcs = new TaskCompletionSource<CompletionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (slot is CompletionResult result)
            earlyTcs.SetResult(result);
        else if (slot is TerminalException ex)
            earlyTcs.SetException(ex);

        // Replace the stored value with the TCS (not strictly required, but keeps the dictionary clean).
        _slots.TryUpdate(entryIndex, earlyTcs, slot);
        return earlyTcs;
    }

    public bool TryComplete(int entryIndex, CompletionResult result)
    {
        if (_slots.TryRemove(entryIndex, out var slot))
        {
            if (slot is TaskCompletionSource<CompletionResult> tcs)
                return tcs.TrySetResult(result);
        }

        // No handler registered yet — store the result for later delivery.
        _slots.TryAdd(entryIndex, result);
        return true;
    }

    public bool TryFail(int entryIndex, ushort code, string message)
    {
        if (_slots.TryRemove(entryIndex, out var slot))
        {
            if (slot is TaskCompletionSource<CompletionResult> tcs)
                return tcs.TrySetException(new TerminalException(message, code));
        }

        // No handler registered yet — store the failure for later delivery.
        _slots.TryAdd(entryIndex, new TerminalException(message, code));
        return true;
    }

    public void CancelAll()
    {
        foreach (var pair in _slots)
        {
            if (pair.Value is TaskCompletionSource<CompletionResult> tcs)
                tcs.TrySetCanceled();
        }

        _slots.Clear();
    }
}
