using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>Typed registration surface for extension contributions.</summary>
public interface IExtensionRegistry
{
    /// <summary>Instance-owned tag catalog used for create-if-not-exists string resolution.</summary>
    TagRegistry Tags { get; }

    /// <summary>Register tag strings (create-if-not-exists).</summary>
    void RegisterTags(IEnumerable<string> tagNames);

    void AddIntegrator(IIntegrator integrator);

    IReadOnlyList<IIntegrator> Integrators { get; }

    bool TryGetIntegrator(string id, out IIntegrator? integrator);

    void AddAccessoryDefinition(AccessoryDefinition definition);

    IReadOnlyList<AccessoryDefinition> AccessoryDefinitions { get; }

    bool TryGetAccessoryDefinition(string id, out AccessoryDefinition? definition);

    void AddCharacterDefinition(CharacterDefinition definition);

    IReadOnlyList<CharacterDefinition> CharacterDefinitions { get; }

    bool TryGetCharacterDefinition(string id, out CharacterDefinition? definition);

    /// <summary>
    /// Registers a JSON effect <paramref name="type"/> factory (case-insensitive).
    /// Duplicate type ids fail fast.
    /// </summary>
    void AddAccessoryEffectFactory(string type, AccessoryEffectFactory factory);

    bool TryGetAccessoryEffectFactory(string type, out AccessoryEffectFactory? factory);
}
