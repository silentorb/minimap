namespace Minimap.Simulation;

/// <summary>Maps continuous facing to the hex cell in front of a character.</summary>
public static class CellFacing
{
    public static HexAxial NeighborOffsetForFacing(SimVec2 facing)
    {
        if (facing.LengthSquared < 1e-10f)
            return HexAxial.NeighborOffsets[0];

        var dir = facing.Normalized();
        var best = HexAxial.NeighborOffsets[0];
        var bestDot = float.NegativeInfinity;
        foreach (var offset in HexAxial.NeighborOffsets)
        {
            var world = HexWorldLayout.ToWorld(offset, 1f);
            if (world.LengthSquared < 1e-10f)
                continue;
            var dot = SimVec2.Dot(dir, world.Normalized());
            if (dot > bestDot)
            {
                bestDot = dot;
                best = offset;
            }
        }

        return best;
    }

    public static HexAxial CellInFront(Character character, float hexSize)
    {
        ArgumentNullException.ThrowIfNull(character);
        var cell = HexWorldLayout.WorldToAxial(character.Position, hexSize);
        return cell + NeighborOffsetForFacing(character.Facing);
    }
}
