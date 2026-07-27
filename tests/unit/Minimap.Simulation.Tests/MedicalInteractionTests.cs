using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class MedicalInteractionTests
{
    private static IEnumerable<ActorResourceAmount> PawnResources { get; } =
    [
        new ActorResourceAmount(TestContent.MaxHealthResource.Tag, CombatTuning.DefaultMaxHealth),
        new ActorResourceAmount(TestContent.HealthResource.Tag, CombatTuning.DefaultMaxHealth),
        new ActorResourceAmount(TestContent.MaxEnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
        new ActorResourceAmount(TestContent.EnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
    ];

    [Fact]
    public void Resolve_finds_free_actor_on_front_hex()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);
        healer.AddResource(TestContent.MedkitsResource.Tag, 3);

        var front = CellFacing.CellInFront(healer, w.HexSize);
        var allyDef = new ActorDefinition("ally", resources: PawnResources, tags: [TestContent.HumanTag]);
        var ally = w.AddActor(1, HexWorldLayout.ToWorld(front, w.HexSize), allyDef);
        ally.Health = 40;

        Assert.Equal(ally, EnvironmentInteraction.ResolveTarget(w, healer));
        Assert.True(EnvironmentInteraction.TryInteract(w, healer));
        Assert.Equal(ally.MaxHealth, ally.Health);
        Assert.Equal(2, healer.GetResource(TestContent.MedkitsResource.Tag));
    }

    [Fact]
    public void Heal_self_activate_restores_full_health()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var humanDef = new ActorDefinition(
            "human_pawn",
            [TestContent.Move],
            resources: PawnResources,
            tags: [TestContent.HumanTag]);
        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), humanDef);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);
        healer.AddResource(TestContent.MedkitsResource.Tag, 3);
        healer.Health = 25;

        Assert.True(AbilityLoadout.TryActivateInstantUse(healer.AbilityLoadout.SelectedModal, healer));
        Assert.Equal(healer.MaxHealth, healer.Health);
        Assert.Equal(2, healer.GetResource(TestContent.MedkitsResource.Tag));
    }

    [Fact]
    public void Heal_self_rejects_at_full_health()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var humanDef = new ActorDefinition(
            "human_pawn",
            [TestContent.Move],
            resources: PawnResources,
            tags: [TestContent.HumanTag]);
        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), humanDef);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);
        healer.AddResource(TestContent.MedkitsResource.Tag, 3);

        Assert.False(AbilityLoadout.TryActivateInstantUse(healer.AbilityLoadout.SelectedModal, healer));
        Assert.Equal(3, healer.GetResource(TestContent.MedkitsResource.Tag));
    }

    [Fact]
    public void Heal_interact_rejects_zombie()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);
        healer.AddResource(TestContent.MedkitsResource.Tag, 3);

        var front = CellFacing.CellInFront(healer, w.HexSize);
        var zombie = w.AddActor(2, HexWorldLayout.ToWorld(front, w.HexSize), TestContent.Zombie);
        zombie.Health = 40;

        Assert.Null(EnvironmentInteraction.ResolveTarget(w, healer));
        Assert.False(EnvironmentInteraction.TryInteract(w, healer));
        Assert.Equal(40, zombie.Health);
        Assert.Equal(3, healer.GetResource(TestContent.MedkitsResource.Tag));
    }

    [Fact]
    public void Heal_interact_rejects_without_medkits()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(healer, w.HexSize);
        var allyDef = new ActorDefinition("ally", resources: PawnResources, tags: [TestContent.HumanTag]);
        var ally = w.AddActor(1, HexWorldLayout.ToWorld(front, w.HexSize), allyDef);
        ally.Health = 40;

        Assert.Null(EnvironmentInteraction.ResolveTarget(w, healer));
        Assert.False(EnvironmentInteraction.TryInteract(w, healer));
        Assert.Equal(40, ally.Health);
    }

    [Fact]
    public void Heal_interact_prefers_injured_ally_over_placeable_when_both_present()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var healer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        healer.AddAccessory(HealAccessory().CreateInstance());
        healer.AbilityLoadout.SelectModal(0);
        healer.AddResource(TestContent.MedkitsResource.Tag, 3);

        var front = CellFacing.CellInFront(healer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, new ActorDefinition("crate"), healer.FactionId));

        var allyDef = new ActorDefinition("ally", resources: PawnResources, tags: [TestContent.AnimalTag]);
        var ally = w.AddActor(1, HexWorldLayout.ToWorld(front, w.HexSize), allyDef);
        ally.Health = 10;

        Assert.Equal(ally, EnvironmentInteraction.ResolveTarget(w, healer));
        Assert.True(EnvironmentInteraction.TryInteract(w, healer));
        Assert.Equal(ally.MaxHealth, ally.Health);
        Assert.True(w.IsCellOccupied(front));
    }

    [Fact]
    public void Farm_harvest_still_resolves_when_ally_stands_on_crop()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var mature = new DepictionConfig(DepictionKinds.Texture, "res://mature.svg");
        var growAccessory = new AccessoryDefinition(
            "grow",
            [new TestGrowEffect(0f, mature, TestContent.FoodResource.Tag, 1)]);
        var cropDef = new ActorDefinition("carrot_growing", [growAccessory]);

        var farmer = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        farmer.AddAccessory(new AccessoryDefinition(
            "farm",
            [new TestHarvestEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        farmer.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(farmer, w.HexSize);
        Assert.True(w.TryPlaceActor(front, cropDef, farmer.FactionId));
        w.TickActorPassives(0.01f);
        Assert.True(w.CellActors[front].Effects.OfType<IGrowEffect>().Single().IsMature);

        var ally = w.AddActor(1, HexWorldLayout.ToWorld(front, w.HexSize), TestContent.Bare);
        Assert.Equal(w.CellActors[front], EnvironmentInteraction.ResolveTarget(w, farmer));
        Assert.True(EnvironmentInteraction.TryInteract(w, farmer));
        Assert.False(w.IsCellOccupied(front));
        Assert.Equal(1, farmer.GetResource(TestContent.FoodResource.Tag));
        Assert.True(ally.IsAlive);
    }

    private static AccessoryDefinition HealAccessory() =>
        new(
            "heal",
            [
                new TestHealEffect(
                    TestContent.HumanTag,
                    TestContent.AnimalTag,
                    TestContent.MedkitsResource.Tag,
                    1),
            ],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
