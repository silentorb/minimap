using System.Diagnostics;
using System.Net.Sockets;
using Grpc.Core;
using Minimap.Automation.Contracts;
using Xunit;

namespace Minimap.Functional.Godot.Tests;

[CollectionDefinition("GodotAutomation", DisableParallelization = true)]
public class GodotAutomationCollection : ICollectionFixture<GodotAutomationFixture>;

public sealed class GodotAutomationFixture : IAsyncLifetime
{
    private Process? _godotProcess;
    private Channel? _channel;
    private int _port;

    public AutomationService.AutomationServiceClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var godotBin = Environment.GetEnvironmentVariable("GODOT_BIN");
        if (string.IsNullOrWhiteSpace(godotBin))
            throw new InvalidOperationException("GODOT_BIN must be set to run Minimap.Functional.Godot.Tests.");

        var projectRoot = FindProjectRoot();
        var configuredPort = Environment.GetEnvironmentVariable("MINIMAP_AUTOMATION_PORT");
        _port = int.TryParse(configuredPort, out var parsedPort) ? parsedPort : ReserveFreePort();

        var startInfo = new ProcessStartInfo
        {
            FileName = godotBin,
            Arguments = $"--path \"{projectRoot}\" --headless",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.Environment["MINIMAP_AUTOMATION_ENABLED"] = "1";
        startInfo.Environment["MINIMAP_AUTOMATION_HOST"] = "127.0.0.1";
        startInfo.Environment["MINIMAP_AUTOMATION_PORT"] = _port.ToString();

        _godotProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start GODOT_BIN process.");

        _channel = new Channel($"127.0.0.1:{_port}", ChannelCredentials.Insecure);
        Client = new AutomationService.AutomationServiceClient(_channel);
        await WaitUntilReadyAsync();
    }

    public async Task DisposeAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await Client.ShutdownAsync(new ShutdownRequest());
            }
            catch
            {
                // Best-effort shutdown, process kill fallback below.
            }

            await _channel.ShutdownAsync();
        }

        if (_godotProcess is { HasExited: false })
            _godotProcess.Kill(true);
        _godotProcess?.Dispose();
    }

    private async Task WaitUntilReadyAsync()
    {
        const int attempts = 80;
        for (var i = 0; i < attempts; i++)
        {
            try
            {
                var response = await Client.PingAsync(new PingRequest(), deadline: DateTime.UtcNow.AddSeconds(1));
                if (response.Ok)
                    return;
            }
            catch
            {
                // Retry while server starts.
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Timed out waiting for GodotRpcHost to accept gRPC requests.");
    }

    private static int ReserveFreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string FindProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "project.godot");
            if (File.Exists(candidate))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate project.godot from test output directory.");
    }
}

/// <summary>Loads the real main scene and exercises input + sync paths through gRPC automation.</summary>
[Collection("GodotAutomation")]
public class WorldRootSceneFunctionalTest(GodotAutomationFixture fixture)
{
    [Fact]
    public async Task World_scene_bootstraps_hex_and_player_layers()
    {
        var load = await fixture.Client.LoadMainSceneAsync(new LoadMainSceneRequest());
        Assert.True(load.Ok, load.Error);

        var step = await fixture.Client.SimulateFramesAsync(new SimulateFramesRequest { FrameCount = 15 });
        Assert.True(step.Ok, step.Error);

        var state = await fixture.Client.GetWorldStateAsync(new GetWorldStateRequest());
        Assert.True(state.Ok, state.Error);
        Assert.True(state.SceneLoaded);
        Assert.True(state.IsWorldRoot);
        Assert.True(state.HexLayerChildren > 0);
        Assert.True(state.PlayerLayerChildren > 0);
    }

    [Fact]
    public async Task Arrow_right_moves_player_visual_after_frames()
    {
        var load = await fixture.Client.LoadMainSceneAsync(new LoadMainSceneRequest());
        Assert.True(load.Ok, load.Error);

        var preStep = await fixture.Client.SimulateFramesAsync(new SimulateFramesRequest { FrameCount = 15 });
        Assert.True(preStep.Ok, preStep.Error);

        var before = await fixture.Client.GetWorldStateAsync(new GetWorldStateRequest());
        Assert.True(before.Ok, before.Error);

        var press = await fixture.Client.SetKeyStateAsync(new SetKeyStateRequest
        {
            KeyCode = (int)global::Godot.Key.Right,
            Pressed = true,
        });
        Assert.True(press.Ok, press.Error);

        var postStep = await fixture.Client.SimulateFramesAsync(new SimulateFramesRequest { FrameCount = 5 });
        Assert.True(postStep.Ok, postStep.Error);

        var release = await fixture.Client.SetKeyStateAsync(new SetKeyStateRequest
        {
            KeyCode = (int)global::Godot.Key.Right,
            Pressed = false,
        });
        Assert.True(release.Ok, release.Error);

        var after = await fixture.Client.GetWorldStateAsync(new GetWorldStateRequest());
        Assert.True(after.Ok, after.Error);
        Assert.False(before.Player0X == after.Player0X && before.Player0Y == after.Player0Y);
    }
}
