using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Harvests a mature food actor, granting yield or emerging an ambush character.</summary>
public sealed class HarvestEffect : AccessoryEffect, IInteractionEffect, IEffectUseCost
{
    public HarvestEffect(TagId? costResourceTag = null, int costAmount = 0)
    {
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        if (!EffectUseCosts.CanAfford(actor, this))
            return false;

        return FindMatureGrow(target) is not null;
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        if (!CanInteract(world, actor, target))
            return false;

        var grow = FindMatureGrow(target);
        if (grow is null)
            return false;

        if (target.Cell is not { } cell)
            return false;

        if (!string.IsNullOrWhiteSpace(grow.EmergeActorId))
        {
            if (!grow.TryEmerge(world, target))
                return false;
            EffectUseCosts.TryConsume(actor, this);
            return true;
        }

        if (!world.TryRemoveActorAt(cell, out _))
            return false;

        if (grow.YieldResourceTag is { } yieldTag && grow.YieldAmount > 0)
            actor.AddResource(yieldTag, grow.YieldAmount);

        EffectUseCosts.TryConsume(actor, this);
        return true;
    }

    private static IGrowEffect? FindMatureGrow(Actor target)
    {
        foreach (var effect in target.Effects)
        {
            if (effect is IGrowEffect grow && grow.IsMature)
                return grow;
        }

        return null;
    }

    public override AccessoryEffect Clone() => new HarvestEffect(CostResourceTag, CostAmount);
}
