using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Holds arrow-right and asserts player0 visual X advances.</summary>
public sealed class ArrowRightMovesPlayerPlaybook : IPlaybook
{
    public string Id => "ArrowRightMovesPlayer";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.ClearLocalPlayContextAsync(cancellationToken);
        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(15, cancellationToken);

        var before = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!before.SceneLoaded || !before.IsWorldRoot)
            return PlaybookResult.Fail("World was not ready before movement.", Diagnostics(before));

        await context.SetMovementKeyAsync((int)Key.Right, pressed: true, cancellationToken);
        await context.WaitFramesAsync(30, cancellationToken);
        await context.SetMovementKeyAsync((int)Key.Right, pressed: false, cancellationToken);

        var after = await context.GetWorldSnapshotAsync(cancellationToken);
        if (after.Player0X <= before.Player0X)
        {
            return PlaybookResult.Fail(
                $"Expected +X motion; before={before.Player0X} after={after.Player0X}",
                $"before={Diagnostics(before)};after={Diagnostics(after)}");
        }

        return PlaybookResult.Success($"before={Diagnostics(before)};after={Diagnostics(after)}");
    }

    private static string Diagnostics(PlaybookWorldSnapshot snap) =>
        $"p0=({snap.Player0X},{snap.Player0Y})";
}
