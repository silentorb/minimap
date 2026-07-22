using System.Runtime.Loader;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Loads extension assemblies and builds playthrough content via the configured integrator.</summary>
public static class ExtensionLoader
{
    /// <summary>Success payload from a completed extension load (failures throw).</summary>
    public sealed record LoadedExtensions(
        ExtensionRegistry Registry,
        IIntegrator Integrator,
        GameContent Content);

    public static LoadedExtensions Load(ExtensionsSettings settings, string configDirectory)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(configDirectory);

        var registry = new ExtensionRegistry();

        foreach (var entry in settings.Extensions)
        {
            var path = ExtensionPathResolver.ResolveAssemblyPath(
                entry,
                settings.SearchPaths,
                configDirectory);
            LoadAssembly(path, registry);
            DefinitionConfig.RegisterFromConfigDirectory(
                DefinitionConfig.ContentDirectoryForAssembly(path),
                registry);
        }

        var integrator = registry.RequireIntegrator(settings.Integrator);
        var content = integrator.CreateGameContent(registry);
        return new LoadedExtensions(registry, integrator, content);
    }

    public static LoadedExtensions LoadFromFile(string settingsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsPath);
        var settings = ExtensionsSettings.LoadFromFile(settingsPath);
        var configDirectory = Path.GetDirectoryName(Path.GetFullPath(settingsPath))
            ?? throw new InvalidOperationException($"Could not resolve directory for '{settingsPath}'.");
        return Load(settings, configDirectory);
    }

    private static void LoadAssembly(string assemblyPath, ExtensionRegistry registry)
    {
        var hostAlc = AssemblyLoadContext.GetLoadContext(typeof(ExtensionLoader).Assembly)
            ?? AssemblyLoadContext.Default;
        var assembly = hostAlc.LoadFromAssemblyPath(assemblyPath);
        var discovered = 0;

        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IExtension).IsAssignableFrom(type))
                continue;

            IExtension extension;
            try
            {
                extension = (IExtension)(Activator.CreateInstance(type)
                    ?? throw new InvalidOperationException($"Failed to construct {type.FullName}."));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create extension {type.FullName}: {ex.Message}", ex);
            }

            extension.Register(registry);
            discovered++;
        }

        if (discovered == 0)
        {
            throw new InvalidOperationException(
                $"No IExtension types found in {assemblyPath}.");
        }
    }
}
