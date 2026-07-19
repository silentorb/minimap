namespace Minimap.Simulation;

/// <summary>Circle (player) vs convex hex (wall cell) overlap and DOOM-style slide movement.</summary>
public static class CircleHexCollision
{
    private const float Epsilon = 1e-5f;

    /// <summary>
    /// Move a circle by <paramref name="displacement"/>, sliding along hex walls on angled contact.
    /// Clips the into-wall component before applying motion (DOOM-style), so glancing hits keep tangential progress.
    /// </summary>
    public static SimVec2 MoveAndSlide(
        SimVec2 position,
        SimVec2 displacement,
        float radius,
        IReadOnlyList<SimVec2[]> walls,
        int maxIterations = 4)
    {
        var pos = position;
        Depenetrate(ref pos, radius, walls);

        var remaining = displacement;
        for (var iter = 0; iter < maxIterations; iter++)
        {
            if (remaining.LengthSquared < Epsilon * Epsilon)
                break;

            var target = pos + remaining;
            if (!TryGetPenetration(target, radius, walls, out var normal, out _))
            {
                pos = target;
                break;
            }

            // Cancel only the into-wall component, then retry with clipped motion (no double-apply).
            var into = SimVec2.Dot(remaining, normal);
            if (into >= 0f)
                break;

            remaining -= normal * into;
        }

        Depenetrate(ref pos, radius, walls);
        return pos;
    }

    public static bool TryGetPenetration(
        SimVec2 center,
        float radius,
        IReadOnlyList<SimVec2[]> walls,
        out SimVec2 normal,
        out float depth)
    {
        normal = SimVec2.Zero;
        depth = 0f;
        var found = false;

        foreach (var wall in walls)
        {
            if (!TryCircleConvex(center, radius, wall, out var n, out var d))
                continue;
            if (!found || d > depth)
            {
                found = true;
                depth = d;
                normal = n;
            }
        }

        return found;
    }

    private static void Depenetrate(ref SimVec2 pos, float radius, IReadOnlyList<SimVec2[]> walls)
    {
        for (var i = 0; i < 8; i++)
        {
            if (!TryGetPenetration(pos, radius, walls, out var normal, out var depth))
                return;
            if (depth <= Epsilon)
                return;
            pos += normal * (depth + Epsilon);
        }
    }

    /// <summary>True if the circle overlaps a solid convex polygon; normal pushes the circle out.</summary>
    public static bool TryCircleConvex(
        SimVec2 center,
        float radius,
        SimVec2[] verts,
        out SimVec2 normal,
        out float penetration)
    {
        normal = SimVec2.Zero;
        penetration = 0f;
        if (verts.Length < 3)
            return false;

        var closest = ClosestPointOnConvex(center, verts, out var inside);
        var delta = center - closest;
        var distSq = delta.LengthSquared;

        if (inside)
        {
            // Center is inside the solid; push out past the boundary by radius.
            if (distSq < Epsilon * Epsilon)
            {
                // Degenerate: use nearest-edge outward normal.
                if (!TryNearestEdgeOutward(center, verts, out normal, out var edgeDist))
                    return false;
                penetration = radius + edgeDist;
                return penetration > Epsilon;
            }

            normal = (-delta).Normalized();
            penetration = radius + MathF.Sqrt(distSq);
            return true;
        }

        if (distSq >= radius * radius)
            return false;

        var dist = MathF.Sqrt(distSq);
        if (dist < Epsilon)
        {
            if (!TryNearestEdgeOutward(center, verts, out normal, out _))
                return false;
            penetration = radius;
            return true;
        }

        normal = delta * (1f / dist);
        penetration = radius - dist;
        return penetration > Epsilon;
    }

    private static SimVec2 ClosestPointOnConvex(SimVec2 p, SimVec2[] verts, out bool inside)
    {
        inside = IsInsideConvex(p, verts);
        var best = verts[0];
        var bestDist = float.MaxValue;
        for (var i = 0; i < verts.Length; i++)
        {
            var a = verts[i];
            var b = verts[(i + 1) % verts.Length];
            var c = ClosestOnSegment(p, a, b);
            var d = (p - c).LengthSquared;
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return best;
    }

    private static bool IsInsideConvex(SimVec2 p, SimVec2[] verts)
    {
        // CCW winding from HexWorldLayout: cross >= 0 means left of edge (inside).
        for (var i = 0; i < verts.Length; i++)
        {
            var a = verts[i];
            var b = verts[(i + 1) % verts.Length];
            var cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
            if (cross < -Epsilon)
                return false;
        }

        return true;
    }

    private static SimVec2 ClosestOnSegment(SimVec2 p, SimVec2 a, SimVec2 b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSquared;
        if (lenSq < Epsilon * Epsilon)
            return a;
        var t = SimVec2.Dot(p - a, ab) / lenSq;
        if (t <= 0f)
            return a;
        if (t >= 1f)
            return b;
        return a + ab * t;
    }

    private static bool TryNearestEdgeOutward(
        SimVec2 p,
        SimVec2[] verts,
        out SimVec2 outward,
        out float distToEdge)
    {
        outward = SimVec2.Zero;
        distToEdge = float.MaxValue;
        var found = false;

        for (var i = 0; i < verts.Length; i++)
        {
            var a = verts[i];
            var b = verts[(i + 1) % verts.Length];
            var edge = b - a;
            var len = edge.Length;
            if (len < Epsilon)
                continue;

            // Outward normal for CCW polygon: rotate edge 90° CW → (ey, -ex)
            var n = new SimVec2(edge.Y / len, -edge.X / len);
            var d = SimVec2.Dot(p - a, n);
            var abs = MathF.Abs(d);
            if (!found || abs < distToEdge)
            {
                found = true;
                distToEdge = abs;
                outward = n;
            }
        }

        return found;
    }
}
