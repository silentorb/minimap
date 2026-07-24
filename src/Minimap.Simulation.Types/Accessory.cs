namespace Minimap.Simulation.Types;

/// <summary>Runtime accessory: definition plus effect instances (no behavior-specific fields).</summary>
public sealed class Accessory
{
    private readonly List<AccessoryEffect> _effects;

    public Accessory(AccessoryDefinition definition, IEnumerable<AccessoryEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(effects);
        Definition = definition;
        _effects = effects.ToList();
    }

    public AccessoryDefinition Definition { get; }

    public IReadOnlyList<AccessoryEffect> Effects => _effects;

    /// <summary>
    /// Whether the accessory participates in the ability loadout.
    /// Orthogonal to effect use costs; updated from <see cref="AccessoryDefinition.EnabledWhen"/>.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
