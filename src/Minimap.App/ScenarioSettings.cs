using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Simulation;

namespace Minimap.App;

/// <summary>Loads <see cref="Scenario"/> from JSON files.</summary>
public static class ScenarioSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static Scenario LoadFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        ScenarioFile? file;
        try
        {
            file = JsonSerializer.Deserialize<ScenarioFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse scenario settings JSON.", ex);
        }

        if (file is null)
            throw new InvalidOperationException("Scenario settings JSON must be a non-empty object.");

        var scenario = new Scenario
        {
            PreparationDuration = file.PreparationDuration
                ?? throw new InvalidOperationException("Scenario JSON must include preparationDuration."),
            WaveCount = file.WaveCount
                ?? throw new InvalidOperationException("Scenario JSON must include waveCount."),
            WaveDuration = file.WaveDuration
                ?? throw new InvalidOperationException("Scenario JSON must include waveDuration."),
            SpawnerCount = file.SpawnerCount
                ?? throw new InvalidOperationException("Scenario JSON must include spawnerCount."),
            SpawnerVolume = file.SpawnerVolume
                ?? throw new InvalidOperationException("Scenario JSON must include spawnerVolume."),
        };

        Scenario.Validate(scenario);
        return scenario;
    }

    public static Scenario LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Scenario settings file not found: {path}", path);

        return LoadFromJson(File.ReadAllText(path));
    }

    private sealed class ScenarioFile
    {
        [JsonPropertyName("preparationDuration")]
        public float? PreparationDuration { get; init; }

        [JsonPropertyName("waveCount")]
        public int? WaveCount { get; init; }

        [JsonPropertyName("waveDuration")]
        public float? WaveDuration { get; init; }

        [JsonPropertyName("spawnerCount")]
        public int? SpawnerCount { get; init; }

        [JsonPropertyName("spawnerVolume")]
        public int? SpawnerVolume { get; init; }
    }
}
