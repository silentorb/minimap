using Xunit;

namespace Minimap.App.Tests;

public class ExtensionPathResolverTests
{
    [Fact]
    public void ResolveAssemblyPath_accepts_absolute_path()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"ext-abs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        var dll = Path.Combine(dir, "Sample.dll");
        try
        {
            File.WriteAllText(dll, "placeholder");
            var resolved = ExtensionPathResolver.ResolveAssemblyPath(
                dll,
                Array.Empty<string>(),
                Path.GetTempPath());
            Assert.Equal(Path.GetFullPath(dll), resolved);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ResolveAssemblyPath_finds_filename_via_parent_of_config_dir()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ext-root-{Guid.NewGuid():N}");
        var configDir = Path.Combine(root, "config");
        var extensionsDir = Path.Combine(root, "extensions");
        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(extensionsDir);
        var dll = Path.Combine(extensionsDir, "CompuQuest.Minimap.dll");
        try
        {
            File.WriteAllText(dll, "placeholder");
            var resolved = ExtensionPathResolver.ResolveAssemblyPath(
                "CompuQuest.Minimap.dll",
                new[] { "extensions" },
                configDir);
            Assert.Equal(Path.GetFullPath(dll), resolved);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ResolveAssemblyPath_fails_when_missing()
    {
        Assert.Throws<FileNotFoundException>(() =>
            ExtensionPathResolver.ResolveAssemblyPath(
                "Missing.dll",
                new[] { "extensions" },
                Path.GetTempPath()));
    }
}
