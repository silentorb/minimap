using Xunit;

namespace Minimap.Simulation.Tests;

public class HexGridTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 7)]
    [InlineData(2, 19)]
    [InlineData(3, 37)]
    public void Disk_cell_count_matches_formula(int radius, int expected)
    {
        var g = new HexGrid(radius);
        Assert.Equal(expected, g.CellCount);
    }

    [Fact]
    public void Set_and_get_roundtrip()
    {
        var g = new HexGrid(2);
        var h = new HexAxial(0, 0);
        g.Set(h, CellType.Wall);
        Assert.Equal(CellType.Wall, g.Get(h));
    }

    [Fact]
    public void Get_outside_disk_is_Empty()
    {
        var g = new HexGrid(1);
        Assert.False(g.Contains(new HexAxial(5, 0)));
        Assert.Equal(CellType.Empty, g.Get(new HexAxial(5, 0)));
    }
}
