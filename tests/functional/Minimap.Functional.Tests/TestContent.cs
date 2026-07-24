using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Functional.Tests;

internal static class TestContent
{
    private static readonly TagRegistry Tags = new();

    public static ResourceDefinition MaxHealthResource { get; } = new(
        WellKnownResourceIds.MaxHealth,
        Tags.GetOrCreate(WellKnownResourceIds.MaxHealth),
        displayName: "Max Health",
        visible: false);

    public static ResourceDefinition HealthResource { get; } = new(
        WellKnownResourceIds.Health,
        Tags.GetOrCreate(WellKnownResourceIds.Health),
        displayName: "Health",
        limitTag: MaxHealthResource.Tag,
        visible: true,
        uiPriority: 1000);

    public static IReadOnlyList<ResourceDefinition> Resources { get; } =
    [
        HealthResource,
        MaxHealthResource,
    ];

    public static ResourceContext ResourceContext { get; } = new(
        Resources,
        HealthResource.Tag,
        MaxHealthResource.Tag);

    public static GameContent Content { get; } = new(
        new CharacterDefinition("generic", Array.Empty<AccessoryDefinition>()),
        resources: Resources);
}
