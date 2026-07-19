namespace Minimap.Simulation;

/// <summary>Pointy-top hex layout in cartesian world units (shared by sim collision and client render).</summary>
public static class HexWorldLayout
{
    public const float DefaultHexSize = 26f;

    public static SimVec2 ToWorld(HexAxial h, float hexSize = DefaultHexSize)
    {
        var x = hexSize * (MathF.Sqrt(3f) * h.Q + MathF.Sqrt(3f) / 2f * h.R);
        var y = hexSize * (1.5f * h.R);
        return new SimVec2(x, y);
    }

    /// <summary>Local vertices relative to hex center; vertex radius = hexSize (center to corner).</summary>
    public static SimVec2[] PointyHexVertices(float hexSize = DefaultHexSize)
    {
        var pts = new SimVec2[6];
        for (var i = 0; i < 6; i++)
        {
            var angle = (60f * i - 30f) * (MathF.PI / 180f);
            pts[i] = new SimVec2(hexSize * MathF.Cos(angle), hexSize * MathF.Sin(angle));
        }

        return pts;
    }

    /// <summary>Absolute world-space vertices for a cell (exact pointy-top hex).</summary>
    public static SimVec2[] AbsoluteHexVertices(HexAxial h, float hexSize = DefaultHexSize)
    {
        var center = ToWorld(h, hexSize);
        var local = PointyHexVertices(hexSize);
        var pts = new SimVec2[6];
        for (var i = 0; i < 6; i++)
            pts[i] = center + local[i];
        return pts;
    }

    /// <summary>Round a world position to the nearest axial hex (pointy-top).</summary>
    public static HexAxial WorldToAxial(SimVec2 world, float hexSize = DefaultHexSize)
    {
        var q = (MathF.Sqrt(3f) / 3f * world.X - 1f / 3f * world.Y) / hexSize;
        var r = (2f / 3f * world.Y) / hexSize;
        return CubeRound(q, r, -q - r);
    }

    private static HexAxial CubeRound(float fracQ, float fracR, float fracS)
    {
        var q = MathF.Round(fracQ);
        var r = MathF.Round(fracR);
        var s = MathF.Round(fracS);

        var qDiff = MathF.Abs(q - fracQ);
        var rDiff = MathF.Abs(r - fracR);
        var sDiff = MathF.Abs(s - fracS);

        if (qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if (rDiff > sDiff)
            r = -q - s;

        return new HexAxial((int)q, (int)r);
    }
}
