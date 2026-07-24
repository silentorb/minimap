using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Grants or adjusts a resource when the owning accessory is acquired.</summary>
public sealed class ModifyResourceEffect : AccessoryEffect, IOnAccessoryAcquired
{
    public ModifyResourceEffect(TagId resourceTag, int amount)
    {
        ResourceTag = resourceTag;
        Amount = amount;
    }

    public TagId ResourceTag { get; }

    public int Amount { get; }

    public void OnAcquired(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        actor.AddResource(ResourceTag, Amount);
    }

    public override AccessoryEffect Clone() => new ModifyResourceEffect(ResourceTag, Amount);
}
