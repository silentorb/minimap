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
        var w = GameWorld.Create(4, 4, 42);
        w.SpawnDefaultRoster(spawn, TestContent.Generic, TestContent.ResourceContext);
        Assert.Equal(1 + 3 + 3, w.Actors.Count);
        Assert.Equal(spawn.AiPerFaction * 2, w.Controllers.Count);
        var human = TestWorldHelpers.FindUnpossessedHuman(w, spawn.PlayerFactionId);
        Assert.Equal(1, w.Actors.Count(c => c.FactionId == 1 && c.Id == human.Id));
        Assert.Equal(4, w.Actors.Count(c => c.FactionId == 1));
        Assert.Equal(3, w.Actors.Count(c => c.FactionId == 2));
    }

    [Fact]
    public void Missile_damages_hostile_and_removes_at_zero_health()
    {
        var gen = new AllGrassGenerator();
        var (w, _, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        var enemy = w.AddActor(99, player.Position + new SimVec2(5f, 0f));
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

        Assert.DoesNotContain(enemy, w.Actors);
    }

    [Fact]
    public void Missile_damages_same_faction_when_friendly_fire()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        var ally = w.AddActor(player.FactionId, player.Position + new SimVec2(5f, 0f));
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

        Assert.Contains(ally, w.Actors);
        Assert.Equal(before - CombatTuning.MissileDamage, ally.Health);
    }

    [Fact]
    public void Missile_does_not_damage_same_faction_when_friendly_fire_off()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        var ally = w.AddActor(player.FactionId, player.Position + new SimVec2(5f, 0f));
        var before = ally.Health;

        w.SpawnMissile(
            player.Position,
            new SimVec2(CombatTuning.MissileSpeed, 0f),
            CombatTuning.MissileDamage,
            player.FactionId,
            player.Id,
            friendlyFire: false);

        for (var i = 0; i < 30; i++)
            w.TickMovement(1f / 60f);
        driver.SetMoveInput(SimVec2.Zero);
        for (var i = 0; i < 30; i++)
            w.Tick(1f / 60f);

        Assert.Contains(ally, w.Actors);
        Assert.Equal(before, ally.Health);
    }

    [Fact]
    public void Shoot_with_aim_fires_along_aim_direction()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        // Closer hostile to the left — aim right must not auto-aim at them.
        w.AddActor(2, player.Position + new SimVec2(-20f, 0f));

        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(true);
        w.Tick(0.016f);
        Assert.True(w.Missiles.Count >= 1);
        var m = w.Missiles[0];
        Assert.True(m.Velocity.X > 0f);
        Assert.Equal(CombatTuning.MissileDamage, m.Damage);
        Assert.Equal(CombatTuning.MissileSpeed, m.Velocity.Length, precision: 1);
    }

    [Fact]
    public void Shoot_with_zero_aim_and_no_fire_does_not_fire()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        w.AddActor(2, player.Position + new SimVec2(40f, 0f));

        driver.SetAimInput(SimVec2.Zero);
        driver.SetFireHeld(false);
        w.Tick(0.016f);
        Assert.Empty(w.Missiles);
    }

    [Fact]
    public void Shoot_with_zero_aim_and_fire_uses_facing()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);
        player.Facing = new SimVec2(0f, 1f);
        driver.SetAimInput(SimVec2.Zero);
        driver.SetFireHeld(true);
        w.Tick(0.016f);
        Assert.True(w.Missiles.Count >= 1);
        Assert.True(w.Missiles[0].Velocity.Y > 0f);
    }

    [Fact]
    public void Ai_shoots_toward_nearest_hostile()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(3, 3, 1, gen);
        w.ApplyGameContent(TestContent.Content);
        var shooter = w.AddActor(1, SimVec2.Zero);
        w.AddActor(2, new SimVec2(40f, 0f));
        var ai = new AiController(new Random(1));
        w.AttachController(ai, shooter);
        var effect = Shoot.FindShootEffect(shooter)!;
        effect.CooldownRemaining = 0f;

        w.Tick(0.016f);
        Assert.True(w.Missiles.Count >= 1);
        Assert.True(w.Missiles[0].Velocity.X > 0f);
    }

    [Fact]
    public void Character_defaults_to_documented_max_health()
    {
        var c = new Actor(0, TestContent.Generic, TestContent.ResourceContext, 1, SimVec2.Zero);
        Assert.Equal(CombatTuning.DefaultMaxHealth, c.MaxHealth);
        Assert.Equal(CombatTuning.DefaultMaxHealth, c.Health);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
