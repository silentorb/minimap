using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Gun shoot parameters and per-instance fire cooldown.</summary>
public sealed class ShootEffect : AccessoryEffect, IShootEffect, IEffectUseCost
{
    public ShootEffect(
        float fireIntervalSeconds,
        float missileSpeed,
        int missileDamage,
        bool friendlyFire = true,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        if (fireIntervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireIntervalSeconds));
        if (missileSpeed <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileSpeed));
        if (missileDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(missileDamage));
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        FriendlyFire = friendlyFire;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public int MissileDamage { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public override AccessoryEffect Clone() =>
        new ShootEffect(
            FireIntervalSeconds,
            MissileSpeed,
            MissileDamage,
            FriendlyFire,
            CostResourceTag,
            CostAmount);
}
