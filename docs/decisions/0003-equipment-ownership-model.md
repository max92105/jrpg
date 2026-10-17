# 0003. Equipped items leave the inventory

Status: accepted
Date: 2026-10-17

## Context
Party members wear one weapon and one armor. Slice 4 of the legacy migration ports equipping into `Party/PartyEquipment`, on top of the `Items/Inventory` built in slice 3a. The question is where a worn copy lives.

The legacy demo kept worn copies inside the inventory: `InventoryItem.EquippedTo` listed the wearers and `AvailableStock` subtracted them, the menu showed an "E" badge, and the quantity included worn copies (Dragon Quest style). Every place that removes items (using, selling, dropping, event costs) then has to know which copies are worn.

Constraints: solo developer, so fewer rules to remember is worth a lot. Inventory stacks are capped by `MaxStack` (99) and `Inventory.Add` returns what didn't fit. Saves (part 2) will store both the inventory and each member's equipment.

## Options
### A. Worn copies stay counted in the inventory (legacy, Dragon Quest style)
- Pros: the menu shows one total per item with an "E" badge, as in the demo. Equipping never runs into a full stack.
- Cons: every removal path must check worn copies, and forgetting once lets a player sell the sword they're holding. `PartyEquipment` needs `FreeCopiesOf`/`WearersOf` scans over the party. The inventory and the members both describe the same copy.

### B. Equipping moves the copy out of the inventory (Final Fantasy / Chrono Trigger style)
- Pros: `Inventory` only holds unworn copies, so `TryRemove` is always safe and needs no knowledge of equipment. Equipping is "remove one copy, put the old item back"; taking an item from another member is unequip then equip. Each copy lives in exactly one place, which also keeps saves simple.
- Cons: the menu shows unworn copies only; showing the total means adding worn ones from the party. Unequipping or swapping can fail when the returning item's stack is already at 99, so the equip calls must report it and the menu must say why.

## Decision
**B** (developer, 2026-10-17): equipping takes the copy out of the inventory; unequipping puts it back.

## Consequences
- `PartyEquipment.TryEquip` and `TryUnequip` return `false` when the returning item doesn't fit; tests cover the full-stack case. The menu (part 3) shows a message for it.
- Selling, dropping and event costs (later plans) can use `Inventory.TryRemove` directly; worn items are never offered.
- A save stores the inventory and each member's equipped item ids separately; the two never overlap.
- The legacy "E" badge and the "take it from whom?" prompt become plain swaps in the new menu.
