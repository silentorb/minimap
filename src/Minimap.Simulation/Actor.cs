using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Runtime actor: accessories, effects, resources, facing, optional cell occupancy.</summary>
public class Actor
{
    private readonly List<Accessory> _accessories = new();
    private readonly List<AccessoryEffect> _effects = new();
    private readonly Dictionary<TagId, int> _resources = new();
    private readonly ResourceContext _resourceContext;

    public Actor(
        int id,
        ActorDefinition definition,
        ResourceContext resourceContext,
        int factionId = 0,
        SimVec2 position = default,
        bool applyDefinitionAccessories = true)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(resourceContext);

        Id = id;
        Definition = definition;
        _resourceContext = resourceContext;
        Facing = new SimVec2(1f, 0f);
        FactionId = factionId;
        Position = position;
        AbilityLoadout = new AbilityLoadout();

        ApplyDefinitionStartingResources();
        if (applyDefinitionAccessories)
            ApplyDefinitionAccessories();
    }

    public int Id { get; }

    public ActorDefinition Definition { get; }

    /// <summary>Faction for hostility / ownership (default 0 for unowned placeables).</summary>
    public int FactionId { get; set; }

    /// <summary>Cell occupancy when this actor is cell-anchored; null when free.</summary>
    public HexAxial? Cell { get; set; }

    public SimVec2 Position { get; set; }

    public SimVec2 MoveIntent { get; set; }

    public AbilityLoadout AbilityLoadout { get; }

    /// <summary>Runtime depiction override (e.g. growing → ripe). Null uses definition depiction.</summary>
    public DepictionConfig? DepictionOverride { get; set; }

    public DepictionConfig? EffectiveDepiction => DepictionOverride ?? Definition.DepictionConfig;

    public SimVec2 Facing { get; set; }

    public ResourceContext ResourceContext => _resourceContext;

    public IReadOnlyDictionary<TagId, int> Resources => _resources;

    public IReadOnlyList<Accessory> Accessories => _accessories;

    public IReadOnlyList<AccessoryEffect> Effects => _effects;

    public int Health
    {
        get => GetResource(ResourceContext.HealthTag);
        set => SetResource(ResourceContext.HealthTag, value);
    }

    public int MaxHealth => GetResource(ResourceContext.MaxHealthTag);

    public int Energy
    {
        get => GetResource(ResourceContext.EnergyTag);
        set => SetResource(ResourceContext.EnergyTag, value);
    }

    public int MaxEnergy => GetResource(ResourceContext.MaxEnergyTag);

    /// <summary>True when the actor has positive max health and can take combat damage.</summary>
    public bool IsDestructible => MaxHealth > 0;

    /// <summary>Indestructible actors are always alive; destructible actors die at health ≤ 0.</summary>
    public bool IsAlive => !IsDestructible || Health > 0;

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

        var previous = GetResource(tag);
        if (amount == 0)
            _resources.Remove(tag);
        else
            _resources[tag] = amount;

        if (previous != amount)
            OnResourcesChanged();
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
        accessory.IsEnabled = AccessoryEnablement.Evaluate(this, accessory);
        _accessories.Add(accessory);
        foreach (var effect in accessory.Effects)
            _effects.Add(effect);

        foreach (var effect in accessory.Effects)
        {
            if (effect is IOnAccessoryAcquired onAcquired)
                onAcquired.OnAcquired(this);
        }

        OnAccessoriesChanged();
    }

    public bool RemoveAccessory(Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (!_accessories.Remove(accessory))
            return false;

        foreach (var effect in accessory.Effects)
            _effects.Remove(effect);
        OnAccessoriesChanged();
        return true;
    }

    /// <summary>First move effect on this actor, if any.</summary>
    public bool TryGetMoveEffect(out IMoveEffect? move)
    {
        foreach (var effect in _effects)
        {
            if (effect is IMoveEffect found)
            {
                move = found;
                return true;
            }
        }

        move = null;
        return false;
    }

    protected void ApplyDefinitionStartingResources()
    {
        foreach (var entry in Definition.Resources)
            SetResource(entry.Tag, entry.Amount);
    }

    protected void ApplyDefinitionAccessories()
    {
        foreach (var accessoryDef in Definition.Accessories)
            AddAccessory(accessoryDef.CreateInstance());
    }

    protected virtual void OnAccessoriesChanged()
    {
        AccessoryEnablement.Sync(this);
        AbilityLoadout.Rebuild(Accessories);
    }

    protected virtual void OnResourcesChanged()
    {
        if (AccessoryEnablement.Sync(this))
            AbilityLoadout.Rebuild(Accessories);
    }
}
