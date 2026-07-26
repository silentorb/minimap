using Minimap.Client.Profiles;
using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Client;

/// <summary>
/// Client world roots call these for settings/extension file I/O without referencing Minimap.App.
/// App registers the real loaders (typically via module initializer).
/// </summary>
public static class WorldHostHooks
{
    public const string DefaultScenarioResPath = "res://config/scenarios/default.json";

    /// <summary>Absolute filesystem path to core.json → map radius.</summary>
    public static Func<string, SimVec2I>? LoadCoreMapRadiusFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path to core.json → starting accessory points.</summary>
    public static Func<string, int>? LoadCoreAccessoryPointsFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path to scenario JSON → <see cref="Scenario"/>.</summary>
    public static Func<string, Scenario>? LoadScenarioFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path to extensions.json → <see cref="GameContent"/>.</summary>
    public static Func<string, GameContent>? LoadGameContentFromAbsolutePath { get; set; }

    /// <summary>
    /// Absolute filesystem path to extensions.json → loaded extensions (registry + integrator + content).
    /// </summary>
    public static Func<string, ExtensionLoadResult>? LoadExtensionsFromAbsolutePath { get; set; }

    /// <summary>Godot cmdline args → optional scenario resource path override.</summary>
    public static Func<IReadOnlyList<string>, string?>? TryGetScenarioPathFromArgs { get; set; }

    /// <summary>Process environment → optional scenario resource path override.</summary>
    public static Func<string?>? TryGetScenarioPathFromEnvironment { get; set; }

    /// <summary>Process environment → optional world seed override.</summary>
    public static Func<int?>? TryGetWorldSeedFromEnvironment { get; set; }

    /// <summary>Process environment → whether boot should redirect main menu to lobby.</summary>
    public static Func<bool>? ShouldStartAtLobby { get; set; }

    /// <summary>Absolute filesystem path to player_profiles.json → catalog (empty if missing).</summary>
    public static Func<string, PlayerProfileCatalog>? LoadPlayerProfilesFromAbsolutePath { get; set; }

    /// <summary>Absolute filesystem path + catalog → write player_profiles.json.</summary>
    public static Action<string, PlayerProfileCatalog>? SavePlayerProfilesToAbsolutePath { get; set; }

    /// <summary>
    /// Import a filesystem image into the avatars directory.
    /// Args: avatarsDirAbsolute, profileId, sourceAbsolute, previousAvatarFile → (ok, avatarFile, error).
    /// </summary>
    public static Func<string, Guid, string, string?, PlayerAvatarImportResult>?
        TryImportPlayerAvatar { get; set; }

    /// <summary>
    /// Delete an avatar filename under the avatars directory.
    /// Args: avatarsDirAbsolute, avatarFile → (ok, error).
    /// </summary>
    public static Func<string, string?, PlayerAvatarDeleteResult>? TryDeletePlayerAvatar { get; set; }

    public const string DefaultPlayerProfilesResPath = "user://player_profiles.json";

    public const string DefaultPlayerAvatarsResPath = "user://profile_avatars/";

    public static SimVec2I RequireCoreMapRadius(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadCoreMapRadiusFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadCoreMapRadiusFromAbsolutePath.");
        return load(absolutePath);
    }

    public static int RequireCoreAccessoryPoints(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadCoreAccessoryPointsFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadCoreAccessoryPointsFromAbsolutePath.");
        return load(absolutePath);
    }

    public static Scenario RequireScenario(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadScenarioFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadScenarioFromAbsolutePath.");
        return load(absolutePath);
    }

    public static GameContent RequireGameContent(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadGameContentFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadGameContentFromAbsolutePath.");
        return load(absolutePath);
    }

    public static ExtensionLoadResult RequireExtensions(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadExtensionsFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadExtensionsFromAbsolutePath.");
        return load(absolutePath);
    }

    public static string? TryResolveScenarioPathFromArgs(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return TryGetScenarioPathFromArgs?.Invoke(args);
    }

    public static string? TryResolveScenarioPathFromEnvironment() =>
        TryGetScenarioPathFromEnvironment?.Invoke();

    public static int? TryResolveWorldSeedFromEnvironment() =>
        TryGetWorldSeedFromEnvironment?.Invoke();

    public static bool ResolveShouldStartAtLobby() =>
        ShouldStartAtLobby?.Invoke() ?? false;

    public static PlayerProfileCatalog RequirePlayerProfiles(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var load = LoadPlayerProfilesFromAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.LoadPlayerProfilesFromAbsolutePath.");
        return load(absolutePath);
    }

    public static void RequireSavePlayerProfiles(string absolutePath, PlayerProfileCatalog catalog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(catalog);
        var save = SavePlayerProfilesToAbsolutePath
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.SavePlayerProfilesToAbsolutePath.");
        save(absolutePath, catalog);
    }

    public static PlayerAvatarImportResult RequireImportPlayerAvatar(
        string avatarsDirectoryAbsolute,
        Guid profileId,
        string sourceAbsolutePath,
        string? previousAvatarFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarsDirectoryAbsolute);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAbsolutePath);
        var import = TryImportPlayerAvatar
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.TryImportPlayerAvatar.");
        return import(avatarsDirectoryAbsolute, profileId, sourceAbsolutePath, previousAvatarFile);
    }

    public static PlayerAvatarDeleteResult RequireDeletePlayerAvatar(
        string avatarsDirectoryAbsolute,
        string? avatarFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarsDirectoryAbsolute);
        var delete = TryDeletePlayerAvatar
            ?? throw new InvalidOperationException(
                "World host hooks are not registered. Minimap.App must set WorldHostHooks.TryDeletePlayerAvatar.");
        return delete(avatarsDirectoryAbsolute, avatarFile);
    }
}

/// <summary>Outcome of copying a profile avatar into the user avatars directory.</summary>
public readonly struct PlayerAvatarImportResult
{
    public PlayerAvatarImportResult(bool ok, string? avatarFile, string? error)
    {
        Ok = ok;
        AvatarFile = avatarFile;
        Error = error;
    }

    public bool Ok { get; }
    public string? AvatarFile { get; }
    public string? Error { get; }
}

/// <summary>Outcome of deleting a profile avatar file.</summary>
public readonly struct PlayerAvatarDeleteResult
{
    public PlayerAvatarDeleteResult(bool ok, string? error)
    {
        Ok = ok;
        Error = error;
    }

    public bool Ok { get; }
    public string? Error { get; }
}

/// <summary>Client-facing snapshot of a successful extension load.</summary>
public sealed class ExtensionLoadResult
{
    public ExtensionLoadResult(
        GameContent content,
        IReadOnlyList<AccessoryDefinition> playerSelectableAccessories,
        IReadOnlyList<DomainDefinition> domains)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(playerSelectableAccessories);
        ArgumentNullException.ThrowIfNull(domains);
        Content = content;
        PlayerSelectableAccessories = playerSelectableAccessories;
        Domains = domains;
    }

    public GameContent Content { get; }

    public IReadOnlyList<AccessoryDefinition> PlayerSelectableAccessories { get; }

    public IReadOnlyList<DomainDefinition> Domains { get; }
}
