using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>
/// Environment interact on a placed computer while Geek is selected.
/// Gameplay beyond a successful interact is forthcoming.
/// </summary>
public sealed class UseComputerEffect : AccessoryEffect, IInteractionEffect, IEffectUseCost
{
    public const string ComputerActorId = "computer";

    public UseComputerEffect(TagId? costResourceTag = null, int costAmount = 0)
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

        return string.Equals(target.Definition.Id, ComputerActorId, StringComparison.Ordinal);
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);

        if (!CanInteract(world, actor, target))
            return false;

        EffectUseCosts.TryConsume(actor, this);
        return true;
    }

    public override AccessoryEffect Clone() => new UseComputerEffect(CostResourceTag, CostAmount);
}
