namespace Minimap.Simulation;

public interface IWorldGenerator
{
    /// <summary>Fills the grid and places players at floor hex world centers.</summary>
    void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random, float hexSize = HexWorldLayout.DefaultHexSize);
}
