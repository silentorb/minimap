namespace Minimap.Client.Profiles;

/// <summary>In-memory profile list with create/rename/delete/death mutations (no I/O).</summary>
public sealed class PlayerProfileCatalog
{
    public const int MaxNameLength = 24;

    private readonly List<PlayerProfileRecord> _profiles = new();

    public IReadOnlyList<PlayerProfileRecord> Profiles => _profiles;

    public void Clear() => _profiles.Clear();

    public void ReplaceAll(IEnumerable<PlayerProfileRecord> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        _profiles.Clear();
        foreach (var p in profiles)
        {
            ArgumentNullException.ThrowIfNull(p);
            _profiles.Add(p);
        }
    }

    public PlayerProfileRecord? Find(Guid id)
    {
        foreach (var p in _profiles)
        {
            if (p.Id == id)
                return p;
        }

        return null;
    }

    public bool TryCreate(string name, out PlayerProfileRecord? profile, out string? error)
    {
        profile = null;
        if (!TryNormalizeName(name, excludeId: null, out var normalized, out error))
            return false;

        profile = new PlayerProfileRecord
        {
            Id = Guid.NewGuid(),
            Name = normalized,
            Deaths = 0,
        };
        _profiles.Add(profile);
        return true;
    }

    public bool TryRename(Guid id, string name, out string? error)
    {
        var existing = Find(id);
        if (existing is null)
        {
            error = "Profile not found.";
            return false;
        }

        if (!TryNormalizeName(name, excludeId: id, out var normalized, out error))
            return false;

        existing.Name = normalized;
        return true;
    }

    public bool TryDelete(Guid id)
    {
        for (var i = 0; i < _profiles.Count; i++)
        {
            if (_profiles[i].Id != id)
                continue;
            _profiles.RemoveAt(i);
            return true;
        }

        return false;
    }

    public bool TryIncrementDeaths(Guid id)
    {
        var existing = Find(id);
        if (existing is null)
            return false;
        existing.Deaths++;
        return true;
    }

    /// <summary>Unlocks an achievement on the profile. Returns false if missing profile or already unlocked.</summary>
    public bool TryUnlockAchievement(Guid id, string achievementId)
    {
        var existing = Find(id);
        if (existing is null)
            return false;
        return existing.TryUnlockAchievement(achievementId);
    }

    private bool TryNormalizeName(string name, Guid? excludeId, out string normalized, out string? error)
    {
        normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            error = "Name cannot be empty.";
            return false;
        }

        if (normalized.Length > MaxNameLength)
        {
            error = $"Name must be at most {MaxNameLength} characters.";
            return false;
        }

        foreach (var p in _profiles)
        {
            if (excludeId is Guid ex && p.Id == ex)
                continue;
            if (string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                error = "A profile with that name already exists.";
                return false;
            }
        }

        error = null;
        return true;
    }
}
