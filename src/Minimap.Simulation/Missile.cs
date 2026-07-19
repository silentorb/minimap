namespace Minimap.Simulation;

/// <summary>Projectile owned by a faction (docs/technical/features/missiles-and-damage.md).</summary>
public sealed class Missile
{
    public Missile(
        int id,
        SimVec2 position,
        SimVec2 velocity,
        float radius,
        float damage,
        int ownerFactionId,
        int? ownerCharacterId)
    {
        Id = id;
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Damage = damage;
        OwnerFactionId = ownerFactionId;
        OwnerCharacterId = ownerCharacterId;
    }

    public int Id { get; }
    public SimVec2 Position { get; set; }
    public SimVec2 Velocity { get; set; }
    public float Radius { get; }
    public float Damage { get; }
    public int OwnerFactionId { get; }
    public int? OwnerCharacterId { get; }
}
