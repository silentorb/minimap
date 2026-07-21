using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Gun shoot parameters and per-instance fire cooldown (docs/game/features/combat.md).</summary>
public sealed class ShootEffect : AccessoryEffect, IShootEffect
{
    public ShootEffect(
        float fireIntervalSeconds,
        float missileSpeed,
        float missileDamage,
        bool friendlyFire = true)
    {
        if (fireIntervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireIntervalSeconds));
        if (missileSpeed <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileSpeed));
        if (missileDamage < 0f)
            throw new ArgumentOutOfRangeException(nameof(missileDamage));

        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        FriendlyFire = friendlyFire;
    }

    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public float MissileDamage { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public override AccessoryEffect Clone() =>
        new ShootEffect(FireIntervalSeconds, MissileSpeed, MissileDamage, FriendlyFire);
}
