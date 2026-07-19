using Minimap.Simulation;
using Xunit;

namespace Minimap.Functional.Tests;

public class GameplaySimulationFunctionalTests
{
    [Fact]
    public void Seeded_world_has_characters_on_floor_within_grid()
    {
        var w = GameWorld.Create(3, 3, 42, spawn: new SpawnConfig { AiPerFaction = 1 });
        Assert.Equal(1 + 1 + 1, w.Characters.Count);
        foreach (var p in w.Characters)
        {
            var hex = HexWorldLayout.WorldToAxial(p.Position, w.HexSize);
            Assert.True(w.Grid.Contains(hex));
            Assert.Equal(CellType.Floor, w.Grid.Get(hex));
        }
    }

    [Fact]
    public void Holding_right_moves_player_continuously_along_plus_x()
    {
        var gen = new FixedLayoutGenerator();
        var w = GameWorld.Create(2, 2, 1, gen, spawn: new SpawnConfig { AiPerFaction = 0 });
        var pawn = w.PlayerController!.Pawn!;
        var before = pawn.Position;

        w.PlayerController.SetMoveInput(new SimVec2(1f, 0f));
        const float dt = 1f / 60f;
        for (var i = 0; i < 30; i++)
            w.Tick(dt);

        var after = pawn.Position;
        Assert.True(after.X > before.X + 1f);
        Assert.Equal(before.Y, after.Y, precision: 2);
    }

    [Fact]
    public void Evolution_loop_maintains_tick_count_and_valid_terrain()
    {
        var w = GameWorld.Create(3, 3, 100, spawn: new SpawnConfig { AiPerFaction = 0 });
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

        foreach (var p in w.Characters)
        {
            var hex = HexWorldLayout.WorldToAxial(p.Position, w.HexSize);
            Assert.True(w.Grid.Contains(hex));
        }
    }

    [Fact]
    public void Zero_health_quietly_removes_character()
    {
        var w = GameWorld.Create(3, 3, 1, new FixedLayoutGenerator(), spawn: new SpawnConfig { AiPerFaction = 0 });
        var victim = w.AddCharacter(2, SimVec2.Zero);
        w.ApplyDamage(victim, CombatTuning.DefaultMaxHealth);
        w.Tick(0.016f);
        Assert.DoesNotContain(victim, w.Characters);
    }
}
