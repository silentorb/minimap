using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class SwingAndActorHealthTests
{
    [Fact]
    public void ApplyDamage_noops_on_indestructible_actor()
    {
        var w = CreateGrassWorld();
        var def = new ActorDefinition("crate");
        Assert.True(w.TryPlaceActor(new HexAxial(0, 0), def));
        Assert.True(w.TryGetActorAt(new HexAxial(0, 0), out var actor));
        Assert.False(actor!.IsDestructible);

        w.ApplyDamage(actor, 50);
        Assert.True(w.IsCellOccupied(new HexAxial(0, 0)));
        Assert.Equal(0, actor.Health);
    }

    [Fact]
    public void Cell_actor_dies_and_is_removed_at_zero_health()
    {
        var w = CreateGrassWorld();
        var healthTag = TestContent.HealthResource.Tag;
        var maxTag = TestContent.MaxHealthResource.Tag;
        var def = new ActorDefinition(
            "computer",
            resources:
            [
                new ActorResourceAmount(maxTag, 40),
                new ActorResourceAmount(healthTag, 40),
            ]);
        Assert.True(w.TryPlaceActor(new HexAxial(0, 0), def));
        Assert.True(w.TryGetActorAt(new HexAxial(0, 0), out var actor));
        Assert.Equal(40, actor!.MaxHealth);
        Assert.True(actor.IsDestructible);

        w.ApplyDamage(actor, 40);
        w.Tick(0.016f);
        Assert.False(w.IsCellOccupied(new HexAxial(0, 0)));
    }

    [Fact]
    public void Missile_damages_destructible_cell_actor()
    {
        var w = CreateGrassWorld();
        var cell = new HexAxial(1, 0);
        var center = HexWorldLayout.ToWorld(cell, w.HexSize);
        var def = new ActorDefinition("carrot_growing",
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 50),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 50),
            ]);
        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.TryGetActorAt(cell, out var actor));
        var before = actor!.Health;

        w.SpawnMissile(
            center,
            SimVec2.Zero,
            CombatTuning.MissileDamage,
            ownerFactionId: 1,
            ownerCharacterId: null);

        w.Tick(0.016f);
        Assert.True(w.TryGetActorAt(cell, out actor));
        Assert.Equal(before - CombatTuning.MissileDamage, actor!.Health);
        Assert.Empty(w.Missiles);
    }

    [Fact]
    public void Swing_damages_character_in_arc_and_misses_behind()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(
            3, 3, 1, gen, definition: new ActorDefinition(
                "swinger",
                [TestContent.Swing],
                resources:
                [
                    new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 100),
                    new ActorResourceAmount(TestContent.HealthResource.Tag, 100),
                    new ActorResourceAmount(TestContent.MaxEnergyResource.Tag, 100),
                    new ActorResourceAmount(TestContent.EnergyResource.Tag, 100),
                ]));
        var front = w.AddActor(99, player.Position + new SimVec2(12f, 0f));
        var behind = w.AddActor(99, player.Position + new SimVec2(-12f, 0f));
        var frontBefore = front.Health;
        var behindBefore = behind.Health;

        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetSecondaryFireHeld(true);
        w.Tick(0.016f);

        Assert.Single(w.SwingArcs);
        Assert.Equal(frontBefore - CombatTuning.SwingDamage, front.Health);
        Assert.Equal(behindBefore, behind.Health);
    }

    [Fact]
    public void Swing_damages_cell_actor_in_front()
    {
        var w = CreateGrassWorld();
        var origin = SimVec2.Zero;
        var cell = new HexAxial(1, 0);
        var center = HexWorldLayout.ToWorld(cell, w.HexSize);
        var def = new ActorDefinition(
            "computer",
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 40),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 40),
            ]);
        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.TryGetActorAt(cell, out var actor));
        var before = actor!.Health;

        var facing = center - origin;
        w.SpawnSwingArc(
            origin,
            facing,
            radius: w.HexSize * 2f,
            arcDegrees: CombatTuning.SwingArcDegrees,
            damage: CombatTuning.SwingDamage,
            ownerFactionId: 1,
            ownerCharacterId: null,
            friendlyFire: true,
            lifetimeSeconds: CombatTuning.SwingVisualDurationSeconds);

        Assert.True(w.TryGetActorAt(cell, out actor));
        Assert.Equal(before - CombatTuning.SwingDamage, actor!.Health);
    }

    [Fact]
    public void IsPointInArc_rejects_out_of_radius()
    {
        Assert.False(Swing.IsPointInArc(
            SimVec2.Zero,
            new SimVec2(1f, 0f),
            radius: 10f,
            arcDegrees: 180f,
            point: new SimVec2(20f, 0f)));
        Assert.True(Swing.IsPointInArc(
            SimVec2.Zero,
            new SimVec2(1f, 0f),
            radius: 10f,
            arcDegrees: 180f,
            point: new SimVec2(5f, 0f)));
    }

    [Fact]
    public void Ai_swings_when_hostile_in_range()
    {
        var w = CreateGrassWorld();
        w.SetSpawnActorDefinition(TestContent.Zombie);
        var swinger = w.AddActor(1, SimVec2.Zero, TestContent.Zombie);
        w.AddActor(2, new SimVec2(10f, 0f));
        var ai = new AiController(new Random(1));
        w.AttachController(ai, swinger);
        var effect = Swing.FindSwingEffect(swinger)!;
        effect.CooldownRemaining = 0f;

        var energyBefore = swinger.Energy;
        w.Tick(0.016f);
        Assert.NotEmpty(w.SwingArcs);
        Assert.Equal(energyBefore - 1, swinger.Energy);
    }

    [Fact]
    public void Swing_rejects_at_zero_energy()
    {
        var (w, driver, player) = TestWorldHelpers.CreateDriven(
            3, 3, 1, new AllGrassGenerator(),
            definition: new ActorDefinition("swinger", [TestContent.Swing]));
        player.Energy = 0;
        w.AddActor(99, player.Position + new SimVec2(12f, 0f));
        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetSecondaryFireHeld(true);
        w.Tick(0.016f);
        Assert.Empty(w.SwingArcs);
    }

    [Fact]
    public void Test_zombie_definition_uses_swing_not_gun()
    {
        Assert.Contains(TestContent.Zombie.Accessories, a => a.Id == "swing");
        Assert.DoesNotContain(TestContent.Zombie.Accessories, a => a.Id == "gun");
    }

    private static GameWorld CreateGrassWorld()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        return w;
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
