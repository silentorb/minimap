namespace Minimap.Simulation.Types;

/// <summary>
/// Opaque visual presentation reference for content definitions.
/// Client resolves <see cref="Kind"/> (e.g. <see cref="DepictionKinds.SpriteFrames"/>);
/// Simulation does not interpret Godot resources.
/// </summary>
public sealed class DepictionConfig
{
    public DepictionConfig(string kind, string resourcePath, string? defaultAnimation = null)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new ArgumentException("Depiction kind must be non-empty.", nameof(kind));
        if (string.IsNullOrWhiteSpace(resourcePath))
            throw new ArgumentException("Depiction resource path must be non-empty.", nameof(resourcePath));

        Kind = kind;
        ResourcePath = resourcePath;
        DefaultAnimation = string.IsNullOrWhiteSpace(defaultAnimation) ? null : defaultAnimation;
    }

    public string Kind { get; }

    public string ResourcePath { get; }

    /// <summary>Optional animation name within a SpriteFrames (or similar) resource.</summary>
    public string? DefaultAnimation { get; }
}
