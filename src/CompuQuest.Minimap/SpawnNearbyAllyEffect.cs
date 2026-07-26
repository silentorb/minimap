using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>One-shot: spawn a same-faction AI character near the owner.</summary>
public sealed class SpawnNearbyAllyEffect : AccessoryEffect, IWorldCharacterPassiveEffect
{
    private bool _spawned;

    public SpawnNearbyAllyEffect(string characterId, float aggression = AiTuning.DefaultAggression)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            throw new ArgumentException("Character id is required.", nameof(characterId));
        if (aggression < 0f || aggression > 1f)
            throw new ArgumentOutOfRangeException(nameof(aggression));

        CharacterId = characterId;
        Aggression = aggression;
    }

    public string CharacterId { get; }

    public float Aggression { get; }

    public void Tick(GameWorld world, Character character, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(character);
        if (_spawned || !character.IsAlive)
            return;

        if (!world.TryGetCharacterDefinition(CharacterId, out var definition) || definition is null)
        {
            throw new InvalidOperationException(
                $"spawn_nearby_ally character definition '{CharacterId}' is not registered.");
        }

        var origin = HexWorldLayout.WorldToAxial(character.Position, world.HexSize);
        var seekCrops = AiController.CharacterSeeksCrops(definition);
        var ally = world.TrySpawnNearbyCharacter(
            origin,
            definition,
            character.FactionId,
            Aggression,
            seekCrops);
        if (ally is not null)
            _spawned = true;
    }

    public override AccessoryEffect Clone() =>
        new SpawnNearbyAllyEffect(CharacterId, Aggression);
}
