using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Drains a resource at a continuous rate (integer amounts applied as they accumulate).</summary>
public sealed class DrainResourceEffect : AccessoryEffect, IPassiveEffect
{
    private float _accumulator;

    public DrainResourceEffect(TagId resourceTag, float amountPerSecond)
    {
        if (amountPerSecond < 0f)
            throw new ArgumentOutOfRangeException(nameof(amountPerSecond));

        ResourceTag = resourceTag;
        AmountPerSecond = amountPerSecond;
    }

    public TagId ResourceTag { get; }

    public float AmountPerSecond { get; }

    public void Tick(Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (dt <= 0f || AmountPerSecond <= 0f)
            return;

        _accumulator += AmountPerSecond * dt;
        var whole = (int)_accumulator;
        if (whole <= 0)
            return;

        _accumulator -= whole;
        actor.AddResource(ResourceTag, -whole);
    }

    public override AccessoryEffect Clone() => new DrainResourceEffect(ResourceTag, AmountPerSecond);
}
