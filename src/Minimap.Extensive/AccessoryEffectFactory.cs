using System.Text.Json;
using Minimap.Simulation.Types;

namespace Minimap.Extensive;

/// <summary>
/// Builds an <see cref="AccessoryEffect"/> from one effect object in accessory definition JSON.
/// </summary>
/// <param name="effectObject">The JSON object for this effect (includes <c>type</c> and type-specific fields).</param>
/// <param name="index">Zero-based index in the accessory's effects array (for error messages).</param>
/// <param name="sourcePath">Optional file path for error messages.</param>
/// <param name="registry">Registry for resolving resource ids (e.g. effect costs).</param>
public delegate AccessoryEffect AccessoryEffectFactory(
    JsonElement effectObject,
    int index,
    string? sourcePath,
    IExtensionRegistry registry);
