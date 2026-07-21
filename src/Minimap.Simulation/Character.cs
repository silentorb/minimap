using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Possessable character pawn in cartesian world space.</summary>
public sealed class Character
{
    private readonly List<Accessory> _accessories = new();
    private readonly List<AccessoryEffect> _effects = new();

    public Character(
        int id,
        int factionId,
        SimVec2 position,
        CharacterDefinition definition,
        float maxHealth = CombatTuning.DefaultMaxHealth)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(definition);

        Id = id;
        FactionId = factionId;
        Position = position;
        Definition = definition;
        MaxHealth = maxHealth;
        Health = maxHealth;

        foreach (var accessoryDef in definition.Accessories)
            AddAccessory(accessoryDef.CreateInstance());
    }

    public int Id { get; }
    public int FactionId { get; }
    public CharacterDefinition Definition { get; }
    public SimVec2 Position { get; set; }
    public float MaxHealth { get; }
    public float Health { get; set; }
    public SimVec2 MoveIntent { get; set; }
    public bool IsAlive => Health > 0f;

    public IReadOnlyList<Accessory> Accessories => _accessories;

    public IReadOnlyList<AccessoryEffect> Effects => _effects;

    public void AddAccessory(Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        _accessories.Add(accessory);
        foreach (var effect in accessory.Effects)
            _effects.Add(effect);
    }

    public bool RemoveAccessory(Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (!_accessories.Remove(accessory))
            return false;

        foreach (var effect in accessory.Effects)
            _effects.Remove(effect);
        return true;
    }
}
