namespace Minimap.Simulation.Types;

/// <summary>Godot-free RGB color with components in 0–1.</summary>
public readonly struct ColorRgb : IEquatable<ColorRgb>
{
    public ColorRgb(float r, float g, float b)
    {
        if (r is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(r), "R must be in [0, 1].");
        if (g is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(g), "G must be in [0, 1].");
        if (b is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(b), "B must be in [0, 1].");

        R = r;
        G = g;
        B = b;
    }

    public float R { get; }
    public float G { get; }
    public float B { get; }

    public static bool TryParseHex(string? text, out ColorRgb color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var s = text.Trim();
        if (s.Length != 7 || s[0] != '#')
            return false;

        if (!byte.TryParse(s.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var rb) ||
            !byte.TryParse(s.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var gb) ||
            !byte.TryParse(s.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var bb))
        {
            return false;
        }

        color = new ColorRgb(rb / 255f, gb / 255f, bb / 255f);
        return true;
    }

    public bool Equals(ColorRgb other) => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B);

    public override bool Equals(object? obj) => obj is ColorRgb other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(R, G, B);

    public static bool operator ==(ColorRgb left, ColorRgb right) => left.Equals(right);

    public static bool operator !=(ColorRgb left, ColorRgb right) => !left.Equals(right);
}
