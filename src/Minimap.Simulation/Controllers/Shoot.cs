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

    public static IShootEffect? FindShootEffect(Character shooter)
    {
        foreach (var effect in shooter.Effects)
        {
            if (effect is IShootEffect shoot)
                return shoot;
        }

        return null;
    }

    /// <summary>
    /// Decrements cooldown on the character's <see cref="IShootEffect"/>; when ready and
    /// <paramref name="fireDirection"/> is non-zero, spawns a missile in that direction.
    /// </summary>
    public static void Tick(GameWorld world, Character shooter, float dt, SimVec2 fireDirection)
    {
        var effect = FindShootEffect(shooter);
        if (effect is null)
            return;

        effect.CooldownRemaining -= dt;
        if (effect.CooldownRemaining > 0f)
            return;

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
    }
}
