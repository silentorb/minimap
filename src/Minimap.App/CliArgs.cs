namespace Minimap.App;

/// <summary>Parses Godot command-line arguments and process env for bootstrap overrides.</summary>
public static class CliArgs
{
    public const string DefaultScenarioPath = "res://config/scenarios/default.json";
    public const string ScenarioEnvVar = "MINIMAP_SCENARIO";
    public const string WorldSeedEnvVar = "MINIMAP_WORLD_SEED";
    public const string StartScreenEnvVar = "START_SCREEN";
    public const string StartScreenLobby = "lobby";

    public static string? TryGetScenarioPath(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--scenario=", StringComparison.Ordinal))
                return arg["--scenario=".Length..];

            if (string.Equals(arg, "--scenario", StringComparison.Ordinal) && i + 1 < args.Count)
                return args[i + 1];
        }

        return null;
    }

    /// <summary>Optional scenario resource/filesystem path from <see cref="ScenarioEnvVar"/>.</summary>
    public static string? TryGetScenarioPathFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(ScenarioEnvVar);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Optional world seed from <see cref="WorldSeedEnvVar"/> when parseable as int.</summary>
    public static int? TryGetWorldSeedFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(WorldSeedEnvVar);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value.Trim(), out var seed) ? seed : null;
    }

    /// <summary>Optional start screen id from <see cref="StartScreenEnvVar"/> (trimmed; may be empty).</summary>
    public static string? TryGetStartScreenFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(StartScreenEnvVar);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>True when <see cref="StartScreenEnvVar"/> is exactly <see cref="StartScreenLobby"/> after trim.</summary>
    public static bool ShouldStartAtLobby()
    {
        var value = TryGetStartScreenFromEnvironment();
        return string.Equals(value, StartScreenLobby, StringComparison.Ordinal);
    }
}
