using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>CompuQuest game integrator.</summary>
public sealed class CompuQuestIntegrator : IIntegrator
{
    public const string IntegratorId = "compuquest";
    public const string PlayerSelectableTag = "player_selectable";
    public const string GenericCharacterId = "generic";
    public const string ZombieCharacterId = "zombie";
    public const string ZombieFarmerCharacterId = "zombie_farmer";
    public const string ZombieSpawnerId = "zombie_spawner";

    public string Id => IntegratorId;

    public GameContent CreateGameContent(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetActorDefinition(GenericCharacterId, out var generic) || generic is null)
        {
            throw new InvalidOperationException(
                $"Actor definition '{GenericCharacterId}' is not registered; cannot create game content.");
        }

        if (!registry.TryGetActorDefinition(ZombieCharacterId, out var zombie) || zombie is null)
        {
            throw new InvalidOperationException(
                $"Actor definition '{ZombieCharacterId}' is not registered; cannot create game content.");
        }

        // Marker pool kept for parked ScenarioRunner wave tests; sandbox places the
        // zombie_spawner actor (intrinsic spawn accessory) via PlaceSpawnerActors.
        var zombieSpawner = new SpawnerDefinition(
            ZombieSpawnerId,
            new WeightedPool<ActorDefinition>(
            [
                new WeightedEntry<ActorDefinition>(zombie, 1),
            ]));

        var spawnerPool = new WeightedPool<SpawnerDefinition>(
        [
            new WeightedEntry<SpawnerDefinition>(zombieSpawner, 1),
        ]);

        return new GameContent(
            generic,
            spawnerPool,
            registry.ActorDefinitions,
            registry.ResourceDefinitions);
    }

    public IReadOnlyList<AccessoryDefinition> GetPlayerSelectableAccessories(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.Tags.TryGet(PlayerSelectableTag, out var selectableTag))
            return Array.Empty<AccessoryDefinition>();

        return registry.AccessoryDefinitions
            .Where(a => a.HasTag(selectableTag))
            .ToList();
    }
}
