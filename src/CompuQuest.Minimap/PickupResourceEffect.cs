using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Default object interaction: remove the cell actor and grant a resource.</summary>
public sealed class PickupResourceEffect : AccessoryEffect, IDefaultInteractionEffect, IEffectUseCost
{
    public PickupResourceEffect(
        TagId resourceTag,
        int amount,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        ResourceTag = resourceTag;
        Amount = amount;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId ResourceTag { get; }

    public int Amount { get; }

    public TagId? CostResourceTag { get; }

    public int CostAmount { get; }

    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);
        if (target.Cell is null)
            return false;
        return EffectUseCosts.CanAfford(actor, this);
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
        EffectUseCosts.TryConsume(actor, this);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new PickupResourceEffect(ResourceTag, Amount, CostResourceTag, CostAmount);
}
