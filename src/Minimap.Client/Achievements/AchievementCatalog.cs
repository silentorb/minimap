namespace Minimap.Client.Achievements;

/// <summary>Static catalog of known achievements.</summary>
public static class AchievementCatalog
{
    private static readonly AchievementDefinition[] All =
    [
        new AchievementDefinition
        {
            Id = AchievementIds.Survive5Minutes,
            Title = "Survive 5 minutes",
            Description = "Stay alive for five consecutive minutes in a single session.",
        },
    ];

    public static IReadOnlyList<AchievementDefinition> Definitions => All;

    public static AchievementDefinition? Find(string id)
    {
        foreach (var def in All)
        {
            if (string.Equals(def.Id, id, StringComparison.Ordinal))
                return def;
        }

        return null;
    }
}
