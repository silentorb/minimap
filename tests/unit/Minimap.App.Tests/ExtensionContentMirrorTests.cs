using CompuQuest.Minimap;
using Minimap.Extensive;
using Xunit;

namespace Minimap.App.Tests;

public class ExtensionContentMirrorTests
{
    [Fact]
    public void MirrorJsonTree_removes_orphan_json_not_in_source()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ext-mirror-{Guid.NewGuid():N}");
        var source = Path.Combine(root, "source");
        var dest = Path.Combine(root, "dest");
        var sourceAccessories = Path.Combine(source, "accessories");
        var destAccessories = Path.Combine(dest, "accessories");

        try
        {
            Directory.CreateDirectory(sourceAccessories);
            Directory.CreateDirectory(destAccessories);

            File.WriteAllText(
                Path.Combine(sourceAccessories, "farm.json"),
                """{ "id": "farm", "displayName": "Farm" }""");
            File.WriteAllText(
                Path.Combine(destAccessories, "farm.json"),
                """{ "id": "farm", "displayName": "Farm" }""");
            File.WriteAllText(
                Path.Combine(destAccessories, "plant_vegetable.json"),
                """{ "id": "plant_vegetable", "displayName": "Plant Vegetable", "tags": ["player_selectable"] }""");

            ExtensionContentMirror.MirrorJsonTree(source, dest);

            Assert.True(File.Exists(Path.Combine(destAccessories, "farm.json")));
            Assert.False(File.Exists(Path.Combine(destAccessories, "plant_vegetable.json")));
            Assert.Equal(
                """{ "id": "farm", "displayName": "Farm" }""",
                File.ReadAllText(Path.Combine(destAccessories, "farm.json")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DeployedCompuQuestContent_SelectableAccessories_AreGunFarmGeekOnly()
    {
        var repoRoot = FindRepoRoot();
        var sourceConfig = Path.Combine(repoRoot, "src", "CompuQuest.Minimap", "config");
        var deployed = Path.Combine(repoRoot, "extensions", "CompuQuest.Minimap");

        // Ensure deploy tree matches source (same wipe+copy the csproj target uses).
        ExtensionContentMirror.MirrorJsonTree(sourceConfig, deployed);

        var registry = new ExtensionRegistry();
        new CompuQuestExtension().Register(registry);
        DefinitionConfig.RegisterFromConfigDirectory(deployed, registry);

        Assert.DoesNotContain(registry.AccessoryDefinitions, a => a.Id == "plant_vegetable");
        Assert.DoesNotContain(registry.AccessoryDefinitions, a => a.Id == "use_computer");

        var selectable = new CompuQuestIntegrator().GetPlayerSelectableAccessories(registry);
        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal) { "gun", "farm", "geek" },
            selectable.Select(a => a.Id).ToHashSet(StringComparer.Ordinal));

        var geek = registry.AccessoryDefinitions.Single(a => a.Id == "geek");
        Assert.Contains(geek.EffectTemplates, e => e is UseComputerEffect);
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
