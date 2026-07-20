using Minimap.Extensive;

namespace CompuQuest.Minimap;

/// <summary>CompuQuest game integrator (API surface empty beyond identity for now).</summary>
public sealed class CompuQuestIntegrator : IIntegrator
{
    public const string IntegratorId = "compuquest";

    public string Id => IntegratorId;
}
