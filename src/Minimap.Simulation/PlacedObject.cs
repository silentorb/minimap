using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Static object occupying a single map cell (not a mobile character).</summary>
public sealed class PlacedObject
{
    public PlacedObject(int id, HexAxial cell, PlacedObjectDefinition definition)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(definition);

        Id = id;
        Cell = cell;
        Definition = definition;
    }

    public int Id { get; }

    public HexAxial Cell { get; }

    public PlacedObjectDefinition Definition { get; }
}
