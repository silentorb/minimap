namespace Minimap.Extensive;

/// <summary>In-memory registry of typed extension contributions.</summary>
public sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly Dictionary<string, IIntegrator> _integrators = new(StringComparer.Ordinal);

    public IReadOnlyList<IIntegrator> Integrators =>
        _integrators.Values.OrderBy(i => i.Id, StringComparer.Ordinal).ToList();

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
}
