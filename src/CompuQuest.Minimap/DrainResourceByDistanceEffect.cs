using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Drains a resource from traveled distance (integer amounts as distance accumulates).</summary>
public sealed class DrainResourceByDistanceEffect : AccessoryEffect, IPassiveEffect
{
    private float _accumulator;
    private SimVec2? _lastPosition;

    public DrainResourceByDistanceEffect(TagId resourceTag, float unitsPerAmount)
    {
        if (unitsPerAmount <= 0f)
            throw new ArgumentOutOfRangeException(nameof(unitsPerAmount));

        ResourceTag = resourceTag;
        UnitsPerAmount = unitsPerAmount;
    }

    public TagId ResourceTag { get; }

    /// <summary>World units traveled per 1 resource drained.</summary>
    public float UnitsPerAmount { get; }

    public void Tick(Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var position = actor.Position;
        if (_lastPosition is not SimVec2 previous)
        {
            _lastPosition = position;
            return;
        }

        var delta = position - previous;
        _lastPosition = position;
        var distance = MathF.Sqrt(delta.LengthSquared);
        if (distance <= 0f)
            return;

        _accumulator += distance;
        var whole = (int)(_accumulator / UnitsPerAmount);
        if (whole <= 0)
            return;

        _accumulator -= whole * UnitsPerAmount;
        actor.AddResource(ResourceTag, -whole);
    }

    public override AccessoryEffect Clone() =>
        new DrainResourceByDistanceEffect(ResourceTag, UnitsPerAmount);
}
