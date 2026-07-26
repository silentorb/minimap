using Godot;
using Minimap.Client.Achievements;
using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Client.MainMenu;
using Minimap.Client.PostSession;
using Minimap.Client.Profiles;
using Minimap.Simulation;
using Minimap.Simulation.Navigation;
using Minimap.Simulation.Types;

namespace Minimap.Client.World;

/// <summary>World scene root: binds Simulation session + ClientSession, drives views and local play.</summary>
public partial class WorldApp : Node, IGameAutomationTarget
{
    private const string LobbyScenePath = "res://scenes/lobby.tscn";

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
    private readonly SessionAchievementLedger _sessionAchievements = new();
    private GameSession? _session;
    private ClientSession? _clientSession;
    private WorldView? _worldView;
    private PlayerHudPanel? _hudPanel;
    private LocalPlayContextNode? _playContext;
    private LocalInputAggregator? _input;
    private ReconnectOverlay? _reconnectOverlay;
    private PostSessionOverlay? _postSessionOverlay;
    private MainMenuPopup? _mainMenuPopup;
    private GodotNavigationHost? _navigation;
    private readonly ReconnectState _reconnect = new();
    private bool _gameplayPaused;
    private bool _postSessionShown;
    private PlayerProfileCatalog? _profileCatalog;
    private string _profilesAbsolutePath = string.Empty;

    public WorldView? WorldView => _worldView;
    public GameSession? Session => _session;
    public int HumanPlayerCount => _clientSession?.Players.Count ?? 0;
    public bool GameplayPaused => _gameplayPaused;
    public ReconnectOverlay? ReconnectOverlay => _reconnectOverlay;
    public PostSessionOverlay? PostSessionOverlay => _postSessionOverlay;
    public MainMenuPopup? MainMenuPopup => _mainMenuPopup;
    public bool MainMenuPopupVisible => _mainMenuPopup?.OverlayVisible ?? false;
    public SessionAchievementLedger SessionAchievements => _sessionAchievements;
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

            var corePath = ProjectSettings.GlobalizePath(CoreSettingsPath);
            var mapRadius = WorldHostHooks.RequireCoreMapRadius(corePath);
            var accessoryPoints = WorldHostHooks.RequireCoreAccessoryPoints(corePath);
            var extensions = WorldHostHooks.RequireExtensions(
                ProjectSettings.GlobalizePath(ExtensionsSettingsPath));
            var content = extensions.Content;
            var scenarioPath = ResolveScenarioPath();
            _playContext.ScenarioPath = scenarioPath;
            var scenario = WorldHostHooks.RequireScenario(
                ProjectSettings.GlobalizePath(scenarioPath));
            var worldSeed = ResolveWorldSeed();
            _boot.MarkSettingsLoaded();

            var spawn = new SpawnConfig
            {
                PlayerFactionId = PlayerFactionId,
                RivalFactionId = RivalFactionId,
                AiPerFaction = AiPerFaction,
                HumanPlayerCount = count,
            };

            var selectedByPlayer = _playContext.Roster.Players
                .Select(p => (IReadOnlyList<AccessoryDefinition>)p.SelectedAccessories.ToList())
                .ToList();

            _session = GameSession.Create(
                mapRadius.X,
                mapRadius.Y,
                worldSeed,
                HexSize,
                spawn,
                scenario,
                count,
                content,
                accessoryPoints,
                selectedByPlayer);

            var displayNames = _playContext.Roster.Players
                .Select(p => p.DisplayName)
                .ToList();
            var profileIds = _playContext.Roster.Players
                .Select(p => p.ProfileId)
                .ToList();
            _clientSession = new ClientSession(_session, extensions.Domains, displayNames, profileIds);
            _sessionAchievements.EnsurePlayerCount(count);

            _profilesAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerProfilesResPath);
            if (profileIds.Any(id => id is not null))
                _profileCatalog = WorldHostHooks.RequirePlayerProfiles(_profilesAbsolutePath);

            _boot.MarkSessionBound();

            _worldView = GetNode<WorldView>("WorldView");
            _worldView.HexSize = HexSize;
            _worldView.Bind(_session.World, _session.HumanPawns);
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
            _postSessionOverlay = GetNode<PostSessionOverlay>("PostSessionOverlay");
            _postSessionOverlay.AllReadyRequested += OnPostSessionAllReady;
            _mainMenuPopup = GetNode<MainMenuPopup>("MainMenuPopup");
            _mainMenuPopup.ContinueRequested += OnMainMenuContinue;
            _mainMenuPopup.EndGameRequested += OnMainMenuEndGame;
            _mainMenuPopup.QuitRequested += OnMainMenuQuit;
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

        var envPath = WorldHostHooks.TryResolveScenarioPathFromEnvironment();
        if (!string.IsNullOrWhiteSpace(envPath))
            return envPath;

        if (!string.IsNullOrWhiteSpace(_playContext?.ScenarioPath))
            return _playContext.ScenarioPath;

        return DefaultScenarioPath;
    }

    private int ResolveWorldSeed() =>
        WorldHostHooks.TryResolveWorldSeedFromEnvironment() ?? WorldSeed;

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
        if (_postSessionOverlay is not null)
            _postSessionOverlay.AllReadyRequested -= OnPostSessionAllReady;

        if (_mainMenuPopup is not null)
        {
            _mainMenuPopup.ContinueRequested -= OnMainMenuContinue;
            _mainMenuPopup.EndGameRequested -= OnMainMenuEndGame;
            _mainMenuPopup.QuitRequested -= OnMainMenuQuit;
        }

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
            var pawn = _clientSession.Players[i].Pawn;
            var origin = pawn?.Position ?? SimVec2.Zero;
            var input = _input.ReadPlayer(i, origin);
            _clientSession.SetMoveInput(i, input.Move);
            _clientSession.SetAimInput(i, input.Aim);
            _clientSession.SetFireHeld(i, input.FireHeld);
            _clientSession.SetSecondaryFireHeld(i, input.SecondaryFireHeld);
            _clientSession.SetAbilityActivatePressed(i, input.AbilityActivatePressed);
            _clientSession.SetAbilityBackPressed(i, input.AbilityBackPressed);
            _clientSession.SetInteractPressed(i, input.InteractPressed);
            _clientSession.SetModalCycle(i, input.ModalCycle);
        }

        EnsureGodotSteering();
        _session.Tick((float)delta);
        RecordProfileDeaths(_session.HumanDeathsThisTick);
        RecordSurviveAchievements(_session.SurviveFiveMinutesThisTick);

        if (_session.LastScenarioTickResult.LevelRegenerated)
        {
            _clientSession.OnLevelRegenerated();
            _worldView.OnLevelRegenerated(_session.HumanPawns);
        }

        if (_session.IsGameOver && !_postSessionShown)
            ShowPostSession("Game Over");

        _worldView.SyncFrame(_clientSession.Players);
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

        if (@event is InputEventKey key && !key.Echo && key.Pressed)
        {
            var device = GameInput.DeviceFromKeyEvent(key);
            if (HandleModalInput(device, key.Keycode, null))
                GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
        {
            var device = GameInput.DeviceFromJoyEvent(joy);
            if (HandleModalInput(device, null, joy.ButtonIndex))
                GetViewport().SetInputAsHandled();
        }
    }

    private bool HandleModalInput(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (_reconnect.IsWaiting && _reconnectOverlay is not null && _playContext is not null)
            return HandleReconnectInput(device, key, button);

        if (_postSessionShown && _postSessionOverlay is not null)
        {
            var activate = key is Key k && GameInput.IsActivateKey(k)
                || button is JoyButton b && GameInput.IsActivateButton(b);
            if (activate)
                return _postSessionOverlay.TryHandleReadyInput(device);
            return false;
        }

        if (_mainMenuPopup is not null && _mainMenuPopup.OverlayVisible)
            return _mainMenuPopup.TryHandleOwnerDismiss(device, key, button);

        return TryOpenMainMenu(device, key, button);
    }

    private bool TryOpenMainMenu(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (_mainMenuPopup is null || _playContext is null || _clientSession is null)
            return false;

        var openFromKey = key is Key k && GameInput.IsMenuOpenKey(k);
        var openFromButton = button is JoyButton b && GameInput.IsMenuOpenButton(b);
        if (!openFromKey && !openFromButton)
            return false;

        if (openFromKey && _clientSession.IsAnyPlacementPreviewing())
            return false;

        if (_playContext.Roster.FindPlayerIndexForDevice(device) is null)
            return false;

        _mainMenuPopup.ShowForOwner(device);
        _gameplayPaused = true;
        return true;
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
            ReturnToLobbyRestoringSession();
            return;
        }

        _sessionAchievements.EnsurePlayerCount(_clientSession.Players.Count);
        _hudPanel?.Apply(_clientSession.BuildHudModels());
        EndReconnectWait();
    }

    private void OnMainMenuContinue()
    {
        if (!_boot.TryTick())
            return;

        _mainMenuPopup?.HideOverlay();
        if (!_reconnect.IsWaiting && !_postSessionShown)
            _gameplayPaused = false;
    }

    private void OnMainMenuEndGame()
    {
        if (!_boot.TryTick())
            return;

        _mainMenuPopup?.HideOverlay();
        ShowPostSession("Session complete");
    }

    private void OnMainMenuQuit()
    {
        if (!_boot.TryTick())
            return;

        GetTree().Quit();
    }

    private void OnPostSessionAllReady()
    {
        if (!_boot.TryTick())
            return;

        ReturnToLobbyRestoringSession();
    }

    private void ShowPostSession(string title)
    {
        if (_postSessionShown || _postSessionOverlay is null || _playContext is null)
            return;

        _postSessionShown = true;
        _gameplayPaused = true;
        _mainMenuPopup?.HideOverlay();
        _postSessionOverlay.ShowSummary(_playContext.Roster, _sessionAchievements, title);
    }

    private void ReturnToLobbyRestoringSession()
    {
        _playContext?.MarkReturningFromSession();
        ChangeSceneOrThrow(LobbyScenePath);
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

    public void ForceGameOverForTests()
    {
        if (!_boot.TryTick() || _session is null || _postSessionOverlay is null)
            return;

        foreach (var player in _session.Players)
        {
            if (player.Character is { IsAlive: true } character)
                _session.World.ApplyDamage(character, character.Health);
        }

        _session.Tick(1f / 60f);
        RecordProfileDeaths(_session.HumanDeathsThisTick);
        RecordSurviveAchievements(_session.SurviveFiveMinutesThisTick);
        if (!_session.IsGameOver)
            return;

        ShowPostSession("Game Over");
    }

    public void ForcePostSessionAllReadyForTests()
    {
        if (!_boot.TryTick() || !_postSessionShown || _postSessionOverlay?.ReadyModel is null)
            return;

        var model = _postSessionOverlay.ReadyModel;
        for (var i = 0; i < model.PlayerCount; i++)
            model.TrySetReady(i, true);

        if (model.AllReady)
            OnPostSessionAllReady();
    }

    private void RecordProfileDeaths(IReadOnlyList<int> playerIndices)
    {
        if (_clientSession is null || _profileCatalog is null || playerIndices.Count == 0)
            return;

        var changed = false;
        foreach (var index in playerIndices)
        {
            if (_clientSession.GetProfileId(index) is not Guid profileId)
                continue;
            if (_profileCatalog.TryIncrementDeaths(profileId))
                changed = true;
        }

        if (changed)
            WorldHostHooks.RequireSavePlayerProfiles(_profilesAbsolutePath, _profileCatalog);
    }

    private void RecordSurviveAchievements(IReadOnlyList<int> playerIndices)
    {
        if (_clientSession is null || playerIndices.Count == 0)
            return;

        var changed = false;
        foreach (var index in playerIndices)
        {
            if (_clientSession.GetProfileId(index) is not Guid profileId)
            {
                // Direct world / no profile: still track session earn for summary display.
                _sessionAchievements.TryRecord(index, AchievementIds.Survive5Minutes, firstTime: true);
                continue;
            }

            _profileCatalog ??= WorldHostHooks.RequirePlayerProfiles(_profilesAbsolutePath);
            var profile = _profileCatalog.Find(profileId);
            var firstTime = profile is null || !profile.HasAchievement(AchievementIds.Survive5Minutes);
            if (!_sessionAchievements.TryRecord(index, AchievementIds.Survive5Minutes, firstTime))
                continue;
            if (firstTime
                && _profileCatalog is not null
                && _profileCatalog.TryUnlockAchievement(profileId, AchievementIds.Survive5Minutes))
            {
                changed = true;
            }
        }

        if (changed && _profileCatalog is not null)
            WorldHostHooks.RequireSavePlayerProfiles(_profilesAbsolutePath, _profileCatalog);
    }
}
