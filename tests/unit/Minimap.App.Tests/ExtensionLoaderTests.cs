using Xunit;

namespace Minimap.App.Tests;

public class ExtensionLoaderTests
{
    [Fact]
    public void Load_with_empty_extensions_fails_when_integrator_missing()
    {
        var settings = new ExtensionsSettings
        {
            SearchPaths = new List<string> { "extensions" },
            Extensions = new List<string>(),
            Integrator = "default",
        };

        Assert.Throws<InvalidOperationException>(() =>
            ExtensionLoader.Load(settings, Path.GetTempPath()));
    }

    [Fact]
    public void Load_fails_when_configured_integrator_missing()
    {
        var settings = new ExtensionsSettings
        {
            SearchPaths = new List<string> { "extensions" },
            Extensions = new List<string>(),
            Integrator = "compuquest",
        };

        Assert.Throws<InvalidOperationException>(() =>
            ExtensionLoader.Load(settings, Path.GetTempPath()));
    }

    [Fact]
    public void Load_registers_compuquest_from_built_extension_dll()
    {
        var repoRoot = FindRepoRoot();
        var dllPath = Path.Combine(repoRoot, "extensions", "CompuQuest.Minimap.dll");
        var contentDir = Path.Combine(repoRoot, "extensions", "CompuQuest.Minimap");
        Assert.True(
            File.Exists(dllPath),
            $"Expected built extension at {dllPath}. Build CompuQuest.Minimap first.");
        Assert.True(
            Directory.Exists(contentDir),
            $"Expected extension content at {contentDir}. Build CompuQuest.Minimap first.");

        var settings = new ExtensionsSettings
        {
            SearchPaths = new List<string> { "extensions" },
            Extensions = new List<string> { "CompuQuest.Minimap.dll" },
            Integrator = "compuquest",
        };

        var result = ExtensionLoader.Load(settings, Path.Combine(repoRoot, "config"));

        Assert.Equal("compuquest", result.Integrator.Id);
        Assert.False(result.Registry.TryGetIntegrator("default", out _));
        Assert.True(result.Registry.TryGetIntegrator("compuquest", out _));
        Assert.Equal("generic", result.Content.DefaultCharacter.Id);
        Assert.False(result.Content.WorldSpawnerPool.IsEmpty);
        Assert.Contains(result.Registry.AccessoryDefinitions, d => d.Id == "gun");
        Assert.Contains(result.Registry.CharacterDefinitions, d => d.Id == "generic");
        Assert.Contains(result.Registry.CharacterDefinitions, d => d.Id == "zombie");
        Assert.True(result.Registry.Tags.TryGet("player_selectable", out _));
        var selectable = result.Integrator.GetPlayerSelectableAccessories(result.Registry);
        Assert.Equal(3, selectable.Count);
    }

    [Fact]
    public void LoadFromFile_uses_shipped_extensions_json_when_dll_present()
    {
        var repoRoot = FindRepoRoot();
        var dllPath = Path.Combine(repoRoot, "extensions", "CompuQuest.Minimap.dll");
        Assert.True(
            File.Exists(dllPath),
            $"Expected built extension at {dllPath}. Build CompuQuest.Minimap first.");

        var result = ExtensionLoader.LoadFromFile(Path.Combine(repoRoot, "config", "extensions.json"));
        Assert.Equal("compuquest", result.Integrator.Id);
        Assert.Equal("generic", result.Content.DefaultCharacter.Id);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Minimap.sln"))
                && Directory.Exists(Path.Combine(dir.FullName, "config")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root from test base directory.");
    }
}
