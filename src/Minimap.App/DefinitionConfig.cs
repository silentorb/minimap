using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Loads accessory and character definitions from JSON under extension content directories.</summary>
public static class DefinitionConfig
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

        using var document = ParseDocument(json, "accessory", sourcePath);
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

        var depiction = ParseDepictionProperty(root, sourcePath);
        return new AccessoryDefinition(id, effects, depiction);
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

        using var document = ParseDocument(json, "character", sourcePath);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                FormatEmptyObjectError("character", sourcePath));
        }

        if (!TryGetStringProperty(root, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                FormatRequiredFieldError("character", "id", sourcePath));
        }

        if (!root.TryGetProperty("accessories", out var accessoriesElement) ||
            accessoriesElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                FormatRequiredFieldError("character", "accessories", sourcePath));
        }

        var accessories = new List<AccessoryDefinition>();
        foreach (var accessoryIdElement in accessoriesElement.EnumerateArray())
        {
            if (accessoryIdElement.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    AppendSource(
                        "Character definition accessories must be non-empty ids.",
                        sourcePath));
            }

            var accessoryId = accessoryIdElement.GetString();
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
                        $"Character definition '{id}' references unknown accessory '{accessoryId}'.",
                        sourcePath));
            }

            accessories.Add(accessory);
        }

        var depiction = ParseDepictionProperty(root, sourcePath);
        return new CharacterDefinition(id, accessories, depiction);
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

    private static JsonDocument ParseDocument(string json, string kind, string? sourcePath)
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
                FormatParseError(kind, sourcePath), ex);
        }
    }

    /// <summary>
    /// Parses optional <c>depiction</c> object. Missing or null → null.
    /// Malformed object / empty required fields fail fast.
    /// </summary>
    private static DepictionConfig? ParseDepictionProperty(JsonElement root, string? sourcePath)
    {
        if (!root.TryGetProperty("depiction", out var depictionElement))
            return null;

        if (depictionElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        if (depictionElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                AppendSource("Definition depiction must be a JSON object or null.", sourcePath));
        }

        if (!TryGetStringProperty(depictionElement, "kind", out var kind) ||
            string.IsNullOrWhiteSpace(kind))
        {
            throw new InvalidOperationException(
                AppendSource("Definition depiction must include kind.", sourcePath));
        }

        if (!TryGetStringProperty(depictionElement, "path", out var path) ||
            string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                AppendSource("Definition depiction must include path.", sourcePath));
        }

        string? animation = null;
        if (depictionElement.TryGetProperty("animation", out var animationElement))
        {
            if (animationElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                animation = null;
            }
            else if (animationElement.ValueKind == JsonValueKind.String)
            {
                animation = animationElement.GetString();
            }
            else
            {
                throw new InvalidOperationException(
                    AppendSource("Definition depiction animation must be a string or null.", sourcePath));
            }
        }

        return new DepictionConfig(kind, path, animation);
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
}
