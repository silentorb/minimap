using Godot;
using Minimap.Simulation.Types;

namespace Minimap.Client;

/// <summary>
/// UI icon with optional domain color swatch under a game-icons white glyph
/// (near-black background pixels made transparent).
/// </summary>
public partial class DomainIconView : Control
{
    private const float DarkLuminanceThreshold = 0.12f;

    private Texture2D? _glyph;
    private IReadOnlyList<ColorRgb> _domainColors = Array.Empty<ColorRgb>();
    private bool _useBakedBackground;

    public void Configure(string? iconPath, IReadOnlyList<ColorRgb>? domainColors)
    {
        _domainColors = domainColors ?? Array.Empty<ColorRgb>();
        _useBakedBackground = _domainColors.Count == 0;
        _glyph = null;

        var source = TryLoadTexture(iconPath);
        if (source is null)
        {
            QueueRedraw();
            return;
        }

        if (_useBakedBackground)
            _glyph = source;
        else
            _glyph = CreateTransparentGlyph(source) ?? source;

        QueueRedraw();
    }

    /// <summary>Builds a square texture suitable for <c>Button.Icon</c>.</summary>
    public static Texture2D? TryCreateTexture(
        string? iconPath,
        IReadOnlyList<ColorRgb>? domainColors,
        int size)
    {
        if (size < 1)
            return null;

        var source = TryLoadTexture(iconPath);
        if (source is null)
            return null;

        var colors = domainColors ?? Array.Empty<ColorRgb>();
        if (colors.Count == 0)
            return source;

        var glyph = CreateTransparentGlyph(source);
        if (glyph is null)
            return source;

        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        DrawSwatchOntoImage(image, colors);
        var glyphImage = glyph.GetImage();
        if (glyphImage is null)
            return ImageTexture.CreateFromImage(image);

        glyphImage.Convert(Image.Format.Rgba8);
        if (glyphImage.GetWidth() != size || glyphImage.GetHeight() != size)
            glyphImage.Resize(size, size, Image.Interpolation.Lanczos);

        BlendGlyphOnto(image, glyphImage);
        return ImageTexture.CreateFromImage(image);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
            QueueRedraw();
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);
        if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
            return;

        if (_useBakedBackground)
        {
            if (_glyph is not null)
                DrawTextureRect(_glyph, rect, tile: false);
            return;
        }

        DrawSwatch(rect, _domainColors);
        if (_glyph is not null)
            DrawTextureRect(_glyph, rect, tile: false);
    }

    private void DrawSwatch(Rect2 rect, IReadOnlyList<ColorRgb> colors)
    {
        if (colors.Count == 0)
            return;

        if (colors.Count == 1)
        {
            DrawRect(rect, ToGodot(colors[0]));
            return;
        }

        if (colors.Count == 2)
        {
            var tl = rect.Position;
            var tr = rect.Position + new Vector2(rect.Size.X, 0f);
            var bl = rect.Position + new Vector2(0f, rect.Size.Y);
            var br = rect.Position + rect.Size;
            // Lower-left triangle = first domain; upper-right = second.
            DrawColoredPolygon([tl, bl, br], ToGodot(colors[0]));
            DrawColoredPolygon([tl, br, tr], ToGodot(colors[1]));
            return;
        }

        var stripeWidth = rect.Size.X / colors.Count;
        for (var i = 0; i < colors.Count; i++)
        {
            var x = rect.Position.X + stripeWidth * i;
            var w = i == colors.Count - 1
                ? rect.Position.X + rect.Size.X - x
                : stripeWidth;
            DrawRect(new Rect2(x, rect.Position.Y, w, rect.Size.Y), ToGodot(colors[i]));
        }
    }

    private static void DrawSwatchOntoImage(Image image, IReadOnlyList<ColorRgb> colors)
    {
        var width = image.GetWidth();
        var height = image.GetHeight();
        if (colors.Count == 1)
        {
            image.Fill(ToGodot(colors[0]));
            return;
        }

        if (colors.Count == 2)
        {
            var c0 = ToGodot(colors[0]);
            var c1 = ToGodot(colors[1]);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Diagonal: below/on the TL→BR diagonal → first color (lower-left).
                    var onOrBelow = (x + 1) * height <= (y + 1) * width;
                    image.SetPixel(x, y, onOrBelow ? c0 : c1);
                }
            }

            return;
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = Math.Clamp(x * colors.Count / width, 0, colors.Count - 1);
                image.SetPixel(x, y, ToGodot(colors[index]));
            }
        }
    }

    private static void BlendGlyphOnto(Image destination, Image glyph)
    {
        var width = Math.Min(destination.GetWidth(), glyph.GetWidth());
        var height = Math.Min(destination.GetHeight(), glyph.GetHeight());
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var g = glyph.GetPixel(x, y);
                if (g.A <= 0.01f)
                    continue;

                var d = destination.GetPixel(x, y);
                var a = g.A;
                destination.SetPixel(x, y, new Color(
                    d.R * (1f - a) + g.R * a,
                    d.G * (1f - a) + g.G * a,
                    d.B * (1f - a) + g.B * a,
                    1f));
            }
        }
    }

    private static Texture2D? CreateTransparentGlyph(Texture2D source)
    {
        var image = source.GetImage();
        if (image is null)
            return null;

        image.Convert(Image.Format.Rgba8);
        var width = image.GetWidth();
        var height = image.GetHeight();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var c = image.GetPixel(x, y);
                var luminance = 0.2126f * c.R + 0.7152f * c.G + 0.0722f * c.B;
                if (luminance <= DarkLuminanceThreshold)
                    image.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                else
                    image.SetPixel(x, y, new Color(1f, 1f, 1f, c.A));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static Texture2D? TryLoadTexture(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        if (!ResourceLoader.Exists(path))
            return null;
        return ResourceLoader.Load<Texture2D>(path);
    }

    private static Color ToGodot(ColorRgb color) => new(color.R, color.G, color.B);
}
