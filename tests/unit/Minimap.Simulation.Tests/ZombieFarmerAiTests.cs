using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class ZombieFarmerAiTests
{
    [Fact]
    public void Farmer_pulls_toward_mature_crop()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var farmer = w.AddCharacter(2, new SimVec2(0f, 0f), TestContent.ZombieFarmer);
        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(0f, mature, TestContent.FoodResource.Tag, 1)]);
        var cropDef = new ActorDefinition("carrot", [growAccessory]);
        w.SetActorDefinitions([cropDef, .. TestContent.Content.Actors]);

        var cropCell = new HexAxial(2, 0);
        Assert.True(w.TryPlaceActor(cropCell, cropDef));
        w.TickCellActors(0.01f);

        var ai = new AiController(
            new Random(1),
            new DirectMoveSteering(),
            aggression: 1f,
            seekCrops: true);
        w.AttachController(ai, farmer);
        ai.Tick(w, 0.016f);

        Assert.True(farmer.MoveIntent.X > 0f);
    }

    [Fact]
    public void Farmer_harvests_mature_crop_in_front()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var farmer = w.AddCharacter(
            2,
            HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize),
            TestContent.ZombieFarmer);
        farmer.Facing = new SimVec2(1f, 0f);

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(0f, mature, TestContent.FoodResource.Tag, 1)]);
        var cropDef = new ActorDefinition("carrot", [growAccessory]);
        w.SetActorDefinitions([cropDef, .. TestContent.Content.Actors]);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, cropDef));
        w.TickCellActors(0.01f);

        var ai = new AiController(
            new Random(1),
            new DirectMoveSteering(),
            aggression: 0f,
            seekCrops: true);
        w.AttachController(ai, farmer);
        ai.Tick(w, 0.016f);

        Assert.False(w.IsCellOccupied(front));
        Assert.Equal(1, farmer.GetResource(TestContent.FoodResource.Tag));
    }

    [Fact]
    public void Farmer_eats_when_energy_low_and_has_food()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var farmer = w.AddCharacter(2, new SimVec2(0f, 0f), TestContent.ZombieFarmer);
        farmer.AddResource(TestContent.FoodResource.Tag, 1);
        farmer.Energy = farmer.MaxEnergy - AiTuning.EatEnergyDeficitThreshold;

        var ai = new AiController(
            new Random(1),
            new DirectMoveSteering(),
            aggression: 0f,
            seekCrops: true);
        w.AttachController(ai, farmer);
        ai.Tick(w, 0.016f);

        Assert.Equal(0, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.Equal(farmer.MaxEnergy, farmer.Energy);
    }

    [Fact]
    public void CharacterSeeksCrops_detects_farm_accessory()
    {
        Assert.False(AiController.CharacterSeeksCrops(TestContent.Zombie));
        Assert.True(AiController.CharacterSeeksCrops(TestContent.ZombieFarmer));
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
