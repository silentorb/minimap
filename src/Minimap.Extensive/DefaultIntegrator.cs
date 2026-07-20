namespace Minimap.Extensive;

/// <summary>Built-in integrator always registered by the host before extension DLLs load.</summary>
public sealed class DefaultIntegrator : IIntegrator
{
    public const string IntegratorId = "default";

    public string Id => IntegratorId;
}
