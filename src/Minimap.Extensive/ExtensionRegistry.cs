using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>In-memory registry of typed extension contributions.</summary>
public sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly Dictionary<string, IIntegrator> _integrators = new(StringComparer.Ordinal);
    private readonly List<AccessoryDefinition> _accessoryDefinitions = new();
    private readonly Dictionary<string, AccessoryDefinition> _accessoryById = new(StringComparer.Ordinal);
    private readonly List<CharacterDefinition> _characterDefinitions = new();
    private readonly Dictionary<string, CharacterDefinition> _characterById = new(StringComparer.Ordinal);

    public IReadOnlyList<IIntegrator> Integrators =>
        _integrators.Values.OrderBy(i => i.Id, StringComparer.Ordinal).ToList();

    public IReadOnlyList<AccessoryDefinition> AccessoryDefinitions => _accessoryDefinitions;

    public IReadOnlyList<CharacterDefinition> CharacterDefinitions => _characterDefinitions;

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
}
