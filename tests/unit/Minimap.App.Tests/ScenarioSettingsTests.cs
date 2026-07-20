using Minimap.Simulation;
using Xunit;

namespace Minimap.App.Tests;

public class ScenarioSettingsTests
{
    [Fact]
    public void LoadFromJson_HappyPath_ReturnsDocumentedDefaults()
    {
        const string json = """
            {
              "preparationDuration": 10.0,
              "waveCount": 3,
              "waveDuration": 15.0,
              "spawnerCount": 4,
              "spawnerVolume": 2
            }
            """;

        var scenario = ScenarioSettings.LoadFromJson(json);

        Assert.Equal(10f, scenario.PreparationDuration);
        Assert.Equal(3, scenario.WaveCount);
        Assert.Equal(15f, scenario.WaveDuration);
        Assert.Equal(4, scenario.SpawnerCount);
        Assert.Equal(2, scenario.SpawnerVolume);
    }

    [Fact]
    public void LoadFromJson_MissingField_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ScenarioSettings.LoadFromJson("""{ "waveCount": 1 }"""));
    }

    [Fact]
    public void LoadFromJson_InvalidValue_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ScenarioSettings.LoadFromJson("""
                {
                  "preparationDuration": 0,
                  "waveCount": 3,
                  "waveDuration": 15.0,
                  "spawnerCount": 4,
                  "spawnerVolume": 2
                }
                """));
    }

    [Fact]
    public void LoadFromFile_ReadsTempFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"scenario-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                {
                  "preparationDuration": 5.0,
                  "waveCount": 2,
                  "waveDuration": 8.0,
                  "spawnerCount": 3,
                  "spawnerVolume": 1
                }
                """);

            var scenario = ScenarioSettings.LoadFromFile(path);

            Assert.Equal(2, scenario.WaveCount);
            Assert.Equal(3, scenario.SpawnerCount);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

public class CliArgsTests
{
    [Fact]
    public void TryGetScenarioPath_EqualsForm_ReturnsPath()
    {
        var path = CliArgs.TryGetScenarioPath(["--scenario=res://custom.json"]);
        Assert.Equal("res://custom.json", path);
    }

    [Fact]
    public void TryGetScenarioPath_SpaceSeparated_ReturnsPath()
    {
        var path = CliArgs.TryGetScenarioPath(["--scenario", "res://custom.json"]);
        Assert.Equal("res://custom.json", path);
    }

    [Fact]
    public void TryGetScenarioPath_Missing_ReturnsNull()
    {
        Assert.Null(CliArgs.TryGetScenarioPath(["--rendering-driver", "dummy"]));
    }
}
