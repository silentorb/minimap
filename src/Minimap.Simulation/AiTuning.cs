namespace Minimap.Simulation;

/// <summary>Shared AI aggression and action thresholds (docs/game/features/gameplay/ai.md).</summary>
public static class AiTuning
{
    /// <summary>Regular rival AI (zombies / farmers): roam with a pull toward significant goals.</summary>
    public const float DefaultAggression = 0.45f;

    /// <summary>Crazed carrot emerge AI: near-beeline with a little roam blend.</summary>
    public const float CrazedCarrotAggression = 0.9f;

    /// <summary>Flee eligibility when remaining health / max health is at or below this fraction.</summary>
    public const float SeriousInjuryHealthFraction = 0.3f;

    /// <summary>How long an injury flee goal lasts before normal goals resume.</summary>
    public const float FleeDurationSeconds = 3f;

    /// <summary>World radius in hexes: owned AI within this of the owner stay still (unless aggression pulls).</summary>
    public const float FollowStayRadiusHexes = 1f;

    /// <summary>Eat when missing at least this much energy (matches Eat grant of 5).</summary>
    public const int EatEnergyDeficitThreshold = 5;

    public const string FarmAccessoryId = "farm";

    public const string EatAccessoryId = "eat";

    public const string FoodResourceId = "food";

    public const string ZombieSpawnerActorId = "zombie_spawner";
}
