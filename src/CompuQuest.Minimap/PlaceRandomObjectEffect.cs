using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Places a random placed-object definition from a pool onto a valid grass cell.</summary>
public sealed class PlaceRandomObjectEffect : AccessoryEffect, ICellPlacementEffect
{
    private readonly List<string> _poolIds;

    public PlaceRandomObjectEffect(IEnumerable<string> poolIds)
    {
        ArgumentNullException.ThrowIfNull(poolIds);
        _poolIds = poolIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList();
        if (_poolIds.Count == 0)
            throw new ArgumentException("Placement pool must include at least one object id.", nameof(poolIds));
    }

    public IReadOnlyList<string> PoolIds => _poolIds;

    public bool CanPlace(GameWorld world, Character placer, HexAxial cell)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(placer);
        if (!world.Grid.Contains(cell))
            return false;
        if (world.Grid.Get(cell) != CellType.Grass)
            return false;
        if (world.IsCellOccupied(cell))
            return false;
        return true;
    }

    public bool TryPlace(GameWorld world, Character placer, HexAxial cell, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(placer);
        ArgumentNullException.ThrowIfNull(random);
        if (!CanPlace(world, placer, cell))
            return false;

        var id = _poolIds[random.Next(_poolIds.Count)];
        if (!world.TryGetPlacedObjectDefinition(id, out var definition) || definition is null)
            return false;

        return world.TryPlaceObject(cell, definition);
    }

    public override AccessoryEffect Clone() => new PlaceRandomObjectEffect(_poolIds);
}
