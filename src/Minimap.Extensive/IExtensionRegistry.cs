using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>Typed registration surface for extension contributions.</summary>
public interface IExtensionRegistry
{
    void AddIntegrator(IIntegrator integrator);

    IReadOnlyList<IIntegrator> Integrators { get; }

    bool TryGetIntegrator(string id, out IIntegrator? integrator);

    void AddAccessoryDefinition(AccessoryDefinition definition);

    IReadOnlyList<AccessoryDefinition> AccessoryDefinitions { get; }

    void AddCharacterDefinition(CharacterDefinition definition);

    IReadOnlyList<CharacterDefinition> CharacterDefinitions { get; }

    /// <summary>
    /// Registers a JSON effect <paramref name="type"/> factory (case-insensitive).
    /// Duplicate type ids fail fast.
    /// </summary>
    void AddAccessoryEffectFactory(string type, AccessoryEffectFactory factory);

    bool TryGetAccessoryEffectFactory(string type, out AccessoryEffectFactory? factory);
}
