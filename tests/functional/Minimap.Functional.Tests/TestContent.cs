using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Functional.Tests;

internal static class TestContent
{
    private static readonly TagRegistry Tags = new();

    public static ResourceDefinition MaxHealthResource { get; } = new(
        WellKnownResourceIds.MaxHealth,
        Tags.GetOrCreate(WellKnownResourceIds.MaxHealth),
        displayName: "Max Health",
        visible: false);

    public static ResourceDefinition HealthResource { get; } = new(
        WellKnownResourceIds.Health,
        Tags.GetOrCreate(WellKnownResourceIds.Health),
        displayName: "Health",
        limitTag: MaxHealthResource.Tag,
        visible: true,
        uiPriority: 1000);

    public static ResourceDefinition MaxEnergyResource { get; } = new(
        WellKnownResourceIds.MaxEnergy,
        Tags.GetOrCreate(WellKnownResourceIds.MaxEnergy),
        displayName: "Max Energy",
        visible: false);

    public static ResourceDefinition EnergyResource { get; } = new(
        WellKnownResourceIds.Energy,
        Tags.GetOrCreate(WellKnownResourceIds.Energy),
        displayName: "Energy",
        limitTag: MaxEnergyResource.Tag,
        visible: true,
        uiPriority: 900);

    public static IReadOnlyList<ResourceDefinition> Resources { get; } =
    [
        HealthResource,
        MaxHealthResource,
        EnergyResource,
        MaxEnergyResource,
    ];

    public static ResourceContext ResourceContext { get; } = new(
        Resources,
        HealthResource.Tag,
        MaxHealthResource.Tag,
        EnergyResource.Tag,
        MaxEnergyResource.Tag);

    public static AccessoryDefinition Move { get; } = new(
        "move",
        [new FunctionalMoveEffect()],
        activation: AccessoryActivation.None);

    public static ActorDefinition Missile { get; } = new(
        "missile",
        displayName: "Missile",
        size: CombatTuning.MissileSize);

    public static ActorDefinition Generic { get; } = new(
        "generic",
        [Move],
        resources:
        [
            new ActorResourceAmount(MaxHealthResource.Tag, CombatTuning.DefaultMaxHealth),
            new ActorResourceAmount(HealthResource.Tag, CombatTuning.DefaultMaxHealth),
            new ActorResourceAmount(MaxEnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
            new ActorResourceAmount(EnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
        ]);

    public static GameContent Content { get; } = new(
        Generic,
        actors: [Generic, Missile],
        resources: Resources);
}

internal sealed class FunctionalMoveEffect : AccessoryEffect, IMoveEffect
{
    public float Speed => CombatTuning.MoveSpeed;

    public override AccessoryEffect Clone() => new FunctionalMoveEffect();
}
