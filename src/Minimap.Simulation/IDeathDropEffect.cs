namespace Minimap.Simulation;

/// <summary>Declares a cell actor to place when the owning character dies.</summary>
public interface IDeathDropEffect
{
    string ActorDefinitionId { get; }
}
