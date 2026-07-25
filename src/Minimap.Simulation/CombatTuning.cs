namespace Minimap.Simulation;

/// <summary>Documented combat / health / energy defaults (docs/game/features).</summary>
public static class CombatTuning
{
    public const int DefaultMaxHealth = 100;
    public const int DefaultMaxEnergy = 100;
    public const int MissileDamage = 25;
    public const float MissileSpeed = 200f;
    public const float FireIntervalSeconds = 1.25f;
    public const int SwingDamage = 30;
    public const float SwingIntervalSeconds = 0.8f;
    public const float SwingArcDegrees = 180f;
    public const float SwingVisualDurationSeconds = 0.15f;
    public const float MoveSpeed = 120f;
}

/// <summary>Generic faction hostility (docs/game/features/gameplay/factions.md).</summary>
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

    /// <summary>Unpossessed human-faction pawns to spawn (Client attaches PlayerControllers).</summary>
    public int HumanPlayerCount { get; init; } = 1;
}
