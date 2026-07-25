using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class DomainColorResolverTests
{
    [Fact]
    public void Resolve_returns_empty_when_no_domain_tags()
    {
        var tags = new TagRegistry();
        var gardening = new DomainDefinition(
            "gardening",
            tags.GetOrCreate("gardening"),
            new ColorRgb(0.2f, 0.5f, 0.3f));
        var accessory = new AccessoryDefinition(
            "gun",
            Array.Empty<AccessoryEffect>(),
            tags: [tags.GetOrCreate("player_selectable")]);

        Assert.Empty(DomainColorResolver.Resolve(accessory, [gardening]));
    }

    [Fact]
    public void Resolve_returns_matching_colors_in_domain_registration_order()
    {
        var tags = new TagRegistry();
        var gardeningTag = tags.GetOrCreate("gardening");
        var computingTag = tags.GetOrCreate("computing");
        var gardening = new DomainDefinition(
            "gardening",
            gardeningTag,
            new ColorRgb(0.2f, 0.5f, 0.3f));
        var computing = new DomainDefinition(
            "computing",
            computingTag,
            new ColorRgb(0.7f, 0.74f, 0.78f));
        var accessory = new AccessoryDefinition(
            "hybrid",
            Array.Empty<AccessoryEffect>(),
            tags: [computingTag, gardeningTag]);

        var colors = DomainColorResolver.Resolve(accessory, [gardening, computing]);
        Assert.Equal([gardening.Color, computing.Color], colors);
    }

    [Fact]
    public void Resolve_ignores_tags_without_domain_definitions()
    {
        var tags = new TagRegistry();
        var gardening = new DomainDefinition(
            "gardening",
            tags.GetOrCreate("gardening"),
            new ColorRgb(0.2f, 0.5f, 0.3f));
        var accessory = new AccessoryDefinition(
            "farm",
            Array.Empty<AccessoryEffect>(),
            tags:
            [
                tags.GetOrCreate("gardening"),
                tags.GetOrCreate("player_selectable"),
            ]);

        var colors = DomainColorResolver.Resolve(accessory, [gardening]);
        Assert.Equal([gardening.Color], colors);
    }
}
