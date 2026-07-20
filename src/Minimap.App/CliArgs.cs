namespace Minimap.App;

/// <summary>Parses Godot command-line arguments for bootstrap overrides.</summary>
public static class CliArgs
{
    public const string DefaultScenarioPath = "res://config/scenarios/default.json";

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
}
