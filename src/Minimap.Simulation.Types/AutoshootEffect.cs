namespace Minimap.Simulation.Types;

/// <summary>Autoshoot parameters and per-instance fire cooldown (docs/game/features/combat.md).</summary>
public sealed class AutoshootEffect : AccessoryEffect
{
    public AutoshootEffect(float fireIntervalSeconds, float missileSpeed, float missileDamage)
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
    }

    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public float MissileDamage { get; }
    public float CooldownRemaining { get; set; }

    public override AccessoryEffect Clone() =>
        new AutoshootEffect(FireIntervalSeconds, MissileSpeed, MissileDamage);
}
