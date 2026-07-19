namespace Minimap.Simulation;

/// <summary>Godot-free 2D vector matching screen/world cartesian space (Y increases downward).</summary>
public readonly record struct SimVec2(float X, float Y)
{
    public static SimVec2 Zero => new(0f, 0f);

    public float Length => MathF.Sqrt(X * X + Y * Y);

    public float LengthSquared => X * X + Y * Y;

    public static SimVec2 operator +(SimVec2 a, SimVec2 b) => new(a.X + b.X, a.Y + b.Y);

    public static SimVec2 operator -(SimVec2 a, SimVec2 b) => new(a.X - b.X, a.Y - b.Y);

    public static SimVec2 operator -(SimVec2 v) => new(-v.X, -v.Y);

    public static SimVec2 operator *(SimVec2 v, float s) => new(v.X * s, v.Y * s);

    public static SimVec2 operator *(float s, SimVec2 v) => v * s;

    public static float Dot(SimVec2 a, SimVec2 b) => a.X * b.X + a.Y * b.Y;

    public SimVec2 Normalized()
    {
        var len = Length;
        if (len < 1e-8f)
            return Zero;
        return new SimVec2(X / len, Y / len);
    }

    public override string ToString() => $"({X}, {Y})";
}
