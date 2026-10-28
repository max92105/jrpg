# Loot: design

Part of [the loot plan](README.md).

## Tunables and placeholders

Every value is a placeholder (decision 196's rule): B1's simulation prints what they give, the developer tunes them.

**`rules.json` `loot`** (`Loot/LootRules`, R3a). Weights are relative. Unique has no row: it never rolls (decision 339). The colors wait for question 362.

| Rarity | `weight` | `minAffixes` | `maxAffixes` | Color (proposed, question 362) |
|---|---|---|---|---|
| common | 650 | 0 | 0 | the body text color |
| magic | 280 | 1 | 1 | blue |
| rare | 70 | 2 | 3 | yellow-gold |
| unique (reserved, no row) | | | | orange |

**`affixes.json`** (`Loot/AffixDefinition`, R3a). Whole points for primaries, max HP and speed, whole percents for the rest (ADR 0026, as item bonuses). Every `weight` 10. One range per affix (decision 340). The affix stats plan adds `life_steal`, `armor_penetration`, `block`, `drop_luck`.

| Affix id | Stat | Range | Rolls on |
|---|---|---|---|
| `power` | power | 2-5 | weapon, ring, amulet, hands |
| `agility` | agility | 2-5 | ring, hands, feet |
| `intelligence` | intelligence | 2-5 | ring, amulet, head |
| `constitution` | constitution | 2-5 | chest, head, offHand |
| `will` | will | 2-5 | amulet, head, chest |
| `max_hp` | maxHp | 10-25 | chest, offHand |
| `speed` | speed | 1-3 | feet |
| `accuracy` | accuracy | 2-5 | weapon, hands |
| `crit_chance` | critChance | 1-3 | weapon, ring |
| `crit_damage` | critDamage | 5-15 | weapon, amulet |
| `dodge` | dodge | 1-3 | feet |
| `blunt_ward`, `piercing_ward`, `slashing_ward` | the type's resistance | 4-10 | chest, offHand, head, feet |
| `fire_ward`, `ice_ward`, `lightning_ward`, `light_ward`, `dark_ward` | the type's resistance | 4-10 | chest, amulet, ring, head |

**`drop-tables.json`** (`Loot/DropTable`, D2). Each entry rolls on its own (decision 343). The forge plan adds Scrap.

| Table | Item | Chance | Quantity |
|---|---|---|---|
| `goblin` | potion | 30 % | 1 |
| `goblin` | bronze_sword | 6 % | 1 |
| `goblin` | leather_cap, leather_boots, power_ring | 4 % each | 1 |
| (training dummy) | none: no `dropTableId` | | |

## Design overview

### What exists
- `Items/ItemDefinition` is a catalog record: consumables have an `Effect`, gear has `EquipmentStats(Kind) { Damage, DamageType, Bonuses }`.
- `Items/Inventory` is a list of `InventoryEntry(ItemId, Quantity)` stacks, gear included (`maxStack` 99 on every item); `HasRoomFor` refuses a full stack, which `PartyEquipment` checks before putting a worn item back (the Equipment page's "no room" notice).
- `Party/WornEquipment` maps the nine `EquipmentSlot`s to `ItemDefinition`s and sums their `Bonuses`; `PartyMember.Weapon`, `EquippedIn`, `SheetWith`, `Wear` take or return `ItemDefinition`; `WornItem(Wearer, Item)`; `PartyEquipment` moves items between the inventory and members (ADR 0003, ADR 0028).
- `Session/GameSession.FinishBattle` carries lasting resources back, grants experience (`BattleState.GrantVictoryExperience` returns `VictoryRewards(Experience, Gains)`), clears the encounter and returns the rewards, null after an escape or a defeat. Its `IRandom` is the session's.
- `Battle/ChanceRolls` (internal) is the permille roll that draws only when uncertain.
- Saves (format 4): `SaveGame.Inventory` entries by id, `PartyMemberProgress.Worn` by slot and item id; `Data/Saving/SaveGameRepair` drops unknown ids and moves misfit worn items back.
- Pages: the Inventory page lists stacks with their quantity; the Equipment page's choice lists owned items with their quantity and worn ones per wearer; `EquipmentComparison` compares two `StatSheet`s, so anything that reaches the sheet shows there.
- The victory screen (`VictoryScreen(VictoryRewards)`, `VictoryScreenView` 249) shows experience and level-ups, then leaves on Confirm.

### Gear pieces (ADR 0029)
- `Items/GearPiece` (G1a): one piece of gear as owned or worn: a base `ItemDefinition` that is equipment, a `Rarity`, and its `RolledAffix`es in roll order. A sealed record with equality by content (affixes compared in order), so two plain bronze swords are equal and interchangeable. `Kind`, `Damage` and `DamageType` come from the base; `Bonuses` is the base's plus every affix's. `GearPiece.Plain(item)` is a Common piece with no affixes (starting items, map gifts, today's fixed items); `GearPiece.Rolled(item, rarity, affixes)` the rest. Both throw `ArgumentException` for an item that isn't equipment.
- `Items/RolledAffix(string AffixId, StatKind Stat, int Value)` (G1a): one rolled line at runtime. The stat is resolved from the pool when the piece is rolled or loaded, so bonuses need no catalog afterwards.
- `Items/Rarity` (G1a): Common, Magic, Rare, Unique (decision 339; Unique reserved, never rolled). Material is not a gear rarity: the forge plan gives materials their own color.
- **Inventory** (G1b-1): `Entries` keeps the stacks (consumables; materials later); `Gear` lists pieces in pickup order. `Add(ItemDefinition, quantity)` puts equipment in as that many plain pieces, so `EventRun`'s give-item step and the new game's starting items need no change; `AddGear(piece)`, `TryRemoveGear(piece)` (one equal piece) and `GearCountOf(itemId)` handle pieces. `QuantityOf`, `TryRemove` and `HasRoomFor` stay stacks-only. No cap on gear (decision 345).
- **Worn** (G1a): `WornEquipment` holds pieces; `PartyMember.Weapon` and `EquippedIn` return `GearPiece?`; `SheetWith(piece, slot)`; `WornItem(Wearer, Piece)`. `PartyEquipment` takes and lists pieces from G1b-2 (`OwnedFor(slot)` drops its catalog parameter).
- **Saves** (S1a): `Items/SavedAffix(AffixId, Value)` and `Items/SavedGear(ItemId, Rarity, Affixes)` are a piece as a save keeps it; `GearPiece.ToSaved()` and `ContentCatalog.PieceFrom(saved)`, which resolves each affix's stat from `ContentCatalog.Affixes` (on the catalog so `Items` never sees `Loot`, S1a). `PartyMemberProgress.Worn` maps slots to `SavedGear`; `SaveGame.Gear` lists owned pieces; `SaveGame.Inventory` keeps stacks. Format 5.

### Loot (ADR 0030)
- `Loot/DropEntry(ItemId, Chance) { MinQuantity = 1, MaxQuantity = 1 }` and `Loot/DropTable(Id, Entries)` (D1): content; the chance a whole percent.
- `Loot/DroppedStack(ItemDefinition Item, int Quantity)` and `Loot/Drops(Stacks, Gear)` (D1): what a won battle dropped, with the items themselves so the victory screen needs no catalog. `Drops.None`.
- `Session/BattleSpoils(VictoryRewards Rewards, Drops Drops)` (D1): what a won battle gave; `GameSession.FinishBattle` returns it (null after an escape or a defeat). `VictoryRewards` stays Battle's experience record, and `Battle` never references `Loot`.
- `Loot/DropRoll` (D1): rolls tables in enemy order: each entry's chance (through `ChanceRolls`), its quantity (`NextBelow` only when the range has two values or more), stacks merged by item. Equipment becomes plain pieces in D1 (a static `Roll(tables, items, random)`); from R3b `DropRoll` is an instance built with the items, a `GearRoll` and the random, and gear rolls through `GearRoll`.
- `Loot/AffixDefinition(Id, Stat, Min, Max, Kinds, Weight)` (S1a, its first reader is the load; R2 rolls it), `Loot/RarityRule(Weight, MinAffixes, MaxAffixes)`, `Loot/LootRules(Rarities)` (R2): content.
- `Loot/GearRoll` (R2): built with the pool, the rules and the random. `Roll(item)` picks a rarity by weight, then an affix count in the rarity's range (a draw only for Rare's 2-3), then that many affixes by weight among the pool's entries for the item's kind, never one twice (decision 359), each value in `Min..Max`. A pool too small for the count gives what it has. The affix stats plan adds drop luck; the forge plan adds `AddAffix` and `Reroll`.
- `Randomness/WeightedPick` (R2): an index by weight with one draw (`NextBelow(total)`); no draw when one weight is all of the total. `Randomness/ChanceRolls` moves here from `Battle/` (D1).
- **Where loot rolls** (D1): `GameSession.FinishBattle`, after the experience: the defeated enemies' tables (`EnemyDefinition.DropTableId`; null drops nothing), the session's `IRandom`. Drops go into the inventory (a stack's extra copies past `maxStack` are lost, as a found item's are; `Drops` lists what was added).

### Content shapes
- `enemies.json`: `"dropTableId": "goblin"` (optional).
- `drop-tables.json`: `[ { "id": "goblin", "entries": [ { "itemId": "potion", "chance": 30 }, { "itemId": "bronze_sword", "chance": 6 } ] } ]`; `minQuantity` and `maxQuantity` default to 1.
- `affixes.json`: `[ { "id": "power", "stat": "power", "min": 2, "max": 5, "kinds": [ "weapon", "ring" ], "weight": 10 } ]`.
- `rules.json`: `"loot": { "rarities": { "common": { "weight": 650, "minAffixes": 0, "maxAffixes": 0 }, "magic": { … }, "rare": { … } } }`.
- Saves (format 5): `"worn": { "weapon": { "itemId": "bronze_sword", "rarity": "rare", "affixes": [ { "affixId": "power", "value": 4 } ] } }`, `"gear": [ … ]` beside `"inventory"`.

### Folder dependencies (Core)
`Stats` and `Randomness` stay leaves. `Loot` never sees `Battle`, `Party` or `Session`: it takes tables, items and numbers. `Battle` doesn't see `Loot` either (drops return beside the rewards, from `Session`).

| Folder | References (new in bold) |
|---|---|
| Randomness | none (`ChanceRolls` moves here, `WeightedPick` lands here) |
| Stats | none |
| Items | Effects, Stats |
| **Loot** | **Items, Randomness, Stats** |
| Party | Items, Progression, Resources, Skills, Stats |
| Battle | unchanged |
| Content | Battle, Items, **Loot**, Party, Progression, Skills, Stats, … |
| Session | Battle, Content, Exploration, Items, **Loot**, Party, Randomness, Saving, World |

**Game:** `Ui/RarityNames`, `Ui/AffixText` (P1a) and `UiTheme`'s rarity colors read Core's `Rarity` and `StatKind` only, and serve `Menus` and `Battle`; Ui depends on no feature folder. `Battle/VictoryScreen` takes `Session/BattleSpoils`; `Battle/VictoryDropsView` (D3) is a leaf of its folder.

### Pages
- **Inventory** (G1c, P1a): stacks then pieces in each tab (the All tab's order changes from pickup order across everything); a piece's row has no quantity and its name in its rarity's color; the detail strip under a selected rolled piece reads "Rare · +4 Power · +2 % crit chance" instead of the description (decision 361; Info hides it, decision 333).
- **Equipment** (G1b-2, P1b): the choice lists one row per owned piece (no quantity column) in its rarity's color; the comparison is unchanged (affixes reach the sheet).
- **Victory** (D3, P1b; decision 355): after the bars, an "Items" step lists the drops (name, ×quantity for stacks, rarity color for pieces); Confirm leaves. No drops: Confirm leaves after the bars as today.

### Key decisions
- **Gear rows, not stacks** (decision 345): rolled pieces are almost never equal, and a stack of identical plain pieces would make "which one is worn" ambiguous for no gain. The cost: the starting items' three staves show as three rows.
- **Loot after the battle** (ADR 0030 A): no battle draw changes; the victory screen has the drops when it opens.
- **A saved affix is its id and value**: the stat comes from content on load, so a changed pool reaches saved pieces with nothing to repair but unknown ids and narrowed ranges (S1b).
- **`Inventory.Add` keeps taking items**: equipment turns into plain pieces there, so `EventRun`, the new game and the tests that add swords to an inventory don't change; the tests that count swords with `QuantityOf` move to `GearCountOf` (G1b-1).

### Extension points
- **Item level** (decision 341): an `ItemLevel` init property on `GearPiece` and `SavedGear` (a save format bump then), set by `GearRoll` from the drop's source; `AffixDefinition` gains a range per level band, or a minimum level. Nothing in this plan's shapes blocks it: rolls are values, ranges live in content.
- **Unique items** (decision 339): a content file of hand-made pieces with fixed affixes and abilities, dropped by an entry naming them; `Rarity.Unique`, its color and its name already exist.
- A rarity: an enum member, a `rules.json` row, a color and a name.
- An affix: a line in `affixes.json` for any existing stat kind.
- Generated names (decision 342, later): a name part on `AffixDefinition`.

### Replacement mapping

| Today | Becomes | Slice |
|---|---|---|
| `WornEquipment`'s `ItemDefinition`s | `GearPiece`s | G1a |
| `PartyMember.Weapon`, `EquippedIn` → `ItemDefinition?` | → `GearPiece?` | G1a |
| `PartyMember.SheetWith(ItemDefinition[, slot])`, `Wear(slot, ItemDefinition)` | take a `GearPiece` | G1a |
| `WornItem(Wearer, Item)` | `WornItem(Wearer, Piece)` | G1a |
| equipment stacks in `Inventory.Entries` | `Inventory.Gear`; `QuantityOf` on gear → `GearCountOf` | G1b-1 |
| `PartyEquipment.TryEquip(member, ItemDefinition[, slot])`, `OwnedFor(slot, items)` | `TryEquip(member, GearPiece[, slot])`, `OwnedFor(slot)` | G1b-2 |
| `HasRoomFor` checks in `PartyEquipment`; `MessageIds.EquipmentNoRoom` | gone (decision 345) | G1b-2 |
| `EquipmentChoice.Items` (`ItemDefinition`s), the choice's quantity column | pieces, one row each | G1b-2 |
| save `worn` slot → item id; gear in `inventory` | slot → `SavedGear`; `gear` list (format 5) | S1a |
| `SaveGameRepair.RepairWorn`, `RepairWornItem`, the gear part of `RepairInventory` | `Data/Saving/GearRepair` | S1a |
| `InventoryFilter.Select` over entries | stacks and pieces | G1c |
| `Battle/ChanceRolls` | `Randomness/ChanceRolls` | D1 |
| `GameSession.FinishBattle()` → `VictoryRewards?`; `VictoryScreen(VictoryRewards)` | → `BattleSpoils?`; `VictoryScreen(BattleSpoils)` | D1, D3 |

## Risks
- **Test churn in the swap.** `EquippedIn`, `Weapon`, `SheetWith`, `Wear` about 45 lines in 15 test files (G1a); `QuantityOf` on gear about 25 lines (G1b-1); `TryEquip`, `OwnedFor`, `TryTakeFrom`, `TryUnequip` about 70 lines in 18 files (G1b-2), plus the room-refusal tests that leave with the rule (G1b-2 lists each); `FinishBattle()` 11 sites (D1). Each slice counts its sites.
- **Saves between G1b-1 and S1a.** With the count-by-id bridge dropped, a save made in the two commits between them loses owned gear (worn gear is still kept by id). Nobody plays on those commits; S1a follows at once and refuses those saves anyway (format 4).
- **Random draw order.** Loot draws after the battle from the session's random, so a seeded test that fights two battles in a row, or walks after a victory into a random encounter, sees a shifted sequence once the goblin has a table (D2) and again once gear rolls (R3b): both slices grep and budget the fixes.
- **Save format.** Format 5 refuses every format 4 save (ADR 0022): the developer's test saves go at S1a.
- **Content coupling.** Two new files (`drop-tables.json`, `affixes.json`) and a `rules.json` block: a broken one stops the game and the editor at start, with the file named (validators).
- **Value equality cost.** Comparing pieces walks their affixes (at most three): negligible. `TryRemoveGear` scans the gear list on a confirm, never per frame.
- **Balance.** Drops start after the first fight; the goblin trio simulation (stats plan, B1) is unaffected. B1 here prints loot per fight so the developer can tune the tables.
- **Determinism.** No wall clock, no unseeded random: every loot roll goes through the session's `IRandom`.
- **Performance.** Rows, affix lines and colors are formatted when the list changes, never in `Draw`.
