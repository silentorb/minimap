using Minimap.Simulation;
using Xunit;

namespace Minimap.App.Tests;

public class CoreSettingsTests
{
    [Fact]
    public void Defaults_MatchDocumentedRadiiAndAccessoryPoints()
    {
        Assert.Equal(new SimVec2I(8, 6), CoreSettings.Defaults.Map.Radius);
        Assert.Equal(2, CoreSettings.Defaults.Player.AccessoryPoints);
    }

    [Fact]
    public void LoadFromJson_HappyPath_ReturnsDocumentedRadii()
    {
        const string json = """
            {
              "map": {
                "radius": [8, 6]
              },
              "player": {
                "accessoryPoints": 2
              }
            }
            """;

        var settings = CoreSettings.LoadFromJson(json);

        Assert.Equal(new SimVec2I(8, 6), settings.Map.Radius);
        Assert.Equal(2, settings.Player.AccessoryPoints);
    }

    [Fact]
    public void LoadFromJson_CustomRadii_RoundTrips()
    {
        const string json = """
            {
              "map": {
                "radius": [3, 4]
              }
            }
            """;

        var settings = CoreSettings.LoadFromJson(json);

        Assert.Equal(new SimVec2I(3, 4), settings.Map.Radius);
    }

    [Fact]
    public void LoadFromJson_InvalidJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => CoreSettings.LoadFromJson("{ not json"));
    }

    [Fact]
    public void LoadFromJson_MissingMap_Throws()
    {
        Assert.ThrowsAny<Exception>(() => CoreSettings.LoadFromJson("{}"));
    }

    [Fact]
    public void LoadFromJson_MissingRadius_Throws()
    {
        Assert.ThrowsAny<Exception>(() => CoreSettings.LoadFromJson("""{ "map": {} }"""));
    }

    [Fact]
    public void LoadFromJson_WrongArrayLength_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            CoreSettings.LoadFromJson("""{ "map": { "radius": [8] } }"""));
        Assert.ThrowsAny<Exception>(() =>
            CoreSettings.LoadFromJson("""{ "map": { "radius": [8, 6, 1] } }"""));
    }

    [Fact]
    public void LoadFromJson_NegativeRadii_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CoreSettings.LoadFromJson("""{ "map": { "radius": [-1, 6] } }"""));
        Assert.Throws<InvalidOperationException>(() =>
            CoreSettings.LoadFromJson("""{ "map": { "radius": [8, -2] } }"""));
    }

    [Fact]
    public void LoadFromFile_ReadsTempFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"core-settings-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                {
                  "map": {
                    "radius": [5, 2]
                  }
                }
                """);

            var settings = CoreSettings.LoadFromFile(path);

            Assert.Equal(new SimVec2I(5, 2), settings.Map.Radius);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LoadFromJson_NegativeAccessoryPoints_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CoreSettings.LoadFromJson("""
                {
                  "map": { "radius": [8, 6] },
                  "player": { "accessoryPoints": -1 }
                }
                """));
    }

    [Fact]
    public void LoadFromJson_MissingPlayer_UsesDefaultAccessoryPoints()
    {
        var settings = CoreSettings.LoadFromJson("""
            {
              "map": { "radius": [8, 6] }
            }
            """);
        Assert.Equal(2, settings.Player.AccessoryPoints);
    }

    [Fact]
    public void LoadFromFile_MissingFile_Throws()
    {
        Assert.Throws<FileNotFoundException>(() =>
            CoreSettings.LoadFromFile(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json")));
    }
}
