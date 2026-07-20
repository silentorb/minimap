using System.Threading;
using System.Threading.Tasks;

namespace Minimap.Automation.Contracts;

/// <summary>Named automation routine discovered inside a playbook library.</summary>
public interface IPlaybook
{
    /// <summary>Stable short name within the library (qualified as AssemblyName.Id in the registry).</summary>
    string Id { get; }

    Task<PlaybookResult> RunAsync(IPlaybookContext context, string argsJson, CancellationToken cancellationToken);
}

/// <summary>Thin host facade for playbooks running on the Godot main thread.</summary>
public interface IPlaybookContext
{
    Task LoadSceneAsync(string? scenePath, CancellationToken cancellationToken = default);

    Task WaitFramesAsync(int frameCount, CancellationToken cancellationToken = default);

    Task SetMovementKeyAsync(int keyCode, bool pressed, CancellationToken cancellationToken = default);

    Task<PlaybookWorldSnapshot> GetWorldSnapshotAsync(CancellationToken cancellationToken = default);
}

/// <summary>Outcome of a playbook run (also mirrored on the gRPC wire).</summary>
public sealed class PlaybookResult
{
    public bool Ok { get; init; }
    public string Error { get; init; } = "";
    public string Diagnostics { get; init; } = "";

    public static PlaybookResult Success(string diagnostics = "") =>
        new() { Ok = true, Diagnostics = diagnostics ?? "" };

    public static PlaybookResult Fail(string error, string diagnostics = "") =>
        new() { Ok = false, Error = error ?? "", Diagnostics = diagnostics ?? "" };
}

/// <summary>Minimal world view snapshot for playbook assertions.</summary>
public sealed class PlaybookWorldSnapshot
{
    public bool SceneLoaded { get; init; }
    public bool IsWorldRoot { get; init; }
    public int HexLayerChildren { get; init; }
    public int PlayerLayerChildren { get; init; }
    public float Player0X { get; init; }
    public float Player0Y { get; init; }
}
