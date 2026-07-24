using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Afford / consume helpers for per-effect use costs.</summary>
public static class EffectUseCosts
{
    public static bool CanAfford(Actor actor, AccessoryEffect effect)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(effect);

        if (effect is not IEffectUseCost cost ||
            cost.CostResourceTag is not { } tag ||
            cost.CostAmount <= 0)
        {
            return true;
        }

        return actor.GetResource(tag) >= cost.CostAmount;
    }

    public static bool TryConsume(Actor actor, AccessoryEffect effect)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(effect);

        if (effect is not IEffectUseCost cost ||
            cost.CostResourceTag is not { } tag ||
            cost.CostAmount <= 0)
        {
            return true;
        }

        return actor.TryConsumeResource(tag, cost.CostAmount);
    }
}
