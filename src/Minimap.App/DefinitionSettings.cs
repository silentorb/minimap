using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Loads accessory and character definitions from JSON under config directories.</summary>
public static class DefinitionSettings
{
    public const string AccessoriesDirectoryName = "accessories";
    public const string CharactersDirectoryName = "characters";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<AccessoryDefinition> LoadAccessoriesFromDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if (!Directory.Exists(directory))
            return Array.Empty<AccessoryDefinition>();

        var definitions = new List<AccessoryDefinition>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").OrderBy(p => p, StringComparer.Ordinal))
            definitions.Add(LoadAccessoryFromFile(path));

        return definitions;
    }

    public static AccessoryDefinition LoadAccessoryFromJson(string json, string? sourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        AccessoryFile? file;
        try
        {
            file = JsonSerializer.Deserialize<AccessoryFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                FormatParseError("accessory", sourcePath), ex);
        }

        if (file is null)
            throw new InvalidOperationException(
                FormatEmptyObjectError("accessory", sourcePath));

        if (string.IsNullOrWhiteSpace(file.Id))
            throw new InvalidOperationException(
                FormatRequiredFieldError("accessory", "id", sourcePath));

        if (file.Effects is null)
            throw new InvalidOperationException(
                FormatRequiredFieldError("accessory", "effects", sourcePath));

        var effects = new List<AccessoryEffect>();
        for (var i = 0; i < file.Effects.Count; i++)
            effects.Add(ParseEffect(file.Effects[i], i, sourcePath));

        return new AccessoryDefinition(file.Id, effects);
    }

    public static AccessoryDefinition LoadAccessoryFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Accessory definition file not found: {path}", path);

        return LoadAccessoryFromJson(File.ReadAllText(path), path);
    }

    public static IReadOnlyList<CharacterDefinition> LoadCharactersFromDirectory(
        string directory,
        IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(registry);

        if (!Directory.Exists(directory))
            return Array.Empty<CharacterDefinition>();

        var byId = registry.AccessoryDefinitions.ToDictionary(d => d.Id, StringComparer.Ordinal);
        var definitions = new List<CharacterDefinition>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").OrderBy(p => p, StringComparer.Ordinal))
            definitions.Add(LoadCharacterFromFile(path, byId));

        return definitions;
    }

    public static CharacterDefinition LoadCharacterFromJson(
        string json,
        IReadOnlyDictionary<string, AccessoryDefinition> accessoriesById,
        string? sourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(accessoriesById);

        CharacterFile? file;
        try
        {
            file = JsonSerializer.Deserialize<CharacterFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                FormatParseError("character", sourcePath), ex);
        }

        if (file is null)
            throw new InvalidOperationException(
                FormatEmptyObjectError("character", sourcePath));

        if (string.IsNullOrWhiteSpace(file.Id))
            throw new InvalidOperationException(
                FormatRequiredFieldError("character", "id", sourcePath));

        if (file.Accessories is null)
            throw new InvalidOperationException(
                FormatRequiredFieldError("character", "accessories", sourcePath));

        var accessories = new List<AccessoryDefinition>();
        foreach (var accessoryId in file.Accessories)
        {
            if (string.IsNullOrWhiteSpace(accessoryId))
            {
                throw new InvalidOperationException(
                    AppendSource(
                        "Character definition accessories must be non-empty ids.",
                        sourcePath));
            }

            if (!accessoriesById.TryGetValue(accessoryId, out var accessory))
            {
                throw new InvalidOperationException(
                    AppendSource(
                        $"Character definition '{file.Id}' references unknown accessory '{accessoryId}'.",
                        sourcePath));
            }

            accessories.Add(accessory);
        }

        return new CharacterDefinition(file.Id, accessories);
    }

    public static CharacterDefinition LoadCharacterFromFile(
        string path,
        IReadOnlyDictionary<string, AccessoryDefinition> accessoriesById)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(accessoriesById);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Character definition file not found: {path}", path);

        return LoadCharacterFromJson(File.ReadAllText(path), accessoriesById, path);
    }

    /// <summary>
    /// Registers JSON definitions from <paramref name="configDirectory"/> into
    /// <paramref name="registry"/> (accessories, then characters).
    /// </summary>
    public static void RegisterFromConfigDirectory(string configDirectory, IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);
        ArgumentNullException.ThrowIfNull(registry);

        var accessoriesDir = Path.Combine(configDirectory, AccessoriesDirectoryName);
        foreach (var accessory in LoadAccessoriesFromDirectory(accessoriesDir))
            registry.AddAccessoryDefinition(accessory);

        var charactersDir = Path.Combine(configDirectory, CharactersDirectoryName);
        foreach (var character in LoadCharactersFromDirectory(charactersDir, registry))
            registry.AddCharacterDefinition(character);
    }

    private static AccessoryEffect ParseEffect(EffectFile effect, int index, string? sourcePath)
    {
        if (effect.Type is null || string.IsNullOrWhiteSpace(effect.Type))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} must include type.",
                    sourcePath));
        }

        return effect.Type.Trim().ToLowerInvariant() switch
        {
            "shoot" => ParseShootEffect(effect, index, sourcePath),
            _ => throw new InvalidOperationException(
                AppendSource(
                    $"Unknown accessory effect type '{effect.Type}' at index {index}.",
                    sourcePath)),
        };
    }

    private static ShootEffect ParseShootEffect(EffectFile effect, int index, string? sourcePath)
    {
        if (effect.FireIntervalSeconds is null)
            throw MissingEffectField("shoot", "fireIntervalSeconds", index, sourcePath);
        if (effect.MissileSpeed is null)
            throw MissingEffectField("shoot", "missileSpeed", index, sourcePath);
        if (effect.MissileDamage is null)
            throw MissingEffectField("shoot", "missileDamage", index, sourcePath);

        try
        {
            return new ShootEffect(
                effect.FireIntervalSeconds.Value,
                effect.MissileSpeed.Value,
                effect.MissileDamage.Value,
                effect.FriendlyFire ?? true);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Invalid shoot effect values at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }

    private static InvalidOperationException MissingEffectField(
        string effectType,
        string field,
        int index,
        string? sourcePath) =>
        new(AppendSource(
            $"Accessory effect '{effectType}' at index {index} must include {field}.",
            sourcePath));

    private static string FormatParseError(string kind, string? sourcePath) =>
        AppendSource($"Failed to parse {kind} definition JSON.", sourcePath);

    private static string FormatEmptyObjectError(string kind, string? sourcePath) =>
        AppendSource($"{kind} definition JSON must be a non-empty object.", sourcePath);

    private static string FormatRequiredFieldError(string kind, string field, string? sourcePath) =>
        AppendSource($"{kind} definition JSON must include {field}.", sourcePath);

    private static string AppendSource(string message, string? sourcePath) =>
        string.IsNullOrWhiteSpace(sourcePath) ? message : $"{message} ({sourcePath})";

    private sealed class AccessoryFile
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("effects")]
        public List<EffectFile>? Effects { get; init; }
    }

    private sealed class EffectFile
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("fireIntervalSeconds")]
        public float? FireIntervalSeconds { get; init; }

        [JsonPropertyName("missileSpeed")]
        public float? MissileSpeed { get; init; }

        [JsonPropertyName("missileDamage")]
        public float? MissileDamage { get; init; }

        [JsonPropertyName("friendlyFire")]
        public bool? FriendlyFire { get; init; }
    }

    private sealed class CharacterFile
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("accessories")]
        public List<string>? Accessories { get; init; }
    }
}
