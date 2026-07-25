using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Swing attack parameters and per-instance cooldown.</summary>
public sealed class SwingEffect : AccessoryEffect, ISwingEffect, IEffectUseCost
{
    public SwingEffect(
        float fireIntervalSeconds,
        int damage,
        float radius,
        float arcDegrees,
        float visualDurationSeconds,
        bool friendlyFire = true,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        if (fireIntervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireIntervalSeconds));
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage));
        if (radius <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (arcDegrees <= 0f || arcDegrees > 360f)
            throw new ArgumentOutOfRangeException(nameof(arcDegrees));
        if (visualDurationSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(visualDurationSeconds));
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        FireIntervalSeconds = fireIntervalSeconds;
        Damage = damage;
        Radius = radius;
        ArcDegrees = arcDegrees;
        VisualDurationSeconds = visualDurationSeconds;
        FriendlyFire = friendlyFire;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public float FireIntervalSeconds { get; }
    public int Damage { get; }
    public float Radius { get; }
    public float ArcDegrees { get; }
    public float VisualDurationSeconds { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public override AccessoryEffect Clone() =>
        new SwingEffect(
            FireIntervalSeconds,
            Damage,
            Radius,
            ArcDegrees,
            VisualDurationSeconds,
            FriendlyFire,
            CostResourceTag,
            CostAmount);
}
