using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class TagRegistryTests
{
    [Fact]
    public void GetOrCreate_same_string_returns_same_id()
    {
        var tags = new TagRegistry();
        var a = tags.GetOrCreate("player_selectable");
        var b = tags.GetOrCreate("player_selectable");
        Assert.Equal(a, b);
        Assert.Equal(1, tags.Count);
    }

    [Fact]
    public void Distinct_strings_get_distinct_ids()
    {
        var tags = new TagRegistry();
        var a = tags.GetOrCreate("a");
        var b = tags.GetOrCreate("b");
        Assert.NotEqual(a, b);
        Assert.True(tags.TryGetName(a, out var nameA));
        Assert.Equal("a", nameA);
        Assert.True(tags.TryGetName(b, out var nameB));
        Assert.Equal("b", nameB);
    }
}

public class WeightedPoolTests
{
    [Fact]
    public void Empty_pool_TryPick_returns_false()
    {
        var pool = WeightedPool<string>.Empty;
        Assert.False(pool.TryPick(new Random(1), out _));
    }

    [Fact]
    public void Single_entry_always_picks_that_item()
    {
        var pool = new WeightedPool<string>([new WeightedEntry<string>("only", 1)]);
        Assert.True(pool.TryPick(new Random(42), out var item));
        Assert.Equal("only", item);
    }
}

public class PlayerSessionTests
{
    [Fact]
    public void Create_applies_selected_accessories_to_player_character()
    {
        var scenario = new Scenario
        {
            PreparationDuration = 10f,
            WaveCount = 1,
            WaveDuration = 10f,
            SpawnerCount = 0,
            SpawnerVolume = 0,
        };
        var spawn = new SpawnConfig { HumanPlayerCount = 1 };
        var selected = new List<IReadOnlyList<AccessoryDefinition>>
        {
            new List<AccessoryDefinition> { TestContent.Gun },
        };

        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1,
            TestContent.Content, accessoryPoints: 2, selected);

        Assert.Single(session.Players);
        Assert.Equal(2, session.Players[0].AccessoryPoints);
        Assert.NotNull(session.Players[0].Character);
        Assert.Contains(session.Players[0].Character!.Accessories, a => a.Definition.Id == "gun");
    }
}
