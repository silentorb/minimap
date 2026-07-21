using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Shared nearest-hostile autoshoot (docs/game/features/combat.md).</summary>
public static class Autoshoot
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

    public static AutoshootEffect? FindAutoshootEffect(Character shooter)
    {
        foreach (var effect in shooter.Effects)
        {
            if (effect is AutoshootEffect autoshoot)
                return autoshoot;
        }

        return null;
    }

    /// <summary>Decrements cooldown on the character's AutoshootEffect; when ready and a hostile exists, spawns a missile.</summary>
    public static void Tick(GameWorld world, Character shooter, float dt)
    {
        var effect = FindAutoshootEffect(shooter);
        if (effect is null)
            return;

        effect.CooldownRemaining -= dt;
        if (effect.CooldownRemaining > 0f)
            return;

        var target = FindNearestHostile(shooter, world.Characters);
        if (target is null)
            return;

        var dir = (target.Position - shooter.Position).Normalized();
        if (dir.LengthSquared < 1e-10f)
            return;

        world.SpawnMissile(
            shooter.Position,
            dir * effect.MissileSpeed,
            effect.MissileDamage,
            shooter.FactionId,
            shooter.Id);
        effect.CooldownRemaining = effect.FireIntervalSeconds;
    }
}
