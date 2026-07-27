using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Gun shoot parameters and per-instance fire cooldown; auto-fires on cell actors.</summary>
public sealed class ShootEffect : AccessoryEffect, IShootEffect, IEffectUseCost, IWorldPassiveEffect
{
    public ShootEffect(
        string projectileActorId,
        float fireIntervalSeconds,
        float missileSpeed,
        int missileDamage,
        float missileRange,
        float missileSizeScale = 1f,
        bool friendlyFire = true,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        if (string.IsNullOrWhiteSpace(projectileActorId))
            throw new ArgumentException("Projectile actor id must be non-empty.", nameof(projectileActorId));
        if (fireIntervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireIntervalSeconds));
        if (missileSpeed <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileSpeed));
        if (missileDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(missileDamage));
        if (missileRange <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileRange));
        if (missileSizeScale <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileSizeScale));
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        ProjectileActorId = projectileActorId;
        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        MissileRange = missileRange;
        MissileSizeScale = missileSizeScale;
        FriendlyFire = friendlyFire;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public string ProjectileActorId { get; }
    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public int MissileDamage { get; }
    public float MissileRange { get; }
    public float MissileSizeScale { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.Cell is null)
            return;
        Shoot.TickCellActorAutoFire(world, actor, dt);
    }

    public override AccessoryEffect Clone() =>
        new ShootEffect(
            ProjectileActorId,
            FireIntervalSeconds,
            MissileSpeed,
            MissileDamage,
            MissileRange,
            MissileSizeScale,
            FriendlyFire,
            CostResourceTag,
            CostAmount);
}
