namespace Minimap.Simulation.Types;

/// <summary>Content definition for a character archetype (accessories applied at instantiate).</summary>
public sealed class CharacterDefinition
{
    private readonly List<AccessoryDefinition> _accessories;

    public CharacterDefinition(string id, IEnumerable<AccessoryDefinition> accessories)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Character definition id must be non-empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(accessories);

        Id = id;
        _accessories = accessories.ToList();
    }

    public string Id { get; }

    public IReadOnlyList<AccessoryDefinition> Accessories => _accessories;
}
