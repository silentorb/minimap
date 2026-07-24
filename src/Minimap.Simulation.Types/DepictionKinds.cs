namespace Minimap.Simulation.Types;

/// <summary>Known <see cref="DepictionConfig.Kind"/> discriminators.</summary>
public static class DepictionKinds
{
    /// <summary>Godot <c>SpriteFrames</c> resource at <see cref="DepictionConfig.ResourcePath"/>.</summary>
    public const string SpriteFrames = "sprite_frames";

    /// <summary>
    /// Static Godot texture (including imported SVG) at <see cref="DepictionConfig.ResourcePath"/>.
    /// Used for prototyping placed-object world art.
    /// </summary>
    public const string Texture = "texture";
}
