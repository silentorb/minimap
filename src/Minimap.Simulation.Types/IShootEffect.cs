namespace Minimap.Simulation.Types;

/// <summary>
/// Contract for shootable accessory effects (cooldown + missile params).
/// Concrete implementations live in content extensions (e.g. CompuQuest).
/// </summary>
public interface IShootEffect
{
    /// <summary>Actor definition id spawned as the projectile.</summary>
    string ProjectileActorId { get; }

    float FireIntervalSeconds { get; }
    float MissileSpeed { get; }
    int MissileDamage { get; }

    /// <summary>Max distance traveled (world units); independent of speed.</summary>
    float MissileRange { get; }

    /// <summary>Scalar on the projectile actor's base size (default 1).</summary>
    float MissileSizeScale { get; }

    bool FriendlyFire { get; }
    float CooldownRemaining { get; set; }
}
