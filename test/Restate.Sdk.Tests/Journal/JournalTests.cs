using Restate.Sdk.Internal.Journal;

namespace Restate.Sdk.Tests.Journal;

public class InvocationJournalTests
{
    [Fact]
    public void Append_IncreasesCount()
    {
        using var journal = new InvocationJournal();
        Assert.Equal(0, journal.Count);

        journal.Append(JournalEntry.Completed(JournalEntryType.Input, new byte[] { 1, 2, 3 }));
        Assert.Equal(1, journal.Count);

        journal.Append(JournalEntry.Pending(JournalEntryType.Call));
        Assert.Equal(2, journal.Count);
    }

    [Fact]
    public void Append_ReturnsEntryIndex()
    {
        using var journal = new InvocationJournal();
        Assert.Equal(0, journal.Append(JournalEntry.Completed(JournalEntryType.Input, Array.Empty<byte>())));
        Assert.Equal(1, journal.Append(JournalEntry.Pending(JournalEntryType.Call)));
        Assert.Equal(2, journal.Append(JournalEntry.Pending(JournalEntryType.Sleep)));
    }

    [Fact]
    public void Indexer_ReturnsCorrectEntry()
    {
        using var journal = new InvocationJournal();
        var data = new byte[] { 10, 20, 30 };
        journal.Append(JournalEntry.Completed(JournalEntryType.Run, data, "side-effect"));

        var entry = journal[0];
        Assert.Equal(JournalEntryType.Run, entry.Type);
        Assert.Equal("side-effect", entry.Name);
        Assert.True(entry.IsCompleted);
        Assert.Equal(data, entry.Result.ToArray());
    }

    [Fact]
    public void Indexer_ThrowsOnOutOfRange()
    {
        using var journal = new InvocationJournal();
        Assert.Throws<ArgumentOutOfRangeException>(() => journal[0]);
    }

    [Fact]
    public void Initialize_SetsKnownEntries()
    {
        using var journal = new InvocationJournal();
        journal.Initialize(5);

        Assert.Equal(5, journal.KnownEntries);
        Assert.True(journal.IsReplaying);
    }

    [Fact]
    public void IsReplaying_FalseWhenCountReachesKnownEntries()
    {
        using var journal = new InvocationJournal();
        journal.Initialize(2);

        Assert.True(journal.IsReplaying);

        journal.Append(JournalEntry.Completed(JournalEntryType.Input, Array.Empty<byte>()));
        Assert.True(journal.IsReplaying);

        journal.Append(JournalEntry.Completed(JournalEntryType.Run, Array.Empty<byte>()));
        Assert.False(journal.IsReplaying);
    }

    [Fact]
    public void IsReplaying_FalseWhenKnownEntriesIsZero()
    {
        using var journal = new InvocationJournal();
        journal.Initialize(0);
        Assert.False(journal.IsReplaying);
    }

    [Fact]
    public void Grows_BeyondInitialCapacity()
    {
        using var journal = new InvocationJournal();

        for (var i = 0; i < 64; i++)
            journal.Append(JournalEntry.Completed(JournalEntryType.Run, new[] { (byte)i }));

        Assert.Equal(64, journal.Count);
        Assert.Equal(63, journal[63].Result.Span[0]);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var journal = new InvocationJournal();
        journal.Append(JournalEntry.Completed(JournalEntryType.Input, Array.Empty<byte>()));
        journal.Dispose();
        journal.Dispose();
    }
}