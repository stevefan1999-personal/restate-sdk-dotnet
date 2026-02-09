namespace Restate.Sdk.Tests;

public class DurableRandomTests
{
    [Fact]
    public void SameSeed_SameSequence()
    {
        var a = new DurableRandom(42);
        var b = new DurableRandom(42);

        for (var i = 0; i < 100; i++)
            Assert.Equal(a.Next(), b.Next());
    }

    [Fact]
    public void NextGuid_IsVersion4()
    {
        var rng = new DurableRandom(123);
        for (var i = 0; i < 100; i++)
        {
            var guid = rng.NextGuid();
            var str = guid.ToString();
            Assert.Equal('4', str[14]);
            Assert.Contains(str[19], "89ab");
        }
    }

    [Fact]
    public void Next_InRange()
    {
        var rng = new DurableRandom(99);
        for (var i = 0; i < 1000; i++)
        {
            Assert.InRange(rng.Next(10), 0, 9);
            Assert.InRange(rng.Next(10, 20), 10, 19);
            Assert.True(rng.NextDouble() is >= 0.0 and < 1.0);
        }

        // Edge cases
        Assert.Equal(0, new DurableRandom(42).Next(1));
        Assert.True(new DurableRandom(0).Next() >= 0);
        Assert.True(new DurableRandom(ulong.MaxValue).Next() >= 0);
    }

    [Fact]
    public void DifferentSeeds_DifferentSequences()
    {
        var a = new DurableRandom(1);
        var b = new DurableRandom(2);

        var anyDifferent = false;
        for (var i = 0; i < 10; i++)
            if (a.Next() != b.Next())
            {
                anyDifferent = true;
                break;
            }

        Assert.True(anyDifferent);
    }
}
