# Farming

Growing and harvesting food via the **Farm** ability. Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [interaction.md](interaction.md), [resources.md](resources.md), [cell-placement.md](cell-placement.md), [hunger.md](hunger.md). Technical: [farming.md](../../../technical/features/gameplay/farming.md).

## Requirements

- **Farm** is a player-selectable modal **ability** (accessory). It plants food actors and harvests mature food.
- Planting: modal activate preview/confirm places a random vegetable actor from a weighted pool on an unoccupied **Grass** cell in front of the player. Costs **1 seed** per successful plant (effect use cost).
- Newly planted vegetables appear as an ambiguous **seedling**. After a configured grow duration (**5** seconds for carrot, corn, melon), the actor becomes harvestable and shows that vegetable’s mature depiction.
- Grow behavior lives on a **grow** accessory/effect on each vegetable actor definition (duration, mature depiction, food yield).
- Harvesting: with Farm selected, environment interact on a mature food actor destroys it and grants the farmer a **food** resource equal to that vegetable’s yield (**carrot 1**, **corn 2**, **melon 3**).
- Acquiring Farm grants **3** seeds via a `modify_resource` effect (not an accessory-level resource block).
- Food is a generic resource; **Eat** spends **1 food** to restore energy (see [hunger.md](hunger.md)). Only vegetables are grown.

## Non-goals (for now)

- Per-crop food resource types
- Watering, soil quality, or multi-stage growth beyond seedling → mature
