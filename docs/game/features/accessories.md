# Accessories

System for attaching game logic to actors (including characters). Related: [actors.md](actors.md), [characters.md](characters.md), [combat.md](combat.md), [tags.md](tags.md), [resources.md](resources.md), [players.md](players.md), [lobby.md](lobby.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [cell-placement.md](cell-placement.md), [hunger.md](hunger.md). Technical: [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Terminology

- **Accessory** — the general container: definition + instance holding a list of **effects**. Beneficial or harmful.
- **Ability** — a prominent positive accessory players can obtain (e.g. Gun, Farm, Geek). Most early accessories are abilities; behavior still lives on effects.
- **Effect** — the unit of behavior. Prefer simple, reusable, composable effects. Use a more complex effect when multiple accessory behaviors must coordinate (e.g. grow maturity + depiction + yield).

## Requirements

- Each **actor** has a list of **accessories**. Each **actor / character definition** lists accessory definitions assigned at instantiation.
- An accessory does not carry behavior-specific fields of its own. It holds a list of **accessory effects**. One accessory can mix independent effects.
- Each accessory instance has an **accessory definition**. Definitions ship as JSON with their content extension (CompuQuest: `src/CompuQuest.Minimap/config/accessories/`; see [../../technical/features/definition-config.md](../../technical/features/definition-config.md)); extensions may also register definitions in code.
- Definitions may include **tags**, **pointCost** (default **0**), **displayName**, **description**, depiction, icon, and optional **activation** (dedicated / modal / none). They do **not** declare consumed resources or starting amounts at the accessory level.
- When an accessory is **added**, its effects join the actor’s flat effect list; on-acquire effects (e.g. `modify_resource`) run; the character **ability loadout** rebuilds when applicable. When it is **removed**, those effects are removed.
- **Use cost** is per **effect** (resource tag + amount; default free). Activatable effects that declare a cost cannot run when the actor cannot afford it; successful activation consumes the cost. Use cost does **not** disable the accessory (e.g. Gun with **0** ammo stays selectable).
- **Enable gate** (optional on the definition, e.g. `enabledWhen`): when unmet, the accessory is **disabled** — omitted from the ability loadout and HUD. Orthogonal to use cost. See [hunger.md](hunger.md) (**Eat** gated on food ≥ 1).
- **Starting stock** is granted by a `modify_resource` (or equivalent) effect whose purpose is to change a resource when the accessory is acquired.
- Shipped player-selectable **abilities** (each lobby point cost **1**): **Gun** (dedicated shoot; grant ammo **6**; shoot costs **1** ammo), **Farm** (modal plant/harvest; grant seeds **3**; plant costs **1** seed), **Geek** (modal place computer + interact with computers; grant computers **1**; place costs **1** computer; further computer behavior forthcoming).
- Shipped non-selectable character accessories: **energy upkeep** (`energy_upkeep`; activation none; drain + vitality), **Eat** (`eat`; modal; enable-gated on food; instant use restores energy). See [hunger.md](hunger.md).
- Passive effects may tick on characters (e.g. `drain_resource`, `modify_resource_by_ratio_bands`). Instant-use effects (`modify_resource_on_use`) run on modal activate without placement preview.
- Lobby selection (see [lobby.md](lobby.md)): players spend accessory points on tagged `player_selectable` accessories. Players do not start with Gun or other selectable abilities unless chosen.

## Non-goals (for now)

- In-world inventory / equip UI beyond lobby selection and modal ability select
- Character stats and stat-modifier apply/revert (future; motivates the effect cache)
- Multiple weapon types beyond Gun
- Ways to refill ammo / seeds / computers after the acquire grant
