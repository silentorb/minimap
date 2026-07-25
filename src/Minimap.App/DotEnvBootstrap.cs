using System.Reflection;
using DotNetEnv;

namespace Minimap.App;

/// <summary>
/// Loads an optional repo-root <c>.env</c> into the process environment for manual Godot play.
/// Never applies under unit/functional tests or when automation is enabled.
/// </summary>
public static class DotEnvBootstrap
{
    public const string DisabledEnvVar = "MINIMAP_DOTENV_DISABLED";
    public const string EnvFileName = ".env";
    public const string ProjectGodotFileName = "project.godot";

    /// <summary>Returns whether dotenv may load for this process.</summary>
    public static bool ShouldApply()
    {
        if (IsTruthy(Environment.GetEnvironmentVariable(DisabledEnvVar)))
            return false;

        if (IsTruthy(Environment.GetEnvironmentVariable("MINIMAP_AUTOMATION_ENABLED")))
            return false;

        if (IsTestHost())
            return false;

        return true;
    }

    /// <summary>
    /// When <see cref="ShouldApply"/> is true and a <c>.env</c> exists next to <c>project.godot</c>,
    /// loads it without clobbering existing process environment variables.
    /// Missing file is a no-op. Malformed content fails fast.
    /// </summary>
    /// <returns>True when a file was loaded; false when skipped or missing.</returns>
    public static bool TryApply()
    {
        if (!ShouldApply())
            return false;

        var path = FindEnvFilePath();
        if (path is null)
            return false;

        ApplyFile(path, clobberExistingVars: false);
        return true;
    }

    /// <summary>
    /// Test helper: load a specific <c>.env</c> path (bypasses <see cref="ShouldApply"/> gates).
    /// Missing file is a no-op. Malformed content fails fast.
    /// </summary>
    /// <returns>True when a file was loaded; false when missing.</returns>
    public static bool ApplyForTests(string absoluteEnvPath, bool clobberExistingVars = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absoluteEnvPath);

        if (!File.Exists(absoluteEnvPath))
            return false;

        ApplyFile(absoluteEnvPath, clobberExistingVars);
        return true;
    }

    internal static string? FindEnvFilePath()
    {
        foreach (var start in EnumerateSearchRoots())
        {
            var projectRoot = FindProjectRoot(start);
            if (projectRoot is null)
                continue;

            var envPath = Path.Combine(projectRoot, EnvFileName);
            if (File.Exists(envPath))
                return envPath;
        }

        return null;
    }

    private static void ApplyFile(string absoluteEnvPath, bool clobberExistingVars)
    {
        try
        {
            if (clobberExistingVars)
                Env.Load(absoluteEnvPath);
            else
                Env.NoClobber().Load(absoluteEnvPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load dotenv file '{absoluteEnvPath}'.", ex);
        }
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static string? FindProjectRoot(string startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
            return null;

        DirectoryInfo? current;
        try
        {
            current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        }
        catch (Exception)
        {
            return null;
        }

        while (current is not null)
        {
            var godot = Path.Combine(current.FullName, ProjectGodotFileName);
            if (File.Exists(godot))
                return current.FullName;
            current = current.Parent;
        }

        return null;
    }

    private static bool IsTestHost()
    {
        var entryName = Assembly.GetEntryAssembly()?.GetName().Name;
        if (string.IsNullOrWhiteSpace(entryName))
            return false;

        if (string.Equals(entryName, "testhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return entryName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTruthy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
