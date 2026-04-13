using Xunit;

namespace Minimap.Simulation.Tests;

public class SeededWorldGeneratorTests
{
    [Fact]
    public void Same_seed_produces_identical_terrain_and_spawns()
    {
        static string Snapshot(int seed)
        {
            var w = GameWorld.Create(3, 2, seed);
            var parts = w.Grid.Cells.OrderBy(kv => kv.Key.Q).ThenBy(kv => kv.Key.R)
                .Select(kv => $"{kv.Key.Q},{kv.Key.R}:{(byte)kv.Value}");
            return string.Join(";", parts)
                   + "|" + string.Join(";", w.Players.Select(p => $"{p.Position.Q},{p.Position.R}"));
        }

        Assert.Equal(Snapshot(42), Snapshot(42));
        Assert.NotEqual(Snapshot(1), Snapshot(2));
    }
}
