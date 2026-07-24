using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class ScenarioRunnerTests
{
    private static Scenario FastScenario() => new()
    {
        PreparationDuration = 1f,
        WaveCount = 2,
        WaveDuration = 1f,
        SpawnerCount = 2,
        SpawnerVolume = 2,
    };

    private static ScenarioRunner EnabledRunner() => new() { Enabled = true };

    [Fact]
    public void Disabled_by_default_never_spawns_or_regenerates()
    {
        var scenario = FastScenario();
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, TestContent.Content);
        var runner = new ScenarioRunner();

        Assert.False(runner.Enabled);

        var result = runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 10f);
        Assert.False(result.LevelRegenerated);
        Assert.Equal(ScenarioPhase.Preparation, runner.Phase);
        Assert.Equal(0, runner.WavesCompleted);
        Assert.Equal(0, CountRivals(world, spawn.RivalFactionId));
        Assert.Equal(1, runner.LevelIndex);
    }

    [Fact]
    public void Preparation_delays_first_wave()
    {
        var scenario = FastScenario();
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, TestContent.Content);
        var runner = EnabledRunner();

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.5f);
        Assert.Equal(ScenarioPhase.Preparation, runner.Phase);
        Assert.Equal(0, CountRivals(world, spawn.RivalFactionId));

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.6f);
        Assert.Equal(ScenarioPhase.Waves, runner.Phase);
        Assert.Equal(1, runner.WavesCompleted);
        Assert.Equal(scenario.SpawnerCount * scenario.SpawnerVolume, CountRivals(world, spawn.RivalFactionId));
    }

    [Fact]
    public void Each_wave_spawns_spawner_count_times_volume_enemies()
    {
        var scenario = FastScenario();
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, TestContent.Content);
        var runner = EnabledRunner();

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 1.1f);
        var afterWave1 = CountRivals(world, spawn.RivalFactionId);

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 1.1f);
        var afterWave2 = CountRivals(world, spawn.RivalFactionId);

        Assert.Equal(4, afterWave1);
        Assert.Equal(8, afterWave2);
        Assert.Equal(ScenarioPhase.LevelComplete, runner.Phase);
    }

    [Fact]
    public void Completing_all_waves_regenerates_level_and_heals_players()
    {
        var scenario = new Scenario
        {
            PreparationDuration = 0.1f,
            WaveCount = 1,
            WaveDuration = 0.1f,
            SpawnerCount = 1,
            SpawnerVolume = 1,
        };
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, TestContent.Content);
        var player = world.Characters.Single(c => c.FactionId == spawn.PlayerFactionId);
        player.Health = 10;
        var runner = EnabledRunner();

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.2f);
        Assert.Equal(1, CountRivals(world, spawn.RivalFactionId));
        Assert.Equal(ScenarioPhase.Waves, runner.Phase);

        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.2f);
        Assert.Equal(ScenarioPhase.LevelComplete, runner.Phase);

        var result = runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.01f);
        Assert.True(result.LevelRegenerated);
        Assert.Equal(ScenarioPhase.Preparation, runner.Phase);
        Assert.Equal(2, runner.LevelIndex);
        Assert.Equal(0, CountRivals(world, spawn.RivalFactionId));
        Assert.Single(world.Spawners);

        var healed = world.Characters.Single(c => c.FactionId == spawn.PlayerFactionId);
        Assert.Equal(CombatTuning.DefaultMaxHealth, healed.Health);
    }

    [Fact]
    public void Level_regen_respawns_dead_human()
    {
        var scenario = new Scenario
        {
            PreparationDuration = 0.1f,
            WaveCount = 1,
            WaveDuration = 0.1f,
            SpawnerCount = 1,
            SpawnerVolume = 1,
        };
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, TestContent.Content);
        var player = world.Characters.Single(c => c.FactionId == spawn.PlayerFactionId);
        world.ApplyDamage(player, CombatTuning.DefaultMaxHealth);
        world.Tick(0.016f);
        Assert.DoesNotContain(player, world.Characters);

        var runner = EnabledRunner();
        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.2f);
        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.2f);
        runner.Tick(world, scenario, spawn, TestContent.SpawnerPool, 0.01f);

        Assert.Single(world.Characters, c => c.FactionId == spawn.PlayerFactionId);
    }

    [Fact]
    public void Empty_character_pool_spawns_nothing()
    {
        var emptySpawner = new SpawnerDefinition(
            "empty",
            WeightedPool<CharacterDefinition>.Empty);
        var pool = new WeightedPool<SpawnerDefinition>(
        [
            new WeightedEntry<SpawnerDefinition>(emptySpawner, 1),
        ]);
        var content = new GameContent(TestContent.Generic, pool, resources: TestContent.Resources);
        var scenario = FastScenario();
        var spawn = new SpawnConfig { RivalFactionId = 2, HumanPlayerCount = 1 };
        var world = GameWorld.Create(4, 4, 42);
        world.InitializeScenarioLevel(scenario, spawn, content);
        var runner = EnabledRunner();

        runner.Tick(world, scenario, spawn, pool, 1.1f);
        Assert.Equal(0, CountRivals(world, spawn.RivalFactionId));
        Assert.Equal(2, world.Spawners.Count);
    }

    private static int CountRivals(GameWorld world, int rivalFactionId) =>
        world.Characters.Count(c => c.FactionId == rivalFactionId);
}
