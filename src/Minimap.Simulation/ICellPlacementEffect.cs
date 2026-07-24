using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>
/// Contract for accessory effects that place actors on map cells.
/// Concrete implementations live in content extensions (e.g. CompuQuest).
/// </summary>
public interface ICellPlacementEffect
{
    bool CanPlace(GameWorld world, Character placer, HexAxial cell);

    /// <summary>Returns false for expected rejection; does not throw for those cases.</summary>
    bool TryPlace(GameWorld world, Character placer, HexAxial cell, Random random);
}
