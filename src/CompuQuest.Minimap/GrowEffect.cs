using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Grows a planted food actor from seedling to mature/harvestable.</summary>
public sealed class GrowEffect : AccessoryEffect, IGrowEffect
{
    private float _elapsed;

    public GrowEffect(
        float durationSeconds,
        DepictionConfig matureDepiction,
        TagId yieldResourceTag,
        int yieldAmount)
    {
        if (durationSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        ArgumentNullException.ThrowIfNull(matureDepiction);
        if (yieldAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(yieldAmount));

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
        ArgumentNullException.ThrowIfNull(actor);
        if (dt <= 0f || IsMature)
            return;

        _elapsed += dt;
        if (!IsMature)
            return;

        actor.DepictionOverride = MatureDepiction;
    }

    public override AccessoryEffect Clone() =>
        new GrowEffect(DurationSeconds, MatureDepiction, YieldResourceTag!.Value, YieldAmount);
}
