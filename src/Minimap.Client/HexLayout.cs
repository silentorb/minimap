using Godot;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Pointy-top hex layout: axial → Godot world pixels.</summary>
public static class HexLayout
{
    public const float DefaultHexSize = 26f;

    public static Vector2 ToWorld(HexAxial h, float hexSize = DefaultHexSize)
    {
        var x = hexSize * (Mathf.Sqrt(3) * h.Q + Mathf.Sqrt(3) / 2f * h.R);
        var y = hexSize * (1.5f * h.R);
        return new Vector2(x, y);
    }

    /// <summary>Vertex radius = hexSize (center to corner), pointy-top.</summary>
    public static Vector2[] PointyHexPolygon(float hexSize = DefaultHexSize)
    {
        var pts = new Vector2[6];
        for (var i = 0; i < 6; i++)
        {
            var angle = Mathf.DegToRad(60f * i - 30f);
            pts[i] = new Vector2(hexSize * Mathf.Cos(angle), hexSize * Mathf.Sin(angle));
        }

        return pts;
    }
}
