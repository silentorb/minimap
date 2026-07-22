using Xunit;

namespace Minimap.App.Tests;

public class ExtensionsSettingsTests
{
    [Fact]
    public void Defaults_use_compuquest_integrator_and_extensions_search_path()
    {
        Assert.Equal("compuquest", ExtensionsSettings.Defaults.Integrator);
        Assert.Equal(new[] { "extensions" }, ExtensionsSettings.Defaults.SearchPaths);
        Assert.Empty(ExtensionsSettings.Defaults.Extensions);
    }

    [Fact]
    public void LoadFromJson_parses_schema()
    {
        const string json = """
            {
              "searchPaths": ["extensions", "/opt/ext"],
              "extensions": ["CompuQuest.Minimap.dll"],
              "integrator": "compuquest"
            }
            """;

        var settings = ExtensionsSettings.LoadFromJson(json);

        Assert.Equal(new[] { "extensions", "/opt/ext" }, settings.SearchPaths);
        Assert.Equal(new[] { "CompuQuest.Minimap.dll" }, settings.Extensions);
        Assert.Equal("compuquest", settings.Integrator);
    }

    [Fact]
    public void LoadFromJson_allows_comments_and_trailing_commas()
    {
        const string json = """
            {
              // ship CompuQuest
              "searchPaths": ["extensions"],
              "extensions": [],
              "integrator": "default",
            }
            """;

        var settings = ExtensionsSettings.LoadFromJson(json);
        Assert.Equal("default", settings.Integrator);
        Assert.Empty(settings.Extensions);
    }

    [Fact]
    public void LoadFromJson_rejects_missing_integrator()
    {
        Assert.ThrowsAny<Exception>(() => ExtensionsSettings.LoadFromJson("""
            {
              "searchPaths": ["extensions"],
              "extensions": []
            }
            """));
    }

    [Fact]
    public void LoadFromJson_rejects_empty_extension_entry()
    {
        Assert.ThrowsAny<Exception>(() => ExtensionsSettings.LoadFromJson("""
            {
              "searchPaths": ["extensions"],
              "extensions": [""],
              "integrator": "default"
            }
            """));
    }

    [Fact]
    public void LoadFromFile_reads_disk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ext-settings-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                {
                  "searchPaths": ["extensions"],
                  "extensions": [],
                  "integrator": "default"
                }
                """);

            var settings = ExtensionsSettings.LoadFromFile(path);
            Assert.Equal("default", settings.Integrator);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
