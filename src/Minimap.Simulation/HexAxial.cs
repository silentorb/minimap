namespace Minimap.Simulation;

/// <summary>Axial coordinates on a pointy-top hex grid.</summary>
public readonly record struct HexAxial(int Q, int R)
{
    public static readonly HexAxial[] NeighborOffsets =
    [
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1),
    ];

    public static HexAxial operator +(HexAxial a, HexAxial b) => new(a.Q + b.Q, a.R + b.R);

    public IEnumerable<HexAxial> Neighbors()
    {
        foreach (var o in NeighborOffsets)
            yield return this + o;
    }

    /// <summary>Cube distance between two hexes (integer steps).</summary>
    public static int Distance(HexAxial a, HexAxial b)
    {
        var dq = a.Q - b.Q;
        var dr = a.R - b.R;
        var ds = -a.Q - a.R - (-b.Q - b.R);
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
    }
}
