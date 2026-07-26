namespace Minimap.Client.Profiles;

/// <summary>One durable local user profile (identity + stats + achievements).</summary>
public sealed class PlayerProfileRecord
{
    private readonly HashSet<string> _unlockedAchievements = new(StringComparer.Ordinal);

    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public int Deaths { get; set; }

    public IReadOnlyCollection<string> UnlockedAchievements => _unlockedAchievements;

    public bool HasAchievement(string achievementId) =>
        !string.IsNullOrEmpty(achievementId) && _unlockedAchievements.Contains(achievementId);

    public bool TryUnlockAchievement(string achievementId)
    {
        if (string.IsNullOrWhiteSpace(achievementId))
            return false;
        return _unlockedAchievements.Add(achievementId);
    }

    public void ReplaceUnlockedAchievements(IEnumerable<string> achievementIds)
    {
        ArgumentNullException.ThrowIfNull(achievementIds);
        _unlockedAchievements.Clear();
        foreach (var id in achievementIds)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _unlockedAchievements.Add(id);
        }
    }
}
