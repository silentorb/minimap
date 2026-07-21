namespace Minimap.Client;

/// <summary>
/// Client scene roots call this for extension preflight without referencing Minimap.App.
/// App registers the real loader (typically via module initializer).
/// </summary>
public static class ExtensionPreflight
{
    /// <summary>Absolute filesystem path → load extensions (throws on failure).</summary>
    public static Action<string>? LoadFromAbsolutePath { get; set; }

    public static void RequireLoadFromAbsolutePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var load = LoadFromAbsolutePath
            ?? throw new InvalidOperationException(
                "Extension preflight is not registered. Minimap.App must set ExtensionPreflight.LoadFromAbsolutePath.");
        load(path);
    }
}
