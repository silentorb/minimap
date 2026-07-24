namespace Minimap.Simulation.Types;

/// <summary>
/// Contract for shootable accessory effects (cooldown + missile params).
/// Concrete implementations live in content extensions (e.g. CompuQuest).
/// </summary>
public interface IShootEffect
{
    float FireIntervalSeconds { get; }
    float MissileSpeed { get; }
    int MissileDamage { get; }
    bool FriendlyFire { get; }
    float CooldownRemaining { get; set; }
}
