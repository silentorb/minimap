using Xunit;

namespace Minimap.App.Tests;

public class DotEnvBootstrapTests
{
    [Fact]
    public void ShouldApply_UnderTestHost_ReturnsFalse()
    {
        Assert.False(DotEnvBootstrap.ShouldApply());
    }

    [Fact]
    public void TryApply_UnderTestHost_DoesNotLoadEnvFile()
    {
        const string sentinelKey = "MINIMAP_DOTENV_UNIT_SENTINEL";
        var previous = Environment.GetEnvironmentVariable(sentinelKey);
        try
        {
            Environment.SetEnvironmentVariable(sentinelKey, null);

            var applied = DotEnvBootstrap.TryApply();

            Assert.False(applied);
            Assert.Null(Environment.GetEnvironmentVariable(sentinelKey));
        }
        finally
        {
            Environment.SetEnvironmentVariable(sentinelKey, previous);
        }
    }

    [Fact]
    public void ApplyForTests_MissingFile_IsNoOp()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"minimap-dotenv-missing-{Guid.NewGuid():N}.env");
        Assert.False(File.Exists(missing));
        Assert.False(DotEnvBootstrap.ApplyForTests(missing));
    }

    [Fact]
    public void ApplyForTests_SetsVars_WithoutClobberingExisting()
    {
        const string keyA = "MINIMAP_DOTENV_TEST_A";
        const string keyB = "MINIMAP_DOTENV_TEST_B";
        var previousA = Environment.GetEnvironmentVariable(keyA);
        var previousB = Environment.GetEnvironmentVariable(keyB);
        var path = Path.Combine(Path.GetTempPath(), $"minimap-dotenv-{Guid.NewGuid():N}.env");

        try
        {
            Environment.SetEnvironmentVariable(keyA, "keep-me");
            Environment.SetEnvironmentVariable(keyB, null);
            File.WriteAllText(path, $"{keyA}=from-file\n{keyB}=from-file\n");

            Assert.True(DotEnvBootstrap.ApplyForTests(path, clobberExistingVars: false));
            Assert.Equal("keep-me", Environment.GetEnvironmentVariable(keyA));
            Assert.Equal("from-file", Environment.GetEnvironmentVariable(keyB));
        }
        finally
        {
            Environment.SetEnvironmentVariable(keyA, previousA);
            Environment.SetEnvironmentVariable(keyB, previousB);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

public class CliArgsEnvironmentTests
{
    [Fact]
    public void TryGetScenarioPathFromEnvironment_WhenSet_ReturnsTrimmed()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.ScenarioEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.ScenarioEnvVar, "  res://custom.json  ");
            Assert.Equal("res://custom.json", CliArgs.TryGetScenarioPathFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.ScenarioEnvVar, previous);
        }
    }

    [Fact]
    public void TryGetWorldSeedFromEnvironment_WhenParseable_ReturnsSeed()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.WorldSeedEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.WorldSeedEnvVar, "99");
            Assert.Equal(99, CliArgs.TryGetWorldSeedFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.WorldSeedEnvVar, previous);
        }
    }

    [Fact]
    public void TryGetWorldSeedFromEnvironment_WhenInvalid_ReturnsNull()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.WorldSeedEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.WorldSeedEnvVar, "not-an-int");
            Assert.Null(CliArgs.TryGetWorldSeedFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.WorldSeedEnvVar, previous);
        }
    }

    [Fact]
    public void ShouldStartAtLobby_WhenUnset_ReturnsFalse()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.StartScreenEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, null);
            Assert.False(CliArgs.ShouldStartAtLobby());
            Assert.Null(CliArgs.TryGetStartScreenFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, previous);
        }
    }

    [Fact]
    public void ShouldStartAtLobby_WhenLobby_ReturnsTrue()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.StartScreenEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, "  lobby  ");
            Assert.True(CliArgs.ShouldStartAtLobby());
            Assert.Equal("lobby", CliArgs.TryGetStartScreenFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, previous);
        }
    }

    [Fact]
    public void ShouldStartAtLobby_WhenOtherOrWrongCase_ReturnsFalse()
    {
        var previous = Environment.GetEnvironmentVariable(CliArgs.StartScreenEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, "Lobby");
            Assert.False(CliArgs.ShouldStartAtLobby());

            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, "main");
            Assert.False(CliArgs.ShouldStartAtLobby());
        }
        finally
        {
            Environment.SetEnvironmentVariable(CliArgs.StartScreenEnvVar, previous);
        }
    }
}
