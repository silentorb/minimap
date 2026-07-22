namespace Minimap.Simulation.Types;

/// <summary>One weighted candidate in a <see cref="WeightedPool{T}"/>.</summary>
public readonly struct WeightedEntry<T>
{
    public WeightedEntry(T item, double weight)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (weight <= 0 || double.IsNaN(weight) || double.IsInfinity(weight))
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be a finite value > 0.");

        Item = item;
        Weight = weight;
    }

    public T Item { get; }

    public double Weight { get; }
}
