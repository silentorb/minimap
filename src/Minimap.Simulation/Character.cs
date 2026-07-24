using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Possessable character pawn in cartesian world space.</summary>
public sealed class Character : Actor
{
    public Character(
        int id,
        int factionId,
        SimVec2 position,
        CharacterDefinition definition,
        ResourceContext resourceContext,
        int maxHealth = CombatTuning.DefaultMaxHealth,
        int maxEnergy = CombatTuning.DefaultMaxEnergy)
        : base(id, definition, resourceContext, applyDefinitionAccessories: false)
    {
        if (maxHealth < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (maxEnergy < 0)
            throw new ArgumentOutOfRangeException(nameof(maxEnergy));

        FactionId = factionId;
        Position = position;
        AbilityLoadout = new AbilityLoadout();

        SetResource(ResourceContext.MaxHealthTag, maxHealth);
        SetResource(ResourceContext.HealthTag, maxHealth);
        SetResource(ResourceContext.MaxEnergyTag, maxEnergy);
        SetResource(ResourceContext.EnergyTag, maxEnergy);

        ApplyDefinitionAccessories();
    }

    public int FactionId { get; }

    public new CharacterDefinition Definition => (CharacterDefinition)base.Definition;

    public SimVec2 Position { get; set; }

    public SimVec2 MoveIntent { get; set; }

    public AbilityLoadout AbilityLoadout { get; }

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

    public bool IsAlive => Health > 0;

    protected override void OnAccessoriesChanged()
    {
        AccessoryEnablement.Sync(this);
        AbilityLoadout.Rebuild(Accessories);
    }

    protected override void OnResourcesChanged()
    {
        if (AccessoryEnablement.Sync(this))
            AbilityLoadout.Rebuild(Accessories);
    }
}
