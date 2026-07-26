using Godot;
using Minimap.Client.LocalPlay;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>Local multiplayer lobby: claim slots, select profile, accessories, ready, start world.</summary>
public partial class LobbyApp : Control, ILobbySnapshotSource
{
    private const string WorldScenePath = "res://scenes/world.tscn";
    private const string MainMenuScenePath = "res://scenes/main_menu.tscn";

    private readonly LobbyStateMachine _lobby = new();
    private readonly LobbySceneBoot _boot = new();
    private LobbyPanel[] _panels = Array.Empty<LobbyPanel>();
    private LocalPlayContextNode? _playContext;
    private string _profilesAbsolutePath = string.Empty;

    [Export] public string ExtensionsSettingsPath { get; set; } = "res://config/extensions.json";
    [Export] public string CoreSettingsPath { get; set; } = "res://config/core.json";

    public LobbyStateMachine LobbyState => _lobby;
    internal LobbySceneBoot Boot => _boot;

    public override void _Ready()
    {
        try
        {
            _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
            var returning = _playContext.ReturningFromSession && !_playContext.Roster.IsEmpty;

            var row = GetNode<HBoxContainer>("Margin/PanelRow");
            _panels = new LobbyPanel[LobbyStateMachine.SlotCount];
            for (var i = 0; i < LobbyStateMachine.SlotCount; i++)
                _panels[i] = row.GetChild<LobbyPanel>(i);

            _boot.MarkPanelsBound();
            RefreshPanels();

            var extensions = WorldHostHooks.RequireExtensions(
                ProjectSettings.GlobalizePath(ExtensionsSettingsPath));
            var accessoryPoints = WorldHostHooks.RequireCoreAccessoryPoints(
                ProjectSettings.GlobalizePath(CoreSettingsPath));
            _lobby.ConfigureAccessories(
                accessoryPoints,
                extensions.PlayerSelectableAccessories,
                extensions.Domains);

            _profilesAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerProfilesResPath);
            _lobby.ConfigureProfiles(WorldHostHooks.RequirePlayerProfiles(_profilesAbsolutePath));

            if (returning)
            {
                _lobby.ApplyFromRoster(
                    _playContext.Roster,
                    device => !device.IsJoypad || GameInput.IsJoypadConnected(device.JoypadDevice));
                _playContext.ClearReturningFromSession();
            }
            else
            {
                _playContext.Clear();
            }

            _boot.MarkExtensionsLoaded();
            RefreshPanels();
        }
        catch (Exception ex)
        {
            _boot.Abort(ex.Message);
            GD.PushError($"Lobby boot failed: {ex.Message}");
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
                HandleDeviceInput(GameInput.DeviceFromKeyEvent(key), key.Keycode, null, null);
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
            HandleDeviceInput(GameInput.DeviceFromJoyEvent(joy), null, joy.ButtonIndex, null);

        if (@event is InputEventJoypadMotion motion)
            HandleDeviceInput(InputDeviceId.Joypad(motion.Device), null, null, motion);
    }

    internal void HandleDeviceInput(
        InputDeviceId device,
        Key? key,
        JoyButton? button,
        InputEventJoypadMotion? motion)
    {
        if (!_boot.TryAcceptInput())
            return;

        if (TryHandleProfileNavigation(device, key, button, motion))
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (TryHandleAccessoryNavigation(device, key, button, motion))
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (key is Key k)
        {
            if (GameInput.IsBackKey(k))
            {
                if (_lobby.TryBack(device))
                    RefreshPanels();
                else if (TryLeaveToMainMenu(device))
                {
                    // scene change
                }

                GetViewport().SetInputAsHandled();
                return;
            }

            if (GameInput.IsActivateKey(k))
            {
                if (TryActivateOrAdvance(device, k, null))
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
                else if (TryLeaveToMainMenu(device))
                {
                    // scene change
                }

                GetViewport().SetInputAsHandled();
                return;
            }

            if (GameInput.IsActivateButton(b))
            {
                if (TryActivateOrAdvance(device, null, b))
                    GetViewport().SetInputAsHandled();
            }
        }
    }

    /// <summary>
    /// Unbound Back/Escape leaves to the main menu (primary backs out after unclaim, or fresh lobby).
    /// </summary>
    private bool TryLeaveToMainMenu(InputDeviceId device)
    {
        if (_lobby.IsDeviceBound(device) || _playContext is null)
            return false;

        _playContext.Clear();
        ChangeSceneOrThrow(MainMenuScenePath);
        return true;
    }

    private bool TryHandleProfileNavigation(
        InputDeviceId device,
        Key? key,
        JoyButton? button,
        InputEventJoypadMotion? motion)
    {
        if (_lobby.FindSlotForDevice(device) is not int slot)
            return false;
        if (_lobby.GetMode(slot) != LobbySlotMode.SelectingProfile)
            return false;

        var delta = 0;
        if (key is Key.Left)
            delta = -1;
        else if (key is Key.Right)
            delta = 1;
        else if (button is JoyButton.DpadLeft)
            delta = -1;
        else if (button is JoyButton.DpadRight)
            delta = 1;
        else if (motion is { Axis: JoyAxis.LeftX } stick && Math.Abs(stick.AxisValue) > 0.5f)
            delta = stick.AxisValue > 0 ? 1 : -1;

        if (delta == 0)
            return false;

        if (_lobby.TryCycleProfile(device, delta))
        {
            RefreshPanels();
            return true;
        }

        return false;
    }

    private bool TryHandleAccessoryNavigation(
        InputDeviceId device,
        Key? key,
        JoyButton? button,
        InputEventJoypadMotion? motion)
    {
        if (_lobby.FindSlotForDevice(device) is not int slot)
            return false;
        if (_lobby.GetMode(slot) != LobbySlotMode.SelectingAccessories)
            return false;

        var panel = _panels[slot].AccessoryPanel;
        if (panel is null || !panel.Visible)
            return false;

        // A alone selects accessory while SelectingAccessories; Start still advances via TryActivateOrAdvance.
        if (button is JoyButton.A)
            return panel.HandleActivate();

        if (key is Key.Space)
            return panel.HandleActivate();

        var delta = Vector2I.Zero;
        if (key is Key arrow)
        {
            delta = arrow switch
            {
                Key.Left => new Vector2I(-1, 0),
                Key.Right => new Vector2I(1, 0),
                Key.Up => new Vector2I(0, -1),
                Key.Down => new Vector2I(0, 1),
                _ => Vector2I.Zero,
            };
        }
        else if (button is JoyButton dpad)
        {
            delta = dpad switch
            {
                JoyButton.DpadLeft => new Vector2I(-1, 0),
                JoyButton.DpadRight => new Vector2I(1, 0),
                JoyButton.DpadUp => new Vector2I(0, -1),
                JoyButton.DpadDown => new Vector2I(0, 1),
                _ => Vector2I.Zero,
            };
        }
        else if (motion is { Axis: JoyAxis.LeftX or JoyAxis.LeftY } stick
                 && Math.Abs(stick.AxisValue) > 0.5f)
        {
            if (stick.Axis == JoyAxis.LeftX)
                delta = new Vector2I(stick.AxisValue > 0 ? 1 : -1, 0);
            else
                delta = new Vector2I(0, stick.AxisValue > 0 ? 1 : -1);
        }

        if (delta != Vector2I.Zero)
            return panel.HandleNavigate(delta);

        return false;
    }

    private bool TryActivateOrAdvance(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (_lobby.FindSlotForDevice(device) is int slot)
        {
            var mode = _lobby.GetMode(slot);
            var confirm = (key is Key k && (k is Key.Enter or Key.KpEnter))
                || button is JoyButton.Start;
            if (!confirm)
                return false;

            if (mode == LobbySlotMode.SelectingProfile)
            {
                if (_lobby.TryConfirmProfile(device))
                {
                    RefreshPanels();
                    return true;
                }

                return false;
            }

            if (mode == LobbySlotMode.SelectingAccessories)
            {
                if (_lobby.TryReady(device))
                {
                    RefreshPanels();
                    TryStartGame();
                    return true;
                }

                return false;
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
        {
            var mode = _lobby.GetMode(i);
            var profileName = ResolveProfileDisplayName(i);
            _panels[i].ApplyMode(mode, i, profileName);

            if (mode == LobbySlotMode.SelectingProfile
                && _lobby.GetProfileSelection(i) is { } profileState)
            {
                _panels[i].ShowProfileSelection(_lobby.GetAvailableProfilesForSlot(i), profileState);
            }
            else if (mode == LobbySlotMode.SelectingAccessories
                     && _lobby.GetAccessorySelection(i) is { } selection)
            {
                _panels[i].ShowAccessorySelection(
                    _lobby.SelectableAccessories,
                    selection,
                    interactive: true,
                    _lobby.Domains);
            }
            else
            {
                _panels[i].HideProfileSelection();
                _panels[i].HideAccessorySelection();
            }
        }
    }

    private string? ResolveProfileDisplayName(int slotIndex)
    {
        var state = _lobby.GetProfileSelection(slotIndex);
        if (state?.ConfirmedProfileId is not Guid id)
            return null;
        return _lobby.Profiles.Find(id)?.Name;
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
