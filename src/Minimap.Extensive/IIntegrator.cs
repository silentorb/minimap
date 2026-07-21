using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>Single authority for integrating extension contributions into a playthrough.</summary>
public interface IIntegrator
{
    string Id { get; }

    /// <summary>Build playthrough <see cref="GameContent"/> from the registry.</summary>
    GameContent CreateGameContent(IExtensionRegistry registry);
}
