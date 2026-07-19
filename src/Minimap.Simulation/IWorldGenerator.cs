namespace Minimap.Simulation;

public interface IWorldGenerator
{
    /// <summary>Fills the grid with terrain (walls / hazards / floors).</summary>
    void GenerateTerrain(HexGrid grid, Random random);
}
