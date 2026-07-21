namespace Minimap.App;

/// <summary>
/// Ordered lobby scene boot (Godot-free): bind panels → extension preflight → accept input.
/// On failure the scene is aborted and must not accept input or continue as if ready.
/// </summary>
public sealed class LobbySceneBoot
{
    public bool PanelsBound { get; private set; }
    public bool ExtensionsLoaded { get; private set; }
    public bool AcceptsInput { get; private set; }
    public bool IsAborted { get; private set; }
    public string? AbortReason { get; private set; }

    /// <summary>Step 1: panel array bound and safe to refresh.</summary>
    public void MarkPanelsBound()
    {
        ThrowIfAborted();
        if (ExtensionsLoaded || AcceptsInput)
            throw new InvalidOperationException("Panels must be marked bound before extensions/input.");
        PanelsBound = true;
    }

    /// <summary>Step 2: extension preflight succeeded. Enables input only after panels are bound.</summary>
    public void MarkExtensionsLoaded()
    {
        ThrowIfAborted();
        if (!PanelsBound)
            throw new InvalidOperationException("Extensions cannot load before panels are bound.");
        ExtensionsLoaded = true;
        AcceptsInput = true;
    }

    /// <summary>Hard-fail: never accept input; scene must not proceed as a playable lobby.</summary>
    public void Abort(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        IsAborted = true;
        AcceptsInput = false;
        AbortReason = reason;
    }

    public bool TryAcceptInput() => AcceptsInput && !IsAborted;

    public bool CanRefreshPanels() => PanelsBound && !IsAborted;

    private void ThrowIfAborted()
    {
        if (IsAborted)
            throw new InvalidOperationException($"Lobby boot already aborted: {AbortReason}");
    }
}
