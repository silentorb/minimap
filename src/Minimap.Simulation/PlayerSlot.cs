namespace Minimap.Simulation;

/// <summary>Logical player position on the hex grid (up to four local players).</summary>
public sealed class PlayerSlot
{
    public PlayerSlot(int index, HexAxial position)
    {
        if (index is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
        Position = position;
    }

    public int Index { get; }
    public HexAxial Position { get; set; }
}
