using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class CrazedCarrotTests
{
    [Fact]
    public void Free_loot_pickup_works_with_none_equipped()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var pickup = new AccessoryDefinition(
            "pickup",
            [new TestPickupEffect(TestContent.FoodResource.Tag, 1, TestContent.EnergyResource.Tag, 1)]);
        var lootDef = new ActorDefinition("carrot_picked", [pickup]);
        w.SetActorDefinitions([lootDef]);

        var picker = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        picker.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        picker.AbilityLoadout.SelectModal(-1);

        var front = CellFacing.CellInFront(picker, w.HexSize);
        Assert.True(w.TryPlaceActor(front, lootDef));
        var energyBefore = picker.Energy;
        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, picker));
        Assert.True(EnvironmentInteraction.TryInteract(w, picker));
        Assert.Equal(1, picker.GetResource(TestContent.FoodResource.Tag));
        Assert.Equal(energyBefore - 1, picker.Energy);
        Assert.False(w.IsCellOccupied(front));
    }

    [Fact]
    public void Free_loot_pickup_rejects_at_zero_energy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var pickup = new AccessoryDefinition(
            "pickup",
            [new TestPickupEffect(TestContent.FoodResource.Tag, 1, TestContent.EnergyResource.Tag, 1)]);
        var lootDef = new ActorDefinition("carrot_picked", [pickup]);
        w.SetActorDefinitions([lootDef]);

        var picker = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        picker.Energy = 0;
        picker.AbilityLoadout.SelectModal(-1);

        var front = CellFacing.CellInFront(picker, w.HexSize);
        Assert.True(w.TryPlaceActor(front, lootDef));
        Assert.Null(EnvironmentInteraction.ResolveTarget(w, picker));
        Assert.False(EnvironmentInteraction.TryInteract(w, picker));
        Assert.True(w.IsCellOccupied(front));
    }

    [Fact]
    public void Crazed_harvest_spawns_character_without_food()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var crazedChar = new ActorDefinition("crazed_carrot", [TestContent.Swing]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            TestContent.SpawnerPool,
            actors: [crazedChar],
            resources: TestContent.Resources));
        w.RivalFactionId = 2;

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow_crazed",
            [new TestGrowEffect(0f, mature, null, 0, "crazed_carrot", 5f)]);
        var cropDef = new ActorDefinition("crazed_crop", [growAccessory]);

        var farmer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        farmer.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        farmer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, cropDef));
        w.TickActorPassives(0.01f);

        Assert.True(EnvironmentInteraction.TryInteract(w, farmer));
        Assert.Equal(0, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.False(w.IsCellOccupied(front));
        Assert.Equal(2, w.Actors.Count);
        var spawned = w.Actors.Single(c => c.Id != farmer.Id);
        Assert.Equal(2, spawned.FactionId);
        Assert.Equal("crazed_carrot", spawned.Definition.Id);
        var ai = Assert.IsType<AiController>(
            Assert.Single(w.Controllers, c => c.Pawn?.Id == spawned.Id));
        Assert.Equal(AiTuning.CrazedCarrotAggression, ai.Aggression);
    }

    [Fact]
    public void Crazed_auto_emerges_after_post_mature_delay()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var crazedChar = new ActorDefinition("crazed_carrot", [TestContent.Swing]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            actors: [crazedChar],
            resources: TestContent.Resources));

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow_crazed",
            [new TestGrowEffect(1f, mature, null, 0, "crazed_carrot", 5f)]);
        var cropDef = new ActorDefinition("crazed_crop", [growAccessory]);
        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceActor(cell, cropDef));

        w.TickActorPassives(1.01f);
        Assert.True(w.IsCellOccupied(cell));
        Assert.Single(w.Actors);
        Assert.Equal("crazed_crop", w.Actors[0].Definition.Id);

        w.TickActorPassives(5.01f);
        Assert.False(w.IsCellOccupied(cell));
        Assert.Single(w.Actors);
        Assert.Equal("crazed_carrot", w.Actors[0].Definition.Id);
        Assert.Null(w.Actors[0].Cell);
    }

    [Fact]
    public void High_aggression_tracks_nearest_hostile_on_retarget()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var chaser = w.AddActor(2, new SimVec2(0f, 0f), TestContent.Zombie);
        var first = w.AddActor(1, new SimVec2(40f, 0f), TestContent.Bare);
        var other = w.AddActor(1, new SimVec2(80f, 0f), TestContent.Bare);

        var ai = new AiController(
            new Random(1),
            new DirectMoveSteering(),
            aggression: 1f);
        w.AttachController(ai, chaser);

        ai.Tick(w, 0.016f);
        Assert.True(chaser.MoveIntent.X > 0f);

        // Nearest becomes other on the right after first moves far left.
        first.Position = new SimVec2(-200f, 0f);
        other.Position = new SimVec2(30f, 0f);
        ai.Tick(w, AiWanderGoals.RetargetMaxSeconds + 0.1f);
        Assert.True(chaser.MoveIntent.X > 0f);
    }

    [Fact]
    public void Death_places_free_loot_carrot_pickable_without_farm()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var pickup = new AccessoryDefinition(
            "pickup",
            [new TestPickupEffect(TestContent.FoodResource.Tag, 1, TestContent.EnergyResource.Tag, 1)]);
        var lootDef = new ActorDefinition("carrot_picked", [pickup]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            resources: TestContent.Resources,
            actors: [lootDef]));

        var dropAccessory = new AccessoryDefinition(
            "death_drop",
            [new TestDeathDropEffect("carrot_picked")]);
        var monsterDef = new ActorDefinition(
            "crazed",
            [dropAccessory, TestContent.Swing],
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 100),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 100),
            ]);
        var monster = w.AddActor(2, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), monsterDef);
        var cell = HexWorldLayout.WorldToAxial(monster.Position, w.HexSize);

        monster.Health = 0;
        w.Tick(0.016f);

        Assert.DoesNotContain(monster, w.Actors);
        Assert.True(w.IsCellOccupied(cell));
        Assert.Equal("carrot_picked", w.CellActors[cell].Definition.Id);

        var picker = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize), TestContent.Bare);
        // Face toward loot cell.
        var lootWorld = HexWorldLayout.ToWorld(cell, w.HexSize);
        picker.Position = lootWorld - new SimVec2(w.HexSize, 0f);
        picker.Facing = new SimVec2(1f, 0f);

        var energyBefore = picker.Energy;
        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, picker));
        Assert.True(EnvironmentInteraction.TryInteract(w, picker));
        Assert.Equal(1, picker.GetResource(TestContent.FoodResource.Tag));
        Assert.Equal(energyBefore - 1, picker.Energy);
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
