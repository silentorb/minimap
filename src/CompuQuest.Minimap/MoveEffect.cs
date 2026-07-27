using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Enables locomotion at a fixed speed (world units per second).</summary>
public sealed class MoveEffect : AccessoryEffect, IMoveEffect
{
    public MoveEffect(float speed)
    {
        if (speed < 0f)
            throw new ArgumentOutOfRangeException(nameof(speed));
        Speed = speed;
    }

    public float Speed { get; }

    public override AccessoryEffect Clone() => new MoveEffect(Speed);
}
