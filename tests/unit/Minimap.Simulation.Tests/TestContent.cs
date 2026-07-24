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

    public static ResourceDefinition AmmoResource { get; } = new(
        "ammo",
        Tags.GetOrCreate("ammo"),
        displayName: "Ammo",
        visible: true,
        uiPriority: 80);

    public static IReadOnlyList<ResourceDefinition> Resources { get; } =
    [
        HealthResource,
        MaxHealthResource,
        AmmoResource,
    ];

    public static ResourceContext ResourceContext { get; } = new(
        Resources,
        HealthResource.Tag,
        MaxHealthResource.Tag);

    public static AccessoryDefinition Gun { get; } = new(
        "gun",
        [new TestShootEffect(1.25f, 200f, 25, true)],
        consumedResourceTag: AmmoResource.Tag,
        startingResourceAmount: 6);

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
