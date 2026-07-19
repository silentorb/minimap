using Xunit;

namespace Minimap.Simulation.Tests;

public class FactionAndCombatTests
{
    [Fact]
    public void AreHostile_when_faction_ids_differ()
    {
        Assert.True(FactionRules.AreHostile(1, 2));
        Assert.False(FactionRules.AreHostile(1, 1));
        Assert.True(FactionRules.AreHostile(7, 3));
    }

    [Fact]
    public void Default_spawn_places_humans_unpossessed_and_ai_on_both_factions()
    {
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2, AiPerFaction = 3, HumanPlayerCount = 1 };
        var w = GameWorld.Create(4, 4, 42, spawn: spawn);
        Assert.Equal(1 + 3 + 3, w.Characters.Count);
        Assert.Equal(spawn.AiPerFaction * 2, w.Controllers.Count);
        var human = TestWorldHelpers.FindUnpossessedHuman(w, spawn.PlayerFactionId);
        Assert.Equal(1, w.Characters.Count(c => c.FactionId == 1 && c.Id == human.Id));
        Assert.Equal(4, w.Characters.Count(c => c.FactionId == 1));
        Assert.Equal(3, w.Characters.Count(c => c.FactionId == 2));
    }

    [Fact]
    public void Missile_damages_hostile_and_removes_at_zero_health()
    {
        var gen = new AllFloorGenerator();
        var (w, _, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        var enemy = w.AddCharacter(99, player.Position + new SimVec2(5f, 0f));
        enemy.Health = CombatTuning.MissileDamage; // one hit kills

        w.SpawnMissile(
            player.Position,
            new SimVec2(CombatTuning.MissileSpeed, 0f),
            CombatTuning.MissileDamage,
            player.FactionId,
            player.Id);

        // Step until missile overlaps enemy
        for (var i = 0; i < 30; i++)
            w.Tick(1f / 60f);

        Assert.DoesNotContain(enemy, w.Characters);
    }

    [Fact]
    public void Missile_does_not_damage_same_faction()
    {
        var gen = new AllFloorGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        var ally = w.AddCharacter(player.FactionId, player.Position + new SimVec2(5f, 0f));
        var before = ally.Health;

        w.SpawnMissile(
            player.Position,
            new SimVec2(CombatTuning.MissileSpeed, 0f),
            CombatTuning.MissileDamage,
            player.FactionId,
            player.Id);

        for (var i = 0; i < 30; i++)
            w.TickMovement(1f / 60f);
        driver.SetMoveInput(SimVec2.Zero);
        for (var i = 0; i < 30; i++)
            w.Tick(1f / 60f);

        Assert.Contains(ally, w.Characters);
        Assert.Equal(before, ally.Health);
    }

    [Fact]
    public void Autoshoot_fires_toward_hostile()
    {
        var gen = new AllFloorGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        w.AddCharacter(2, player.Position + new SimVec2(40f, 0f));

        driver.SetMoveInput(SimVec2.Zero);
        // Fire interval is 1.25s; cooldown starts at 0 so first tick should fire
        w.Tick(0.016f);
        Assert.True(w.Missiles.Count >= 1);
        var m = w.Missiles[0];
        Assert.True(m.Velocity.X > 0f);
        Assert.Equal(CombatTuning.MissileDamage, m.Damage);
        Assert.Equal(CombatTuning.MissileSpeed, m.Velocity.Length, precision: 1);
    }

    [Fact]
    public void Character_defaults_to_documented_max_health()
    {
        var c = new Character(0, 1, SimVec2.Zero);
        Assert.Equal(CombatTuning.DefaultMaxHealth, c.MaxHealth);
        Assert.Equal(CombatTuning.DefaultMaxHealth, c.Health);
    }

    private sealed class AllFloorGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Floor);
        }
    }
}
