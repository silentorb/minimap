using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Default object interaction: remove the cell actor and grant a resource.</summary>
public sealed class PickupResourceEffect : AccessoryEffect, IDefaultInteractionEffect
{
    public PickupResourceEffect(TagId resourceTag, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        ResourceTag = resourceTag;
        Amount = amount;
    }

    public TagId ResourceTag { get; }

    public int Amount { get; }

    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);
        return target.Cell is not null;
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        if (!CanInteract(world, actor, target))
            return false;
        if (target.Cell is not { } cell)
            return false;
        if (!world.TryRemoveActorAt(cell, out _))
            return false;

        actor.AddResource(ResourceTag, Amount);
        return true;
    }

    public override AccessoryEffect Clone() => new PickupResourceEffect(ResourceTag, Amount);
}
