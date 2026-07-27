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
            if (registry.ActorDefinitions.Count == 0)
                throw new InvalidOperationException("No characters.");

            var tags = registry.Tags;
            var maxHealth = new ResourceDefinition(
                WellKnownResourceIds.MaxHealth,
                tags.GetOrCreate(WellKnownResourceIds.MaxHealth),
                visible: false);
            var health = new ResourceDefinition(
                WellKnownResourceIds.Health,
                tags.GetOrCreate(WellKnownResourceIds.Health),
                limitTag: maxHealth.Tag,
                visible: true,
                uiPriority: 1000);
            var maxEnergy = new ResourceDefinition(
                WellKnownResourceIds.MaxEnergy,
                tags.GetOrCreate(WellKnownResourceIds.MaxEnergy),
                visible: false);
            var energy = new ResourceDefinition(
                WellKnownResourceIds.Energy,
                tags.GetOrCreate(WellKnownResourceIds.Energy),
                limitTag: maxEnergy.Tag,
                visible: true,
                uiPriority: 900);
            return new GameContent(
                registry.ActorDefinitions[0],
                resources: [health, maxHealth, energy, maxEnergy]);
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
        var charA = new ActorDefinition("char-a", [gunA]);
        var charB = new ActorDefinition("char-b", [gunB]);

        registry.AddAccessoryDefinition(gunA);
        registry.AddAccessoryDefinition(gunB);
        registry.AddActorDefinition(charA);
        registry.AddActorDefinition(charB);

        Assert.Equal(["gun-a", "gun-b"], registry.AccessoryDefinitions.Select(d => d.Id));
        Assert.Equal(["char-a", "char-b"], registry.ActorDefinitions.Select(d => d.Id));
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
    public void AddDomainDefinition_rejects_duplicate_id_or_tag()
    {
        var registry = new ExtensionRegistry();
        var tag = registry.Tags.GetOrCreate("gardening");
        var color = new ColorRgb(0.2f, 0.5f, 0.3f);
        registry.AddDomainDefinition(new DomainDefinition("gardening", tag, color));

        Assert.Throws<InvalidOperationException>(() =>
            registry.AddDomainDefinition(new DomainDefinition("gardening", tag, color)));

        var otherTag = registry.Tags.GetOrCreate("other");
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddDomainDefinition(new DomainDefinition("other", tag, color)));

        Assert.True(registry.TryGetDomainDefinition("gardening", out var byId));
        Assert.Equal("gardening", byId!.Id);
        Assert.True(registry.TryGetDomainDefinition(tag, out var byTag));
        Assert.Same(byId, byTag);
        Assert.False(registry.TryGetDomainDefinition(otherTag, out _));
    }

    [Fact]
    public void AddActorDefinition_rejects_duplicate_id()
    {
        var registry = new ExtensionRegistry();
        var def = new ActorDefinition("generic", Array.Empty<AccessoryDefinition>());
        registry.AddActorDefinition(def);
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddActorDefinition(new ActorDefinition("generic", Array.Empty<AccessoryDefinition>())));
    }

    [Fact]
    public void AddAccessoryEffectFactory_rejects_duplicate_type()
    {
        var registry = new ExtensionRegistry();
        registry.AddAccessoryEffectFactory("shoot", (_, _, _, _) =>
            throw new InvalidOperationException("unused"));
        Assert.Throws<InvalidOperationException>(() =>
            registry.AddAccessoryEffectFactory("SHOOT", (_, _, _, _) =>
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
        var first = new ActorDefinition("first", Array.Empty<AccessoryDefinition>());
        var second = new ActorDefinition("second", Array.Empty<AccessoryDefinition>());
        registry.AddActorDefinition(first);
        registry.AddActorDefinition(second);

        var content = new TestIntegrator().CreateGameContent(registry);
        Assert.Same(first, content.DefaultActor);
    }

    [Fact]
    public void CreateGameContent_throws_when_no_characters()
    {
        var registry = new ExtensionRegistry();
        Assert.Throws<InvalidOperationException>(() =>
            new TestIntegrator().CreateGameContent(registry));
    }
}
