using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

internal static class TestContent
{
    public static AccessoryDefinition Gun { get; } = new(
        "gun",
        [new TestShootEffect(1.25f, 200f, 25f, true)]);

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

    public static GameContent Content { get; } = new(Generic, SpawnerPool);
}
