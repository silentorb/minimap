using Godot;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Pointy-top hex layout: thin Godot wrapper over <see cref="HexWorldLayout"/>.</summary>
public static class HexLayout
{
    public const float DefaultHexSize = HexWorldLayout.DefaultHexSize;

    public static Vector2 ToWorld(HexAxial h, float hexSize = DefaultHexSize)
    {
        var v = HexWorldLayout.ToWorld(h, hexSize);
        return new Vector2(v.X, v.Y);
    }

    /// <summary>Vertex radius = hexSize (center to corner), pointy-top.</summary>
    public static Vector2[] PointyHexPolygon(float hexSize = DefaultHexSize)
    {
        var src = HexWorldLayout.PointyHexVertices(hexSize);
        var pts = new Vector2[src.Length];
        for (var i = 0; i < src.Length; i++)
            pts[i] = new Vector2(src[i].X, src[i].Y);
        return pts;
    }
}
