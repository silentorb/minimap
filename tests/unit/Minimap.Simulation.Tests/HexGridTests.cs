using Xunit;

namespace Minimap.Simulation.Tests;

public class HexGridTests
{
    [Fact]
    public void Equal_axis_grid_includes_origin_and_horizontal_extremes()
    {
        var g = new HexGrid(1);
        Assert.True(g.Contains(new HexAxial(0, 0)));
        Assert.True(g.Contains(new HexAxial(1, 0)));
        Assert.True(g.Contains(new HexAxial(-1, 0)));
        Assert.Equal(1, g.RadiusX);
        Assert.Equal(1, g.RadiusY);
        Assert.True(g.CellCount >= 3);
    }

    [Fact]
    public void Ellipse_includes_horizontal_and_vertical_screen_extremes()
    {
        const int radiusX = 8;
        const int radiusY = 6;
        var g = new HexGrid(radiusX, radiusY);
        Assert.True(g.Contains(new HexAxial(radiusX, 0)));
        Assert.True(g.Contains(new HexAxial(-radiusX, 0)));
        // Pointy-top: pure screen-vertical extremes sit on X≈0 with R = ±radiusY.
        Assert.True(g.Contains(new HexAxial(radiusY / 2, -radiusY)));
        Assert.True(g.Contains(new HexAxial(-radiusY / 2, radiusY)));
        // Corner of axial box is outside the ellipse.
        Assert.False(g.Contains(new HexAxial(radiusX, radiusY)));
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
    public void Get_outside_grid_is_Empty()
    {
        var g = new HexGrid(1);
        Assert.False(g.Contains(new HexAxial(50, 0)));
        Assert.Equal(CellType.Empty, g.Get(new HexAxial(50, 0)));
    }
}
