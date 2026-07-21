using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>Built-in integrator always registered by the host before extension DLLs load.</summary>
public sealed class DefaultIntegrator : IIntegrator
{
    public const string IntegratorId = "default";

    public string Id => IntegratorId;

    public GameContent CreateGameContent(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (registry.CharacterDefinitions.Count == 0)
        {
            throw new InvalidOperationException(
                "No character definitions are registered; cannot create game content.");
        }

        return new GameContent(registry.CharacterDefinitions[0]);
    }
}
