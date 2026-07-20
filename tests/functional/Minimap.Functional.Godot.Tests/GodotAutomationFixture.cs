using System.Diagnostics;
using System.Net.Sockets;
using Grpc.Core;
using Minimap.Automation.Contracts;
using Xunit;

namespace Minimap.Functional.Godot.Tests;

[CollectionDefinition("GodotAutomation", DisableParallelization = true)]
public class GodotAutomationCollection : ICollectionFixture<GodotAutomationFixture>;

/// <summary>Starts one headless Godot process and loads the default playbook library.</summary>
public sealed class GodotAutomationFixture : IAsyncLifetime
{
    public const string WorldBootstrapId = "Minimap.Functional.Godot.Playbooks.WorldBootstrap";
    public const string ArrowRightMovesPlayerId = "Minimap.Functional.Godot.Playbooks.ArrowRightMovesPlayer";
    public const string LobbyBootstrapId = "Minimap.Functional.Godot.Playbooks.LobbyBootstrap";
    public const string LobbyClaimReadyStartId = "Minimap.Functional.Godot.Playbooks.LobbyClaimReadyStart";
    public const string LobbyBackUnclaimsId = "Minimap.Functional.Godot.Playbooks.LobbyBackUnclaims";
    public const string LobbyJoypadClaimReadyId = "Minimap.Functional.Godot.Playbooks.LobbyJoypadClaimReady";
    public const string ReconnectOverlayDropId = "Minimap.Functional.Godot.Playbooks.ReconnectOverlayDrop";

    private Process? _godotProcess;
    private Channel? _channel;
    private int _port;

    public AutomationService.AutomationServiceClient Client { get; private set; } = null!;

    public string PlaybooksAssemblyPath { get; private set; } = "";

    public async Task InitializeAsync()
    {
        var godotBin = Environment.GetEnvironmentVariable("GODOT_BIN");
        if (string.IsNullOrWhiteSpace(godotBin))
            throw new InvalidOperationException("GODOT_BIN must be set to run Minimap.Functional.Godot.Tests.");

        var projectRoot = FindProjectRoot();
        PlaybooksAssemblyPath = FindPlaybooksAssembly(projectRoot);

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

        var load = await Client.LoadPlaybookLibraryAsync(new LoadPlaybookLibraryRequest
        {
            AssemblyPath = PlaybooksAssemblyPath,
        });
        if (!load.Ok)
            throw new InvalidOperationException($"LoadPlaybookLibrary failed: {load.Error}");
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

    private static string FindPlaybooksAssembly(string projectRoot)
    {
        const string fileName = "Minimap.Functional.Godot.Playbooks.dll";
        var playbooksRoot = Path.Combine(projectRoot, "tests", "functional", "Minimap.Functional.Godot.Playbooks");
        foreach (var configuration in new[] { "Debug", "Release" })
        {
            var candidate = Path.Combine(playbooksRoot, "bin", configuration, "net8.0", fileName);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        var fromBase = Directory.EnumerateFiles(AppContext.BaseDirectory, fileName, SearchOption.AllDirectories)
            .FirstOrDefault();
        if (fromBase is not null)
            return Path.GetFullPath(fromBase);

        throw new FileNotFoundException(
            $"Could not find {fileName}. Build Minimap.Functional.Godot.Playbooks before running Godot tests.");
    }
}
