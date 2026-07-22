using CompuQuest.Minimap;
using Minimap.Extensive;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class DefinitionConfigTests
{
    private static ExtensionRegistry RegistryWithShootFactory()
    {
        var registry = new ExtensionRegistry();
        registry.AddAccessoryEffectFactory(ShootEffectFactory.TypeId, ShootEffectFactory.Create);
        return registry;
    }

    [Fact]
    public void LoadAccessoryFromJson_Gun_ReturnsShootEffect()
    {
        const string json = """
            {
              "id": "gun",
              "effects": [
                {
                  "type": "shoot",
                  "fireIntervalSeconds": 1.25,
                  "missileSpeed": 200,
                  "missileDamage": 25,
                  "friendlyFire": true
                }
              ]
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithShootFactory());

        Assert.Equal("gun", def.Id);
        var shoot = Assert.IsType<ShootEffect>(Assert.Single(def.EffectTemplates));
        Assert.Equal(1.25f, shoot.FireIntervalSeconds);
        Assert.Equal(200f, shoot.MissileSpeed);
        Assert.Equal(25f, shoot.MissileDamage);
        Assert.True(shoot.FriendlyFire);
        Assert.Null(def.DepictionConfig);
        Assert.Null(def.IconConfig);
    }

    [Fact]
    public void LoadAccessoryFromJson_WithDepiction_ParsesDepictionConfig()
    {
        const string json = """
            {
              "id": "gun",
              "effects": [],
              "depiction": {
                "kind": "sprite_frames",
                "path": "res://assets/compuquest/kenney-1bit/depict/gun.tres",
                "animation": "default"
              }
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithShootFactory());
        var depiction = Assert.IsType<DepictionConfig>(def.DepictionConfig);
        Assert.Equal(DepictionKinds.SpriteFrames, depiction.Kind);
        Assert.Equal("res://assets/compuquest/kenney-1bit/depict/gun.tres", depiction.ResourcePath);
        Assert.Equal("default", depiction.DefaultAnimation);
    }

    [Fact]
    public void LoadAccessoryFromJson_WithIcon_ParsesIconConfig()
    {
        const string json = """
            {
              "id": "gun",
              "effects": [],
              "icon": {
                "path": "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg"
              }
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithShootFactory());
        var icon = Assert.IsType<IconConfig>(def.IconConfig);
        Assert.Equal(
            "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg",
            icon.ResourcePath);
    }

    [Fact]
    public void LoadAccessoryFromJson_DepictionMissingKind_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson("""
                {
                  "id": "gun",
                  "effects": [],
                  "depiction": { "path": "res://x.tres" }
                }
                """, RegistryWithShootFactory()));
    }

    [Fact]
    public void LoadAccessoryFromJson_IconMissingPath_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson("""
                {
                  "id": "gun",
                  "effects": [],
                  "icon": {}
                }
                """, RegistryWithShootFactory()));
    }

    [Fact]
    public void LoadAccessoryFromJson_UnknownEffectType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson("""
                {
                  "id": "odd",
                  "effects": [ { "type": "explode" } ]
                }
                """, RegistryWithShootFactory()));
    }

    [Fact]
    public void LoadAccessoryFromJson_MissingId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson("""{ "effects": [] }""", RegistryWithShootFactory()));
    }

    [Fact]
    public void LoadCharacterFromJson_ResolvesAccessories()
    {
        var gun = new AccessoryDefinition("gun", Array.Empty<AccessoryEffect>());
        var byId = new Dictionary<string, AccessoryDefinition>(StringComparer.Ordinal)
        {
            ["gun"] = gun,
        };

        var def = DefinitionConfig.LoadCharacterFromJson(
            """{ "id": "generic", "accessories": ["gun"] }""",
            byId);

        Assert.Equal("generic", def.Id);
        Assert.Same(gun, Assert.Single(def.Accessories));
        Assert.Null(def.DepictionConfig);
        Assert.Null(def.IconConfig);
    }

    [Fact]
    public void LoadCharacterFromJson_WithDepiction_ParsesDepictionConfig()
    {
        var byId = new Dictionary<string, AccessoryDefinition>(StringComparer.Ordinal);

        var def = DefinitionConfig.LoadCharacterFromJson(
            """
            {
              "id": "generic",
              "accessories": [],
              "depiction": {
                "kind": "sprite_frames",
                "path": "res://assets/compuquest/kenney-1bit/depict/generic.tres"
              }
            }
            """,
            byId);

        var depiction = Assert.IsType<DepictionConfig>(def.DepictionConfig);
        Assert.Equal(DepictionKinds.SpriteFrames, depiction.Kind);
        Assert.Equal("res://assets/compuquest/kenney-1bit/depict/generic.tres", depiction.ResourcePath);
        Assert.Null(depiction.DefaultAnimation);
    }

    [Fact]
    public void LoadCharacterFromJson_WithIcon_ParsesIconConfig()
    {
        var byId = new Dictionary<string, AccessoryDefinition>(StringComparer.Ordinal);

        var def = DefinitionConfig.LoadCharacterFromJson(
            """
            {
              "id": "generic",
              "accessories": [],
              "icon": {
                "path": "res://assets/compuquest/game-icons/delapouite/person.svg"
              }
            }
            """,
            byId);

        var icon = Assert.IsType<IconConfig>(def.IconConfig);
        Assert.Equal(
            "res://assets/compuquest/game-icons/delapouite/person.svg",
            icon.ResourcePath);
    }

    [Fact]
    public void LoadCharacterFromJson_UnknownAccessory_Throws()
    {
        var byId = new Dictionary<string, AccessoryDefinition>(StringComparer.Ordinal);

        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadCharacterFromJson(
                """{ "id": "generic", "accessories": ["gun"] }""",
                byId));
    }

    [Fact]
    public void LoadAccessoriesFromDirectory_MissingDir_ReturnsEmpty()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"defs-missing-{Guid.NewGuid():N}");
        Assert.Empty(DefinitionConfig.LoadAccessoriesFromDirectory(missing, RegistryWithShootFactory()));
    }

    [Fact]
    public void ContentDirectoryForAssembly_UsesAssemblyNameBesideDll()
    {
        var path = Path.Combine(Path.GetTempPath(), "extensions", "CompuQuest.Minimap.dll");
        var content = DefinitionConfig.ContentDirectoryForAssembly(path);
        Assert.Equal(
            Path.Combine(Path.GetTempPath(), "extensions", "CompuQuest.Minimap"),
            content);
    }

    [Fact]
    public void RegisterFromConfigDirectory_LoadsShippedGunAndGeneric()
    {
        var repoRoot = FindRepoRoot();
        var registry = RegistryWithShootFactory();

        DefinitionConfig.RegisterFromConfigDirectory(
            Path.Combine(repoRoot, "src", "CompuQuest.Minimap", "config"),
            registry);

        var gun = Assert.Single(registry.AccessoryDefinitions);
        Assert.Equal("gun", gun.Id);
        Assert.IsType<ShootEffect>(Assert.Single(gun.EffectTemplates));
        Assert.NotNull(gun.DepictionConfig);
        Assert.Equal(DepictionKinds.SpriteFrames, gun.DepictionConfig.Kind);
        Assert.Equal(
            "res://assets/compuquest/kenney-1bit/depict/gun.tres",
            gun.DepictionConfig.ResourcePath);
        Assert.NotNull(gun.IconConfig);
        Assert.Equal(
            "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg",
            gun.IconConfig.ResourcePath);

        var generic = Assert.Single(registry.CharacterDefinitions);
        Assert.Equal("generic", generic.Id);
        Assert.Equal("gun", Assert.Single(generic.Accessories).Id);
        Assert.NotNull(generic.DepictionConfig);
        Assert.Equal(DepictionKinds.SpriteFrames, generic.DepictionConfig.Kind);
        Assert.Equal(
            "res://assets/compuquest/kenney-1bit/depict/generic.tres",
            generic.DepictionConfig.ResourcePath);
        Assert.NotNull(generic.IconConfig);
        Assert.Equal(
            "res://assets/compuquest/game-icons/delapouite/person.svg",
            generic.IconConfig.ResourcePath);
    }

    [Fact]
    public void RegisterFromConfigDirectory_TempDirs_RegistersFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"defs-{Guid.NewGuid():N}");
        var accessories = Path.Combine(root, DefinitionConfig.AccessoriesDirectoryName);
        var characters = Path.Combine(root, DefinitionConfig.CharactersDirectoryName);
        Directory.CreateDirectory(accessories);
        Directory.CreateDirectory(characters);

        try
        {
            File.WriteAllText(Path.Combine(accessories, "gun.json"), """
                {
                  "id": "gun",
                  "effects": [
                    {
                      "type": "shoot",
                      "fireIntervalSeconds": 1.0,
                      "missileSpeed": 100,
                      "missileDamage": 10
                    }
                  ]
                }
                """);
            File.WriteAllText(Path.Combine(characters, "generic.json"), """
                { "id": "generic", "accessories": ["gun"] }
                """);

            var registry = RegistryWithShootFactory();
            DefinitionConfig.RegisterFromConfigDirectory(root, registry);

            Assert.Equal("gun", Assert.Single(registry.AccessoryDefinitions).Id);
            Assert.Equal("generic", Assert.Single(registry.CharacterDefinitions).Id);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
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
