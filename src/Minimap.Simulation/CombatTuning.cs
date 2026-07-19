namespace Minimap.Simulation;

/// <summary>Documented combat / health defaults (docs/game/features).</summary>
public static class CombatTuning
{
    public const float DefaultMaxHealth = 100f;
    public const float MissileDamage = 25f;
    public const float MissileSpeed = 200f;
    public const float FireIntervalSeconds = 1.25f;
    public const float MoveSpeed = 120f;
}

/// <summary>Generic faction hostility (docs/game/features/factions.md).</summary>
public static class FactionRules
{
    public static bool AreHostile(int factionA, int factionB) => factionA != factionB;
}

/// <summary>Bootstrap-only spawn parameters (faction ids belong here, not in combat/AI helpers).</summary>
public sealed class SpawnConfig
{
    public int PlayerFactionId { get; init; } = 1;
    public int RivalFactionId { get; init; } = 2;
    public int AiPerFaction { get; init; } = 3;
}
