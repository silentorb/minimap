namespace Minimap.Simulation;

/// <summary>Gameplay pacing and map-init parameters for a playthrough.</summary>
public sealed class Scenario
{
    public static Scenario Defaults { get; } = new()
    {
        PreparationDuration = 10f,
        WaveCount = 3,
        WaveDuration = 15f,
        SpawnerCount = 2,
        SpawnerVolume = 2,
    };

    public required float PreparationDuration { get; init; }
    public required int WaveCount { get; init; }
    public required float WaveDuration { get; init; }
    public required int SpawnerCount { get; init; }
    public required int SpawnerVolume { get; init; }

    public static void Validate(Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        if (scenario.PreparationDuration <= 0f)
        {
            throw new InvalidOperationException(
                $"preparationDuration must be > 0 (got {scenario.PreparationDuration}).");
        }

        if (scenario.WaveDuration <= 0f)
        {
            throw new InvalidOperationException(
                $"waveDuration must be > 0 (got {scenario.WaveDuration}).");
        }

        if (scenario.WaveCount < 1)
        {
            throw new InvalidOperationException(
                $"waveCount must be >= 1 (got {scenario.WaveCount}).");
        }

        if (scenario.SpawnerCount < 1)
        {
            throw new InvalidOperationException(
                $"spawnerCount must be >= 1 (got {scenario.SpawnerCount}).");
        }

        if (scenario.SpawnerVolume < 1)
        {
            throw new InvalidOperationException(
                $"spawnerVolume must be >= 1 (got {scenario.SpawnerVolume}).");
        }
    }
}
