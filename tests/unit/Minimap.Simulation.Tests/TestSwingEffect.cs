using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

internal sealed class TestSwingEffect : AccessoryEffect, ISwingEffect, IEffectUseCost
{
    public TestSwingEffect(
        float fireIntervalSeconds,
        int damage,
        float radius,
        float arcDegrees = 180f,
        float visualDurationSeconds = 0.15f,
        bool friendlyFire = true,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
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
        new TestSwingEffect(
            FireIntervalSeconds,
            Damage,
            Radius,
            ArcDegrees,
            VisualDurationSeconds,
            FriendlyFire,
            CostResourceTag,
            CostAmount);
}
