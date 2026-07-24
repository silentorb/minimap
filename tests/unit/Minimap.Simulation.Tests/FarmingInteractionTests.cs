using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class FarmingInteractionTests
{
    [Fact]
    public void Grow_matures_and_sets_depiction_override()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://mature.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(1f, mature, TestContent.FoodResource.Tag, 2)]);
        var actorDef = new ActorDefinition(
            "carrot",
            [growAccessory],
            new DepictionConfig(DepictionKinds.Texture, "res://seedling.svg"));

        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceActor(cell, actorDef));
        var crop = w.CellActors[cell];
        Assert.Null(crop.DepictionOverride);

        w.TickCellActors(0.5f);
        Assert.False(crop.Effects.OfType<IGrowEffect>().Single().IsMature);

        w.TickCellActors(0.6f);
        Assert.True(crop.Effects.OfType<IGrowEffect>().Single().IsMature);
        Assert.Equal("res://mature.svg", crop.DepictionOverride!.ResourcePath);
    }

    [Fact]
    public void Harvest_grants_food_and_removes_actor()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://mature.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(0f, mature, TestContent.FoodResource.Tag, 3)]);
        var actorDef = new ActorDefinition("melon", [growAccessory]);

        var farmer = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        var farmAbility = new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect()],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));
        farmer.AddAccessory(farmAbility.CreateInstance());
        farmer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, actorDef));
        w.TickCellActors(0.01f);

        Assert.Equal(0, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, farmer));
        Assert.True(EnvironmentInteraction.TryInteract(w, farmer));
        Assert.Equal(3, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.False(w.IsCellOccupied(front));
    }

    [Fact]
    public void Interact_rejects_immature_crop()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://mature.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(5f, mature, TestContent.FoodResource.Tag, 1)]);
        var actorDef = new ActorDefinition("carrot", [growAccessory]);

        var farmer = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        farmer.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect()],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        farmer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, actorDef));
        Assert.Null(EnvironmentInteraction.ResolveTarget(w, farmer));
        Assert.False(EnvironmentInteraction.TryInteract(w, farmer));
        Assert.True(w.IsCellOccupied(front));
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
