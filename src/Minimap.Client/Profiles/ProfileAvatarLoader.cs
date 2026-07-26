using Godot;

namespace Minimap.Client.Profiles;

/// <summary>Load profile avatar images from absolute filesystem paths for UI display.</summary>
public static class ProfileAvatarLoader
{
    public static string ResolveAbsolutePath(string avatarsDirectoryAbsolute, string? avatarFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarsDirectoryAbsolute);
        if (string.IsNullOrWhiteSpace(avatarFile))
            return string.Empty;
        return Path.Combine(avatarsDirectoryAbsolute, avatarFile.Trim());
    }

    /// <summary>Loads a texture from an absolute image path. Returns null when missing or unloadable.</summary>
    public static Texture2D? TryLoad(string? absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
            return null;

        var image = new Image();
        var error = image.Load(absolutePath);
        if (error != Error.Ok)
            return null;

        return ImageTexture.CreateFromImage(image);
    }

    public static void ApplyTo(TextureRect? target, string? absolutePath, Vector2 size)
    {
        if (target is null)
            return;

        var texture = TryLoad(absolutePath);
        target.CustomMinimumSize = size;
        if (texture is null)
        {
            target.Texture = null;
            target.Visible = false;
            return;
        }

        target.Texture = texture;
        target.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        target.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        target.Visible = true;
    }
}
