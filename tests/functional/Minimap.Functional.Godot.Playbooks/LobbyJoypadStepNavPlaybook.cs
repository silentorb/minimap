using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>
/// After joypad claim, A advances via Forward (profile confirm) and B presses Back
/// (regression: step nav was mouse-only / Start-only).
/// </summary>
public sealed class LobbyJoypadStepNavPlaybook : IPlaybook
{
    public string Id => "LobbyJoypadStepNav";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        PlaybookProfileSeed.EnsureProfiles("Playbook");

        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await TapJoy(context, 0, JoyButton.A, cancellationToken);
        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "SelectingProfile")
            return PlaybookResult.Fail($"Expected SelectingProfile; got {claimed.SlotModes.FirstOrDefault()}.");

        // Activate / Forward: A should confirm profile (same as Forward button / Start).
        await TapJoy(context, 0, JoyButton.A, cancellationToken);
        var accessories = await context.GetLobbySnapshotAsync(cancellationToken);
        if (accessories.SlotModes.FirstOrDefault() != "SelectingAccessories")
        {
            return PlaybookResult.Fail(
                $"Expected SelectingAccessories after joypad A (Forward); got {accessories.SlotModes.FirstOrDefault()}.");
        }

        // Back button equivalent.
        await TapJoy(context, 0, JoyButton.B, cancellationToken);
        var backToProfile = await context.GetLobbySnapshotAsync(cancellationToken);
        if (backToProfile.SlotModes.FirstOrDefault() != "SelectingProfile")
        {
            return PlaybookResult.Fail(
                $"Expected SelectingProfile after joypad B (Back); got {backToProfile.SlotModes.FirstOrDefault()}.");
        }

        return PlaybookResult.Success("joypad-step-nav");
    }

    private static async Task TapJoy(
        IPlaybookContext context,
        int device,
        JoyButton button,
        CancellationToken cancellationToken)
    {
        await context.SetJoypadButtonAsync(device, (int)button, true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetJoypadButtonAsync(device, (int)button, false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
    }
}
