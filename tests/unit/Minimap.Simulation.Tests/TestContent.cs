using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Shared test character/accessory definitions (Gun + ShootEffect).</summary>
internal static class TestContent
{
    public static AccessoryDefinition Gun { get; } = new(
        "gun",
        [
            new ShootEffect(
                fireIntervalSeconds: CombatTuning.FireIntervalSeconds,
                missileSpeed: CombatTuning.MissileSpeed,
                missileDamage: CombatTuning.MissileDamage),
        ]);

    public static CharacterDefinition Generic { get; } = new("generic", [Gun]);

    public static CharacterDefinition Bare { get; } = new("bare", Array.Empty<AccessoryDefinition>());

    public static GameContent Content { get; } = new(Generic);
}
