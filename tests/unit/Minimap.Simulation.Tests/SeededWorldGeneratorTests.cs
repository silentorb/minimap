using Xunit;

namespace Minimap.Simulation.Tests;

public class SeededWorldGeneratorTests
{
    [Fact]
    public void Same_seed_produces_identical_terrain_and_spawns()
    {
        static string Snapshot(int seed)
        {
            var w = GameWorld.Create(3, 3, seed, spawn: new SpawnConfig { AiPerFaction = 1 });
            var parts = w.Grid.Cells.OrderBy(kv => kv.Key.Q).ThenBy(kv => kv.Key.R)
                .Select(kv => $"{kv.Key.Q},{kv.Key.R}:{(byte)kv.Value}");
            var chars = w.Characters.OrderBy(c => c.Id)
                .Select(c => $"{c.Id}:{c.FactionId}:{c.Position.X:F3},{c.Position.Y:F3}");
            return string.Join(";", parts) + "|" + string.Join(";", chars);
        }

        Assert.Equal(Snapshot(42), Snapshot(42));
        Assert.NotEqual(Snapshot(1), Snapshot(2));
    }
}
