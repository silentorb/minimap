namespace Minimap.Simulation.Types;

/// <summary>Playthrough content bag. Not an integration-facing type.</summary>
public sealed class GameContent
{
    public GameContent(
        CharacterDefinition defaultCharacter,
        WeightedPool<SpawnerDefinition>? worldSpawnerPool = null)
    {
        ArgumentNullException.ThrowIfNull(defaultCharacter);
        DefaultCharacter = defaultCharacter;
        WorldSpawnerPool = worldSpawnerPool ?? WeightedPool<SpawnerDefinition>.Empty;
    }

    public CharacterDefinition DefaultCharacter { get; }

    /// <summary>Weighted pool of spawner definitions placed during level init.</summary>
    public WeightedPool<SpawnerDefinition> WorldSpawnerPool { get; }
}
