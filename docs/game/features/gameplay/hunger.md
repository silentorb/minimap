# Hunger

Character energy drain, vitality, and eating food. Related: [resources.md](resources.md), [health.md](health.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [player-hud.md](../ui/player-hud.md). Technical: [hunger.md](../../../technical/features/gameplay/hunger.md).

## Requirements

- Every character has **current energy** (`energy` resource) and **max energy** (`max_energy` resource). Energy is limited by max energy, same pattern as health.
- Default **max energy** (and starting energy) is **100**.
- Characters gradually lose energy over time (**1** energy per second by default). Time drain and vitality live on a hidden **energy upkeep** accessory (`energy_upkeep`): activation **none**, not lobby-selectable, present on every mobile actor definition that uses hunger.
- Characters also lose energy from movement: **1** energy per **120** world units traveled (about **1** energy per second at full move speed **120**). Distance drain lives on a separate hidden **movement energy** accessory (`movement_energy`): activation **none**, not lobby-selectable, present on every mobile actor definition that uses hunger.
- **Vitality** pulses every **1** second and modifies **health** from the character’s energy as a percent of max:

  | Energy (% of max) | Health delta / pulse |
  |-------------------|----------------------|
  | exactly **0%** | **−2** |
  | **(0%, 33%]** | **−1** |
  | **(33%, 66%]** | **0** |
  | **(66%, 100%]** | **+1** |

- Energy at **0** does **not** kill the character. Only **health ≤ 0** removes the character (see [health.md](health.md)).
- Every character has a modal **Eat** ability (`eat`). It is **disabled** when the character has **food < 1**, and **enabled** when **food ≥ 1**. Eating costs **1 food** and restores **+5 energy**.
- **Disabled** abilities are omitted from the ability loadout and HUD (they cannot be selected). Inability to pay an effect **use cost** does **not** disable an ability (e.g. Gun with **0** ammo stays equipped and selectable).

## Non-goals (for now)

- Death directly from energy depletion
- Distinct hunger meter UI beyond the energy resource row

Zombie farmers use Eat when they hold food and are missing at least **5** energy (see [ai.md](ai.md)).
