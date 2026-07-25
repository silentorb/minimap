using Godot;
using Minimap.Automation;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>
/// After keyboard claim + profile confirm, accessory selection fills CustomizeArea with three
/// vertically scrolling child panels and fits horizontally without whole-panel clip.
/// </summary>
public sealed class LobbyAccessorySelectionFitsPlaybook : IPlaybook
{
    private const float Epsilon = 1f;

    public string Id => "LobbyAccessorySelectionFits";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        PlaybookProfileSeed.EnsureProfiles("Playbook");

        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "SelectingProfile")
        {
            return PlaybookResult.Fail(
                $"Slot 0 should be SelectingProfile; got {claimed.SlotModes.FirstOrDefault()}.");
        }

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var accessories = await context.GetLobbySnapshotAsync(cancellationToken);
        if (accessories.SlotModes.FirstOrDefault() != "SelectingAccessories")
        {
            return PlaybookResult.Fail(
                $"Slot 0 should be SelectingAccessories; got {accessories.SlotModes.FirstOrDefault()}.");
        }

        const string panelPath = "Margin/PanelRow/LobbyPanel";
        const string areaPath = "Margin/PanelRow/LobbyPanel/Margin/VBox/CustomizeArea";
        const string selectionPath = "Margin/PanelRow/LobbyPanel/Margin/VBox/CustomizeArea/AccessorySelection";

        var panelSnap = await context.GetControlRectAsync(panelPath, cancellationToken);
        if (!panelSnap.Found)
            return PlaybookResult.Fail($"LobbyPanel not found at {panelPath}.");

        var areaSnap = await context.GetControlRectAsync(areaPath, cancellationToken);
        if (!areaSnap.Found)
            return PlaybookResult.Fail($"CustomizeArea not found at {areaPath}.");
        if (string.Equals(areaSnap.ClassName, "ScrollContainer", StringComparison.Ordinal))
        {
            return PlaybookResult.Fail(
                "CustomizeArea must not be a whole-panel ScrollContainer (use Control + inner vertical scrolls).");
        }

        var selectionSnap = await context.GetControlRectAsync(selectionPath, cancellationToken);
        if (!selectionSnap.Found)
            return PlaybookResult.Fail($"AccessorySelection not found at {selectionPath}.");
        if (!selectionSnap.Visible)
            return PlaybookResult.Fail("AccessorySelection should be visible after profile confirm.");

        var viewportSnap = await context.GetViewportVisibleRectAsync(cancellationToken);
        if (!viewportSnap.Found)
            return PlaybookResult.Fail("Viewport visible rect unavailable.");

        var panelRect = ToRect(panelSnap);
        var areaRect = ToRect(areaSnap);
        var selectionRect = ToRect(selectionSnap);
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

        if (!ControlLayout.Contains(areaRect, selectionRect, Epsilon))
        {
            return PlaybookResult.Fail(
                $"AccessorySelection overflows CustomizeArea. selection={Format(selectionRect)} area={Format(areaRect)}.");
        }

        if (selectionSnap.MinWidth > areaSnap.Width + Epsilon)
        {
            return PlaybookResult.Fail(
                "AccessorySelection min width exceeds CustomizeArea (horizontal fit required). "
                + $"selMinW={selectionSnap.MinWidth:0.##} areaW={areaSnap.Width:0.##}.");
        }

        foreach (var section in new[] { "Available", "Description", "Owned" })
        {
            var sectionPath = $"{selectionPath}/{section}";
            var sectionSnap = await context.GetControlRectAsync(sectionPath, cancellationToken);
            if (!sectionSnap.Found)
                return PlaybookResult.Fail($"Missing child panel '{section}' at {sectionPath}.");

            var scrollPath = $"{sectionPath}/Content/Scroll";
            var scrollSnap = await context.GetControlRectAsync(scrollPath, cancellationToken);
            if (!scrollSnap.Found)
                return PlaybookResult.Fail($"Missing vertical scroll at {scrollPath}.");
            if (!string.Equals(scrollSnap.ClassName, "ScrollContainer", StringComparison.Ordinal))
            {
                return PlaybookResult.Fail(
                    $"Expected ScrollContainer at {scrollPath}; got {scrollSnap.ClassName}.");
            }
        }

        return PlaybookResult.Success("three-panels-fit");
    }

    private static Rect2 ToRect(PlaybookControlRectSnapshot snap) =>
        new(snap.X, snap.Y, snap.Width, snap.Height);

    private static string Format(Rect2 rect) =>
        $"({rect.Position.X:0.##},{rect.Position.Y:0.##},{rect.Size.X:0.##}x{rect.Size.Y:0.##})";
}
