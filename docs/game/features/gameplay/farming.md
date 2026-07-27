# Farming

Growing and harvesting food via the **Farm** ability. Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [interaction.md](interaction.md), [resources.md](resources.md), [cell-placement.md](cell-placement.md), [hunger.md](hunger.md), [ai.md](ai.md). Technical: [farming.md](../../../technical/features/gameplay/farming.md).

## Plant stages

Standard vocabulary for vegetables:

- **growing** — cell-anchored planted plant (seedling art at plant time); actor id `{plant}_growing`
- **ripe** — harvestable maturity **state** of a growing plant (grow effect / depiction override; not a separate definition)
- **picked** — produce outside the ground (pickup actor); actor id `{plant}_picked`

Creature/mobile forms (e.g. **crazed carrot**) stay unqualified (`crazed_carrot`).

## Requirements

- **Farm** is a player-selectable modal **ability** (accessory). It plants growing food actors and harvests ripe plants.
- Planting: modal activate preview/confirm places a random vegetable actor from a weighted pool on an unoccupied **Grass** cell in front of the player. Costs **1 seed** per successful plant (effect use cost).
- Plant pool weights: **carrot_growing / corn_growing / melon_growing = 3** each, **crazed_carrot_growing = 1** (**10%** crazed).
- Newly planted vegetables appear as an ambiguous **seedling**. After a configured grow duration (**5** seconds), the actor becomes **ripe** and shows that vegetable’s mature depiction. A ripe **crazed carrot** growing plant uses the same art as a regular carrot.
- Grow behavior lives on a **grow** accessory/effect on each growing vegetable actor definition (duration, mature depiction, food yield and/or ambush emerge).
- Harvesting normal plants: with Farm selected, environment interact on a ripe food actor costs **1 energy**, destroys the growing plant, and grants the farmer a **food** resource equal to that vegetable’s yield (**carrot 1**, **corn 2**, **melon 3**).
- **Crazed carrot** growing plants: Farm harvest costs **1 energy** and grants **no** resources; the plant is removed and a **crazed carrot** actor spawns at that cell on the **rival** faction (same as zombies). If a ripe crazed plant is not harvested, it **emerges on its own 5 seconds** after becoming ripe.
- Emerged crazed carrots use high-aggression AI (**0.9**) and Swing; on death they drop a **picked carrot** (`carrot_picked`, fully grown carrot art) that any actor can pick up via default environment interact for **+1 food** at a cost of **1 energy** (Farm not required). See [interaction.md](interaction.md).
- **Zombie farmers** (rival AI) can harvest ripe plants with Farm and consume food with Eat; they do not plant.
- Acquiring Farm grants **3** seeds via a `modify_resource` effect (not an accessory-level resource block).
- Food is a generic resource; **Eat** spends **1 food** to restore energy (see [hunger.md](hunger.md)). Only vegetables are grown.

## Non-goals (for now)

- Per-plant food resource types
- Watering, soil quality, or multi-stage growth beyond growing → ripe (picked is a separate actor)
