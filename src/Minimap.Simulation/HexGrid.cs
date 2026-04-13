namespace Minimap.Simulation;

/// <summary>Hex disk centered at origin with the given axial radius (inclusive).</summary>
public sealed class HexGrid
{
    private readonly Dictionary<HexAxial, CellType> _cells = new();

    public HexGrid(int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        Radius = radius;
        foreach (var h in EnumerateDisk(radius))
            _cells[h] = CellType.Floor;
    }

    public int Radius { get; }

    public int CellCount => _cells.Count;

    public bool Contains(HexAxial h) => _cells.ContainsKey(h);

    public CellType Get(HexAxial h) =>
        _cells.TryGetValue(h, out var t) ? t : CellType.Empty;

    public void Set(HexAxial h, CellType type)
    {
        if (!_cells.ContainsKey(h))
            throw new ArgumentOutOfRangeException(nameof(h), h, "Hex outside grid disk.");
        _cells[h] = type;
    }

    public IReadOnlyDictionary<HexAxial, CellType> Cells => _cells;

    public IEnumerable<HexAxial> AllHexes() => _cells.Keys;

    public static IEnumerable<HexAxial> EnumerateDisk(int radius)
    {
        for (var q = -radius; q <= radius; q++)
        {
            var r1 = Math.Max(-radius, -q - radius);
            var r2 = Math.Min(radius, -q + radius);
            for (var r = r1; r <= r2; r++)
                yield return new HexAxial(q, r);
        }
    }
}
