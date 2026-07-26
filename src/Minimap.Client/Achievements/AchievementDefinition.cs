namespace Minimap.Client.Achievements;

/// <summary>Code-owned achievement definition (catalog entry).</summary>
public sealed class AchievementDefinition
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}
