using Minimap.Simulation.Types;

namespace Minimap.Simulation;

public enum ScenarioPhase
{
    Preparation,
    Waves,
    LevelComplete,
}

/// <summary>Wave timing and level transitions for a <see cref="Scenario"/>.</summary>
public sealed class ScenarioRunner
{
    public int LevelIndex { get; private set; } = 1;
    public int WavesCompleted { get; private set; }
    public ScenarioPhase Phase { get; private set; } = ScenarioPhase.Preparation;
    public float ElapsedInPhase { get; private set; }

    private float _waveIntervalElapsed;

    public ScenarioTickResult Tick(
        GameWorld world,
        Scenario scenario,
        SpawnConfig spawn,
        WeightedPool<SpawnerDefinition> spawnerPool,
        float dt)
    {
        if (dt <= 0f)
            return ScenarioTickResult.None;

        switch (Phase)
        {
            case ScenarioPhase.Preparation:
                ElapsedInPhase += dt;
                if (ElapsedInPhase < scenario.PreparationDuration)
                    return ScenarioTickResult.None;

                SpawnWave(world, scenario, spawn.RivalFactionId);
                WavesCompleted = 1;
                Phase = ScenarioPhase.Waves;
                ElapsedInPhase = 0f;
                _waveIntervalElapsed = 0f;
                return ScenarioTickResult.None;

            case ScenarioPhase.Waves:
                _waveIntervalElapsed += dt;
                if (_waveIntervalElapsed < scenario.WaveDuration)
                    return ScenarioTickResult.None;

                if (WavesCompleted >= scenario.WaveCount)
                {
                    Phase = ScenarioPhase.LevelComplete;
                    ElapsedInPhase = 0f;
                    return ScenarioTickResult.None;
                }

                SpawnWave(world, scenario, spawn.RivalFactionId);
                WavesCompleted++;
                _waveIntervalElapsed = 0f;

                if (WavesCompleted >= scenario.WaveCount)
                    Phase = ScenarioPhase.LevelComplete;

                return ScenarioTickResult.None;

            case ScenarioPhase.LevelComplete:
                world.RegenerateLevel(scenario, spawn, LevelIndex, spawnerPool);
                LevelIndex++;
                WavesCompleted = 0;
                Phase = ScenarioPhase.Preparation;
                ElapsedInPhase = 0f;
                _waveIntervalElapsed = 0f;
                return new ScenarioTickResult { LevelRegenerated = true };

            default:
                return ScenarioTickResult.None;
        }
    }

    private static void SpawnWave(GameWorld world, Scenario scenario, int rivalFactionId)
    {
        foreach (var spawner in world.Spawners)
            world.SpawnWaveEnemies(spawner, scenario.SpawnerVolume, rivalFactionId);
    }
}

public readonly struct ScenarioTickResult
{
    public static ScenarioTickResult None { get; } = default;

    public bool LevelRegenerated { get; init; }
}
