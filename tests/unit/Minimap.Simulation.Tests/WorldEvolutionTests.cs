using Xunit;

namespace Minimap.Simulation.Tests;

public class WorldEvolutionTests
{
    [Fact]
    public void Tick_advances_tick_index()
    {
        var w = GameWorld.Create(2, 2, 99);
        Assert.Equal(0, w.TickIndex);
        var rng = new Random(1);
        WorldEvolution.Tick(w, rng);
        Assert.Equal(1, w.TickIndex);
    }

    [Fact]
    public void Tick_rebuilds_wall_colliders()
    {
        var w = GameWorld.Create(2, 2, 7);
        var before = w.WallPolygons.Count;
        Assert.True(before > 0);

        foreach (var h in w.Grid.AllHexes())
            w.Grid.Set(h, CellType.Floor);
        w.RebuildWallColliders();
        var floorsOnly = w.WallPolygons.Count;

        w.Grid.Set(new HexAxial(0, 0), CellType.Wall);
        w.RebuildWallColliders();
        Assert.True(w.WallPolygons.Count > floorsOnly);
    }
}
