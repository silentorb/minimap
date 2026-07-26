using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Client.Profiles;

namespace Minimap.App;

/// <summary>Load/save durable player profiles under a user data path (not shipped core.json).</summary>
public static class PlayerProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };

    public static PlayerProfileCatalog LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var catalog = new PlayerProfileCatalog();
        if (!File.Exists(path))
            return catalog;

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
            return catalog;

        PlayerProfilesFile? file;
        try
        {
            file = JsonSerializer.Deserialize<PlayerProfilesFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse player profiles JSON.", ex);
        }

        if (file?.Profiles is null)
            throw new InvalidOperationException("Player profiles JSON must include a \"profiles\" array.");

        var seenIds = new HashSet<Guid>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var records = new List<PlayerProfileRecord>();
        foreach (var entry in file.Profiles)
        {
            if (entry is null)
                throw new InvalidOperationException("Player profiles JSON contains a null profile entry.");
            if (entry.Id == Guid.Empty)
                throw new InvalidOperationException("Each profile must have a non-empty id.");
            if (!seenIds.Add(entry.Id))
                throw new InvalidOperationException($"Duplicate profile id: {entry.Id}.");
            var name = (entry.Name ?? string.Empty).Trim();
            if (name.Length == 0)
                throw new InvalidOperationException($"Profile {entry.Id} has an empty name.");
            if (name.Length > PlayerProfileCatalog.MaxNameLength)
            {
                throw new InvalidOperationException(
                    $"Profile {entry.Id} name exceeds {PlayerProfileCatalog.MaxNameLength} characters.");
            }

            if (!seenNames.Add(name))
                throw new InvalidOperationException($"Duplicate profile name: {name}.");
            if (entry.Deaths < 0)
                throw new InvalidOperationException($"Profile {entry.Id} deaths must be >= 0.");

            var record = new PlayerProfileRecord
            {
                Id = entry.Id,
                Name = name,
                Deaths = entry.Deaths,
            };
            if (entry.UnlockedAchievements is { Count: > 0 })
                record.ReplaceUnlockedAchievements(entry.UnlockedAchievements);
            records.Add(record);
        }

        catalog.ReplaceAll(records);
        return catalog;
    }

    public static void SaveToFile(string path, PlayerProfileCatalog catalog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(catalog);

        var file = new PlayerProfilesFile
        {
            Profiles = catalog.Profiles
                .Select(p => new PlayerProfileFileEntry
                {
                    Id = p.Id,
                    Name = p.Name,
                    Deaths = p.Deaths,
                    UnlockedAchievements = p.UnlockedAchievements.Count == 0
                        ? null
                        : p.UnlockedAchievements.OrderBy(id => id, StringComparer.Ordinal).ToList(),
                })
                .ToList(),
        };

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(file, JsonOptions);
        File.WriteAllText(path, json);
    }

    private sealed class PlayerProfilesFile
    {
        [JsonPropertyName("profiles")]
        public List<PlayerProfileFileEntry>? Profiles { get; set; }
    }

    private sealed class PlayerProfileFileEntry
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("unlockedAchievements")]
        public List<string>? UnlockedAchievements { get; set; }
    }
}
