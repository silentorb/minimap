namespace Minimap.Simulation.Types;

/// <summary>
/// Opaque UI icon reference for content definitions (data-record display).
/// Client resolves <see cref="ResourcePath"/> as a Godot texture (e.g. SVG);
/// Simulation does not interpret Godot resources. Not used for world depiction.
/// </summary>
public sealed class IconConfig
{
    public IconConfig(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
            throw new ArgumentException("Icon resource path must be non-empty.", nameof(resourcePath));

        ResourcePath = resourcePath;
    }

    public string ResourcePath { get; }
}
