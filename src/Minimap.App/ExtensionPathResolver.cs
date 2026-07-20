namespace Minimap.App;

/// <summary>Resolves extension assembly paths from settings search paths.</summary>
public static class ExtensionPathResolver
{
    public static string ResolveAssemblyPath(
        string entry,
        IReadOnlyList<string> searchPaths,
        string configDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        ArgumentNullException.ThrowIfNull(searchPaths);
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);

        if (Path.IsPathRooted(entry))
        {
            var absolute = Path.GetFullPath(entry);
            if (!File.Exists(absolute))
                throw new FileNotFoundException($"Extension library not found: {absolute}", absolute);
            return absolute;
        }

        var bases = GetSearchBases(configDirectory);
        foreach (var search in searchPaths)
        {
            if (string.IsNullOrWhiteSpace(search))
                continue;

            if (Path.IsPathRooted(search))
            {
                var candidate = Path.GetFullPath(Path.Combine(search, entry));
                if (File.Exists(candidate))
                    return candidate;
                continue;
            }

            foreach (var baseDir in bases)
            {
                var dir = Path.GetFullPath(Path.Combine(baseDir, search));
                var candidate = Path.GetFullPath(Path.Combine(dir, entry));
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        throw new FileNotFoundException(
            $"Extension library '{entry}' not found in search paths: {string.Join(", ", searchPaths)}.",
            entry);
    }

    internal static IReadOnlyList<string> GetSearchBases(string configDirectory)
    {
        var bases = new List<string>();
        var configDir = Path.GetFullPath(configDirectory);
        AddUnique(bases, configDir);

        var parent = Directory.GetParent(configDir);
        if (parent is not null)
            AddUnique(bases, parent.FullName);

        AddUnique(bases, Path.GetFullPath(Directory.GetCurrentDirectory()));
        return bases;
    }

    private static void AddUnique(List<string> bases, string path)
    {
        foreach (var existing in bases)
        {
            if (string.Equals(existing, path, StringComparison.OrdinalIgnoreCase))
                return;
        }

        bases.Add(path);
    }
}
