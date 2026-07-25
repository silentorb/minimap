namespace Minimap.Simulation.Types;

/// <summary>
/// Contract for Swing accessory effects (cooldown + half-disk attack params).
/// Concrete implementations live in content extensions (e.g. CompuQuest).
/// </summary>
public interface ISwingEffect
{
    float FireIntervalSeconds { get; }
    int Damage { get; }
    float Radius { get; }
    float ArcDegrees { get; }
    float VisualDurationSeconds { get; }
    bool FriendlyFire { get; }
    float CooldownRemaining { get; set; }
}
