namespace Minimap.Simulation;

/// <summary>Authoritative simulation state: terrain and player slots.</summary>
public sealed class GameWorld
{
    public static GameWorld Create(int gridRadius, int playerCount, int seed, IWorldGenerator? generator = null)
    {
        if (playerCount is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(playerCount));
        var grid = new HexGrid(gridRadius);
        var gen = generator ?? new SeededWorldGenerator();
        var rng = new Random(seed);
        var slots = new PlayerSlot[playerCount];
        gen.Generate(grid, slots.AsSpan(), rng);
        return new GameWorld(grid, slots);
    }

    public GameWorld(HexGrid grid, IReadOnlyList<PlayerSlot> players)
    {
        Grid = grid;
        Players = players.ToArray();
        if (Players.Length > 4)
            throw new ArgumentException("At most four players.", nameof(players));
    }

    public HexGrid Grid { get; }
    public PlayerSlot[] Players { get; }
    public int TickIndex { get; private set; }

    public void AdvanceTick() => TickIndex++;

    /// <summary>Try to move player one step in direction 0..5 (axial neighbor index).</summary>
    public bool TryMovePlayer(int playerIndex, int neighborDirectionIndex)
    {
        if ((uint)playerIndex >= (uint)Players.Length)
            return false;
        if ((uint)neighborDirectionIndex >= 6u)
            return false;
        var p = Players[playerIndex];
        var dir = HexAxial.NeighborOffsets[neighborDirectionIndex];
        var next = p.Position + dir;
        if (!Grid.Contains(next))
            return false;
        if (Grid.Get(next) is CellType.Wall)
            return false;
        p.Position = next;
        return true;
    }
}
