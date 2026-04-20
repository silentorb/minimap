using System.Collections.Concurrent;
using Godot;
using Grpc.Core;
using Minimap.Automation.Contracts;

namespace Minimap.Client;

/// <summary>
/// Runtime autoload that exposes gRPC automation endpoints for external xUnit tests.
/// </summary>
public partial class GodotRpcHost : Node
{
    private const string DefaultScenePath = "res://scenes/world.tscn";
    private readonly ConcurrentQueue<Func<Task>> _mainThreadQueue = new();
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
        var frames = Math.Max(1, frameCount);
        for (var i = 0; i < frames; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private WorldRoot? GetWorldRoot() => GetTree().CurrentScene as WorldRoot;

    private sealed class AutomationServiceImpl(GodotRpcHost owner) : AutomationService.AutomationServiceBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse { Ok = true, Message = "pong" });
        }

        public override async Task<CommandResponse> LoadMainScene(LoadMainSceneRequest request, ServerCallContext context)
        {
            try
            {
                await owner.RunOnMainThread(async () =>
                {
                    var scenePath = string.IsNullOrWhiteSpace(request.ScenePath) ? DefaultScenePath : request.ScenePath;
                    var error = owner.GetTree().ChangeSceneToFile(scenePath);
                    if (error != Error.Ok)
                        throw new InvalidOperationException($"ChangeSceneToFile failed with {error}.");
                    await owner.WaitFramesAsync(2);
                });
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
                await owner.RunOnMainThread(() => owner.WaitFramesAsync(request.FrameCount));
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
                await owner.RunOnMainThread(() =>
                {
                    var worldRoot = owner.GetWorldRoot();
                    if (worldRoot is null)
                        throw new InvalidOperationException("Current scene is not WorldRoot.");

                    if (request.Pressed)
                        worldRoot.TryMovePlayerFromKey((Key)request.KeyCode, request.ShiftPressed);
                    return Task.CompletedTask;
                });

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
                return await owner.RunOnMainThread(async () =>
                {
                    await owner.WaitFramesAsync(1);
                    var worldRoot = owner.GetWorldRoot();
                    var pos = worldRoot?.TryGetPlayerPosition(0);
                    return new WorldStateResponse
                    {
                        Ok = true,
                        SceneLoaded = owner.GetTree().CurrentScene is not null,
                        IsWorldRoot = worldRoot is not null,
                        HexLayerChildren = worldRoot?.HexLayerChildCount ?? 0,
                        PlayerLayerChildren = worldRoot?.PlayerLayerChildCount ?? 0,
                        Player0X = pos?.X ?? 0f,
                        Player0Y = pos?.Y ?? 0f,
                    };
                });
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
