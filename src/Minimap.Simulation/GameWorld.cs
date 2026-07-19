namespace Minimap.Simulation;

/// <summary>Authoritative simulation state: terrain and player slots in cartesian space.</summary>
public sealed class GameWorld
{
    private readonly SimVec2[] _inputs;
    private readonly List<SimVec2[]> _wallPolygons = new();

    public static GameWorld Create(
        int gridRadius,
        int playerCount,
        int seed,
        IWorldGenerator? generator = null,
        float hexSize = HexWorldLayout.DefaultHexSize)
    {
        if (playerCount is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(playerCount));
        var grid = new HexGrid(gridRadius);
        var gen = generator ?? new SeededWorldGenerator();
        var rng = new Random(seed);
        var slots = new PlayerSlot[playerCount];
        gen.Generate(grid, slots.AsSpan(), rng, hexSize);
        return new GameWorld(grid, slots, hexSize);
    }

    public GameWorld(HexGrid grid, IReadOnlyList<PlayerSlot> players, float hexSize = HexWorldLayout.DefaultHexSize)
    {
        Grid = grid;
        Players = players.ToArray();
        if (Players.Length > 4)
            throw new ArgumentException("At most four players.", nameof(players));
        HexSize = hexSize;
        MoveSpeed = 120f;
        PlayerRadius = hexSize * 0.35f;
        _inputs = new SimVec2[Players.Length];
        RebuildWallColliders();
    }

    public HexGrid Grid { get; }
    public PlayerSlot[] Players { get; }
    public float HexSize { get; }
    public float MoveSpeed { get; set; }
    public float PlayerRadius { get; set; }
    public int TickIndex { get; private set; }

    /// <summary>Solid hex polygons (walls + out-of-map boundary cells).</summary>
    public IReadOnlyList<SimVec2[]> WallPolygons => _wallPolygons;

    public void AdvanceTick() => TickIndex++;

    /// <summary>Set desired move direction for a player (screen axes; will be normalized on tick).</summary>
    public void SetPlayerInput(int playerIndex, SimVec2 direction)
    {
        if ((uint)playerIndex >= (uint)Players.Length)
            return;
        _inputs[playerIndex] = direction;
    }

    /// <summary>Integrate held input and resolve circle vs exact wall hexes with DOOM-style slide.</summary>
    public void TickMovement(float dt)
    {
        if (dt <= 0f)
            return;

        for (var i = 0; i < Players.Length; i++)
        {
            var input = _inputs[i];
            if (input.LengthSquared < 1e-10f)
                continue;

            var dir = input.Normalized();
            var displacement = dir * (MoveSpeed * dt);
            Players[i].Position = CircleHexCollision.MoveAndSlide(
                Players[i].Position,
                displacement,
                PlayerRadius,
                _wallPolygons);
        }
    }

    /// <summary>Rebuild solid hex colliders from current terrain (call after evolution).</summary>
    public void RebuildWallColliders()
    {
        _wallPolygons.Clear();
        var seen = new HashSet<HexAxial>();

        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) is CellType.Wall)
                AddWallHex(h, seen);

            foreach (var n in h.Neighbors())
            {
                if (!Grid.Contains(n))
                    AddWallHex(n, seen);
            }
        }
    }

    private void AddWallHex(HexAxial h, HashSet<HexAxial> seen)
    {
        if (!seen.Add(h))
            return;
        _wallPolygons.Add(HexWorldLayout.AbsoluteHexVertices(h, HexSize));
    }
}
