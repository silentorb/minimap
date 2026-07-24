using Minimap.Simulation.Types;

namespace Minimap.Functional.Tests;

/// <summary>Test double for <see cref="IShootEffect"/> (functional sim tests must not reference CompuQuest).</summary>
internal sealed class TestShootEffect : AccessoryEffect, IShootEffect
{
    public TestShootEffect(
        float fireIntervalSeconds,
        float missileSpeed,
        int missileDamage,
        bool friendlyFire = true)
    {
        if (fireIntervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireIntervalSeconds));
        if (missileSpeed <= 0f)
            throw new ArgumentOutOfRangeException(nameof(missileSpeed));
        if (missileDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(missileDamage));

        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        FriendlyFire = friendlyFire;
    }

    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public int MissileDamage { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public override AccessoryEffect Clone() =>
        new TestShootEffect(FireIntervalSeconds, MissileSpeed, MissileDamage, FriendlyFire);
}
