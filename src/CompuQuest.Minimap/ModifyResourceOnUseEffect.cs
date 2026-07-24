using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>On instant use: optionally consume a cost, then modify a resource.</summary>
public sealed class ModifyResourceOnUseEffect : AccessoryEffect, IInstantUseEffect, IEffectUseCost
{
    public ModifyResourceOnUseEffect(
        TagId resourceTag,
        int amount,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
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

    public bool TryUse(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (!EffectUseCosts.TryConsume(actor, this))
            return false;

        actor.AddResource(ResourceTag, Amount);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new ModifyResourceOnUseEffect(ResourceTag, Amount, CostResourceTag, CostAmount);
}
