namespace Minimap.App;

/// <summary>
/// Mirrors an extension config tree into a deploy directory (wipe destination, then copy).
/// Used so gitignored <c>extensions/*/</c> cannot keep retired JSON after pull+rebuild.
/// </summary>
public static class ExtensionContentMirror
{
    /// <summary>
    /// Deletes <paramref name="destinationDirectory"/> if it exists, recreates it, and copies every
    /// <c>*.json</c> under <paramref name="sourceDirectory"/> preserving relative paths.
    /// </summary>
    public static void MirrorJsonTree(string sourceDirectory, string destinationDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);

        var sourceRoot = Path.GetFullPath(sourceDirectory);
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException($"Extension config source not found: {sourceRoot}");

        var destRoot = Path.GetFullPath(destinationDirectory);
        if (Directory.Exists(destRoot))
            Directory.Delete(destRoot, recursive: true);

        Directory.CreateDirectory(destRoot);

        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*.json", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, sourcePath);
            var destPath = Path.Combine(destRoot, relative);
            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
                Directory.CreateDirectory(destDir);
            File.Copy(sourcePath, destPath, overwrite: true);
        }
    }
}
