using System.Collections.Concurrent;
using Godot;
using Grpc.Core;
using Minimap.Automation;
using Minimap.Automation.Contracts;
using Minimap.Client.Lobby;

namespace Minimap.Client;

/// <summary>
/// Runtime autoload that exposes gRPC automation endpoints for external xUnit tests,
/// including loading and running playbook libraries after process start.
/// </summary>
public partial class GodotRpcHost : Node
{
    private const string DefaultScenePath = "res://scenes/world.tscn";
    private readonly ConcurrentQueue<Func<Task>> _mainThreadQueue = new();
    private readonly PlaybookRegistry _playbooks = new();
    private Server? _server;
    private bool _isEnabled;

    public override void _Ready()
    {
        _isEnabled = ReadEnabledFlag();
        if (!_isEnabled)
            return;

        var host = System.Environment.GetEnvironmentVariable("MINIMAP_AUTOMATION_HOST");
        if (string.IsNullOrWhiteSpace(host))
            host = "127.0.0.1";

        var port = 50061;
        var rawPort = System.Environment.GetEnvironmentVariable("MINIMAP_AUTOMATION_PORT");
        if (!string.IsNullOrWhiteSpace(rawPort) && int.TryParse(rawPort, out var parsed))
            port = parsed;

        _server = new Server
        {
            Services = { AutomationService.BindService(new AutomationServiceImpl(this)) },
            Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
        };
        _server.Start();
        GD.Print($"GodotRpcHost listening on {host}:{port}");
    }

    public override async void _ExitTree()
    {
        if (_server is null)
            return;
        await _server.ShutdownAsync();
        _server = null;
    }

    public override void _Process(double delta)
    {
        if (!_isEnabled)
            return;

        while (_mainThreadQueue.TryDequeue(out var action))
            _ = action();
    }

    private static bool ReadEnabledFlag()
    {
        var value = System.Environment.GetEnvironmentVariable("MINIMAP_AUTOMATION_ENABLED");
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private Task<T> RunOnMainThread<T>(Func<Task<T>> action)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _mainThreadQueue.Enqueue(async () =>
        {
            try
            {
                var value = await action();
                tcs.TrySetResult(value);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });
        return tcs.Task;
    }

    private Task RunOnMainThread(Func<Task> action)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _mainThreadQueue.Enqueue(async () =>
        {
            try
            {
                await action();
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });
        return tcs.Task;
    }

    private async Task WaitFramesAsync(int frameCount)
    {
        await SceneFrames.WaitAsync(GetTree(), frameCount);
    }

    private WorldView? GetWorldView() =>
        SceneNodes.FindInCurrentScene<WorldView>(GetTree(), "WorldView");

    private IGameAutomationTarget? GetGameTarget() =>
        GetTree().CurrentScene as IGameAutomationTarget;

    private ILobbySnapshotSource? GetLobbySource() =>
        GetTree().CurrentScene as ILobbySnapshotSource;

    private sealed class PlaybookContext(GodotRpcHost owner) : IPlaybookContext
    {
        public Task LoadSceneAsync(string? scenePath, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = string.IsNullOrWhiteSpace(scenePath) ? DefaultScenePath : scenePath;
                var error = owner.GetTree().ChangeSceneToFile(path);
                if (error != Error.Ok)
                    throw new InvalidOperationException($"ChangeSceneToFile failed with {error}.");
                await owner.WaitFramesAsync(2);
            });

        public Task WaitFramesAsync(int frameCount, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await owner.WaitFramesAsync(frameCount);
            });

        public Task SetMovementKeyAsync(int keyCode, bool pressed, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var worldView = owner.GetWorldView()
                    ?? throw new InvalidOperationException("Current scene has no WorldView.");
                MovementKeys.Set(worldView, (Key)keyCode, pressed);
                return Task.CompletedTask;
            });

        public Task<PlaybookWorldSnapshot> GetWorldSnapshotAsync(CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await owner.WaitFramesAsync(1);
                var worldView = owner.GetWorldView();
                var game = owner.GetGameTarget();
                var pos = worldView?.TryGetPlayerPosition(0);
                return new PlaybookWorldSnapshot
                {
                    SceneLoaded = owner.GetTree().CurrentScene is not null,
                    IsWorldRoot = worldView is not null,
                    HexLayerChildren = worldView?.HexLayerChildCount ?? 0,
                    PlayerLayerChildren = worldView?.PlayerLayerChildCount ?? 0,
                    Player0X = pos?.X ?? 0f,
                    Player0Y = pos?.Y ?? 0f,
                    HumanPlayerCount = game?.HumanPlayerCount ?? 0,
                };
            });

        public Task<PlaybookLobbySnapshot> GetLobbySnapshotAsync(CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await owner.WaitFramesAsync(1);
                var lobby = owner.GetLobbySource();
                if (lobby is null)
                {
                    return new PlaybookLobbySnapshot
                    {
                        IsLobbyScene = false,
                    };
                }

                var snap = lobby.GetLobbySnapshot();
                return new PlaybookLobbySnapshot
                {
                    IsLobbyScene = snap.IsLobbyScene,
                    SlotModes = snap.SlotModes.Select(m => m.ToString()).ToList(),
                    ClaimedCount = snap.ClaimedCount,
                    CanStartGame = snap.CanStartGame,
                };
            });

        public Task SetActivateKeyAsync(bool pressed, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                LobbyInput.SetActivateKey(owner, pressed);
                return Task.CompletedTask;
            });

        public Task SetBackKeyAsync(bool pressed, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                LobbyInput.SetBackKey(owner, pressed);
                return Task.CompletedTask;
            });

        public Task SetJoypadButtonAsync(
            int deviceIndex,
            int joyButton,
            bool pressed,
            CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                LobbyInput.SetJoypadButton(owner, deviceIndex, (JoyButton)joyButton, pressed);
                return Task.CompletedTask;
            });

        public Task<PlaybookPauseOverlaySnapshot> GetPauseOverlaySnapshotAsync(
            CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await owner.WaitFramesAsync(1);
                var game = owner.GetGameTarget();
                var overlay = game?.ReconnectOverlay;
                return new PlaybookPauseOverlaySnapshot
                {
                    Visible = overlay?.OverlayVisible ?? false,
                    DropButtonVisible = overlay?.DropButtonVisible ?? false,
                    TreePaused = game?.GameplayPaused ?? false,
                };
            });

        public Task SimulateJoypadDisconnectAsync(int playerIndex, CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                owner.GetGameTarget()?.SimulateJoypadDisconnectForTests(playerIndex);
                return Task.CompletedTask;
            });

        public Task FocusReconnectDropAsync(CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                owner.GetGameTarget()?.ReconnectOverlay?.FocusDropForAutomation();
                return Task.CompletedTask;
            });

        public Task ClearLocalPlayContextAsync(CancellationToken cancellationToken = default) =>
            owner.RunOnMainThread(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                owner.GetNode<LocalPlayContextNode>("/root/LocalPlayContext").Clear();
                return Task.CompletedTask;
            });
    }

    private sealed class AutomationServiceImpl(GodotRpcHost owner) : AutomationService.AutomationServiceBase
    {
        private readonly PlaybookContext _playbookContext = new(owner);

        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse { Ok = true, Message = "pong" });
        }

        public override async Task<CommandResponse> LoadMainScene(LoadMainSceneRequest request, ServerCallContext context)
        {
            try
            {
                await _playbookContext.LoadSceneAsync(request.ScenePath);
                return new CommandResponse { Ok = true };
            }
            catch (Exception ex)
            {
                return new CommandResponse { Ok = false, Error = ex.Message };
            }
        }

        public override async Task<CommandResponse> SimulateFrames(SimulateFramesRequest request, ServerCallContext context)
        {
            try
            {
                await _playbookContext.WaitFramesAsync(request.FrameCount);
                return new CommandResponse { Ok = true };
            }
            catch (Exception ex)
            {
                return new CommandResponse { Ok = false, Error = ex.Message };
            }
        }

        public override async Task<CommandResponse> SetKeyState(SetKeyStateRequest request, ServerCallContext context)
        {
            try
            {
                await _playbookContext.SetMovementKeyAsync(request.KeyCode, request.Pressed);
                return new CommandResponse { Ok = true };
            }
            catch (Exception ex)
            {
                return new CommandResponse { Ok = false, Error = ex.Message };
            }
        }

        public override async Task<WorldStateResponse> GetWorldState(GetWorldStateRequest request, ServerCallContext context)
        {
            try
            {
                var snap = await _playbookContext.GetWorldSnapshotAsync();
                return new WorldStateResponse
                {
                    Ok = true,
                    SceneLoaded = snap.SceneLoaded,
                    IsWorldRoot = snap.IsWorldRoot,
                    HexLayerChildren = snap.HexLayerChildren,
                    PlayerLayerChildren = snap.PlayerLayerChildren,
                    Player0X = snap.Player0X,
                    Player0Y = snap.Player0Y,
                };
            }
            catch (Exception ex)
            {
                return new WorldStateResponse
                {
                    Ok = false,
                    Error = ex.Message,
                };
            }
        }

        public override Task<LoadPlaybookLibraryResponse> LoadPlaybookLibrary(
            LoadPlaybookLibraryRequest request,
            ServerCallContext context)
        {
            var outcome = owner._playbooks.LoadLibrary(request.AssemblyPath);
            var response = new LoadPlaybookLibraryResponse
            {
                Ok = outcome.Success,
                Error = outcome.Error,
            };
            response.PlaybookIds.AddRange(outcome.PlaybookIds);
            return Task.FromResult(response);
        }

        public override Task<ListPlaybooksResponse> ListPlaybooks(
            ListPlaybooksRequest request,
            ServerCallContext context)
        {
            var response = new ListPlaybooksResponse { Ok = true };
            response.PlaybookIds.AddRange(owner._playbooks.ListIds());
            return Task.FromResult(response);
        }

        public override async Task<PlaybookResultResponse> RunPlaybook(
            RunPlaybookRequest request,
            ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.PlaybookId))
            {
                return new PlaybookResultResponse
                {
                    Ok = false,
                    Error = "playbook_id is required.",
                };
            }

            if (!owner._playbooks.TryGet(request.PlaybookId, out var playbook) || playbook is null)
            {
                return new PlaybookResultResponse
                {
                    Ok = false,
                    Error = $"Unknown playbook id '{request.PlaybookId}'.",
                };
            }

            try
            {
                var result = await playbook.RunAsync(
                    _playbookContext,
                    request.ArgsJson ?? "",
                    context.CancellationToken);
                return new PlaybookResultResponse
                {
                    Ok = result.Ok,
                    Error = result.Error,
                    Diagnostics = result.Diagnostics,
                };
            }
            catch (Exception ex)
            {
                return new PlaybookResultResponse
                {
                    Ok = false,
                    Error = ex.Message,
                };
            }
        }

        public override async Task<CommandResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            try
            {
                await owner.RunOnMainThread(() =>
                {
                    owner.GetTree().Quit();
                    return Task.CompletedTask;
                });
                return new CommandResponse { Ok = true };
            }
            catch (Exception ex)
            {
                return new CommandResponse { Ok = false, Error = ex.Message };
            }
        }
    }
}
