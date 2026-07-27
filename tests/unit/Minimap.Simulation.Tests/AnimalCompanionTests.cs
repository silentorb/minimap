using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class AnimalCompanionTests
{
    [Fact]
    public void Companion_accessory_spawns_one_same_faction_ally_once()
    {
        var w = GameWorld.Create(4, 4, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));

        var owner = w.AddActor(
            1,
            HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize),
            TestContent.Bare);
        owner.AddAccessory(foxAbility.CreateInstance());

        Assert.Single(w.Actors);

        w.TickActorPassives(0.016f);
        Assert.Equal(2, w.Actors.Count);

        var ally = Assert.Single(w.Actors, c => c.Id != owner.Id);
        Assert.Equal(owner.FactionId, ally.FactionId);
        Assert.Equal("fox", ally.Definition.Id);
        Assert.Contains(
            w.Controllers,
            c => c.Pawn?.Id == ally.Id && c is AiController { Aggression: AiTuning.DefaultAggression });

        w.TickActorPassives(0.016f);
        Assert.Equal(2, w.Actors.Count);
    }

    [Fact]
    public void Companion_activation_none_is_not_equipped_in_loadout()
    {
        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.AddAccessory(foxAbility.CreateInstance());

        Assert.Empty(character.AbilityLoadout.Modal);
        Assert.False(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.PrimaryFire, out _));
        Assert.False(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.SecondaryFire, out _));
        Assert.Null(character.AbilityLoadout.SelectedModal);
    }

    [Fact]
    public void Companion_ally_is_hostile_to_rivals_not_owner()
    {
        var w = GameWorld.Create(4, 4, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));

        var owner = w.AddActor(
            1,
            HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize),
            TestContent.Bare);
        owner.AddAccessory(foxAbility.CreateInstance());
        w.TickActorPassives(0.016f);

        var ally = Assert.Single(w.Actors, c => c.Id != owner.Id);
        var rival = w.AddActor(
            2,
            HexWorldLayout.ToWorld(new HexAxial(2, 0), w.HexSize),
            TestContent.Bare);

        Assert.False(FactionRules.AreHostile(ally.FactionId, owner.FactionId));
        Assert.True(FactionRules.AreHostile(ally.FactionId, rival.FactionId));
        Assert.Null(Shoot.FindNearestHostile(ally, [owner, ally]));
        Assert.Same(rival, Shoot.FindNearestHostile(ally, [owner, ally, rival]));
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
