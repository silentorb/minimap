using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>In-memory registry of typed extension contributions.</summary>
public sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly TagRegistry _tags = new();
    private readonly Dictionary<string, IIntegrator> _integrators = new(StringComparer.Ordinal);
    private readonly List<AccessoryDefinition> _accessoryDefinitions = new();
    private readonly Dictionary<string, AccessoryDefinition> _accessoryById = new(StringComparer.Ordinal);
    private readonly List<CharacterDefinition> _characterDefinitions = new();
    private readonly Dictionary<string, CharacterDefinition> _characterById = new(StringComparer.Ordinal);
    private readonly List<PlacedObjectDefinition> _placedObjectDefinitions = new();
    private readonly Dictionary<string, PlacedObjectDefinition> _placedObjectById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AccessoryEffectFactory> _effectFactories =
        new(StringComparer.OrdinalIgnoreCase);

    public TagRegistry Tags => _tags;

    public IReadOnlyList<IIntegrator> Integrators =>
        _integrators.Values.OrderBy(i => i.Id, StringComparer.Ordinal).ToList();

    public IReadOnlyList<AccessoryDefinition> AccessoryDefinitions => _accessoryDefinitions;

    public IReadOnlyList<CharacterDefinition> CharacterDefinitions => _characterDefinitions;

    public IReadOnlyList<PlacedObjectDefinition> PlacedObjectDefinitions => _placedObjectDefinitions;

    public void RegisterTags(IEnumerable<string> tagNames)
    {
        ArgumentNullException.ThrowIfNull(tagNames);
        foreach (var name in tagNames)
            _tags.GetOrCreate(name);
    }

    public void AddIntegrator(IIntegrator integrator)
    {
        ArgumentNullException.ThrowIfNull(integrator);
        if (string.IsNullOrWhiteSpace(integrator.Id))
            throw new InvalidOperationException("Integrator id must be non-empty.");

        if (!_integrators.TryAdd(integrator.Id, integrator))
        {
            throw new InvalidOperationException(
                $"Duplicate integrator id '{integrator.Id}'.");
        }
    }

    public bool TryGetIntegrator(string id, out IIntegrator? integrator)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            integrator = null;
            return false;
        }

        return _integrators.TryGetValue(id, out integrator);
    }

    public IIntegrator RequireIntegrator(string id)
    {
        if (!TryGetIntegrator(id, out var integrator) || integrator is null)
        {
            throw new InvalidOperationException(
                $"Integrator '{id}' is not registered.");
        }

        return integrator;
    }

    public void AddAccessoryDefinition(AccessoryDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_accessoryById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate accessory definition id '{definition.Id}'.");
        }

        _accessoryDefinitions.Add(definition);
    }

    public bool TryGetAccessoryDefinition(string id, out AccessoryDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _accessoryById.TryGetValue(id, out definition);
    }

    public void AddCharacterDefinition(CharacterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_characterById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate character definition id '{definition.Id}'.");
        }

        _characterDefinitions.Add(definition);
    }

    public bool TryGetCharacterDefinition(string id, out CharacterDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _characterById.TryGetValue(id, out definition);
    }

    public void AddPlacedObjectDefinition(PlacedObjectDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_placedObjectById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate placed object definition id '{definition.Id}'.");
        }

        _placedObjectDefinitions.Add(definition);
    }

    public bool TryGetPlacedObjectDefinition(string id, out PlacedObjectDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _placedObjectById.TryGetValue(id, out definition);
    }

    public void AddAccessoryEffectFactory(string type, AccessoryEffectFactory factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(factory);

        var key = type.Trim();
        if (!_effectFactories.TryAdd(key, factory))
        {
            throw new InvalidOperationException(
                $"Duplicate accessory effect type '{key}'.");
        }
    }

    public bool TryGetAccessoryEffectFactory(string type, out AccessoryEffectFactory? factory)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            factory = null;
            return false;
        }

        return _effectFactories.TryGetValue(type.Trim(), out factory);
    }
}
