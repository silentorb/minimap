namespace Minimap.Simulation;

public interface IWorldGenerator
{
    /// <summary>Fills the grid and returns player spawn positions (same length as desired player count).</summary>
    void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random);
}
