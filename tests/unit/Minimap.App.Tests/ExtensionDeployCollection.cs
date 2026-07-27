using Xunit;

namespace Minimap.App.Tests;

/// <summary>
/// Serializes tests that read or wipe+mirror the shared
/// <c>extensions/CompuQuest.Minimap/</c> deploy tree.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ExtensionDeployCollection : ICollectionFixture<object>
{
    public const string Name = "ExtensionDeploy";
}
