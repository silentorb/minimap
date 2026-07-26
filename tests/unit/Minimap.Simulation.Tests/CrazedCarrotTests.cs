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
            [new TestPickupEffect(TestContent.FoodResource.Tag, 1)]);
        var lootDef = new ActorDefinition("loose_carrot", [pickup]);
        w.SetActorDefinitions([lootDef]);

        var picker = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        picker.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect()],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        picker.AbilityLoadout.SelectModal(-1);

        var front = CellFacing.CellInFront(picker, w.HexSize);
        Assert.True(w.TryPlaceActor(front, lootDef));
        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, picker));
        Assert.True(EnvironmentInteraction.TryInteract(w, picker));
        Assert.Equal(1, picker.GetResource(TestContent.FoodResource.Tag));
        Assert.False(w.IsCellOccupied(front));
    }

    [Fact]
    public void Crazed_harvest_spawns_character_without_food()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var crazedChar = new CharacterDefinition("crazed_carrot", [TestContent.Swing]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            TestContent.SpawnerPool,
            resources: TestContent.Resources,
            characters: [crazedChar]));
        w.RivalFactionId = 2;

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow_crazed",
            [new TestGrowEffect(0f, mature, null, 0, "crazed_carrot", 5f)]);
        var cropDef = new ActorDefinition("crazed_crop", [growAccessory]);

        var farmer = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        farmer.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect()],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        farmer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, cropDef));
        w.TickCellActors(0.01f);

        Assert.True(EnvironmentInteraction.TryInteract(w, farmer));
        Assert.Equal(0, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.False(w.IsCellOccupied(front));
        Assert.Equal(2, w.Characters.Count);
        var spawned = w.Characters.Single(c => c.Id != farmer.Id);
        Assert.Equal(2, spawned.FactionId);
        Assert.Equal("crazed_carrot", spawned.Definition.Id);
        Assert.Contains(w.Controllers, c => c is ChaseAiController);
    }

    [Fact]
    public void Crazed_auto_emerges_after_post_mature_delay()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var crazedChar = new CharacterDefinition("crazed_carrot", [TestContent.Swing]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            resources: TestContent.Resources,
            characters: [crazedChar]));

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://carrot.svg");
        var growAccessory = new AccessoryDefinition(
            "grow_crazed",
            [new TestGrowEffect(1f, mature, null, 0, "crazed_carrot", 5f)]);
        var cropDef = new ActorDefinition("crazed_crop", [growAccessory]);
        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceActor(cell, cropDef));

        w.TickCellActors(1.01f);
        Assert.True(w.IsCellOccupied(cell));
        Assert.Empty(w.Characters);

        w.TickCellActors(5.01f);
        Assert.False(w.IsCellOccupied(cell));
        Assert.Single(w.Characters);
        Assert.Equal("crazed_carrot", w.Characters[0].Definition.Id);
    }

    [Fact]
    public void Chase_ai_keeps_locked_target_until_reconsider()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var chaser = w.AddCharacter(2, new SimVec2(0f, 0f), TestContent.Zombie);
        var first = w.AddCharacter(1, new SimVec2(40f, 0f), TestContent.Bare);
        var other = w.AddCharacter(1, new SimVec2(80f, 0f), TestContent.Bare);

        var ai = new ChaseAiController(new Random(1), new DirectMoveSteering());
        w.AttachController(ai, chaser);

        // Locks nearest (first, to the right).
        ai.Tick(w, 0.016f);
        Assert.True(chaser.MoveIntent.X > 0f);

        // Make first farther on the left; other becomes nearer on the right.
        // Sticky lock should keep chasing first (left) until reconsider.
        first.Position = new SimVec2(-200f, 0f);
        other.Position = new SimVec2(30f, 0f);
        ai.Tick(w, 0.016f);
        Assert.True(chaser.MoveIntent.X < 0f);

        // After reconsider window, switch to nearer other (right).
        ai.Tick(w, AiWanderGoals.ChaseReconsiderMaxSeconds + 0.1f);
        Assert.True(chaser.MoveIntent.X > 0f);
    }

    [Fact]
    public void Death_places_free_loot_carrot_pickable_without_farm()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var pickup = new AccessoryDefinition(
            "pickup",
            [new TestPickupEffect(TestContent.FoodResource.Tag, 1)]);
        var lootDef = new ActorDefinition("loose_carrot", [pickup]);
        w.ApplyGameContent(new GameContent(
            TestContent.Generic,
            resources: TestContent.Resources,
            actors: [lootDef]));

        var dropAccessory = new AccessoryDefinition(
            "death_drop",
            [new TestDeathDropEffect("loose_carrot")]);
        var monsterDef = new CharacterDefinition("crazed", [dropAccessory, TestContent.Swing]);
        var monster = w.AddCharacter(2, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), monsterDef);
        var cell = HexWorldLayout.WorldToAxial(monster.Position, w.HexSize);

        monster.Health = 0;
        w.Tick(0.016f);

        Assert.DoesNotContain(monster, w.Characters);
        Assert.True(w.IsCellOccupied(cell));
        Assert.Equal("loose_carrot", w.CellActors[cell].Definition.Id);

        var picker = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize), TestContent.Bare);
        picker.Facing = (HexWorldLayout.ToWorld(cell, w.HexSize) - picker.Position);
        // Face toward loot cell.
        var lootWorld = HexWorldLayout.ToWorld(cell, w.HexSize);
        picker.Position = lootWorld - new SimVec2(w.HexSize, 0f);
        picker.Facing = new SimVec2(1f, 0f);

        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, picker));
        Assert.True(EnvironmentInteraction.TryInteract(w, picker));
        Assert.Equal(1, picker.GetResource(TestContent.FoodResource.Tag));
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
