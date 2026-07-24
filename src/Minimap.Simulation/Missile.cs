namespace Minimap.Simulation;

/// <summary>Projectile owned by a faction (docs/technical/features/missiles-and-damage.md).</summary>
public sealed class Missile
{
    public Missile(
        int id,
        SimVec2 position,
        SimVec2 velocity,
        float radius,
        int damage,
        int ownerFactionId,
        int? ownerCharacterId,
        bool friendlyFire = true)
    {
        Id = id;
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Damage = damage;
        OwnerFactionId = ownerFactionId;
        OwnerCharacterId = ownerCharacterId;
        FriendlyFire = friendlyFire;
    }

    public int Id { get; }
    public SimVec2 Position { get; set; }
    public SimVec2 Velocity { get; set; }
    public float Radius { get; }
    public int Damage { get; }
    public int OwnerFactionId { get; }
    public int? OwnerCharacterId { get; }
    public bool FriendlyFire { get; }
}
