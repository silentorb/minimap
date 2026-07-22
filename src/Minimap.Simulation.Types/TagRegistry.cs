namespace Minimap.Simulation.Types;

/// <summary>
/// Instance-owned tag catalog. Resolves string tags to <see cref="TagId"/> via create-if-not-exists.
/// Ids are not stable across process runs.
/// </summary>
public sealed class TagRegistry
{
    private readonly Dictionary<string, TagId> _byName = new(StringComparer.Ordinal);
    private readonly List<string> _namesById = new();

    public int Count => _namesById.Count;

    /// <summary>Register <paramref name="name"/> if missing; return its id either way.</summary>
    public TagId GetOrCreate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var key = name.Trim();
        if (_byName.TryGetValue(key, out var existing))
            return existing;

        var id = new TagId(_namesById.Count);
        _namesById.Add(key);
        _byName.Add(key, id);
        return id;
    }

    public bool TryGet(string name, out TagId id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            id = default;
            return false;
        }

        return _byName.TryGetValue(name.Trim(), out id);
    }

    public bool TryGetName(TagId id, out string? name)
    {
        if (id.Value < 0 || id.Value >= _namesById.Count)
        {
            name = null;
            return false;
        }

        name = _namesById[id.Value];
        return true;
    }
}
