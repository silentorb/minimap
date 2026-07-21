using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Minimap.Client;

namespace Minimap.App;

/// <summary>Registers App-owned I/O hooks that Client scene roots may call.</summary>
internal static class AppHostRegistration
{
    // Intentional: App is a class library for tests but also host composition; register before lobby runs.
    [ModuleInitializer]
    [SuppressMessage("Performance", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "Registers ExtensionPreflight before lobby scene roots run; App is host composition.")]
    internal static void RegisterClientHooks()
    {
        ExtensionPreflight.LoadFromAbsolutePath = path => ExtensionLoader.LoadFromFile(path);
    }
}
