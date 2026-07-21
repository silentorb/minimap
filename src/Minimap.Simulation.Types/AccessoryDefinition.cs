namespace Minimap.Simulation.Types;

/// <summary>Content definition for an accessory (effect templates only).</summary>
public sealed class AccessoryDefinition
{
    private readonly List<AccessoryEffect> _effectTemplates;

    public AccessoryDefinition(string id, IEnumerable<AccessoryEffect> effectTemplates)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Accessory definition id must be non-empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(effectTemplates);

        Id = id;
        _effectTemplates = effectTemplates.ToList();
    }

    public string Id { get; }

    public IReadOnlyList<AccessoryEffect> EffectTemplates => _effectTemplates;

    /// <summary>Create a runtime accessory with cloned effect instances.</summary>
    public Accessory CreateInstance()
    {
        var effects = _effectTemplates.Select(t => t.Clone()).ToList();
        return new Accessory(this, effects);
    }
}
