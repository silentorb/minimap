using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test double for CompuQuest <c>heal</c>.</summary>
internal sealed class TestHealEffect : AccessoryEffect, IInstantUseEffect, IInteractionEffect, IEffectUseCost
{
    private readonly TagId _humanTag;
    private readonly TagId _animalTag;

    public TestHealEffect(
        TagId humanTag,
        TagId animalTag,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        _humanTag = humanTag;
        _animalTag = animalTag;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool TryUse(Actor actor)
    {
        if (!CanHealTarget(actor) || !EffectUseCosts.CanAfford(actor, this))
            return false;
        if (!EffectUseCosts.TryConsume(actor, this))
            return false;
        actor.Health = actor.MaxHealth;
        return true;
    }

    public bool CanInteract(GameWorld world, Actor actor, Actor target) =>
        CanHealTarget(target) && EffectUseCosts.CanAfford(actor, this);

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!CanInteract(world, actor, target))
            return false;
        if (!EffectUseCosts.TryConsume(actor, this))
            return false;
        target.Health = target.MaxHealth;
        return true;
    }

    public override AccessoryEffect Clone() =>
        new TestHealEffect(_humanTag, _animalTag, CostResourceTag, CostAmount);

    private bool CanHealTarget(Actor target)
    {
        if (!target.IsAlive || !target.IsDestructible)
            return false;
        if (target.Health >= target.MaxHealth)
            return false;
        return target.Definition.HasTag(_humanTag) || target.Definition.HasTag(_animalTag);
    }
}
