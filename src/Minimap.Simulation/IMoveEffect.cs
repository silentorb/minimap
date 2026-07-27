namespace Minimap.Simulation;

/// <summary>Locomotion enable + speed (world units per second).</summary>
public interface IMoveEffect
{
    float Speed { get; }
}
