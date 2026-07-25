namespace Minimap.Simulation;

/// <summary>Brief frontal half-disk attack for visualization and one-shot hit resolution.</summary>
public sealed class SwingArc
{
    public SwingArc(
        int id,
        SimVec2 origin,
        SimVec2 facing,
        float radius,
        float arcDegrees,
        int damage,
        int ownerFactionId,
        int? ownerCharacterId,
        bool friendlyFire,
        float lifetimeSeconds)
    {
        if (radius <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (arcDegrees <= 0f || arcDegrees > 360f)
            throw new ArgumentOutOfRangeException(nameof(arcDegrees));
        if (lifetimeSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds));

        Id = id;
        Origin = origin;
        Facing = facing.LengthSquared >= 1e-10f ? facing.Normalized() : new SimVec2(1f, 0f);
        Radius = radius;
        ArcDegrees = arcDegrees;
        Damage = damage;
        OwnerFactionId = ownerFactionId;
        OwnerCharacterId = ownerCharacterId;
        FriendlyFire = friendlyFire;
        TimeRemaining = lifetimeSeconds;
    }

    public int Id { get; }
    public SimVec2 Origin { get; }
    public SimVec2 Facing { get; }
    public float Radius { get; }
    public float ArcDegrees { get; }
    public int Damage { get; }
    public int OwnerFactionId { get; }
    public int? OwnerCharacterId { get; }
    public bool FriendlyFire { get; }
    public float TimeRemaining { get; set; }
}
