using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Loads accessory and character definitions from JSON under extension content directories.</summary>
public static class DefinitionConfig
{
    public const string AccessoriesDirectoryName = "accessories";
    public const string CharactersDirectoryName = "characters";
    public const string PlacedObjectsDirectoryName = "placed_objects";

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
        var icon = ParseIconProperty(root, sourcePath);
        var tags = ParseTagsProperty(root, registry.Tags, sourcePath);
        var pointCost = ParsePointCostProperty(root, sourcePath);
        var activation = ParseActivationProperty(root, sourcePath);
        TryGetStringProperty(root, "displayName", out var displayName);
        TryGetStringProperty(root, "description", out var description);
        return new AccessoryDefinition(
            id,
            effects,
            depiction,
            icon,
            tags,
            pointCost,
            displayName,
            description,
            activation);
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
        var icon = ParseIconProperty(root, sourcePath);
        return new CharacterDefinition(id, accessories, depiction, icon);
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
    /// <paramref name="registry"/> (placed objects, accessories, then characters).
    /// Effect <c>type</c> values must already be registered via
    /// <see cref="IExtensionRegistry.AddAccessoryEffectFactory"/>.
    /// </summary>
    public static void RegisterFromConfigDirectory(string contentDirectory, IExtensionRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentDirectory);
        ArgumentNullException.ThrowIfNull(registry);

        var placedObjectsDir = Path.Combine(contentDirectory, PlacedObjectsDirectoryName);
        foreach (var placed in LoadPlacedObjectsFromDirectory(placedObjectsDir))
            registry.AddPlacedObjectDefinition(placed);

        var accessoriesDir = Path.Combine(contentDirectory, AccessoriesDirectoryName);
        foreach (var accessory in LoadAccessoriesFromDirectory(accessoriesDir, registry))
            registry.AddAccessoryDefinition(accessory);

        var charactersDir = Path.Combine(contentDirectory, CharactersDirectoryName);
        foreach (var character in LoadCharactersFromDirectory(charactersDir, registry))
            registry.AddCharacterDefinition(character);
    }

    public static IReadOnlyList<PlacedObjectDefinition> LoadPlacedObjectsFromDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if (!Directory.Exists(directory))
            return Array.Empty<PlacedObjectDefinition>();

        var definitions = new List<PlacedObjectDefinition>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").OrderBy(p => p, StringComparer.Ordinal))
            definitions.Add(LoadPlacedObjectFromFile(path));

        return definitions;
    }

    public static PlacedObjectDefinition LoadPlacedObjectFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Placed object definition file not found: {path}", path);

        return LoadPlacedObjectFromJson(File.ReadAllText(path), path);
    }

    public static PlacedObjectDefinition LoadPlacedObjectFromJson(string json, string? sourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = ParseDocument(json, "placed object", sourcePath);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                FormatEmptyObjectError("placed object", sourcePath));
        }

        if (!TryGetStringProperty(root, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                FormatRequiredFieldError("placed object", "id", sourcePath));
        }

        var depiction = ParseDepictionProperty(root, sourcePath);
        TryGetStringProperty(root, "displayName", out var displayName);
        return new PlacedObjectDefinition(id, depiction, displayName);
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

    private static IReadOnlyList<TagId> ParseTagsProperty(
        JsonElement root,
        TagRegistry tags,
        string? sourcePath)
    {
        if (!root.TryGetProperty("tags", out var tagsElement))
            return Array.Empty<TagId>();

        if (tagsElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return Array.Empty<TagId>();

        if (tagsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                AppendSource("Accessory tags must be a JSON array of strings.", sourcePath));
        }

        var result = new List<TagId>();
        var index = 0;
        foreach (var element in tagsElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(element.GetString()))
            {
                throw new InvalidOperationException(
                    AppendSource(
                        $"Accessory tags[{index}] must be a non-empty string.",
                        sourcePath));
            }

            result.Add(tags.GetOrCreate(element.GetString()!));
            index++;
        }

        return result;
    }

    private static int ParsePointCostProperty(JsonElement root, string? sourcePath)
    {
        if (!root.TryGetProperty("pointCost", out var costElement))
            return 0;

        if (costElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return 0;

        if (costElement.ValueKind != JsonValueKind.Number || !costElement.TryGetInt32(out var cost))
        {
            throw new InvalidOperationException(
                AppendSource("Accessory pointCost must be a non-negative integer.", sourcePath));
        }

        if (cost < 0)
        {
            throw new InvalidOperationException(
                AppendSource("Accessory pointCost must be a non-negative integer.", sourcePath));
        }

        return cost;
    }

    private static AccessoryActivation ParseActivationProperty(JsonElement root, string? sourcePath)
    {
        if (!root.TryGetProperty("activation", out var activationElement))
            return AccessoryActivation.None;

        if (activationElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return AccessoryActivation.None;

        if (activationElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                AppendSource("Accessory activation must be a JSON object or null.", sourcePath));
        }

        if (!TryGetStringProperty(activationElement, "kind", out var kindRaw) ||
            string.IsNullOrWhiteSpace(kindRaw))
        {
            throw new InvalidOperationException(
                AppendSource("Accessory activation must include kind.", sourcePath));
        }

        var kind = kindRaw.Trim().ToLowerInvariant() switch
        {
            "none" => AccessoryActivationKind.None,
            "dedicated" => AccessoryActivationKind.Dedicated,
            "modal" => AccessoryActivationKind.Modal,
            _ => throw new InvalidOperationException(
                AppendSource(
                    $"Accessory activation kind '{kindRaw}' is not supported.",
                    sourcePath)),
        };

        string? bind = null;
        if (activationElement.TryGetProperty("bind", out var bindElement))
        {
            if (bindElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                bind = null;
            }
            else if (bindElement.ValueKind == JsonValueKind.String)
            {
                bind = bindElement.GetString();
            }
            else
            {
                throw new InvalidOperationException(
                    AppendSource("Accessory activation bind must be a string or null.", sourcePath));
            }
        }

        try
        {
            return new AccessoryActivation(kind, bind);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(AppendSource(ex.Message, sourcePath), ex);
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

    /// <summary>
    /// Parses optional <c>icon</c> object. Missing or null → null.
    /// Malformed object / empty required fields fail fast.
    /// </summary>
    private static IconConfig? ParseIconProperty(JsonElement root, string? sourcePath)
    {
        if (!root.TryGetProperty("icon", out var iconElement))
            return null;

        if (iconElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        if (iconElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                AppendSource("Definition icon must be a JSON object or null.", sourcePath));
        }

        if (!TryGetStringProperty(iconElement, "path", out var path) ||
            string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                AppendSource("Definition icon must include path.", sourcePath));
        }

        return new IconConfig(path);
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
