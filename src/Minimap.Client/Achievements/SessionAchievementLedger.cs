namespace Minimap.Client.Achievements;

/// <summary>Per-player session earns (includes re-earns). No I/O.</summary>
public sealed class SessionAchievementLedger
{
    private readonly List<List<SessionAchievementEarn>> _byPlayer = new();

    public void EnsurePlayerCount(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        while (_byPlayer.Count < count)
            _byPlayer.Add(new List<SessionAchievementEarn>());
        while (_byPlayer.Count > count)
            _byPlayer.RemoveAt(_byPlayer.Count - 1);
    }

    public IReadOnlyList<SessionAchievementEarn> GetEarns(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _byPlayer.Count)
            return Array.Empty<SessionAchievementEarn>();
        return _byPlayer[playerIndex];
    }

    public int PlayerCount => _byPlayer.Count;

    /// <summary>Records an earn if this achievement id was not already earned by this player this session.</summary>
    public bool TryRecord(int playerIndex, string achievementId, bool firstTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId);
        if (playerIndex < 0 || playerIndex >= _byPlayer.Count)
            return false;

        var list = _byPlayer[playerIndex];
        foreach (var earn in list)
        {
            if (string.Equals(earn.AchievementId, achievementId, StringComparison.Ordinal))
                return false;
        }

        list.Add(new SessionAchievementEarn(achievementId, firstTime));
        return true;
    }

    public void Clear()
    {
        foreach (var list in _byPlayer)
            list.Clear();
        _byPlayer.Clear();
    }
}
