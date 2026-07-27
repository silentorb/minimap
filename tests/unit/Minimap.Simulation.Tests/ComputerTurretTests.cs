using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class ComputerTurretTests
{
    [Fact]
    public void Computer_turret_grants_ammo_and_auto_fires_at_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var gun = new AccessoryDefinition(
            "computer_gun",
            [
                new TestGrantResourceEffect(TestContent.AmmoResource.Tag, 10),
                new TestShootEffect(
                    CombatTuning.FireIntervalSeconds,
                    CombatTuning.MissileSpeed,
                    CombatTuning.MissileDamage,
                    true,
                    TestContent.AmmoResource.Tag,
                    1),
            ],
            activation: AccessoryActivation.None);
        var computerDef = new ActorDefinition("computer", [gun]);
        w.SetActorDefinitions([computerDef, TestContent.Missile]);

        var cell = new HexAxial(0, 0);
        Assert.True(w.TryPlaceActor(cell, computerDef, factionId: 1));
        var turret = w.CellActors[cell];
        Assert.Equal(1, turret.FactionId);
        Assert.Equal(10, turret.GetResource(TestContent.AmmoResource.Tag));
        Assert.Contains(turret, w.Actors);

        var hostile = w.AddActor(
            2,
            HexWorldLayout.ToWorld(new HexAxial(2, 0), w.HexSize),
            TestContent.Bare);
        Assert.NotEqual(turret.Id, hostile.Id);

        w.TickActorPassives(0.016f);
        var missile = Assert.Single(TestWorldHelpers.Projectiles(w));
        Assert.Equal(9, turret.GetResource(TestContent.AmmoResource.Tag));
        Assert.Equal(1, missile.FactionId);
        Assert.Equal(turret.Id, missile.Projectile!.OwnerActorId);
        Assert.True(hostile.IsAlive);
    }

    [Fact]
    public void Computer_turret_does_not_fire_without_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var gun = new AccessoryDefinition(
            "computer_gun",
            [
                new TestGrantResourceEffect(TestContent.AmmoResource.Tag, 10),
                new TestShootEffect(
                    CombatTuning.FireIntervalSeconds,
                    CombatTuning.MissileSpeed,
                    CombatTuning.MissileDamage,
                    true,
                    TestContent.AmmoResource.Tag,
                    1),
            ],
            activation: AccessoryActivation.None);
        var computerDef = new ActorDefinition("computer", [gun]);
        w.SetActorDefinitions([computerDef, TestContent.Missile]);

        var cell = new HexAxial(0, 0);
        Assert.True(w.TryPlaceActor(cell, computerDef, factionId: 1));
        w.TickActorPassives(0.016f);
        Assert.Empty(TestWorldHelpers.Projectiles(w));
        Assert.Equal(10, w.CellActors[cell].GetResource(TestContent.AmmoResource.Tag));
    }

    [Fact]
    public void Computer_turret_does_not_fire_at_ally()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var gun = new AccessoryDefinition(
            "computer_gun",
            [
                new TestGrantResourceEffect(TestContent.AmmoResource.Tag, 10),
                new TestShootEffect(
                    CombatTuning.FireIntervalSeconds,
                    CombatTuning.MissileSpeed,
                    CombatTuning.MissileDamage,
                    true,
                    TestContent.AmmoResource.Tag,
                    1),
            ],
            activation: AccessoryActivation.None);
        var computerDef = new ActorDefinition("computer", [gun]);
        w.SetActorDefinitions([computerDef, TestContent.Missile]);

        var cell = new HexAxial(0, 0);
        Assert.True(w.TryPlaceActor(cell, computerDef, factionId: 1));
        w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(2, 0), w.HexSize), TestContent.Bare);

        w.TickActorPassives(0.016f);
        Assert.Empty(TestWorldHelpers.Projectiles(w));
        Assert.Equal(10, w.CellActors[cell].GetResource(TestContent.AmmoResource.Tag));
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
