namespace Minimap.Simulation;

/// <summary>
/// Accessory effect that can place content on a map cell.
/// Concrete implementations live in content extensions (e.g. CompuQuest).
/// </summary>
public interface ICellPlacementEffect
{
    bool CanPlace(GameWorld world, Character placer, HexAxial cell);

    /// <summary>
    /// Attempts placement. Returns false for expected failures (invalid cell);
    /// does not throw for those cases.
    /// </summary>
    bool TryPlace(GameWorld world, Character placer, HexAxial cell, Random random);
}
