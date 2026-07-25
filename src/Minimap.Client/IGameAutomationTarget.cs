namespace Minimap.Client;

/// <summary>Automation hooks for in-world play (implemented by WorldApp).</summary>
public interface IGameAutomationTarget
{
    int HumanPlayerCount { get; }

    bool GameplayPaused { get; }

    bool MainMenuPopupVisible { get; }

    ReconnectOverlay? ReconnectOverlay { get; }

    void SimulateJoypadDisconnectForTests(int playerIndex);

    void ForceGameOverForTests();
}
