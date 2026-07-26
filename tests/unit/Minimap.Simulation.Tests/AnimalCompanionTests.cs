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
        w.SetCharacterDefinitions(
        [
            .. TestContent.Content.Characters,
            TestContent.Fox,
        ]);

        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));

        var owner = w.AddCharacter(
            1,
            HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize),
            TestContent.Bare);
        owner.AddAccessory(foxAbility.CreateInstance());

        Assert.Single(w.Characters);

        w.TickCharacterPassives(0.016f);
        Assert.Equal(2, w.Characters.Count);

        var ally = Assert.Single(w.Characters, c => c.Id != owner.Id);
        Assert.Equal(owner.FactionId, ally.FactionId);
        Assert.Equal("fox", ally.Definition.Id);
        Assert.Contains(
            w.Controllers,
            c => c.Pawn?.Id == ally.Id && c is AiController { Aggression: AiTuning.DefaultAggression });

        w.TickCharacterPassives(0.016f);
        Assert.Equal(2, w.Characters.Count);
    }

    [Fact]
    public void Companion_activation_none_is_not_equipped_in_loadout()
    {
        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
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
        w.SetCharacterDefinitions(
        [
            .. TestContent.Content.Characters,
            TestContent.Fox,
        ]);

        var foxAbility = new AccessoryDefinition(
            "fox",
            [new TestSpawnNearbyAllyEffect("fox")],
            activation: new AccessoryActivation(AccessoryActivationKind.None));

        var owner = w.AddCharacter(
            1,
            HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize),
            TestContent.Bare);
        owner.AddAccessory(foxAbility.CreateInstance());
        w.TickCharacterPassives(0.016f);

        var ally = Assert.Single(w.Characters, c => c.Id != owner.Id);
        var rival = w.AddCharacter(
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
