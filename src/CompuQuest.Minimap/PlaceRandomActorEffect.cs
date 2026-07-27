using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Places a random actor from a weighted pool on a grass cell.</summary>
public sealed class PlaceRandomActorEffect : AccessoryEffect, ICellPlacementEffect, IEffectUseCost
{
    private readonly WeightedPool<string> _pool;

    public PlaceRandomActorEffect(
        WeightedPool<string> pool,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        ArgumentNullException.ThrowIfNull(pool);
        if (pool.IsEmpty)
            throw new ArgumentException("Placement pool must be non-empty.", nameof(pool));
        if (costAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(costAmount));
        if (costResourceTag is null && costAmount != 0)
            throw new ArgumentException("Cost amount requires a cost resource tag.", nameof(costAmount));

        _pool = pool;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool CanPlace(GameWorld world, Actor placer, HexAxial cell)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(placer);

        if (!world.Grid.Contains(cell))
            return false;
        if (world.Grid.Get(cell) != CellType.Grass)
            return false;
        if (world.IsCellOccupied(cell))
            return false;
        if (!EffectUseCosts.CanAfford(placer, this))
            return false;

        return true;
    }

    public bool TryPlace(GameWorld world, Actor placer, HexAxial cell, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(placer);
        ArgumentNullException.ThrowIfNull(random);

        if (!CanPlace(world, placer, cell))
            return false;

        if (!_pool.TryPick(random, out var id) || string.IsNullOrWhiteSpace(id))
            return false;

        if (!world.TryGetActorDefinition(id, out var definition) || definition is null)
            return false;

        if (!world.TryPlaceActor(cell, definition, placer.FactionId))
            return false;

        EffectUseCosts.TryConsume(placer, this);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new PlaceRandomActorEffect(_pool, CostResourceTag, CostAmount);
}
