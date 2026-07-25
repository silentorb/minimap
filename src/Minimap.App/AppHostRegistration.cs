using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Minimap.Client;

namespace Minimap.App;

/// <summary>Registers App-owned I/O hooks that Client scene roots may call.</summary>
internal static class AppHostRegistration
{
    // Intentional: App is a class library for tests but also host composition; register before scene roots run.
    [ModuleInitializer]
    [SuppressMessage("Performance", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "Registers Client host hooks before lobby/world scene roots run; App is host composition.")]
    internal static void RegisterClientHooks()
    {
        DotEnvBootstrap.TryApply();

        ExtensionPreflight.LoadFromAbsolutePath = path => ExtensionLoader.LoadFromFile(path);
        WorldHostHooks.LoadCoreMapRadiusFromAbsolutePath = path =>
            CoreSettings.LoadFromFile(path).Map.Radius;
        WorldHostHooks.LoadCoreAccessoryPointsFromAbsolutePath = path =>
            CoreSettings.LoadFromFile(path).Player.AccessoryPoints;
        WorldHostHooks.LoadScenarioFromAbsolutePath = ScenarioSettings.LoadFromFile;
        WorldHostHooks.LoadGameContentFromAbsolutePath = path =>
            ExtensionLoader.LoadFromFile(path).Content;
        WorldHostHooks.LoadExtensionsFromAbsolutePath = path =>
        {
            var loaded = ExtensionLoader.LoadFromFile(path);
            return new ExtensionLoadResult(
                loaded.Content,
                loaded.Integrator.GetPlayerSelectableAccessories(loaded.Registry),
                loaded.Registry.DomainDefinitions);
        };
        WorldHostHooks.TryGetScenarioPathFromArgs = CliArgs.TryGetScenarioPath;
        WorldHostHooks.TryGetScenarioPathFromEnvironment = CliArgs.TryGetScenarioPathFromEnvironment;
        WorldHostHooks.TryGetWorldSeedFromEnvironment = CliArgs.TryGetWorldSeedFromEnvironment;
        WorldHostHooks.ShouldStartAtLobby = CliArgs.ShouldStartAtLobby;
        WorldHostHooks.LoadPlayerProfilesFromAbsolutePath = PlayerProfileStore.LoadFromFile;
        WorldHostHooks.SavePlayerProfilesToAbsolutePath = PlayerProfileStore.SaveToFile;
    }
}
