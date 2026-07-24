using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Possessable character pawn in cartesian world space.</summary>
public sealed class Character
{
    private readonly List<Accessory> _accessories = new();
    private readonly List<AccessoryEffect> _effects = new();
    private readonly Dictionary<TagId, int> _resources = new();
    private readonly ResourceContext _resourceContext;

    public Character(
        int id,
        int factionId,
        SimVec2 position,
        CharacterDefinition definition,
        ResourceContext resourceContext,
        int maxHealth = CombatTuning.DefaultMaxHealth)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(resourceContext);
        if (maxHealth < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth));

        Id = id;
        FactionId = factionId;
        Position = position;
        Definition = definition;
        _resourceContext = resourceContext;
        Facing = new SimVec2(1f, 0f);
        AbilityLoadout = new AbilityLoadout();

        SetResource(_resourceContext.MaxHealthTag, maxHealth);
        SetResource(_resourceContext.HealthTag, maxHealth);

        foreach (var accessoryDef in definition.Accessories)
            AddAccessory(accessoryDef.CreateInstance());
    }

    public int Id { get; }
    public int FactionId { get; }
    public CharacterDefinition Definition { get; }
    public SimVec2 Position { get; set; }
    public SimVec2 MoveIntent { get; set; }

    /// <summary>Last non-zero move direction (normalized). Used for placement and aim-line.</summary>
    public SimVec2 Facing { get; set; }

    public AbilityLoadout AbilityLoadout { get; }

    public ResourceContext ResourceContext => _resourceContext;

    public IReadOnlyDictionary<TagId, int> Resources => _resources;

    public int Health
    {
        get => GetResource(_resourceContext.HealthTag);
        set => SetResource(_resourceContext.HealthTag, value);
    }

    public int MaxHealth => GetResource(_resourceContext.MaxHealthTag);

    public bool IsAlive => Health > 0;

    public IReadOnlyList<Accessory> Accessories => _accessories;

    public IReadOnlyList<AccessoryEffect> Effects => _effects;

    public int GetResource(TagId tag) =>
        _resources.TryGetValue(tag, out var amount) ? amount : 0;

    public void SetResource(TagId tag, int amount)
    {
        if (amount < 0)
            amount = 0;

        if (_resourceContext.TryGet(tag, out var definition) &&
            definition?.LimitTag is { } limitTag)
        {
            var max = GetResource(limitTag);
            if (amount > max)
                amount = max;
        }

        if (amount == 0)
            _resources.Remove(tag);
        else
            _resources[tag] = amount;
    }

    public void AddResource(TagId tag, int amount)
    {
        if (amount == 0)
            return;
        if (amount < 0)
        {
            TryConsumeResource(tag, -amount);
            return;
        }

        SetResource(tag, GetResource(tag) + amount);
    }

    /// <summary>Returns false when stock is insufficient.</summary>
    public bool TryConsumeResource(TagId tag, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount == 0)
            return true;

        var current = GetResource(tag);
        if (current < amount)
            return false;

        SetResource(tag, current - amount);
        return true;
    }

    public void AddAccessory(Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        _accessories.Add(accessory);
        foreach (var effect in accessory.Effects)
            _effects.Add(effect);

        if (accessory.Definition.ConsumedResourceTag is { } resourceTag &&
            accessory.Definition.StartingResourceAmount > 0)
        {
            AddResource(resourceTag, accessory.Definition.StartingResourceAmount);
        }

        AbilityLoadout.Rebuild(_accessories);
    }

    public bool RemoveAccessory(Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (!_accessories.Remove(accessory))
            return false;

        foreach (var effect in accessory.Effects)
            _effects.Remove(effect);
        AbilityLoadout.Rebuild(_accessories);
        return true;
    }
}
