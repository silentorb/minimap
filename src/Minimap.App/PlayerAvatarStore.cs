namespace Minimap.App;

/// <summary>Copy/delete profile avatar image files under a user data directory.</summary>
public static class PlayerAvatarStore
{
    public const long MaxSourceBytes = 8L * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
    };

    /// <summary>
    /// Copies <paramref name="sourceAbsolutePath"/> into the avatars directory as
    /// <c>{profileId}{ext}</c>. Deletes <paramref name="previousAvatarFile"/> when it differs.
    /// </summary>
    public static bool TryImport(
        string avatarsDirectoryAbsolute,
        Guid profileId,
        string sourceAbsolutePath,
        string? previousAvatarFile,
        out string? avatarFile,
        out string? error)
    {
        avatarFile = null;
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarsDirectoryAbsolute);

        if (profileId == Guid.Empty)
        {
            error = "Profile id is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(sourceAbsolutePath))
        {
            error = "Source image path is required.";
            return false;
        }

        if (!File.Exists(sourceAbsolutePath))
        {
            error = "Selected image file was not found.";
            return false;
        }

        var extension = Path.GetExtension(sourceAbsolutePath);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            error = "Image must be PNG, JPG, JPEG, or WebP.";
            return false;
        }

        long length;
        try
        {
            length = new FileInfo(sourceAbsolutePath).Length;
        }
        catch (IOException ex)
        {
            error = $"Could not read image file: {ex.Message}";
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = $"Could not read image file: {ex.Message}";
            return false;
        }

        if (length > MaxSourceBytes)
        {
            error = "Image must be 8 MiB or smaller.";
            return false;
        }

        var normalizedExt = extension.ToLowerInvariant();
        var destinationName = $"{profileId:D}{normalizedExt}";
        var destinationPath = Path.Combine(avatarsDirectoryAbsolute, destinationName);

        try
        {
            Directory.CreateDirectory(avatarsDirectoryAbsolute);
            File.Copy(sourceAbsolutePath, destinationPath, overwrite: true);
        }
        catch (IOException ex)
        {
            error = $"Could not save avatar image: {ex.Message}";
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = $"Could not save avatar image: {ex.Message}";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(previousAvatarFile)
            && !string.Equals(previousAvatarFile, destinationName, StringComparison.Ordinal))
        {
            TryDelete(avatarsDirectoryAbsolute, previousAvatarFile, out _);
        }

        avatarFile = destinationName;
        error = null;
        return true;
    }

    public static bool TryDelete(
        string avatarsDirectoryAbsolute,
        string? avatarFile,
        out string? error)
    {
        error = null;
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarsDirectoryAbsolute);

        if (string.IsNullOrWhiteSpace(avatarFile))
            return true;

        if (!IsValidAvatarFileName(avatarFile, out error))
            return false;

        var path = Path.Combine(avatarsDirectoryAbsolute, avatarFile);
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch (IOException ex)
        {
            error = $"Could not delete avatar image: {ex.Message}";
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = $"Could not delete avatar image: {ex.Message}";
            return false;
        }
    }

    public static bool IsAllowedExtension(string? extension) =>
        !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);

    public static bool IsValidAvatarFileName(string? avatarFile, out string? error)
    {
        if (string.IsNullOrWhiteSpace(avatarFile))
        {
            error = "Avatar file name cannot be empty.";
            return false;
        }

        var trimmed = avatarFile.Trim();
        if (!string.Equals(trimmed, Path.GetFileName(trimmed), StringComparison.Ordinal)
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.Contains(Path.DirectorySeparatorChar)
            || trimmed.Contains(Path.AltDirectorySeparatorChar))
        {
            error = "Avatar file must be a plain filename.";
            return false;
        }

        var extension = Path.GetExtension(trimmed);
        if (!IsAllowedExtension(extension))
        {
            error = "Avatar file must be PNG, JPG, JPEG, or WebP.";
            return false;
        }

        error = null;
        return true;
    }
}
