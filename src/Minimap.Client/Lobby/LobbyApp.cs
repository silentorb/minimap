using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client.Lobby;

/// <summary>Local multiplayer lobby: claim slots, ready up, start world.</summary>
public partial class LobbyApp : Control, ILobbySnapshotSource
{
    private const string WorldScenePath = "res://scenes/world.tscn";

    private readonly LobbyStateMachine _lobby = new();
    private readonly LobbySceneBoot _boot = new();
    private LobbyPanel[] _panels = Array.Empty<LobbyPanel>();
    private LocalPlayContextNode? _playContext;

    [Export] public string ExtensionsSettingsPath { get; set; } = "res://config/extensions.json";

    public LobbyStateMachine LobbyState => _lobby;
    internal LobbySceneBoot Boot => _boot;

    public override void _Ready()
    {
        try
        {
            _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
            _playContext.Clear();

            var row = GetNode<HBoxContainer>("Margin/PanelRow");
            _panels = new LobbyPanel[LobbyStateMachine.SlotCount];
            for (var i = 0; i < LobbyStateMachine.SlotCount; i++)
                _panels[i] = row.GetChild<LobbyPanel>(i);

            _boot.MarkPanelsBound();
            RefreshPanels();

            // Fail fast if extension config / DLLs are invalid before the player starts a game.
            ExtensionPreflight.RequireLoadFromAbsolutePath(
                ProjectSettings.GlobalizePath(ExtensionsSettingsPath));
            _boot.MarkExtensionsLoaded();
        }
        catch (Exception ex)
        {
            _boot.Abort(ex.Message);
            GD.PushError($"Lobby boot failed: {ex.Message}");
            // Do not leave a half-initialized lobby interactive; exit after this frame.
            CallDeferred(MethodName.QuitAfterBootFailure);
        }
    }

    private void QuitAfterBootFailure() => GetTree().Quit(1);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_boot.TryAcceptInput())
            return;

        if (@event is InputEventKey key && !key.Echo)
        {
            if (key.Pressed)
                HandleDeviceInput(GameInput.DeviceFromKeyEvent(key), key.Keycode, null);
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
            HandleDeviceInput(GameInput.DeviceFromJoyEvent(joy), null, joy.ButtonIndex);
    }

    internal void HandleDeviceInput(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (!_boot.TryAcceptInput())
            return;

        if (key is Key k)
        {
            if (GameInput.IsBackKey(k))
            {
                if (_lobby.TryBack(device))
                    RefreshPanels();
                GetViewport().SetInputAsHandled();
                return;
            }

            if (GameInput.IsActivateKey(k))
            {
                if (TryActivateOrReady(device, k, null))
                    GetViewport().SetInputAsHandled();
                return;
            }
        }

        if (button is JoyButton b)
        {
            if (GameInput.IsBackButton(b))
            {
                if (_lobby.TryBack(device))
                    RefreshPanels();
                GetViewport().SetInputAsHandled();
                return;
            }

            if (GameInput.IsActivateButton(b))
            {
                if (TryActivateOrReady(device, null, b))
                    GetViewport().SetInputAsHandled();
            }
        }
    }

    private bool TryActivateOrReady(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (_lobby.FindSlotForDevice(device) is int slot)
        {
            if (_lobby.GetMode(slot) != LobbySlotMode.Claimed)
                return false;

            var ready = key is Key k && GameInput.IsActivateKey(k)
                || button is JoyButton.Start;
            if (!ready)
                return false;

            if (_lobby.TryReady(device))
            {
                RefreshPanels();
                TryStartGame();
                return true;
            }

            return false;
        }

        if (_lobby.TryClaim(device, out _))
        {
            RefreshPanels();
            return true;
        }

        return false;
    }

    private void TryStartGame()
    {
        if (!_boot.TryAcceptInput() || !_lobby.CanStartGame || _playContext is null)
            return;

        _playContext.ApplyFromLobby(_lobby.BuildRoster());
        ChangeSceneOrThrow(WorldScenePath);
    }

    private void ChangeSceneOrThrow(string path)
    {
        var error = GetTree().ChangeSceneToFile(path);
        if (error == Error.Ok)
            return;

        GD.PushError($"ChangeSceneToFile failed for '{path}': {error}");
        throw new InvalidOperationException($"ChangeSceneToFile failed for '{path}' with {error}.");
    }

    private void RefreshPanels()
    {
        if (!_boot.CanRefreshPanels())
            return;

        for (var i = 0; i < LobbyStateMachine.SlotCount; i++)
            _panels[i].ApplyMode(_lobby.GetMode(i), i);
    }

    public LobbySnapshot GetLobbySnapshot() =>
        new()
        {
            IsLobbyScene = true,
            SlotModes = Enumerable.Range(0, LobbyStateMachine.SlotCount)
                .Select(_lobby.GetMode)
                .ToList(),
            ClaimedCount = _lobby.ClaimedCount,
            CanStartGame = _lobby.CanStartGame,
        };
}
