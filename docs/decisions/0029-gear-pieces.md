# 0029. Gear pieces

Status: accepted
Date: 2026-10-28

## Context
Gear is a catalog record today: `Items/ItemDefinition` holds a weapon's damage and an item's fixed bonuses, the
inventory stacks gear by id like potions, and members wear `ItemDefinition`s by slot (ADR 0028). The loot plan rolls a
rarity and random affixes onto dropped gear (decisions 339, 340), so two bronze swords may now differ, and a worn piece
must keep its rolls wherever it moves (ADR 0003: equipping takes the copy out of the inventory). Starting items and map
gifts stay plain (decision 344); there is no inventory cap and no room rule for gear (decision 345); saves are refused,
not migrated, until the first release (ADR 0022); content is the only source of truth (ADR 0011).

## Options
### A. A gear piece is a value: a base item, a rarity and its rolled affixes
Compared by content; the inventory holds stacks for consumables and materials and a list of pieces for gear; saves keep
each piece's base id, rarity and affixes.
- Pros: fixed-stat items are plain Common pieces, so today's content and starting items keep working; two plain swords
  are equal and interchangeable; a changed base reaches every owned piece; affixes stay lines that can be shown and
  rerolled (the forge plan).
- Cons: the inventory has two shapes (stacks and pieces), and every caller of the worn and owned gear changes type.

### B. Every item is an instance with a unique id
Consumables too.
- Pros: one shape for everything.
- Cons: potions lose stacking, the inventory and saves grow, and ids need a generator in a deterministic Core.

### C. A rolled piece becomes a generated `ItemDefinition`
Such as "bronze_sword#3f2a", kept in a save-side catalog.
- Pros: the fewest code changes.
- Cons: rolls are flattened into bonuses (no affix lines, no reroll); a catalog record stops being content (ADR 0011);
  a changed base never reaches saved pieces.

### D. A piece is a base id and a seed
Its rolls are worked out from the seed and content on load.
- Pros: tiny saves.
- Cons: any change to the affix pool or the rarity table silently rewrites every owned piece, and crafting needs seeds
  that chain.

## Decision
A.
- `Items/GearPiece` is one piece of gear, owned or worn: a base item that is equipment, a `Rarity`, and its rolled
  affixes in roll order. Two pieces are equal when all three are (affixes compared in order). `GearPiece.Plain(item)` is
  a Common piece without affixes.
- `Items/Rarity`: Common, Magic, Rare, Unique. Unique is reserved and never rolls in the loot plan. Material is the
  materials' tier for color only, not a gear rarity (decision 339).
- The inventory keeps stacks for consumables and materials (`Entries`) and a list of pieces for gear (`Gear`): one row
  per piece, no cap and no room rule (decision 345). `Inventory.Add(item, quantity)` turns equipment into plain pieces.
  `QuantityOf` and `TryRemove` stay stacks-only; `GearCountOf(itemId)` counts a base's pieces.
- Members wear pieces, and `PartyEquipment` moves pieces: a worn piece is not in the inventory and keeps its rolls
  wherever it goes (ADR 0003's move model).
- A rolled affix carries its stat at runtime (`RolledAffix`); a save keeps only its id and value (`SavedAffix`), the
  stat resolved from content on load (ADR 0011).
- Saves keep each piece as `SavedGear` (base id, rarity, affixes), worn by slot and owned in a `gear` list: format 5,
  older formats refused (ADR 0022).

## Consequences
- Fixed-stat items, starting items and map gifts keep working as plain pieces.
- Two plain swords are interchangeable; the inventory lists one row per piece, so three staves are three rows.
- An item level can join the piece later as one more field on `GearPiece` and `SavedGear` (decision 341), with a save
  format bump.
- Saves made before format 5 are refused.
