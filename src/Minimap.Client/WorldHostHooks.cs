using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Client;

/// <summary>
/// Client world roots call these for settings/extension file I/O without referencing Minimap.App.
/// App registers the real loaders (typically via module initializer).
/// </summary>
public static class WorldHostHooks
{
    public const string DefaultScenarioResPath = "res://config/scenarios/default.json";

    /// <summary>Absolute filesystem path to core.json → map radius.</summary>
    public static Func<string, SimVec2I>? LoadCoreMapRadiusFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path to scenario JSON → <see cref="Scenario"/>.</summary>
    public static Func<string, Scenario>? LoadScenarioFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path to extensions.json → <see cref="GameContent"/>.</summary>
    public static Func<string, GameContent>? LoadGameContentFromAbsolutePath { get; set; }

    /// <summary>Godot cmdline args → optional scenario resource path override.</summary>
    public static Func<IReadOnlyList<string>, string?>? TryGetScenarioPathFromArgs { get; set; }

    public static SimVec2I RequireCoreMapRadius(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadCoreMapRadiusFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadCoreMapRadiusFromAbsolutePath.");
        return load(absolutePath);
    }

    public static Scenario RequireScenario(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadScenarioFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadScenarioFromAbsolutePath.");
        return load(absolutePath);
    }

    public static GameContent RequireGameContent(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadGameContentFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadGameContentFromAbsolutePath.");
        return load(absolutePath);
    }

    public static string? TryResolveScenarioPathFromArgs(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return TryGetScenarioPathFromArgs?.Invoke(args);
    }
}
