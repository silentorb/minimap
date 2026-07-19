namespace Minimap.Simulation;

/// <summary>Godot-free 2D integer vector (e.g. axial half-extents, grid indices).</summary>
public readonly record struct SimVec2I(int X, int Y)
{
    public static SimVec2I Zero => new(0, 0);

    public override string ToString() => $"({X}, {Y})";
}
