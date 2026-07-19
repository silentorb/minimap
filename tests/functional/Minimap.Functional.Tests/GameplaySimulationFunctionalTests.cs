using Minimap.Simulation;
using Xunit;

namespace Minimap.Functional.Tests;

public class GameplaySimulationFunctionalTests
{
    [Fact]
    public void Seeded_world_has_players_on_floor_within_grid()
    {
        var w = GameWorld.Create(3, 2, 42);
        Assert.Equal(2, w.Players.Length);
        foreach (var p in w.Players)
        {
            var hex = HexWorldLayout.WorldToAxial(p.Position, w.HexSize);
            Assert.True(w.Grid.Contains(hex));
            Assert.Equal(CellType.Floor, w.Grid.Get(hex));
        }
    }

    [Fact]
    public void Holding_right_moves_player_continuously_along_plus_x()
    {
        var gen = new FixedLayoutGenerator(new HexAxial(0, 0));
        var w = GameWorld.Create(2, 1, 1, gen);
        var before = w.Players[0].Position;

        w.SetPlayerInput(0, new SimVec2(1f, 0f));
        const float dt = 1f / 60f;
        for (var i = 0; i < 30; i++)
            w.TickMovement(dt);

        var after = w.Players[0].Position;
        Assert.True(after.X > before.X + 1f);
        Assert.Equal(before.Y, after.Y, precision: 2);
    }

    [Fact]
    public void Evolution_loop_maintains_tick_count_and_valid_terrain()
    {
        var w = GameWorld.Create(3, 2, 100);
        var rng = new Random(999);
        const int n = 30;
        for (var i = 0; i < n; i++)
            WorldEvolution.Tick(w, rng);

        Assert.Equal(n, w.TickIndex);
        foreach (var h in w.Grid.AllHexes())
        {
            var t = w.Grid.Get(h);
            Assert.True(t == CellType.Floor || t == CellType.Wall || t == CellType.Hazard);
        }

        foreach (var p in w.Players)
        {
            var hex = HexWorldLayout.WorldToAxial(p.Position, w.HexSize);
            Assert.True(w.Grid.Contains(hex));
        }
    }
}
