using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test doubles for grow / harvest / interaction.</summary>
internal sealed class TestGrowEffect : AccessoryEffect, IGrowEffect
{
    private float _elapsed;

    public TestGrowEffect(
        float durationSeconds,
        DepictionConfig matureDepiction,
        TagId yieldResourceTag,
        int yieldAmount)
    {
        DurationSeconds = durationSeconds;
        MatureDepiction = matureDepiction;
        YieldResourceTag = yieldResourceTag;
        YieldAmount = yieldAmount;
    }

    public float DurationSeconds { get; }
    public DepictionConfig MatureDepiction { get; }
    public TagId? YieldResourceTag { get; }
    public int YieldAmount { get; }
    public bool IsMature => _elapsed >= DurationSeconds;

    public void Tick(Actor actor, float dt)
    {
        if (dt <= 0f || IsMature)
            return;
        _elapsed += dt;
        if (IsMature)
            actor.DepictionOverride = MatureDepiction;
    }

    public override AccessoryEffect Clone() =>
        new TestGrowEffect(DurationSeconds, MatureDepiction, YieldResourceTag!.Value, YieldAmount);
}

internal sealed class TestHarvestEffect : AccessoryEffect, IInteractionEffect
{
    public bool CanInteract(GameWorld world, Actor actor, Actor target)
    {
        foreach (var effect in target.Effects)
        {
            if (effect is IGrowEffect grow && grow.IsMature)
                return true;
        }

        return false;
    }

    public bool TryInteract(GameWorld world, Actor actor, Actor target)
    {
        if (!CanInteract(world, actor, target))
            return false;

        IGrowEffect? grow = null;
        foreach (var effect in target.Effects)
        {
            if (effect is IGrowEffect g && g.IsMature)
            {
                grow = g;
                break;
            }
        }

        if (grow is null || target.Cell is not { } cell)
            return false;
        if (!world.TryRemoveActorAt(cell, out _))
            return false;
        if (grow.YieldResourceTag is { } tag && grow.YieldAmount > 0)
            actor.AddResource(tag, grow.YieldAmount);
        return true;
    }

    public override AccessoryEffect Clone() => new TestHarvestEffect();
}
