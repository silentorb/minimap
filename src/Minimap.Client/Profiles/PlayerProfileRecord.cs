namespace Minimap.Client.Profiles;

/// <summary>One durable local user profile (identity + stats).</summary>
public sealed class PlayerProfileRecord
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public int Deaths { get; set; }
}
