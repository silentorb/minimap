using CompuQuest.Minimap;
using Minimap.Extensive;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class DefinitionConfigTests
{
    private static ExtensionRegistry RegistryWithCompuQuestFactories()
    {
        var registry = new ExtensionRegistry();
        registry.AddAccessoryEffectFactory(ShootEffectFactory.TypeId, ShootEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            PlaceRandomActorEffectFactory.TypeId,
            PlaceRandomActorEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceEffectFactory.TypeId,
            ModifyResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(GrowEffectFactory.TypeId, GrowEffectFactory.Create);
        registry.AddAccessoryEffectFactory(HarvestEffectFactory.TypeId, HarvestEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            DrainResourceEffectFactory.TypeId,
            DrainResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceByRatioBandsEffectFactory.TypeId,
            ModifyResourceByRatioBandsEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceOnUseEffectFactory.TypeId,
            ModifyResourceOnUseEffectFactory.Create);
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

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithCompuQuestFactories());

        Assert.Equal("gun", def.Id);
        var shoot = Assert.IsType<ShootEffect>(Assert.Single(def.EffectTemplates));
        Assert.Equal(1.25f, shoot.FireIntervalSeconds);
        Assert.Equal(200f, shoot.MissileSpeed);
        Assert.Equal(25, shoot.MissileDamage);
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
              "effects": [
                {
                  "type": "shoot",
                  "fireIntervalSeconds": 1.25,
                  "missileSpeed": 200,
                  "missileDamage": 25
                }
              ],
              "depiction": {
                "kind": "sprite_frames",
                "path": "res://assets/compuquest/kenney-1bit/depict/gun.tres",
                "animation": "default"
              }
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithCompuQuestFactories());
        Assert.NotNull(def.DepictionConfig);
        Assert.Equal(DepictionKinds.SpriteFrames, def.DepictionConfig.Kind);
    }

    [Fact]
    public void LoadAccessoryFromJson_WithIcon_ParsesIconConfig()
    {
        const string json = """
            {
              "id": "gun",
              "effects": [
                {
                  "type": "shoot",
                  "fireIntervalSeconds": 1.25,
                  "missileSpeed": 200,
                  "missileDamage": 25
                }
              ],
              "icon": {
                "path": "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg"
              }
            }
            """;

        var def = DefinitionConfig.LoadAccessoryFromJson(json, RegistryWithCompuQuestFactories());
        Assert.NotNull(def.IconConfig);
        Assert.Equal(
            "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg",
            def.IconConfig.ResourcePath);
    }

    [Fact]
    public void LoadAccessoryFromJson_UnknownEffectType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson(
                """
                {
                  "id": "gun",
                  "effects": [ { "type": "nope" } ]
                }
                """, RegistryWithCompuQuestFactories()));
    }

    [Fact]
    public void LoadAccessoryFromJson_MissingEffects_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson(
                """{ "id": "gun" }""", RegistryWithCompuQuestFactories()));
    }

    [Fact]
    public void LoadAccessoryFromJson_MissingId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson(
                """{ "effects": [] }""", RegistryWithCompuQuestFactories()));
    }

    [Fact]
    public void LoadAccessoryFromJson_LegacyResourceBlock_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.LoadAccessoryFromJson(
                """
                {
                  "id": "gun",
                  "resource": { "id": "ammo", "startingAmount": 6 },
                  "effects": []
                }
                """,
                RegistryWithCompuQuestFactories()));
    }

    [Fact]
    public void RegisterFromConfigDirectory_LoadsShippedCompuQuestContent()
    {
        var repoRoot = FindRepoRoot();
        var registry = RegistryWithCompuQuestFactories();
        registry.RegisterTags(["player_selectable"]);

        DefinitionConfig.RegisterFromConfigDirectory(
            Path.Combine(repoRoot, "src", "CompuQuest.Minimap", "config"),
            registry);

        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "gun");
        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "farm");
        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "geek");
        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "grow_carrot");
        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "energy_upkeep");
        Assert.Contains(registry.AccessoryDefinitions, a => a.Id == "eat");

        var gun = registry.AccessoryDefinitions.Single(a => a.Id == "gun");
        Assert.Equal(1, gun.PointCost);
        Assert.True(gun.HasTag(registry.Tags.GetOrCreate("player_selectable")));
        Assert.Contains(gun.EffectTemplates, e => e is ModifyResourceEffect);
        Assert.Contains(gun.EffectTemplates, e => e is ShootEffect shoot && shoot.CostAmount == 1);

        var farm = registry.AccessoryDefinitions.Single(a => a.Id == "farm");
        Assert.Equal(AccessoryActivationKind.Modal, farm.Activation.Kind);
        Assert.Contains(farm.EffectTemplates, e => e is PlaceRandomActorEffect);
        Assert.Contains(farm.EffectTemplates, e => e is HarvestEffect);

        var geek = registry.AccessoryDefinitions.Single(a => a.Id == "geek");
        Assert.Equal(AccessoryActivationKind.Modal, geek.Activation.Kind);

        var eat = registry.AccessoryDefinitions.Single(a => a.Id == "eat");
        Assert.Equal(AccessoryActivationKind.Modal, eat.Activation.Kind);
        Assert.NotNull(eat.EnabledWhen);
        Assert.Contains(eat.EffectTemplates, e => e is ModifyResourceOnUseEffect);

        var upkeep = registry.AccessoryDefinitions.Single(a => a.Id == "energy_upkeep");
        Assert.Equal(AccessoryActivationKind.None, upkeep.Activation.Kind);
        Assert.Contains(upkeep.EffectTemplates, e => e is DrainResourceEffect);
        Assert.Contains(upkeep.EffectTemplates, e => e is ModifyResourceByRatioBandsEffect);

        Assert.Equal(4, registry.ActorDefinitions.Count);
        Assert.Contains(registry.ActorDefinitions, p => p.Id == "carrot");
        Assert.Contains(registry.ActorDefinitions, p => p.Id == "corn");
        Assert.Contains(registry.ActorDefinitions, p => p.Id == "melon");
        Assert.Contains(registry.ActorDefinitions, p => p.Id == "computer");

        var carrot = registry.ActorDefinitions.Single(a => a.Id == "carrot");
        Assert.Equal("grow_carrot", Assert.Single(carrot.Accessories).Id);
        Assert.Equal(
            "res://assets/compuquest/game-icons/delapouite/seedling.svg",
            carrot.DepictionConfig!.ResourcePath);

        Assert.Equal(8, registry.ResourceDefinitions.Count);
        Assert.Contains(registry.ResourceDefinitions, r => r.Id == "food");
        Assert.Contains(registry.ResourceDefinitions, r => r.Id == "seeds");
        Assert.Contains(registry.ResourceDefinitions, r => r.Id == "energy");
        Assert.Contains(registry.ResourceDefinitions, r => r.Id == "max_energy");

        Assert.Equal(2, registry.CharacterDefinitions.Count);
        var generic = registry.CharacterDefinitions.Single(c => c.Id == "generic");
        Assert.Equal(["energy_upkeep", "eat"], generic.Accessories.Select(a => a.Id).ToArray());

        var zombie = registry.CharacterDefinitions.Single(c => c.Id == "zombie");
        Assert.Equal(
            ["energy_upkeep", "eat", "gun"],
            zombie.Accessories.Select(a => a.Id).ToArray());
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

            var registry = RegistryWithCompuQuestFactories();
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

    [Fact]
    public void ValidateResourceLimits_rejects_limit_resource_that_is_itself_limited()
    {
        var tags = new TagRegistry();
        var other = new ResourceDefinition("other", tags.GetOrCreate("other"));
        var maxHealth = new ResourceDefinition(
            "max_health",
            tags.GetOrCreate("max_health"),
            limitTag: other.Tag,
            visible: false);
        var health = new ResourceDefinition(
            "health",
            tags.GetOrCreate("health"),
            limitTag: maxHealth.Tag);

        Assert.Throws<InvalidOperationException>(() =>
            DefinitionConfig.ValidateResourceLimits([health, maxHealth, other]));
    }

    [Fact]
    public void RegisterFromConfigDirectory_TempDirs_WithModifyResource_LoadsGrantEffect()
    {
        var root = Path.Combine(Path.GetTempPath(), $"defs-{Guid.NewGuid():N}");
        var resources = Path.Combine(root, DefinitionConfig.ResourcesDirectoryName);
        var accessories = Path.Combine(root, DefinitionConfig.AccessoriesDirectoryName);
        Directory.CreateDirectory(resources);
        Directory.CreateDirectory(accessories);

        try
        {
            File.WriteAllText(Path.Combine(resources, "ammo.json"), """
                { "id": "ammo", "displayName": "Ammo", "visible": true, "uiPriority": 80 }
                """);
            File.WriteAllText(Path.Combine(accessories, "gun.json"), """
                {
                  "id": "gun",
                  "effects": [
                    { "type": "modify_resource", "id": "ammo", "amount": 6 },
                    {
                      "type": "shoot",
                      "fireIntervalSeconds": 1.0,
                      "missileSpeed": 100,
                      "missileDamage": 10,
                      "cost": { "id": "ammo", "amount": 1 }
                    }
                  ]
                }
                """);

            var registry = RegistryWithCompuQuestFactories();
            DefinitionConfig.RegisterFromConfigDirectory(root, registry);

            var gun = Assert.Single(registry.AccessoryDefinitions);
            Assert.Contains(gun.EffectTemplates, e => e is ModifyResourceEffect);
            var shoot = Assert.IsType<ShootEffect>(
                gun.EffectTemplates.Single(e => e is ShootEffect));
            Assert.Equal(1, shoot.CostAmount);
            Assert.Equal("ammo", Assert.Single(registry.ResourceDefinitions).Id);
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
