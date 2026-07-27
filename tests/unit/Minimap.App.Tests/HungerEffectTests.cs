using CompuQuest.Minimap;
using Minimap.Extensive;
using Minimap.Simulation;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class HungerEffectTests
{
    [Theory]
    [InlineData(0, 100, -2)]
    [InlineData(1, 100, -1)]
    [InlineData(33, 100, -1)]
    [InlineData(34, 100, 0)]
    [InlineData(66, 100, 0)]
    [InlineData(67, 100, 1)]
    [InlineData(100, 100, 1)]
    public void ResolveAmount_matches_documented_vitality_bands(int energy, int max, int expected)
    {
        ResourceRatioBand[] bands =
        [
            new(0f, -2),
            new(1f / 3f, -1),
            new(2f / 3f, 0),
            new(1f, 1),
        ];
        Assert.Equal(expected, ModifyResourceByRatioBandsEffect.ResolveAmount(energy, max, bands));
    }

    [Fact]
    public void LoadAccessoryFromJson_Eat_ParsesEnabledWhenAndOnUse()
    {
        var registry = new ExtensionRegistry();
        new CompuQuestExtension().Register(registry);
        registry.AddResourceDefinition(new ResourceDefinition(
            "food", registry.Tags.GetOrCreate("food")));
        registry.AddResourceDefinition(new ResourceDefinition(
            "energy", registry.Tags.GetOrCreate("energy")));

        const string json = """
            {
              "id": "eat",
              "activation": { "kind": "modal" },
              "enabledWhen": { "id": "food", "atLeast": 1 },
              "effects": [
                {
                  "type": "modify_resource_on_use",
                  "id": "energy",
                  "amount": 5,
                  "cost": { "id": "food", "amount": 1 }
                }
              ]
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, registry);
        Assert.NotNull(def.EnabledWhen);
        Assert.Equal(1, def.EnabledWhen.AtLeast);
        Assert.Equal(AccessoryActivationKind.Modal, def.Activation.Kind);
        var onUse = Assert.IsType<ModifyResourceOnUseEffect>(Assert.Single(def.EffectTemplates));
        Assert.Equal(5, onUse.Amount);
        Assert.Equal(1, onUse.CostAmount);
    }

    [Fact]
    public void DrainResourceEffect_ticks_down_energy()
    {
        var tags = new TagRegistry();
        var maxEnergy = new ResourceDefinition(
            WellKnownResourceIds.MaxEnergy, tags.GetOrCreate(WellKnownResourceIds.MaxEnergy), visible: false);
        var energy = new ResourceDefinition(
            WellKnownResourceIds.Energy, tags.GetOrCreate(WellKnownResourceIds.Energy),
            limitTag: maxEnergy.Tag);
        var maxHealth = new ResourceDefinition(
            WellKnownResourceIds.MaxHealth, tags.GetOrCreate(WellKnownResourceIds.MaxHealth), visible: false);
        var health = new ResourceDefinition(
            WellKnownResourceIds.Health, tags.GetOrCreate(WellKnownResourceIds.Health),
            limitTag: maxHealth.Tag);
        var context = new ResourceContext(
            [health, maxHealth, energy, maxEnergy],
            health.Tag, maxHealth.Tag, energy.Tag, maxEnergy.Tag);
        var character = new Actor(
            0,
            new ActorDefinition(
                "bare",
                Array.Empty<AccessoryDefinition>(),
                resources:
                [
                    new ActorResourceAmount(maxEnergy.Tag, CombatTuning.DefaultMaxEnergy),
                    new ActorResourceAmount(energy.Tag, CombatTuning.DefaultMaxEnergy),
                ]),
            context,
            1,
            SimVec2.Zero);
        var effect = new DrainResourceEffect(energy.Tag, 1f);
        effect.Tick(character, 1f);
        Assert.Equal(CombatTuning.DefaultMaxEnergy - 1, character.Energy);
    }

    [Fact]
    public void DrainResourceByDistanceEffect_drains_after_unitsPerAmount()
    {
        var tags = new TagRegistry();
        var maxEnergy = new ResourceDefinition(
            WellKnownResourceIds.MaxEnergy, tags.GetOrCreate(WellKnownResourceIds.MaxEnergy), visible: false);
        var energy = new ResourceDefinition(
            WellKnownResourceIds.Energy, tags.GetOrCreate(WellKnownResourceIds.Energy),
            limitTag: maxEnergy.Tag);
        var maxHealth = new ResourceDefinition(
            WellKnownResourceIds.MaxHealth, tags.GetOrCreate(WellKnownResourceIds.MaxHealth), visible: false);
        var health = new ResourceDefinition(
            WellKnownResourceIds.Health, tags.GetOrCreate(WellKnownResourceIds.Health),
            limitTag: maxHealth.Tag);
        var context = new ResourceContext(
            [health, maxHealth, energy, maxEnergy],
            health.Tag, maxHealth.Tag, energy.Tag, maxEnergy.Tag);
        var character = new Actor(
            0,
            new ActorDefinition(
                "bare",
                Array.Empty<AccessoryDefinition>(),
                resources:
                [
                    new ActorResourceAmount(maxEnergy.Tag, CombatTuning.DefaultMaxEnergy),
                    new ActorResourceAmount(energy.Tag, CombatTuning.DefaultMaxEnergy),
                ]),
            context,
            1,
            SimVec2.Zero);
        var effect = new DrainResourceByDistanceEffect(energy.Tag, 120f);
        effect.Tick(character, 0.016f);
        Assert.Equal(CombatTuning.DefaultMaxEnergy, character.Energy);
        character.Position = new SimVec2(120f, 0f);
        effect.Tick(character, 0.016f);
        Assert.Equal(CombatTuning.DefaultMaxEnergy - 1, character.Energy);
    }
}
