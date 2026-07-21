using Godot;
using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Simulation;
using Minimap.Simulation.Navigation;

namespace Minimap.Client.World;

/// <summary>World scene root: binds Simulation session + ClientSession, drives views and local play.</summary>
public partial class WorldApp : Node, IGameAutomationTarget
{
    private const string LobbyScenePath = "res://scenes/lobby.tscn";
    private const string WorldScenePath = "res://scenes/world.tscn";

    [Export] public string CoreSettingsPath { get; set; } = "res://config/core.json";
    [Export] public string ExtensionsSettingsPath { get; set; } = "res://config/extensions.json";
    [Export] public string DefaultScenarioPath { get; set; } = WorldHostHooks.DefaultScenarioResPath;
    [Export] public int WorldSeed { get; set; } = 42;
    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;
    [Export] public int PlayerFactionId { get; set; } = 1;
    [Export] public int RivalFactionId { get; set; } = 2;
    [Export] public int AiPerFaction { get; set; } = 3;
    [Export] public int LocalPlayerCount { get; set; } = 1;

    private readonly WorldSceneBoot _boot = new();
    private GameSession? _session;
    private ClientSession? _clientSession;
    private WorldView? _worldView;
    private PlayerHudPanel? _hudPanel;
    private LocalPlayContextNode? _playContext;
    private LocalInputAggregator? _input;
    private ReconnectOverlay? _reconnectOverlay;
    private GameOverOverlay? _gameOverOverlay;
    private GodotNavigationHost? _navigation;
    private readonly ReconnectState _reconnect = new();
    private bool _gameplayPaused;
    private bool _gameOverShown;

    public WorldView? WorldView => _worldView;
    public GameSession? Session => _session;
    public int HumanPlayerCount => _clientSession?.Players.Count ?? 0;
    public bool GameplayPaused => _gameplayPaused;
    public ReconnectOverlay? ReconnectOverlay => _reconnectOverlay;
    public GameOverOverlay? GameOverOverlay => _gameOverOverlay;
    internal WorldSceneBoot Boot => _boot;

    public override void _Ready()
    {
        try
        {
            _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
            if (_playContext.Roster.IsEmpty)
                _playContext.ApplyDefaultSoloKeyboard();

            var count = _playContext.Roster.IsEmpty
                ? Math.Clamp(LocalPlayerCount, 1, 4)
                : _playContext.Roster.PlayerCount;

            var mapRadius = WorldHostHooks.RequireCoreMapRadius(
                ProjectSettings.GlobalizePath(CoreSettingsPath));
            var content = WorldHostHooks.RequireGameContent(
                ProjectSettings.GlobalizePath(ExtensionsSettingsPath));
            var scenarioPath = ResolveScenarioPath();
            _playContext.ScenarioPath = scenarioPath;
            var scenario = WorldHostHooks.RequireScenario(
                ProjectSettings.GlobalizePath(scenarioPath));
            _boot.MarkSettingsLoaded();

            var spawn = new SpawnConfig
            {
                PlayerFactionId = PlayerFactionId,
                RivalFactionId = RivalFactionId,
                AiPerFaction = AiPerFaction,
                HumanPlayerCount = count,
            };

            _session = GameSession.Create(
                mapRadius.X,
                mapRadius.Y,
                WorldSeed,
                HexSize,
                spawn,
                scenario,
                count,
                content);
            _clientSession = new ClientSession(_session);
            _boot.MarkSessionBound();

            _worldView = GetNode<WorldView>("WorldView");
            _worldView.HexSize = HexSize;
            _worldView.Bind(_session.World, _session.Rng, _session.HumanPawns);
            _worldView.TerrainChanged += OnTerrainChanged;

            _navigation = GodotNavigationHost.Create(
                _worldView,
                _session.World,
                _session.World.PlayerRadius,
                _session.World.MoveSpeed);
            EnsureGodotSteering();

            _hudPanel = GetNode<PlayerHudPanel>("PlayerHudPanel");
            _hudPanel.Apply(_clientSession.BuildHudModels());

            _input = new LocalInputAggregator(_playContext.Roster, _worldView);
            _reconnectOverlay = GetNode<ReconnectOverlay>("ReconnectOverlay");
            _reconnectOverlay.DropPlayerRequested += OnDropDisconnectedPlayer;
            _gameOverOverlay = GetNode<GameOverOverlay>("GameOverOverlay");
            _gameOverOverlay.ContinueRequested += OnGameOverContinue;
            Input.JoyConnectionChanged += OnJoyConnectionChanged;
            _boot.MarkViewsBound();
        }
        catch (Exception ex)
        {
            _boot.Abort(ex.Message);
            GD.PushError($"World boot failed: {ex.Message}");
            CallDeferred(MethodName.QuitAfterBootFailure);
        }
    }

    private void QuitAfterBootFailure() => GetTree().Quit(1);

    private string ResolveScenarioPath()
    {
        var cliPath = WorldHostHooks.TryResolveScenarioPathFromArgs(OS.GetCmdlineArgs());
        if (!string.IsNullOrWhiteSpace(cliPath))
            return cliPath;

        if (!string.IsNullOrWhiteSpace(_playContext?.ScenarioPath))
            return _playContext.ScenarioPath;

        return DefaultScenarioPath;
    }

    private void OnJoyConnectionChanged(long device, bool connected)
    {
        if (!connected)
            CheckJoypadConnections();
    }

    public override void _ExitTree()
    {
        Input.JoyConnectionChanged -= OnJoyConnectionChanged;
        if (_worldView is not null)
            _worldView.TerrainChanged -= OnTerrainChanged;
        if (_gameOverOverlay is not null)
            _gameOverOverlay.ContinueRequested -= OnGameOverContinue;
        _navigation?.Dispose();
        _navigation = null;
    }

    public override void _Process(double delta)
    {
        if (!_boot.TryTick())
            return;

        if (_session is null || _clientSession is null || _worldView is null
            || _hudPanel is null || _input is null)
            return;

        if (_gameplayPaused)
            return;

        for (var i = 0; i < _clientSession.Players.Count; i++)
        {
            _clientSession.SetMoveInput(i, _input.ReadMove(i));
            _clientSession.SetAimInput(i, _input.ReadAim(i));
        }

        EnsureGodotSteering();
        _session.Tick((float)delta);

        if (_session.LastScenarioTickResult.LevelRegenerated)
        {
            _clientSession.OnLevelRegenerated();
            _worldView.OnLevelRegenerated(_session.HumanPawns);
        }

        if (_session.IsGameOver && !_gameOverShown && _gameOverOverlay is not null)
        {
            _gameOverShown = true;
            _gameplayPaused = true;
            _gameOverOverlay.ShowOverlay();
        }

        _worldView.SyncFrame();
        _hudPanel.Apply(_clientSession.BuildHudModels());
    }

    private void OnTerrainChanged()
    {
        if (_session is null || _navigation is null)
            return;
        _navigation.Rebuild(_session.World);
        EnsureGodotSteering();
    }

    private void EnsureGodotSteering()
    {
        if (_session is null || _navigation is null)
            return;

        foreach (var controller in _session.World.Controllers)
        {
            if (controller is not AiController ai)
                continue;
            if (ai.Steering is GodotCrowdSteering)
                continue;
            ai.ReplaceSteering(_navigation.CreateCrowdSteering());
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_boot.TryTick())
            return;

        if (!_reconnect.IsWaiting || _reconnectOverlay is null || _playContext is null)
            return;

        if (@event is InputEventKey key && !key.Echo && key.Pressed)
        {
            if (HandleReconnectInput(GameInput.DeviceFromKeyEvent(key), key.Keycode, null))
                GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
        {
            if (HandleReconnectInput(GameInput.DeviceFromJoyEvent(joy), null, joy.ButtonIndex))
                GetViewport().SetInputAsHandled();
        }
    }

    internal void CheckJoypadConnections()
    {
        if (!_boot.TryTick())
            return;

        if (_playContext is null || _session is null || _reconnectOverlay is null)
            return;

        for (var i = 0; i < _playContext.Roster.PlayerCount; i++)
        {
            var player = _playContext.Roster.Players[i];
            foreach (var device in player.Devices)
            {
                if (!device.IsJoypad)
                    continue;
                if (GameInput.IsJoypadConnected(device.JoypadDevice))
                    continue;

                BeginReconnectWait(i);
                return;
            }
        }
    }

    internal void BeginReconnectWait(int playerIndex)
    {
        if (!_boot.TryTick())
            return;

        if (_playContext is null || _reconnectOverlay is null)
            return;

        var player = _playContext.Roster.Players[playerIndex];
        foreach (var device in player.Devices.ToList())
        {
            if (device.IsJoypad && !GameInput.IsJoypadConnected(device.JoypadDevice))
                player.RemoveDevice(device);
        }

        _reconnect.BeginWait(playerIndex);
        var showDrop = _reconnect.HasOtherConnectedPlayer(
            _playContext.Roster,
            playerIndex,
            GameInput.IsJoypadConnected);
        _reconnectOverlay.ShowForPlayer(playerIndex, showDrop);
        _gameplayPaused = true;
    }

    internal bool HandleReconnectInput(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (!_boot.TryTick())
            return false;

        if (_playContext is null || _reconnectOverlay is null || _clientSession is null)
            return false;
        if (!_reconnect.IsWaiting)
            return false;

        if (_reconnectOverlay.TryHandleActivate(device, key, button))
            return true;

        var isActivate = key is Key k && GameInput.IsActivateKey(k)
            || button is JoyButton b && GameInput.IsActivateButton(b);
        if (!isActivate)
            return false;

        if (_reconnect.CanDropPlayer(_playContext.Roster, device, GameInput.IsJoypadConnected))
        {
            OnDropDisconnectedPlayer();
            return true;
        }

        if (device.IsJoypad
            && _reconnect.CanRebindJoypad(_playContext.Roster, device.JoypadDevice)
            && _reconnect.WaitingPlayerIndex is int waiting)
        {
            _playContext.Roster.Players[waiting].AddDevice(device);
            EndReconnectWait();
            return true;
        }

        return false;
    }

    private void OnDropDisconnectedPlayer()
    {
        if (!_boot.TryTick())
            return;

        if (_clientSession is null || _playContext is null || _reconnectOverlay is null)
            return;
        if (_reconnect.WaitingPlayerIndex is not int waiting)
            return;

        _clientSession.DropHumanPlayer(waiting);
        _playContext.Roster.RemovePlayerAt(waiting);

        if (_clientSession.Players.Count == 0)
        {
            _gameplayPaused = false;
            ChangeSceneOrThrow(LobbyScenePath);
            return;
        }

        _hudPanel?.Apply(_clientSession.BuildHudModels());
        EndReconnectWait();
    }

    private void OnGameOverContinue()
    {
        if (!_boot.TryTick())
            return;

        if (_playContext is null)
            return;

        var nextScene = _playContext.EnteredFromLobby ? LobbyScenePath : WorldScenePath;
        ChangeSceneOrThrow(nextScene);
    }

    internal void EndReconnectWait()
    {
        _reconnect.ClearWait();
        _reconnectOverlay?.HideOverlay();
        _gameplayPaused = false;
    }

    private void ChangeSceneOrThrow(string path)
    {
        var error = GetTree().ChangeSceneToFile(path);
        if (error == Error.Ok)
            return;

        GD.PushError($"ChangeSceneToFile failed for '{path}': {error}");
        throw new InvalidOperationException($"ChangeSceneToFile failed for '{path}' with {error}.");
    }

    public void SimulateJoypadDisconnectForTests(int playerIndex) =>
        BeginReconnectWait(playerIndex);
}
