using System.Text.Json;
using System.Text.Json.Serialization;

namespace Minimap.App;

/// <summary>Shipped settings for extension search paths, libraries, and active integrator.</summary>
public sealed class ExtensionsSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static ExtensionsSettings Defaults { get; } = new()
    {
        SearchPaths = new List<string> { "extensions" },
        Extensions = new List<string>(),
        Integrator = "compuquest",
    };

    [JsonPropertyName("searchPaths")]
    public required List<string> SearchPaths { get; init; }

    [JsonPropertyName("extensions")]
    public required List<string> Extensions { get; init; }

    [JsonPropertyName("integrator")]
    public required string Integrator { get; init; }

    public static ExtensionsSettings LoadFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        ExtensionsSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<ExtensionsSettings>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse extensions settings JSON.", ex);
        }

        if (settings is null)
            throw new InvalidOperationException("Extensions settings JSON deserialized to null.");

        Validate(settings);
        return settings;
    }

    public static ExtensionsSettings LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Extensions settings file not found: {path}", path);

        return LoadFromJson(File.ReadAllText(path));
    }

    private static void Validate(ExtensionsSettings settings)
    {
        if (settings.SearchPaths is null)
            throw new InvalidOperationException("Extensions settings JSON must include \"searchPaths\".");
        if (settings.Extensions is null)
            throw new InvalidOperationException("Extensions settings JSON must include \"extensions\".");
        if (string.IsNullOrWhiteSpace(settings.Integrator))
            throw new InvalidOperationException("Extensions settings JSON must include a non-empty \"integrator\".");

        for (var i = 0; i < settings.SearchPaths.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(settings.SearchPaths[i]))
            {
                throw new InvalidOperationException(
                    $"searchPaths[{i}] must be a non-empty string.");
            }
        }

        for (var i = 0; i < settings.Extensions.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(settings.Extensions[i]))
            {
                throw new InvalidOperationException(
                    $"extensions[{i}] must be a non-empty string.");
            }
        }
    }
}
