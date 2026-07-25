using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class ColorRgbTests
{
    [Theory]
    [InlineData("#3A8F4B", 0x3A / 255f, 0x8F / 255f, 0x4B / 255f)]
    [InlineData("#B4BEC8", 0xB4 / 255f, 0xBE / 255f, 0xC8 / 255f)]
    [InlineData("#000000", 0f, 0f, 0f)]
    [InlineData("#FFFFFF", 1f, 1f, 1f)]
    public void TryParseHex_parses_rrggbb(string text, float r, float g, float b)
    {
        Assert.True(ColorRgb.TryParseHex(text, out var color));
        Assert.Equal(r, color.R, precision: 5);
        Assert.Equal(g, color.G, precision: 5);
        Assert.Equal(b, color.B, precision: 5);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("3A8F4B")]
    [InlineData("#3A8F4")]
    [InlineData("#GG0000")]
    [InlineData("#3A8F4BAA")]
    public void TryParseHex_rejects_invalid(string? text)
    {
        Assert.False(ColorRgb.TryParseHex(text, out _));
    }

    [Fact]
    public void Constructor_rejects_out_of_range_components()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColorRgb(-0.1f, 0f, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColorRgb(0f, 1.1f, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColorRgb(0f, 0f, 2f));
    }
}
