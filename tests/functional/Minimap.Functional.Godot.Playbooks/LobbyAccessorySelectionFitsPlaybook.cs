using Godot;
using Minimap.Automation;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>
/// After keyboard claim, CustomizeArea scrolls/clips accessory selection so layout
/// stays inside the player panel and lobby viewport.
/// </summary>
public sealed class LobbyAccessorySelectionFitsPlaybook : IPlaybook
{
    private const float Epsilon = 1f;
    private const int ConstrainedWidth = 800;
    private const int ConstrainedHeight = 480;

    public string Id => "LobbyAccessorySelectionFits";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetWindowSizeAsync(ConstrainedWidth, ConstrainedHeight, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "Claimed")
            return PlaybookResult.Fail($"Slot 0 should be Claimed; got {claimed.SlotModes.FirstOrDefault()}.");

        const string panelPath = "Margin/PanelRow/LobbyPanel";
        const string areaPath = "Margin/PanelRow/LobbyPanel/Margin/VBox/CustomizeArea";
        const string selectionPath = "Margin/PanelRow/LobbyPanel/Margin/VBox/CustomizeArea/AccessorySelection";

        var panelSnap = await context.GetControlRectAsync(panelPath, cancellationToken);
        if (!panelSnap.Found)
            return PlaybookResult.Fail($"LobbyPanel not found at {panelPath}.");

        var areaSnap = await context.GetControlRectAsync(areaPath, cancellationToken);
        if (!areaSnap.Found)
            return PlaybookResult.Fail($"CustomizeArea not found at {areaPath}.");

        var selectionSnap = await context.GetControlRectAsync(selectionPath, cancellationToken);
        if (!selectionSnap.Found)
            return PlaybookResult.Fail($"AccessorySelection not found at {selectionPath}.");
        if (!selectionSnap.Visible)
            return PlaybookResult.Fail("AccessorySelection should be visible after claim.");

        var viewportSnap = await context.GetViewportVisibleRectAsync(cancellationToken);
        if (!viewportSnap.Found)
            return PlaybookResult.Fail("Viewport visible rect unavailable.");

        var panelRect = ToRect(panelSnap);
        var areaRect = ToRect(areaSnap);
        var viewportRect = ToRect(viewportSnap);

        if (!ControlLayout.Contains(viewportRect, panelRect, Epsilon))
        {
            return PlaybookResult.Fail(
                $"LobbyPanel overflows viewport. panel={Format(panelRect)} viewport={Format(viewportRect)}.");
        }

        if (!ControlLayout.Contains(panelRect, areaRect, Epsilon))
        {
            return PlaybookResult.Fail(
                $"CustomizeArea overflows LobbyPanel. area={Format(areaRect)} panel={Format(panelRect)}.");
        }

        if (!ControlLayout.Contains(viewportRect, areaRect, Epsilon))
        {
            return PlaybookResult.Fail(
                $"CustomizeArea overflows viewport. area={Format(areaRect)} viewport={Format(viewportRect)}.");
        }

        // Bare Control + FullRect selection draws past the area when content grows; docs require
        // a ScrollContainer so overflow scrolls/clips instead of escaping the panel/window.
        if (!string.Equals(areaSnap.ClassName, "ScrollContainer", StringComparison.Ordinal))
        {
            return PlaybookResult.Fail(
                "CustomizeArea must be a ScrollContainer so accessory selection cannot overflow the panel. "
                + $"Got class={areaSnap.ClassName}, "
                + $"selMin=({selectionSnap.MinWidth:0.##}x{selectionSnap.MinHeight:0.##}), "
                + $"area=({areaSnap.Width:0.##}x{areaSnap.Height:0.##}).");
        }

        return PlaybookResult.Success("selection-scrolls");
    }

    private static Rect2 ToRect(PlaybookControlRectSnapshot snap) =>
        new(snap.X, snap.Y, snap.Width, snap.Height);

    private static string Format(Rect2 rect) =>
        $"({rect.Position.X:0.##},{rect.Position.Y:0.##},{rect.Size.X:0.##}x{rect.Size.Y:0.##})";
}
