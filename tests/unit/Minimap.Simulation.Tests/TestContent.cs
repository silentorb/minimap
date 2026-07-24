using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

internal static class TestContent
{
    private static readonly TagRegistry Tags = new();

    public static ResourceDefinition MaxHealthResource { get; } = new(
        WellKnownResourceIds.MaxHealth,
        Tags.GetOrCreate(WellKnownResourceIds.MaxHealth),
        displayName: "Max Health",
        visible: false,
        uiPriority: 0);

    public static ResourceDefinition HealthResource { get; } = new(
        WellKnownResourceIds.Health,
        Tags.GetOrCreate(WellKnownResourceIds.Health),
        displayName: "Health",
        iconConfig: null,
        limitTag: MaxHealthResource.Tag,
        visible: true,
        uiPriority: 1000);

    public static ResourceDefinition MaxEnergyResource { get; } = new(
        WellKnownResourceIds.MaxEnergy,
        Tags.GetOrCreate(WellKnownResourceIds.MaxEnergy),
        displayName: "Max Energy",
        visible: false,
        uiPriority: 0);

    public static ResourceDefinition EnergyResource { get; } = new(
        WellKnownResourceIds.Energy,
        Tags.GetOrCreate(WellKnownResourceIds.Energy),
        displayName: "Energy",
        limitTag: MaxEnergyResource.Tag,
        visible: true,
        uiPriority: 900);

    public static ResourceDefinition AmmoResource { get; } = new(
        "ammo",
        Tags.GetOrCreate("ammo"),
        displayName: "Ammo",
        visible: true,
        uiPriority: 80);

    public static ResourceDefinition FoodResource { get; } = new(
        "food",
        Tags.GetOrCreate("food"),
        displayName: "Food",
        visible: true,
        uiPriority: 70);

    public static IReadOnlyList<ResourceDefinition> Resources { get; } =
    [
        HealthResource,
        MaxHealthResource,
        EnergyResource,
        MaxEnergyResource,
        AmmoResource,
        FoodResource,
    ];

    public static ResourceContext ResourceContext { get; } = new(
        Resources,
        HealthResource.Tag,
        MaxHealthResource.Tag,
        EnergyResource.Tag,
        MaxEnergyResource.Tag);

    public static AccessoryDefinition Gun { get; } = new(
        "gun",
        [
            new TestGrantResourceEffect(AmmoResource.Tag, 6),
            new TestShootEffect(1.25f, 200f, 25, true, AmmoResource.Tag, 1),
        ]);

    public static CharacterDefinition Bare { get; } =
        new("bare", Array.Empty<AccessoryDefinition>());

    public static CharacterDefinition Generic { get; } = new("generic", [Gun]);

    public static CharacterDefinition Zombie { get; } = new("zombie", [Gun]);

    public static SpawnerDefinition ZombieSpawner { get; } = new(
        "zombie_spawner",
        new WeightedPool<CharacterDefinition>(
        [
            new WeightedEntry<CharacterDefinition>(Zombie, 1),
        ]));

    public static WeightedPool<SpawnerDefinition> SpawnerPool { get; } = new(
    [
        new WeightedEntry<SpawnerDefinition>(ZombieSpawner, 1),
    ]);

    public static GameContent Content { get; } = new(Generic, SpawnerPool, resources: Resources);
}
