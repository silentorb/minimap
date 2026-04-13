using Xunit;

namespace Minimap.Simulation.Tests;

public class HexAxialTests
{
    [Fact]
    public void Distance_to_self_is_zero()
    {
        var h = new HexAxial(2, -1);
        Assert.Equal(0, HexAxial.Distance(h, h));
    }

    [Fact]
    public void Neighbors_returns_six_distinct_cells()
    {
        var h = new HexAxial(0, 0);
        var n = h.Neighbors().ToHashSet();
        Assert.Equal(6, n.Count);
        foreach (var x in n)
            Assert.Equal(1, HexAxial.Distance(h, x));
    }

    [Fact]
    public void Distance_is_symmetric()
    {
        var a = new HexAxial(1, 2);
        var b = new HexAxial(-2, 0);
        Assert.Equal(HexAxial.Distance(a, b), HexAxial.Distance(b, a));
    }
}
