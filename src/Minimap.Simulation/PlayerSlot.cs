namespace Minimap.Simulation;

/// <summary>Logical player in cartesian world space (up to four local players).</summary>
public sealed class PlayerSlot
{
    public PlayerSlot(int index, SimVec2 position)
    {
        if (index is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
        Position = position;
    }

    public int Index { get; }
    public SimVec2 Position { get; set; }
}
