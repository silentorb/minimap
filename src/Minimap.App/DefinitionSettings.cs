using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Loads accessory and character definitions from JSON under extension content directories.</summary>
public static class DefinitionSettings
{
    public const string AccessoriesDirectoryName = "accessories";
    public const string CharactersDirectoryName = "characters";

    /// <summary>
    /// Content root beside a loaded extension DLL:
    /// <c>{dllDir}/{assemblyName}/</c> (e.g. <c>extensions/CompuQuest.Minimap/</c>).
    /// </summary>
    public static string ContentDirectoryForAssembly(string assemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        var fullPath = Path.GetFullPath(assemblyPath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{assemblyPath}'.");
        var name = Path.GetFileNameWithoutExtension(fullPath);
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException($"Could not resolve assembly name for '{assemblyPath}'.");
        return Path.Combine(directory, name);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<AccessoryDefinition> LoadAccessoriesFromDirectory(
        string directory,
        IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(registry);

        if (!Directory.Exists(directory))
            return Array.Empty<AccessoryDefinition>();

        var definitions = new List<AccessoryDefinition>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").OrderBy(p => p, StringComparer.Ordinal))
            definitions.Add(LoadAccessoryFromFile(path, registry));

        return definitions;
    }

    public static AccessoryDefinition LoadAccessoryFromJson(
        string json,
        IExtensionRegistry registry,
        string? sourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(registry);

        using var document = ParseAccessoryDocument(json, sourcePath);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                FormatEmptyObjectError("accessory", sourcePath));
        }

        if (!TryGetStringProperty(root, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                FormatRequiredFieldError("accessory", "id", sourcePath));
        }

        if (!root.TryGetProperty("effects", out var effectsElement) ||
            effectsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                FormatRequiredFieldError("accessory", "effects", sourcePath));
        }

        var effects = new List<AccessoryEffect>();
        var index = 0;
        foreach (var effectElement in effectsElement.EnumerateArray())
        {
            effects.Add(ParseEffect(effectElement, index, registry, sourcePath));
            index++;
        }

        return new AccessoryDefinition(id, effects);
    }

    public static AccessoryDefinition LoadAccessoryFromFile(string path, IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(registry);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Accessory definition file not found: {path}", path);

        return LoadAccessoryFromJson(File.ReadAllText(path), registry, path);
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
    /// Registers JSON definitions from <paramref name="contentDirectory"/> into
    /// <paramref name="registry"/> (accessories, then characters).
    /// Effect <c>type</c> values must already be registered via
    /// <see cref="IExtensionRegistry.AddAccessoryEffectFactory"/>.
    /// </summary>
    public static void RegisterFromConfigDirectory(string contentDirectory, IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentDirectory);
        ArgumentNullException.ThrowIfNull(registry);

        var accessoriesDir = Path.Combine(contentDirectory, AccessoriesDirectoryName);
        foreach (var accessory in LoadAccessoriesFromDirectory(accessoriesDir, registry))
            registry.AddAccessoryDefinition(accessory);

        var charactersDir = Path.Combine(contentDirectory, CharactersDirectoryName);
        foreach (var character in LoadCharactersFromDirectory(charactersDir, registry))
            registry.AddCharacterDefinition(character);
    }

    private static JsonDocument ParseAccessoryDocument(string json, string? sourcePath)
    {
        try
        {
            return JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                FormatParseError("accessory", sourcePath), ex);
        }
    }

    private static AccessoryEffect ParseEffect(
        JsonElement effectElement,
        int index,
        IExtensionRegistry registry,
        string? sourcePath)
    {
        if (effectElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} must be a JSON object.",
                    sourcePath));
        }

        if (!TryGetStringProperty(effectElement, "type", out var type) ||
            string.IsNullOrWhiteSpace(type))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} must include type.",
                    sourcePath));
        }

        if (!registry.TryGetAccessoryEffectFactory(type, out var factory) || factory is null)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Unknown accessory effect type '{type}' at index {index}.",
                    sourcePath));
        }

        return factory(effectElement, index, sourcePath);
    }

    private static bool TryGetStringProperty(JsonElement obj, string name, out string? value)
    {
        value = null;
        if (!obj.TryGetProperty(name, out var prop))
            return false;
        if (prop.ValueKind != JsonValueKind.String)
            return false;
        value = prop.GetString();
        return true;
    }

    private static string FormatParseError(string kind, string? sourcePath) =>
        AppendSource($"Failed to parse {kind} definition JSON.", sourcePath);

    private static string FormatEmptyObjectError(string kind, string? sourcePath) =>
        AppendSource($"{kind} definition JSON must be a non-empty object.", sourcePath);

    private static string FormatRequiredFieldError(string kind, string field, string? sourcePath) =>
        AppendSource($"{kind} definition JSON must include {field}.", sourcePath);

    private static string AppendSource(string message, string? sourcePath) =>
        string.IsNullOrWhiteSpace(sourcePath) ? message : $"{message} ({sourcePath})";

    private sealed class CharacterFile
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("accessories")]
        public List<string>? Accessories { get; init; }
    }
}
