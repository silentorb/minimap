namespace Minimap.Simulation.Types;

/// <summary>Immutable weighted bag with optional random pick.</summary>
public sealed class WeightedPool<T>
{
    private readonly WeightedEntry<T>[] _entries;
    private readonly double _totalWeight;

    public WeightedPool(IEnumerable<WeightedEntry<T>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entries = entries.ToArray();
        _totalWeight = 0;
        foreach (var entry in _entries)
            _totalWeight += entry.Weight;
    }

    public static WeightedPool<T> Empty { get; } = new(Array.Empty<WeightedEntry<T>>());

    public IReadOnlyList<WeightedEntry<T>> Entries => _entries;

    public bool IsEmpty => _entries.Length == 0;

    public int Count => _entries.Length;

    /// <summary>Pick one item by weight. Returns false when the pool is empty.</summary>
    public bool TryPick(Random random, out T? item)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (_entries.Length == 0)
        {
            item = default;
            return false;
        }

        var roll = random.NextDouble() * _totalWeight;
        var cumulative = 0.0;
        foreach (var entry in _entries)
        {
            cumulative += entry.Weight;
            if (roll < cumulative)
            {
                item = entry.Item;
                return true;
            }
        }

        item = _entries[^1].Item;
        return true;
    }
}
