using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Shared shoot helper: cooldown on IShootEffect; fire direction from controller or auto-aim (docs/game/features/gameplay/combat.md).</summary>
public static class Shoot
{
    public static Actor? FindNearestHostile(
        int ownerFactionId,
        SimVec2 origin,
        int ownerActorId,
        IReadOnlyList<Actor> characters)
    {
        Actor? best = null;
        var bestDistSq = float.MaxValue;
        foreach (var other in characters)
        {
            if (other.Id == ownerActorId || !other.IsAlive || other.IsProjectile)
                continue;
            if (!FactionRules.AreHostile(ownerFactionId, other.FactionId))
                continue;
            var d = other.Position - origin;
            var distSq = d.LengthSquared;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = other;
            }
        }

        return best;
    }

    public static Actor? FindNearestHostile(Actor shooter, IReadOnlyList<Actor> characters) =>
        FindNearestHostile(shooter.FactionId, shooter.Position, shooter.Id, characters);

    public static AccessoryEffect? FindShootEffectInstance(Actor shooter)
    {
        ArgumentNullException.ThrowIfNull(shooter);
        foreach (var effect in shooter.Effects)
        {
            if (effect is IShootEffect)
                return effect;
        }

        return null;
    }

    public static IShootEffect? FindShootEffect(Actor shooter) =>
        FindShootEffectInstance(shooter) as IShootEffect;

    /// <summary>
    /// Decrements cooldown on the actor's <see cref="IShootEffect"/>; when ready,
    /// <paramref name="wantsFire"/> is true, and a fire direction is available
    /// (aim, else facing), spawns a projectile actor from <paramref name="origin"/>.
    /// </summary>
    public static void Tick(
        GameWorld world,
        Actor shooter,
        SimVec2 origin,
        float dt,
        SimVec2 aimDirection,
        bool wantsFire)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(shooter);

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

        if (!world.TryGetActorDefinition(effect.ProjectileActorId, out var projectileDef) ||
            projectileDef is null ||
            projectileDef.Size is not float baseSize)
        {
            return;
        }

        var dir = fireDirection.Normalized();
        var projectile = world.AddActor(shooter.FactionId, origin, projectileDef);
        projectile.Facing = dir;
        projectile.Projectile = new ProjectileFlight(
            dir * effect.MissileSpeed,
            effect.MissileDamage,
            effect.FriendlyFire,
            shooter.Id,
            baseSize * effect.MissileSizeScale,
            effect.MissileRange,
            origin);

        effect.CooldownRemaining = effect.FireIntervalSeconds;
        EffectUseCosts.TryConsume(shooter, effectInstance);
    }

    /// <summary>Actor convenience overload: fires from <see cref="Actor.Position"/>.</summary>
    public static void Tick(
        GameWorld world,
        Actor shooter,
        float dt,
        SimVec2 aimDirection,
        bool wantsFire) =>
        Tick(world, shooter, shooter.Position, dt, aimDirection, wantsFire);

    /// <summary>
    /// Cell-actor auto-fire: aim at nearest hostile from the cell center; fire when a target exists.
    /// </summary>
    public static void TickCellActorAutoFire(GameWorld world, Actor shooter, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(shooter);
        if (shooter.Cell is not HexAxial cell || !shooter.IsAlive)
            return;
        if (FindShootEffectInstance(shooter) is null)
            return;

        var origin = HexWorldLayout.ToWorld(cell, world.HexSize);
        var target = FindNearestHostile(shooter.FactionId, origin, shooter.Id, world.Actors);
        if (target is null)
        {
            Tick(world, shooter, origin, dt, SimVec2.Zero, wantsFire: false);
            return;
        }

        var aim = target.Position - origin;
        if (aim.LengthSquared >= 1e-10f)
            shooter.Facing = aim.Normalized();
        Tick(world, shooter, origin, dt, aim, wantsFire: true);
    }
}
