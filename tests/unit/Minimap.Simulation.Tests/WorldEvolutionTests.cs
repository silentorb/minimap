using Xunit;

namespace Minimap.Simulation.Tests;

public class WorldEvolutionTests
{
    [Fact]
    public void Tick_advances_tick_index()
    {
        var w = GameWorld.Create(2, 1, 99);
        Assert.Equal(0, w.TickIndex);
        var rng = new Random(1);
        WorldEvolution.Tick(w, rng);
        Assert.Equal(1, w.TickIndex);
    }

    [Fact]
    public void TryMovePlayer_blocked_by_wall()
    {
        var w = GameWorld.Create(2, 1, 7);
        var p = w.Players[0];
        var pos = p.Position;
        foreach (var n in pos.Neighbors())
        {
            if (w.Grid.Contains(n))
                w.Grid.Set(n, CellType.Wall);
        }

        Assert.False(w.TryMovePlayer(0, 0));
        Assert.Equal(pos, p.Position);
    }
}
