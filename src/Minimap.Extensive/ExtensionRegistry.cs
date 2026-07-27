using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>In-memory registry of typed extension contributions.</summary>
public sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly TagRegistry _tags = new();
    private readonly Dictionary<string, IIntegrator> _integrators = new(StringComparer.Ordinal);
    private readonly List<AccessoryDefinition> _accessoryDefinitions = new();
    private readonly Dictionary<string, AccessoryDefinition> _accessoryById = new(StringComparer.Ordinal);
    private readonly List<ActorDefinition> _actorDefinitions = new();
    private readonly Dictionary<string, ActorDefinition> _actorById = new(StringComparer.Ordinal);
    private readonly List<ResourceDefinition> _resourceDefinitions = new();
    private readonly Dictionary<string, ResourceDefinition> _resourceById = new(StringComparer.Ordinal);
    private readonly Dictionary<TagId, ResourceDefinition> _resourceByTag = new();
    private readonly List<DomainDefinition> _domainDefinitions = new();
    private readonly Dictionary<string, DomainDefinition> _domainById = new(StringComparer.Ordinal);
    private readonly Dictionary<TagId, DomainDefinition> _domainByTag = new();
    private readonly Dictionary<string, AccessoryEffectFactory> _effectFactories =
        new(StringComparer.OrdinalIgnoreCase);

    public TagRegistry Tags => _tags;

    public IReadOnlyList<IIntegrator> Integrators =>
        _integrators.Values.OrderBy(i => i.Id, StringComparer.Ordinal).ToList();

    public IReadOnlyList<AccessoryDefinition> AccessoryDefinitions => _accessoryDefinitions;

    public IReadOnlyList<ActorDefinition> ActorDefinitions => _actorDefinitions;

    public IReadOnlyList<ResourceDefinition> ResourceDefinitions => _resourceDefinitions;

    public IReadOnlyList<DomainDefinition> DomainDefinitions => _domainDefinitions;

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

    public void AddActorDefinition(ActorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_actorById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate actor definition id '{definition.Id}'.");
        }

        _actorDefinitions.Add(definition);
    }

    public bool TryGetActorDefinition(string id, out ActorDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _actorById.TryGetValue(id, out definition);
    }

    public void AddResourceDefinition(ResourceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_resourceById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate resource definition id '{definition.Id}'.");
        }

        if (!_resourceByTag.TryAdd(definition.Tag, definition))
        {
            _resourceById.Remove(definition.Id);
            throw new InvalidOperationException(
                $"Duplicate resource definition tag for id '{definition.Id}'.");
        }

        _resourceDefinitions.Add(definition);
    }

    public bool TryGetResourceDefinition(string id, out ResourceDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _resourceById.TryGetValue(id, out definition);
    }

    public bool TryGetResourceDefinition(TagId tag, out ResourceDefinition? definition) =>
        _resourceByTag.TryGetValue(tag, out definition);

    public void AddDomainDefinition(DomainDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!_domainById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException(
                $"Duplicate domain definition id '{definition.Id}'.");
        }

        if (!_domainByTag.TryAdd(definition.Tag, definition))
        {
            _domainById.Remove(definition.Id);
            throw new InvalidOperationException(
                $"Duplicate domain definition tag for id '{definition.Id}'.");
        }

        _domainDefinitions.Add(definition);
    }

    public bool TryGetDomainDefinition(string id, out DomainDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _domainById.TryGetValue(id, out definition);
    }

    public bool TryGetDomainDefinition(TagId tag, out DomainDefinition? definition) =>
        _domainByTag.TryGetValue(tag, out definition);

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
