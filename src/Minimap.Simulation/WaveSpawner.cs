namespace Minimap.Simulation;

/// <summary>Map marker that emits hostile AI during waves.</summary>
public sealed class WaveSpawner
{
    public WaveSpawner(int id, HexAxial position)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        Id = id;
        Position = position;
    }

    public int Id { get; }
    public HexAxial Position { get; }
}
