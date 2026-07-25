using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Shared Swing helper: cooldown on ISwingEffect; attack direction from controller.</summary>
public static class Swing
{
    public static AccessoryEffect? FindSwingEffectInstance(Character attacker)
    {
        foreach (var effect in attacker.Effects)
        {
            if (effect is ISwingEffect)
                return effect;
        }

        return null;
    }

    public static ISwingEffect? FindSwingEffect(Character attacker) =>
        FindSwingEffectInstance(attacker) as ISwingEffect;

    /// <summary>
    /// Decrements cooldown on the character's <see cref="ISwingEffect"/>; when ready,
    /// <paramref name="wantsSwing"/> is true, and a direction is available
    /// (aim, else facing), spawns a Swing arc and resolves hits once.
    /// </summary>
    public static void Tick(
        GameWorld world,
        Character attacker,
        float dt,
        SimVec2 aimDirection,
        bool wantsSwing)
    {
        var effectInstance = FindSwingEffectInstance(attacker);
        if (effectInstance is not ISwingEffect effect)
            return;

        effect.CooldownRemaining -= dt;
        if (effect.CooldownRemaining > 0f)
            return;

        if (!wantsSwing)
            return;

        if (!EffectUseCosts.CanAfford(attacker, effectInstance))
            return;

        var swingDirection = aimDirection;
        if (swingDirection.LengthSquared < 1e-10f)
            swingDirection = attacker.Facing;
        if (swingDirection.LengthSquared < 1e-10f)
            return;

        var facing = swingDirection.Normalized();
        var radius = effect.Radius > 0f ? effect.Radius : world.HexSize;
        world.SpawnSwingArc(
            attacker.Position,
            facing,
            radius,
            effect.ArcDegrees,
            effect.Damage,
            attacker.FactionId,
            attacker.Id,
            effect.FriendlyFire,
            effect.VisualDurationSeconds);
        effect.CooldownRemaining = effect.FireIntervalSeconds;

        EffectUseCosts.TryConsume(attacker, effectInstance);
    }

    /// <summary>
    /// True when <paramref name="point"/> lies in the half-disk (or arc) ahead of
    /// <paramref name="origin"/> facing <paramref name="facing"/>.
    /// </summary>
    public static bool IsPointInArc(
        SimVec2 origin,
        SimVec2 facing,
        float radius,
        float arcDegrees,
        SimVec2 point,
        float targetRadius = 0f)
    {
        if (radius <= 0f)
            return false;

        var delta = point - origin;
        var reach = radius + Math.Max(0f, targetRadius);
        if (delta.LengthSquared > reach * reach)
            return false;

        if (delta.LengthSquared < 1e-10f)
            return true;

        var dir = facing.LengthSquared >= 1e-10f ? facing.Normalized() : new SimVec2(1f, 0f);
        var toTarget = delta.Normalized();
        var cosHalf = MathF.Cos(arcDegrees * 0.5f * (MathF.PI / 180f));
        return SimVec2.Dot(dir, toTarget) >= cosHalf - 1e-5f;
    }
}
