# Hunger (technical)

Energy resources, upkeep drain/vitality, and food-gated Eat. Implements [hunger.md](../../../game/features/gameplay/hunger.md). Related: [resources.md](resources.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [player-hud.md](../ui/player-hud.md), [farming.md](farming.md), [definition-config.md](../platform/definition-config.md), [characters.md](characters.md), [controllers.md](controllers.md).

## Requirements

- **`WellKnownResourceIds`**: `energy`, `max_energy` (mirror `health` / `max_health`).
- **`GameContent`** / **`ResourceContext`**: require those types in the catalog and expose resolved **`EnergyTag`** / **`MaxEnergyTag`**. Character spawn initializes energy / max energy like health (default max/start **100**).
- CompuQuest accessories:
  - **`energy_upkeep`** — `activation.kind: none`; not `player_selectable`; effects `drain_resource` (energy, default **1**/sec) and `modify_resource_by_ratio_bands` (vitality pulse every **1**s: source energy/max_energy → target health; bands per game doc).
  - **`eat`** — modal; optional **`enabledWhen`** `{ "id": "food", "atLeast": 1 }`; `modify_resource_on_use` (+5 energy) with use cost **1 food**.
- Character defs (`generic`, `zombie`, …) list **`energy_upkeep`** and **`eat`** in accessories (zombie keeps `gun`).
- Accessory **enable gate** (orthogonal to **`IEffectUseCost`**): definition `enabledWhen`; runtime **`Accessory.IsEnabled`** from actor resources. **`AbilityLoadout.Rebuild`** skips disabled accessories, remaps modal selection when the previous selection drops, and rebuilds when enable state flips (not only on add/remove).
- Passive tick: Simulation contract such as **`IPassiveEffect`**; **`GameWorld`** ticks it for characters (`drain_resource`, `modify_resource_by_ratio_bands`). Cell-actor grow tick remains separate.
- Instant modal use: **`IInstantUseEffect`** (or equivalent) for `modify_resource_on_use`; **`PlayerController`** activates immediately when the selected modal has instant-use effects and no placement preview.
- HUD: selected modal ability icon/name comes from the filtered loadout (disabled Eat omitted); see [player-hud.md](../ui/player-hud.md).

## Non-goals (for now)

- Server-side hunger simulation distinct from character passive ticks
- Death-at-zero-energy paths
