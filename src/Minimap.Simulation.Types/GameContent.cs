namespace Minimap.Simulation.Types;

/// <summary>Playthrough content bag (e.g. default character). Not an integration-facing type.</summary>
public sealed class GameContent
{
    public GameContent(CharacterDefinition defaultCharacter)
    {
        ArgumentNullException.ThrowIfNull(defaultCharacter);
        DefaultCharacter = defaultCharacter;
    }

    public CharacterDefinition DefaultCharacter { get; }
}
