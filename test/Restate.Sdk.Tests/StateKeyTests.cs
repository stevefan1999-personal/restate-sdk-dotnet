namespace Restate.Sdk.Tests;

public class StateKeyTests
{
    [Fact]
    public void StateKey_StoresName()
    {
        var key = new StateKey<int>("count");
        Assert.Equal("count", key.Name);
    }

    [Fact]
    public void StateKey_ValueEquality()
    {
        var a = new StateKey<string>("status");
        var b = new StateKey<string>("status");
        Assert.Equal(a, b);
    }

    [Fact]
    public void StateKey_DifferentNames_NotEqual()
    {
        var a = new StateKey<int>("x");
        var b = new StateKey<int>("y");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void StateKey_HashCode_ConsistentForEqualKeys()
    {
        var a = new StateKey<int>("count");
        var b = new StateKey<int>("count");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void StateKey_HashCode_DifferentForDifferentKeys()
    {
        var a = new StateKey<int>("x");
        var b = new StateKey<int>("y");
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void StateKey_ToString_ContainsName()
    {
        var key = new StateKey<double>("temperature");
        Assert.Contains("temperature", key.ToString());
    }

    [Fact]
    public void StateKey_CanBeUsedAsDictionaryKey()
    {
        var dict = new Dictionary<StateKey<int>, string>
        {
            [new StateKey<int>("a")] = "first",
            [new StateKey<int>("b")] = "second"
        };

        Assert.Equal("first", dict[new StateKey<int>("a")]);
        Assert.Equal("second", dict[new StateKey<int>("b")]);
    }

    [Fact]
    public void StateKey_DifferentGenericTypes_SameNameAreDistinct()
    {
        var intKey = new StateKey<int>("x");
        var strKey = new StateKey<string>("x");

        Assert.NotEqual(intKey.GetType(), strKey.GetType());
    }
}