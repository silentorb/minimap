using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Simulation;

namespace Minimap.App;

/// <summary>Shipped core bootstrap settings (distinct from a future user settings file).</summary>
public sealed class CoreSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new SimVec2IJsonConverter() },
    };

    public static CoreSettings Defaults { get; } = new()
    {
        Map = new CoreMapSettings { Radius = new SimVec2I(8, 6) },
    };

    [JsonPropertyName("map")]
    public required CoreMapSettings Map { get; init; }

    public static CoreSettings LoadFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        CoreSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<CoreSettings>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse core settings JSON.", ex);
        }

        if (settings?.Map is null)
            throw new InvalidOperationException("Core settings JSON must include a \"map\" object.");

        Validate(settings);
        return settings;
    }

    public static CoreSettings LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Core settings file not found: {path}", path);

        return LoadFromJson(File.ReadAllText(path));
    }

    private static void Validate(CoreSettings settings)
    {
        var radius = settings.Map.Radius;
        if (radius.X < 0 || radius.Y < 0)
        {
            throw new InvalidOperationException(
                $"map.radius components must be >= 0 (got [{radius.X}, {radius.Y}]).");
        }
    }
}

public sealed class CoreMapSettings
{
    [JsonPropertyName("radius")]
    [JsonConverter(typeof(SimVec2IJsonConverter))]
    public required SimVec2I Radius { get; init; }
}
