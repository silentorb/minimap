using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Shared shoot helper: cooldown on IShootEffect; fire direction from controller (docs/game/features/combat.md).</summary>
public static class Shoot
{
    public static Character? FindNearestHostile(Character shooter, IReadOnlyList<Character> characters)
    {
        Character? best = null;
        var bestDistSq = float.MaxValue;
        foreach (var other in characters)
        {
            if (other.Id == shooter.Id || !other.IsAlive)
                continue;
            if (!FactionRules.AreHostile(shooter.FactionId, other.FactionId))
                continue;
            var d = other.Position - shooter.Position;
            var distSq = d.LengthSquared;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = other;
            }
        }

        return best;
    }

    public static AccessoryEffect? FindShootEffectInstance(Character shooter)
    {
        foreach (var effect in shooter.Effects)
        {
            if (effect is IShootEffect)
                return effect;
        }

        return null;
    }

    public static IShootEffect? FindShootEffect(Character shooter) =>
        FindShootEffectInstance(shooter) as IShootEffect;

    /// <summary>
    /// Decrements cooldown on the character's <see cref="IShootEffect"/>; when ready,
    /// <paramref name="wantsFire"/> is true, and a fire direction is available
    /// (aim, else facing), spawns a missile.
    /// </summary>
    public static void Tick(
        GameWorld world,
        Character shooter,
        float dt,
        SimVec2 aimDirection,
        bool wantsFire)
    {
        var effectInstance = FindShootEffectInstance(shooter);
        if (effectInstance is not IShootEffect effect)
            return;

        effect.CooldownRemaining -= dt;
        if (effect.CooldownRemaining > 0f)
            return;

        if (!wantsFire)
            return;

        if (!EffectUseCosts.CanAfford(shooter, effectInstance))
            return;

        var fireDirection = aimDirection;
        if (fireDirection.LengthSquared < 1e-10f)
            fireDirection = shooter.Facing;
        if (fireDirection.LengthSquared < 1e-10f)
            return;

        var dir = fireDirection.Normalized();
        world.SpawnMissile(
            shooter.Position,
            dir * effect.MissileSpeed,
            effect.MissileDamage,
            shooter.FactionId,
            shooter.Id,
            effect.FriendlyFire);
        effect.CooldownRemaining = effect.FireIntervalSeconds;

        EffectUseCosts.TryConsume(shooter, effectInstance);
    }
}
