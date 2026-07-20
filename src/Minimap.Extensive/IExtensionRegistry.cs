namespace Minimap.Extensive;

/// <summary>Typed registration surface for extension contributions.</summary>
public interface IExtensionRegistry
{
    void AddIntegrator(IIntegrator integrator);

    IReadOnlyList<IIntegrator> Integrators { get; }

    bool TryGetIntegrator(string id, out IIntegrator? integrator);
}
