using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class SpawnEffectTests
{
    [Fact]
    public void Spawn_effect_emits_volume_after_interval()
    {
        var w = GameWorld.Create(4, 4, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.RivalFactionId = 2;

        var spawnAccessory = new AccessoryDefinition(
            "spawn_zombies",
            [
                new TestSpawnEffect(
                    intervalSeconds: 1f,
                    volume: 2,
                    new WeightedPool<string>([new WeightedEntry<string>("zombie", 1)])),
            ]);
        var spawnerDef = new ActorDefinition(
            "spawner",
            [spawnAccessory],
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 400),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 400),
            ]);
        w.SetActorDefinitions([spawnerDef, .. TestContent.Content.Actors]);

        var cell = new HexAxial(0, 0);
        Assert.True(w.TryPlaceActor(cell, spawnerDef));
        Assert.Empty(w.Characters);

        w.TickCellActors(0.5f);
        Assert.Empty(w.Characters);

        w.TickCellActors(0.6f);
        Assert.Equal(2, w.Characters.Count);
        Assert.All(w.Characters, c => Assert.Equal(2, c.FactionId));
        Assert.All(w.Characters, c => Assert.Equal("zombie", c.Definition.Id));
        Assert.Contains(w.Controllers, c => c is AiController { Aggression: AiTuning.DefaultAggression });
    }

    [Fact]
    public void Dead_spawner_emits_nothing()
    {
        var w = GameWorld.Create(4, 4, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.RivalFactionId = 2;

        var spawnAccessory = new AccessoryDefinition(
            "spawn_zombies",
            [
                new TestSpawnEffect(
                    1f,
                    2,
                    new WeightedPool<string>([new WeightedEntry<string>("zombie", 1)])),
            ]);
        var spawnerDef = new ActorDefinition(
            "spawner",
            [spawnAccessory],
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 400),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 400),
            ]);
        w.SetActorDefinitions([spawnerDef]);

        var cell = new HexAxial(0, 0);
        Assert.True(w.TryPlaceActor(cell, spawnerDef));
        w.CellActors[cell].Health = 0;
        w.Tick(0.016f);
        Assert.False(w.IsCellOccupied(cell));

        w.TickCellActors(2f);
        Assert.Empty(w.Characters);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
