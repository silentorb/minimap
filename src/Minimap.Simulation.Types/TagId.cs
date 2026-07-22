namespace Minimap.Simulation.Types;

/// <summary>Runtime-only tag identity. Not stable across process runs.</summary>
public readonly struct TagId : IEquatable<TagId>
{
    public TagId(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        Value = value;
    }

    public int Value { get; }

    public bool Equals(TagId other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is TagId other && Equals(other);

    public override int GetHashCode() => Value;

    public static bool operator ==(TagId left, TagId right) => left.Equals(right);

    public static bool operator !=(TagId left, TagId right) => !left.Equals(right);

    public override string ToString() => Value.ToString();
}
