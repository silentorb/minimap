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
        int maxHealth = CombatTuning.DefaultMaxHealth)
        : base(id, definition, resourceContext, applyDefinitionAccessories: false)
    {
        if (maxHealth < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth));

        FactionId = factionId;
        Position = position;
        AbilityLoadout = new AbilityLoadout();

        SetResource(ResourceContext.MaxHealthTag, maxHealth);
        SetResource(ResourceContext.HealthTag, maxHealth);

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

    public bool IsAlive => Health > 0;

    protected override void OnAccessoriesChanged() => AbilityLoadout.Rebuild(Accessories);
}
