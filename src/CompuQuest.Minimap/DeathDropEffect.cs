using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Places a cell actor when the owning character dies.</summary>
public sealed class DeathDropEffect : AccessoryEffect, IDeathDropEffect
{
    public DeathDropEffect(string actorDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(actorDefinitionId))
            throw new ArgumentException("Actor definition id must be non-empty.", nameof(actorDefinitionId));
        ActorDefinitionId = actorDefinitionId;
    }

    public string ActorDefinitionId { get; }

    public override AccessoryEffect Clone() => new DeathDropEffect(ActorDefinitionId);
}
