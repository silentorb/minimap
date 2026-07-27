using CompuQuest.Minimap;
using Minimap.Extensive;
using Minimap.Simulation;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class HealEffectTests
{
    [Fact]
    public void LoadAccessoryFromJson_Heal_ParsesGrantAndEffect()
    {
        var registry = new ExtensionRegistry();
        new CompuQuestExtension().Register(registry);
        registry.AddResourceDefinition(new ResourceDefinition(
            "medkits", registry.Tags.GetOrCreate("medkits")));

        const string json = """
            {
              "id": "heal",
              "activation": { "kind": "modal" },
              "enabledWhen": { "id": "medkits", "atLeast": 1 },
              "effects": [
                { "type": "modify_resource", "id": "medkits", "amount": 3 },
                { "type": "heal", "cost": { "id": "medkits", "amount": 1 } }
              ]
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, registry);
        Assert.NotNull(def.EnabledWhen);
        Assert.Equal(1, def.EnabledWhen.AtLeast);
        Assert.Equal(AccessoryActivationKind.Modal, def.Activation.Kind);
        Assert.Contains(def.EffectTemplates, e => e is ModifyResourceEffect);
        var heal = Assert.IsType<HealEffect>(
            Assert.Single(def.EffectTemplates, e => e is HealEffect));
        Assert.Equal(1, heal.CostAmount);
        Assert.Equal(registry.Tags.GetOrCreate("medkits"), heal.CostResourceTag);
    }

    [Fact]
    public void HealEffect_instant_and_interact_restore_to_max()
    {
        var tags = new TagRegistry();
        var humanTag = tags.GetOrCreate("human");
        var animalTag = tags.GetOrCreate("animal");
        var medkitsTag = tags.GetOrCreate("medkits");
        var maxHealth = new ResourceDefinition(
            WellKnownResourceIds.MaxHealth, tags.GetOrCreate(WellKnownResourceIds.MaxHealth), visible: false);
        var health = new ResourceDefinition(
            WellKnownResourceIds.Health, tags.GetOrCreate(WellKnownResourceIds.Health),
            limitTag: maxHealth.Tag);
        var maxEnergy = new ResourceDefinition(
            WellKnownResourceIds.MaxEnergy, tags.GetOrCreate(WellKnownResourceIds.MaxEnergy), visible: false);
        var energy = new ResourceDefinition(
            WellKnownResourceIds.Energy, tags.GetOrCreate(WellKnownResourceIds.Energy),
            limitTag: maxEnergy.Tag);
        var medkits = new ResourceDefinition("medkits", medkitsTag);
        var context = new ResourceContext(
            [health, maxHealth, energy, maxEnergy, medkits],
            health.Tag, maxHealth.Tag, energy.Tag, maxEnergy.Tag);

        var humanDef = new ActorDefinition(
            "human",
            resources:
            [
                new ActorResourceAmount(maxHealth.Tag, 100),
                new ActorResourceAmount(health.Tag, 100),
            ],
            tags: [humanTag]);
        var animalDef = new ActorDefinition(
            "fox",
            resources:
            [
                new ActorResourceAmount(maxHealth.Tag, 100),
                new ActorResourceAmount(health.Tag, 100),
            ],
            tags: [animalTag]);

        var world = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        world.ApplyGameContent(new GameContent(
            humanDef,
            actors: [humanDef, animalDef],
            resources: [health, maxHealth, energy, maxEnergy, medkits]));

        var caster = world.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), world.HexSize), humanDef);
        caster.AddResource(medkitsTag, 3);
        caster.Health = 20;
        var heal = new HealEffect(humanTag, animalTag, medkitsTag, 1);
        Assert.True(heal.TryUse(caster));
        Assert.Equal(100, caster.Health);
        Assert.Equal(2, caster.GetResource(medkitsTag));

        var front = CellFacing.CellInFront(caster, world.HexSize);
        var ally = world.AddActor(1, HexWorldLayout.ToWorld(front, world.HexSize), animalDef);
        ally.Health = 15;
        Assert.True(heal.CanInteract(world, caster, ally));
        Assert.True(heal.TryInteract(world, caster, ally));
        Assert.Equal(100, ally.Health);
        Assert.Equal(1, caster.GetResource(medkitsTag));
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
