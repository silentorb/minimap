# Domains (technical)

Tagged theme records and domain-colored UI icons. Implements [../../game/features/domains.md](../../game/features/domains.md). Related: [tags.md](tags.md), [accessories.md](accessories.md), [ui-icons.md](ui-icons.md), [definition-config.md](definition-config.md), [extensions.md](extensions.md), [player-hud.md](player-hud.md), [lobby.md](lobby.md).

## Requirements

- **`ColorRgb`** / **`DomainDefinition`** in **`Minimap.Simulation.Types`**: `Id`, `Tag` (`TagId`), optional `DisplayName`, `Color` (`ColorRgb` floats 0–1). Loading a domain JSON `id` uses **`TagRegistry.GetOrCreate`** (same pattern as resources).
- **`DomainColorResolver`** (Types): given an accessory’s tags and a domain catalog, returns domain colors for tags that match a registered domain, in **domain registration order**.
- **`IExtensionRegistry`**: `AddDomainDefinition`, `DomainDefinitions`, `TryGetDomainDefinition` by id and by `TagId`. Duplicate ids or duplicate tags fail fast.
- JSON under extension `config/domains/` (mirrored beside the DLL). Schema: `id` (required), optional `displayName`, required `color` as `#RRGGBB`. Invalid color → fail-fast load.
- Load order inside `DefinitionConfig.RegisterFromConfigDirectory`: **resources → domains → accessories → actors → characters**.
- **`ExtensionLoadResult`** includes `IReadOnlyList<DomainDefinition> Domains` so lobby and world Client code can resolve colors without a second catalog.
- Client **`DomainIconView`**: draws domain color swatch (solid / diagonal two-triangle / vertical stripes) under a glyph texture derived from game-icons SVGs (near-black pixels made transparent; light pixels kept as the white glyph). Used by lobby accessory selection and the player HUD ability row.
- Shipped CompuQuest: `gardening`, `computing` under `src/CompuQuest.Minimap/config/domains/`.

## Non-goals (for now)

- Validating that every accessory domain tag has a `DomainDefinition` at load time (unknown tags simply do not contribute colors)
- Shader-based SVG recolor pipeline beyond Client image processing
- Domain catalog inside `GameContent` (Client uses `ExtensionLoadResult.Domains`)
