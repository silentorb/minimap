using Godot;
using Minimap.Client;
using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;

namespace Minimap.App;

/// <summary>Local multiplayer lobby: claim slots, ready up, start world.</summary>
public partial class LobbyApp : Control, ILobbySnapshotSource
{
    private const string WorldScenePath = "res://scenes/world.tscn";

    private readonly LobbyStateMachine _lobby = new();
    private LobbyPanel[] _panels = Array.Empty<LobbyPanel>();
    private LocalPlayContextNode? _playContext;

    public LobbyStateMachine LobbyState => _lobby;

    public override void _Ready()
    {
        _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
        _playContext.Clear();

        var row = GetNode<HBoxContainer>("Margin/PanelRow");
        _panels = new LobbyPanel[LobbyStateMachine.SlotCount];
        for (var i = 0; i < LobbyStateMachine.SlotCount; i++)
            _panels[i] = row.GetChild<LobbyPanel>(i);

        RefreshPanels();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
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
        if (!_lobby.CanStartGame || _playContext is null)
            return;

        _playContext.ApplyFromLobby(_lobby.BuildRoster());
        GetTree().ChangeSceneToFile(WorldScenePath);
    }

    private void RefreshPanels()
    {
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
