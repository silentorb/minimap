using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>One-shot: spawn a same-faction AI actor near the owner.</summary>
public sealed class SpawnNearbyAllyEffect : AccessoryEffect, IWorldActorPassiveEffect
{
    private bool _spawned;

    public SpawnNearbyAllyEffect(string actorId, float aggression = AiTuning.DefaultAggression)
    {
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        if (aggression < 0f || aggression > 1f)
            throw new ArgumentOutOfRangeException(nameof(aggression));

        ActorId = actorId;
        Aggression = aggression;
    }

    public string ActorId { get; }

    public float Aggression { get; }

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        if (_spawned || !actor.IsAlive)
            return;

        if (!world.TryGetActorDefinition(ActorId, out var definition) || definition is null)
        {
            throw new InvalidOperationException(
                $"spawn_nearby_ally actor definition '{ActorId}' is not registered.");
        }

        var origin = HexWorldLayout.WorldToAxial(actor.Position, world.HexSize);
        var seekCrops = AiController.ActorSeeksCrops(definition);
        var ally = world.TrySpawnNearbyActor(
            origin,
            definition,
            actor.FactionId,
            Aggression,
            seekCrops);
        if (ally is not null)
            _spawned = true;
    }

    public override AccessoryEffect Clone() =>
        new SpawnNearbyAllyEffect(ActorId, Aggression);
}
