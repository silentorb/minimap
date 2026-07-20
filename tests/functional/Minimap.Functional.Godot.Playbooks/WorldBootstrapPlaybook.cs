using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Loads the main world scene and asserts hex + player visual layers are populated.</summary>
public sealed class WorldBootstrapPlaybook : IPlaybook
{
    public string Id => "WorldBootstrap";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.ClearLocalPlayContextAsync(cancellationToken);
        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(15, cancellationToken);

        var snap = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!snap.SceneLoaded)
            return PlaybookResult.Fail("Scene was not loaded.");
        if (!snap.IsWorldRoot)
            return PlaybookResult.Fail("Current scene is not a WorldView root.");
        if (snap.HexLayerChildren <= 0)
            return PlaybookResult.Fail("Hex layer has no children.", Diagnostics(snap));
        if (snap.PlayerLayerChildren <= 0)
            return PlaybookResult.Fail("Player layer has no children.", Diagnostics(snap));

        return PlaybookResult.Success(Diagnostics(snap));
    }

    private static string Diagnostics(PlaybookWorldSnapshot snap) =>
        $"hex={snap.HexLayerChildren};players={snap.PlayerLayerChildren};p0=({snap.Player0X},{snap.Player0Y})";
}
