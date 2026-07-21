using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>CompuQuest game integrator.</summary>
public sealed class CompuQuestIntegrator : IIntegrator
{
    public const string IntegratorId = "compuquest";

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
