using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Functional.Tests;

/// <summary>Test double for <see cref="IShootEffect"/> (functional sim tests must not reference CompuQuest).</summary>
internal sealed class TestShootEffect : AccessoryEffect, IShootEffect
{
    public TestShootEffect(
        float fireIntervalSeconds,
        float missileSpeed,
        int missileDamage,
        bool friendlyFire = true,
        string projectileActorId = "missile",
        float missileRange = CombatTuning.MissileRange,
        float missileSizeScale = 1f)
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

        ProjectileActorId = projectileActorId;
        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        MissileRange = missileRange;
        MissileSizeScale = missileSizeScale;
        FriendlyFire = friendlyFire;
    }

    public string ProjectileActorId { get; }
    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public int MissileDamage { get; }
    public float MissileRange { get; }
    public float MissileSizeScale { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public override AccessoryEffect Clone() =>
        new TestShootEffect(
            FireIntervalSeconds,
            MissileSpeed,
            MissileDamage,
            FriendlyFire,
            ProjectileActorId,
            MissileRange,
            MissileSizeScale);
}
