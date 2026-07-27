namespace Minimap.Simulation;

/// <summary>In-flight projectile state on a free actor (docs/technical/features/gameplay/missiles-and-damage.md).</summary>
public sealed class ProjectileFlight
{
    public ProjectileFlight(
        SimVec2 velocity,
        int damage,
        bool friendlyFire,
        int? ownerActorId,
        float size,
        float range,
        SimVec2 origin)
    {
        if (size <= 0f)
            throw new ArgumentOutOfRangeException(nameof(size));
        if (range <= 0f)
            throw new ArgumentOutOfRangeException(nameof(range));
        if (ownerActorId is < 0)
            throw new ArgumentOutOfRangeException(nameof(ownerActorId));

        Velocity = velocity;
        Damage = damage;
        FriendlyFire = friendlyFire;
        OwnerActorId = ownerActorId;
        Size = size;
        Range = range;
        Origin = origin;
        DistanceTraveled = 0f;
    }

    public SimVec2 Velocity { get; set; }
    public int Damage { get; }
    public bool FriendlyFire { get; }
    public int? OwnerActorId { get; }
    public float Size { get; }
    public float Range { get; }
    public SimVec2 Origin { get; }
    public float DistanceTraveled { get; set; }
}
