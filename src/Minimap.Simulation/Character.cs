namespace Minimap.Simulation;

/// <summary>Possessable character pawn in cartesian world space.</summary>
public sealed class Character
{
    public Character(int id, int factionId, SimVec2 position, float maxHealth = CombatTuning.DefaultMaxHealth)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        Id = id;
        FactionId = factionId;
        Position = position;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public int Id { get; }
    public int FactionId { get; }
    public SimVec2 Position { get; set; }
    public float MaxHealth { get; }
    public float Health { get; set; }
    public SimVec2 MoveIntent { get; set; }
    public bool IsAlive => Health > 0f;
}
