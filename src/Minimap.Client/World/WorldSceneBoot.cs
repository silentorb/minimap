namespace Minimap.Client.World;

/// <summary>
/// Ordered world scene boot (Godot-free): settings → session → views → tick.
/// On failure the scene is aborted and must not tick or continue as if ready.
/// </summary>
public sealed class WorldSceneBoot
{
    public bool SettingsLoaded { get; private set; }
    public bool SessionBound { get; private set; }
    public bool ViewsBound { get; private set; }
    public bool IsAborted { get; private set; }
    public string? AbortReason { get; private set; }

    /// <summary>Step 1: core, extensions, and scenario settings loaded.</summary>
    public void MarkSettingsLoaded()
    {
        ThrowIfAborted();
        if (SessionBound || ViewsBound)
            throw new InvalidOperationException("Settings must be marked loaded before session/views.");
        SettingsLoaded = true;
    }

    /// <summary>Step 2: Simulation <c>GameSession</c> and client session created.</summary>
    public void MarkSessionBound()
    {
        ThrowIfAborted();
        if (!SettingsLoaded)
            throw new InvalidOperationException("Session cannot bind before settings are loaded.");
        if (ViewsBound)
            throw new InvalidOperationException("Session must be marked bound before views.");
        SessionBound = true;
    }

    /// <summary>Step 3: WorldView, HUD, overlays bound. Enables simulation tick.</summary>
    public void MarkViewsBound()
    {
        ThrowIfAborted();
        if (!SessionBound)
            throw new InvalidOperationException("Views cannot bind before session is bound.");
        ViewsBound = true;
    }

    /// <summary>Hard-fail: never tick; scene must not proceed as a playable world.</summary>
    public void Abort(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        IsAborted = true;
        AbortReason = reason;
    }

    public bool TryTick() => ViewsBound && !IsAborted;

    private void ThrowIfAborted()
    {
        if (IsAborted)
            throw new InvalidOperationException($"World boot already aborted: {AbortReason}");
    }
}
