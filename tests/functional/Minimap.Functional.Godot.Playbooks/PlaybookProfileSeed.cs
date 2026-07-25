using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Minimap.Functional.Godot.Playbooks;

internal static class PlaybookProfileSeed
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void EnsureProfiles(params string[] names)
    {
        var path = ProjectSettings.GlobalizePath("user://player_profiles.json");
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var profiles = new List<ProfileEntry>();
        if (File.Exists(path))
        {
            try
            {
                var existing = JsonSerializer.Deserialize<ProfilesFile>(File.ReadAllText(path), JsonOptions);
                if (existing?.Profiles is not null)
                    profiles.AddRange(existing.Profiles.Where(p => p is not null)!);
            }
            catch (JsonException)
            {
                profiles.Clear();
            }
        }

        foreach (var name in names)
        {
            if (profiles.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                continue;
            profiles.Add(new ProfileEntry
            {
                Id = Guid.NewGuid(),
                Name = name,
                Deaths = 0,
            });
        }

        var json = JsonSerializer.Serialize(new ProfilesFile { Profiles = profiles }, JsonOptions);
        File.WriteAllText(path, json);
    }

    private sealed class ProfilesFile
    {
        [JsonPropertyName("profiles")]
        public List<ProfileEntry>? Profiles { get; set; }
    }

    private sealed class ProfileEntry
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }
    }
}
