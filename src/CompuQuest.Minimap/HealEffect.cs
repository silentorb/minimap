using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>
/// Instant self-heal and interact heal: restore injured human/animal targets to max health.
/// </summary>
public sealed class HealEffect : AccessoryEffect, IInstantUseEffect, IInteractionEffect, IEffectUseCost
{
    public const string HumanTagName = "human";
    public const string AnimalTagName = "animal";

    public HealEffect(
        TagId humanTag,
        TagId animalTag,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        HumanTag = humanTag;
        AnimalTag = animalTag;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId HumanTag { get; }

    public TagId AnimalTag { get; }

    public TagId? CostResourceTag { get; }

    public int CostAmount { get; }

    public bool TryUse(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (!CanHealTarget(actor) || !EffectUseCosts.CanAfford(actor, this))
            return false;

        if (!EffectUseCosts.TryConsume(actor, this))
            return false;

        actor.Health = actor.MaxHealth;
        return true;
    }

    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        return CanHealTarget(target) && EffectUseCosts.CanAfford(actor, this);
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        if (!CanInteract(world, actor, target))
            return false;

        if (!EffectUseCosts.TryConsume(actor, this))
            return false;

        target.Health = target.MaxHealth;
        return true;
    }

    public override AccessoryEffect Clone() =>
        new HealEffect(HumanTag, AnimalTag, CostResourceTag, CostAmount);

    private bool CanHealTarget(Actor target)
    {
        if (!target.IsAlive || !target.IsDestructible)
            return false;
        if (target.Health >= target.MaxHealth)
            return false;
        return target.Definition.HasTag(HumanTag) || target.Definition.HasTag(AnimalTag);
    }
}
