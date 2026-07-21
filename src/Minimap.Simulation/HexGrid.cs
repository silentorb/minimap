namespace Minimap.Simulation;

/// <summary>
/// Playable hex map. Default shape is a screen-space rectangle (see docs/technical/features/hex-grid-shape.md).
/// </summary>
public sealed class HexGrid
{
    private readonly Dictionary<HexAxial, CellType> _cells = new();

    /// <summary>Square convenience: same extent on both axes.</summary>
    public HexGrid(int radius)
        : this(radius, radius)
    {
    }

    public HexGrid(int radiusX, int radiusY, float hexSize = HexWorldLayout.DefaultHexSize)
    {
        if (radiusX < 0)
            throw new ArgumentOutOfRangeException(nameof(radiusX));
        if (radiusY < 0)
            throw new ArgumentOutOfRangeException(nameof(radiusY));
        RadiusX = radiusX;
        RadiusY = radiusY;
        foreach (var h in EnumerateRectangle(radiusX, radiusY, hexSize))
            _cells[h] = CellType.Floor;
    }

    public int RadiusX { get; }
    public int RadiusY { get; }

    /// <summary>Legacy alias for equal-axis grids; prefers RadiusX when unequal.</summary>
    public int Radius => RadiusX == RadiusY ? RadiusX : throw new InvalidOperationException("Grid has unequal axes; use RadiusX/RadiusY.");

    public int CellCount => _cells.Count;

    public bool Contains(HexAxial h) => _cells.ContainsKey(h);

    public CellType Get(HexAxial h) =>
        _cells.TryGetValue(h, out var t) ? t : CellType.Empty;

    public void Set(HexAxial h, CellType type)
    {
        if (!_cells.ContainsKey(h))
            throw new ArgumentOutOfRangeException(nameof(h), h, "Hex outside playable grid.");
        _cells[h] = type;
    }

    public IReadOnlyDictionary<HexAxial, CellType> Cells => _cells;

    public IEnumerable<HexAxial> AllHexes() => _cells.Keys;

    public static IEnumerable<HexAxial> EnumerateRectangle(int radiusX, int radiusY, float hexSize = HexWorldLayout.DefaultHexSize)
    {
        if (radiusX == 0 && radiusY == 0)
        {
            yield return new HexAxial(0, 0);
            yield break;
        }

        var maxX = MathF.Abs(HexWorldLayout.ToWorld(new HexAxial(radiusX, 0), hexSize).X);
        var maxY = MathF.Abs(HexWorldLayout.ToWorld(new HexAxial(0, radiusY), hexSize).Y);
        if (maxX < 1e-6f)
            maxX = 1e-6f;
        if (maxY < 1e-6f)
            maxY = 1e-6f;

        var bound = Math.Max(radiusX, radiusY) + 2;
        for (var q = -bound; q <= bound; q++)
        {
            for (var r = -bound; r <= bound; r++)
            {
                var h = new HexAxial(q, r);
                var p = HexWorldLayout.ToWorld(h, hexSize);
                if (MathF.Abs(p.X) <= maxX + 1e-5f && MathF.Abs(p.Y) <= maxY + 1e-5f)
                    yield return h;
            }
        }
    }

    /// <summary>Axial disk (legacy); kept for tests that need exact disk cell counts.</summary>
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
