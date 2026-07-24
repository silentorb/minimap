using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test double for IShootEffect with optional use cost.</summary>
internal sealed class TestShootEffect : AccessoryEffect, IShootEffect, IEffectUseCost
{
    public TestShootEffect(
        float fireIntervalSeconds,
        float missileSpeed,
        int missileDamage,
        bool friendlyFire = true,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        FireIntervalSeconds = fireIntervalSeconds;
        MissileSpeed = missileSpeed;
        MissileDamage = missileDamage;
        FriendlyFire = friendlyFire;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public float FireIntervalSeconds { get; }
    public float MissileSpeed { get; }
    public int MissileDamage { get; }
    public bool FriendlyFire { get; }
    public float CooldownRemaining { get; set; }
    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public override AccessoryEffect Clone() =>
        new TestShootEffect(
            FireIntervalSeconds,
            MissileSpeed,
            MissileDamage,
            FriendlyFire,
            CostResourceTag,
            CostAmount);
}

/// <summary>Test double for on-acquire resource grant.</summary>
internal sealed class TestGrantResourceEffect : AccessoryEffect, IOnAccessoryAcquired
{
    public TestGrantResourceEffect(TagId resourceTag, int amount)
    {
        ResourceTag = resourceTag;
        Amount = amount;
    }

    public TagId ResourceTag { get; }
    public int Amount { get; }

    public void OnAcquired(Actor actor) => actor.AddResource(ResourceTag, Amount);

    public override AccessoryEffect Clone() => new TestGrantResourceEffect(ResourceTag, Amount);
}
