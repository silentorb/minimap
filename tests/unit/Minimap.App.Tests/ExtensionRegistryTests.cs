using Minimap.Extensive;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class ExtensionRegistryTests
{
    private sealed class TestIntegrator : IIntegrator
    {
        public string Id { get; init; } = "test";

        public GameContent CreateGameContent(IExtensionRegistry registry)
        {
            if (registry.CharacterDefinitions.Count == 0)
                throw new InvalidOperationException("No characters.");
            return new GameContent(registry.CharacterDefinitions[0]);
        }

        public IReadOnlyList<AccessoryDefinition> GetPlayerSelectableAccessories(
            IExtensionRegistry registry) =>
            Array.Empty<AccessoryDefinition>();
    }

    [Fact]
    public void AddIntegrator_registers_by_id()
    {
        var registry = new ExtensionRegistry();
        registry.AddIntegrator(new TestIntegrator { Id = "default" });

        Assert.True(registry.TryGetIntegrator("default", out var integrator));
        Assert.Equal("default", integrator!.Id);
        Assert.Single(registry.Integrators);
    }

    [Fact]
    public void AddIntegrator_rejects_duplicate_id()
    {
        var registry = new ExtensionRegistry();
        registry.AddIntegrator(new TestIntegrator { Id = "default" });

        Assert.Throws<InvalidOperationException>(() =>
            registry.AddIntegrator(new TestIntegrator { Id = "default" }));
    }

    [Fact]
    public void RequireIntegrator_throws_when_missing()
    {
        var registry = new ExtensionRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.RequireIntegrator("missing"));
    }

    [Fact]
    public void Accessory_and_character_definitions_preserve_registration_order()
    {
        var registry = new ExtensionRegistry();
        var gunA = new AccessoryDefinition("gun-a", Array.Empty<AccessoryEffect>());
        var gunB = new AccessoryDefinition("gun-b", Array.Empty<AccessoryEffect>());
        var charA = new CharacterDefinition("char-a", [gunA]);
        var charB = new CharacterDefinition("char-b", [gunB]);

        registry.AddAccessoryDefinition(gunA);
        registry.AddAccessoryDefinition(gunB);
        registry.AddCharacterDefinition(charA);
        registry.AddCharacterDefinition(charB);

        Assert.Equal(["gun-a", "gun-b"], registry.AccessoryDefinitions.Select(d => d.Id));
        Assert.Equal(["char-a", "char-b"], registry.CharacterDefinitions.Select(d => d.Id));
    }

    [Fact]
    public void AddAccessoryDefinition_rejects_duplicate_id()
    {
        var registry = new ExtensionRegistry();
        var gun = new AccessoryDefinition("gun", Array.Empty<AccessoryEffect>());
        registry.AddAccessoryDefinition(gun);
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddAccessoryDefinition(new AccessoryDefinition("gun", Array.Empty<AccessoryEffect>())));
    }

    [Fact]
    public void AddCharacterDefinition_rejects_duplicate_id()
    {
        var registry = new ExtensionRegistry();
        var def = new CharacterDefinition("generic", Array.Empty<AccessoryDefinition>());
        registry.AddCharacterDefinition(def);
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddCharacterDefinition(new CharacterDefinition("generic", Array.Empty<AccessoryDefinition>())));
    }

    [Fact]
    public void AddAccessoryEffectFactory_rejects_duplicate_type()
    {
        var registry = new ExtensionRegistry();
        registry.AddAccessoryEffectFactory("shoot", (_, _, _) =>
            throw new InvalidOperationException("unused"));
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddAccessoryEffectFactory("SHOOT", (_, _, _) =>
                throw new InvalidOperationException("unused")));
    }

    [Fact]
    public void RegisterTags_get_or_create_is_idempotent()
    {
        var registry = new ExtensionRegistry();
        registry.RegisterTags(["player_selectable", "player_selectable"]);
        Assert.True(registry.Tags.TryGet("player_selectable", out var a));
        Assert.True(registry.Tags.TryGet("player_selectable", out var b));
        Assert.Equal(a, b);
        Assert.Equal(1, registry.Tags.Count);
    }

    [Fact]
    public void CreateGameContent_uses_first_registered_character()
    {
        var registry = new ExtensionRegistry();
        var first = new CharacterDefinition("first", Array.Empty<AccessoryDefinition>());
        var second = new CharacterDefinition("second", Array.Empty<AccessoryDefinition>());
        registry.AddCharacterDefinition(first);
        registry.AddCharacterDefinition(second);

        var content = new TestIntegrator().CreateGameContent(registry);
        Assert.Same(first, content.DefaultCharacter);
    }

    [Fact]
    public void CreateGameContent_throws_when_no_characters()
    {
        var registry = new ExtensionRegistry();
        Assert.Throws<InvalidOperationException>(() =>
            new TestIntegrator().CreateGameContent(registry));
    }
}
