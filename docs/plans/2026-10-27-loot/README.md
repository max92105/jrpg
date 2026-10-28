# Loot
Status: approved, in progress

Sixth feature plan after the migration, following the stats and gear slots plan (its "Out of scope": "the next plan, loot"; decision 294).
- Numbers: decisions 338-355, 359-361 and question 362 here; 356 goes to [the affix stats plan](../2026-10-27-affix-stats/README.md), 357 (a question) and 358 to [the forge plan](../2026-10-27-forge/README.md). ADRs 0029 and 0030. The next free numbers are in `docs/plans/README.md` (decision 363, ADR 0031).
- The developer approved the draft on 2026-10-27 with answers to questions 339-355 and the devil's advocate's edits: the plan was split in three (this one keeps the playable loop; the four affix stats and the forge became their own plans), R1 folded into G1a, G1b split in two, the save bridge dropped so S1a follows G1b, drops returned beside the rewards (`Session/BattleSpoils`), and an affix's stat resolved from content on load.

Developer's request (2026-10-26, from the stats plan's README): "Gear should be more like an ARPG than a classic JRPG: I want to mash the genres, so rarity, random rolls, drop tables, more gear slots (ring, amulet…), salvage, materials and a bit of crafting." The slots and fixed-stat items are done (stats plan, phase 7); the loot half is three plans: this one (kill, drop, roll, equip, save, show), [affix stats](../2026-10-27-affix-stats/README.md) (lifesteal, armor penetration, block, drop luck) and [the forge](../2026-10-27-forge/README.md) (materials, salvage, upgrade, reroll at a forge master).

## Goal
At the end of this plan:
1. **Gear is owned and worn as pieces**: a piece is a base item from `items.json`, a rarity and the affixes it rolled. Two pieces of the same base can differ; consumables still stack.
2. **Enemies drop loot**: an enemy names a drop table in `drop-tables.json` (several enemies may share one); a won battle rolls the defeated enemies' tables with the session's seeded random, adds the loot to the inventory, and the victory screen lists it.
3. **Dropped gear rolls a rarity and affixes**: Common (none), Magic (one) or Rare (two or three) by weight, each affix drawn from a pool in content, never one twice, its value in its range. Starting items and map gifts stay plain Common.
4. **Saves keep every piece with its rolls** (format 5; older formats refused, ADR 0022), repaired against content on load.
5. **The pages show it**: every piece name in its rarity's color, affix lines under the selected piece, the Equipment comparison counting affixes as it counts base bonuses.

## Order, and why
1. **Phase 0: clean-up (C1, C2, C3).** `InventoryPage` (280) and `EquipmentPage` (281) split before the slices that grow them; the two save test files at 289 and 285 split before the save slice (backlog lines 46, 76, 85).
2. **Phase 1: gear pieces (ADR 0029, G1a, G1b-1, G1b-2, S1a, G1c).** The model swap first, while every piece is still plain: members wear pieces (with rarity and affixes from birth), then the inventory holds them, then equipping moves them, then saves keep them (no count-by-id bridge, so S1a follows G1b-2 at once), then the Inventory page lists them.
3. **Phase 2: drops (ADR 0030, D1, D2, D3).** Plain drops end to end: the roll in Core, the tables in content, the victory screen's list.
4. **Phase 3: rolls (R2, R3a, S1b, R3b).** The roll, the pool and rarity table in content, the load repair, then drops roll for real.
5. **Phase 4: pages (P1a, P1b).** Rarity colors and affix lines, once rolled pieces exist in play. P1a waits for question 362 (the colors).
6. **Phase 5: numbers and walk-through (B1, Z).**

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0019, ADR 0026): rarity weights and affix counts, the affix pool and drop tables are content. Every number is a placeholder (design.md, "Tunables").
- **Rolls draw only when uncertain** (ADR 0026): a 100 % drop, a single possible rarity, a one-value range or a fixed affix count draw nothing, so seeded fixtures keep their sequences. Chances go through `ChanceRolls` (moved to `Randomness/` in D1); weighted choices through `Randomness/WeightedPick`.
- **Loot rolls after the battle, never inside it**: `GameSession.FinishBattle` rolls with the session's `IRandom`, so no battle's draw sequence changes.
- **A piece is a value** (ADR 0029): two pieces with the same base, rarity and affixes are interchangeable; equality is by content.
- **Pure presenter + thin view** (`docs/map.md`). Every Game slice gives its raw estimate and, when it adds a view type, the 1.6x the class mechanics plan measured. One selection look (decision 252).
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks; each slice counts its call sites and names the test file new tests go in when the old one is near 300.
- **Files near 300 lines split right before the slice that grows them**: `Menus/InventoryPage.cs` 280 (C1), `Menus/EquipmentPage.cs` 281 (C2), `Data/Saving/SaveGameRepair.cs` 255 (S1a moves gear repair out), `Party/PartyMember.cs` 250, `Battle/BattleState.cs` 269 (not grown: loot lives in Session and `Loot/`), `Battle/VictoryScreenView.cs` 249 (D3 adds a view beside it); tests `InventoryPageTests` 329, `FileSaveStoreTests` 289, `SaveGameRepairTests` 285 (C1, C3), `EquipmentPageTests` 270, `GameSessionSaveTests` 270, `GameSessionBattleTests` 242.
- **Saves:** one format change (S1a, 4 to 5), refused when older (ADR 0022).
- **No NuGet package. No editor change** in this plan: the editor never edits items, enemies or rules; it loads them through Data and follows the new files without code.

## Out of scope (follow-ups)
- **[Affix stats](../2026-10-27-affix-stats/README.md)** (draft): lifesteal, armor penetration, block, drop luck, and the Status page showing all twenty-four stats (decision 350).
- **[The forge](../2026-10-27-forge/README.md)** (draft): materials and their tier, salvage, upgrade, reroll, the forge master NPC and its event step with editor support (decisions 351-354).
- **Unique items** (decision 339): the tier is reserved here (enum member, color, name); hand-made unique items with fixed mods and special abilities are a later plan.
- **Item levels** (decision 341): later; the piece model stays open to them (design.md, "Extension points").
- **Gold, shops and prices** (question 222): drops are items only.
- **Generated names** for rolled pieces (decision 342: not for now).
- **Gear art**: drops reuse today's icons.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 0: clean-up
- [x] **C1. `InventoryPage`'s list moves to `Menus/InventoryList`; `InventoryPageTests` split** (Game, mechanical, about 180 lines moved) → [slices/C1.md](slices/C1.md)
- [x] **C2. `EquipmentPage`'s open choice moves to `Menus/SlotChoice`** (Game, mechanical, about 110 lines moved) → [slices/C2.md](slices/C2.md)
- [x] **C3. Save test files split: `FileSaveStoreTests` format checks apart, `SaveGameRepairTests` inventory and world apart** (tests, mechanical, about 200 lines moved) → [slices/C3.md](slices/C3.md)

### Phase 1: gear pieces
- [x] **ADR 0029: Gear pieces** (before G1a) → [slices/ADR-0029.md](slices/ADR-0029.md)
- [x] **G1a. Members wear gear pieces, with a rarity and affixes from birth** (Core + Game, about 250 lines) → [slices/G1a.md](slices/G1a.md)
- [x] **G1b-1. The inventory holds gear as a list of pieces** (Core + tests, about 200 lines) → [slices/G1b-1.md](slices/G1b-1.md)
- [ ] **G1b-2. Equipping moves pieces; the room rule leaves; the Equipment choice lists pieces** (Core + Game, about 220 lines) → [slices/G1b-2.md](slices/G1b-2.md)
- [ ] **S1a. Saves keep every piece with its rolls: format 5** (Core + Data, about 260 lines; the `GearRepair` move first if it heads past 280) → [slices/S1a.md](slices/S1a.md)
- [ ] **G1c. The Inventory page lists each piece** (Game, about 130 lines) → [slices/G1c.md](slices/G1c.md)

### Phase 2: drops
- [ ] **ADR 0030: Loot rolls** (before D1) → [slices/ADR-0030.md](slices/ADR-0030.md)
- [ ] **D1. Defeated enemies drop items: `Loot/DropRoll` at `FinishBattle`, `Session/BattleSpoils`** (Core, about 250 lines) → [slices/D1.md](slices/D1.md)
- [ ] **D2. Drop tables in content; the goblin drops** (Data + content + tests, about 200 lines) → [slices/D2.md](slices/D2.md)
- [ ] **D3. The victory screen lists the drops** (Game, raw 140, about 220 lines at 1.6x) → [slices/D3.md](slices/D3.md)

### Phase 3: rolls
- [ ] **R2. `Loot/GearRoll`: rarity by weight, affixes from a pool** (Core, about 230 lines) → [slices/R2.md](slices/R2.md)
- [ ] **R3a. The affix pool and the rarity table in content** (Data + content, about 220 lines) → [slices/R3a.md](slices/R3a.md)
- [ ] **S1b. A loaded piece's affixes are repaired against the pool** (Data, about 130 lines) → [slices/S1b.md](slices/S1b.md)
- [ ] **R3b. Dropped gear rolls its rarity and affixes** (Core + content + tests, about 180 lines) → [slices/R3b.md](slices/R3b.md)

### Phase 4: pages
- [ ] **P1a. The Inventory page shows rarity and affix lines** (Game + text, raw 150, about 240 lines at 1.6x; waits for question 362) → [slices/P1a.md](slices/P1a.md)
- [ ] **P1b. Rarity on the Equipment page and the victory drops** (Game, raw 100, about 160 lines at 1.6x) → [slices/P1b.md](slices/P1b.md)

### Phase 5: numbers and walk-through
- [ ] **B1. Loot simulation: drops and rarities per goblin trio** (Simulations, about 120 lines) → [slices/B1.md](slices/B1.md)
- [ ] **Z. Walk-through: drops, pieces, saves and pages by hand** (manual) → [slices/Z.md](slices/Z.md)

## Decisions
Questions and decisions share numbers.

- **338. This plan's scope** (the developer's request of 2026-10-26 and decision 294): rarity, random affix rolls, drop tables, salvage, materials and a bit of crafting; lifesteal, armor penetration, block and drop luck as affixes, not base stats. Split on 2026-10-27 into this plan, the affix stats plan and the forge plan.

The developer's answers (2026-10-27):
- **339. Five tiers.** Common (0 mods), Magic (1), Rare (2 to 3), Unique (hand-made items in content with several fixed mods and special abilities), and Material as the materials' own tier and color, not a gear rarity. This plan rolls Common, Magic and Rare; it reserves Unique (an enum member, a color, a name) and leaves unique items and their abilities to a later plan. Material's color comes with the forge plan's materials.
- **340. Rarity gives the mod count only** (Rare rolls 2 or 3); values roll in one range per affix.
- **341. Item level limits will come, not in this plan**; the piece model stays open to an item level (design.md, "Extension points").
- **342. A rolled piece's name** is its base's name in its rarity's color, with the affixes as lines under it, for now.
- **343. Drop tables live in their own file** (`drop-tables.json`), referenced by id from an enemy (`dropTableId`); several enemies may share one table. Each entry rolls on its own, with a chance and a quantity range.
- **344. Only dropped gear rolls**; the new game's starting items and items given by map events stay plain Common pieces.
- **345. No inventory cap and no room rule for gear**; one row per piece; the Equipment page's "no room" notice leaves (G1b-2).
- **346. Lifesteal** as recommended (affix stats plan).
- **347. Armor penetration** as recommended (affix stats plan).
- **348. Block** as recommended (affix stats plan).
- **349. Drop luck raises both the drop chance and the weights of the rarities above Common**, by the same percent (placeholder; affix stats plan).
- **350. The Status page shows everything**, the four new stats included: the affix stats plan redoes the page's arithmetic for twenty-four numbers, in a slice of its own.
- **351. Crafting is upgrade and reroll**, paid with materials (forge plan).
- **352. Reroll rolls every affix at once** (forge plan).
- **353. Crafting happens at a forge master NPC on the map**, not a menu tab: a new event step opens the Forge screen, with its editor support (forge plan).
- **354. Salvage and materials** as recommended: Scrap and Essence, worn pieces can't be salvaged, salvage asks to confirm (forge plan).
- **355. The victory screen shows the drops as a second step** after the experience bars (D3).

Taken with the devil's advocate's edits (2026-10-27), the plan's behavior made explicit:
- **359. A piece never rolls the same affix twice**; a pool too small for the count gives what it has (R2).
- **360. A saved affix value outside its content range is clamped into it and reported** as a repair change, not silently (S1b).
- **361. A rolled piece's affix line replaces its description** in the Inventory's detail strip; a plain piece keeps its description (P1a).

Earlier decisions that apply: 103 (drops join the victory screen), 137 (taking a worn item from another member), 165 (anyone wears anything), 222 (gold, still open), 252 (one selection look), 294, 329 (the same ring twice), 333 (Info shows or hides detail), 337 (the Equipment comparison).

## Open design questions (developer decides)
- **362. The tiers' colors.** `.claude/rules/game.md` says to ask before departing from the reference look, and the reference has no rarity colors. *Proposed:* Common the body text color, Magic blue, Rare yellow-gold, Unique orange (the ARPG convention the request mashes in), Material a muted teal (forge plan). Alternative: a softer set closer to the reference's pale palette. P1a waits for the answer.

Still open from earlier plans: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

## ADRs
Each is a checkbox step right before the first slice that needs it. The developer chose option A for both when approving the plan; each step writes it from this entry.
- **0029 Gear pieces** (before G1a).
  - **A. A gear piece is a value: a base item, a rarity and its rolled affixes, compared by content; the inventory holds stacks for consumables and materials and a list of pieces for gear; saves keep each piece's base id, rarity and affixes (decided).** Fixed-stat items are plain Common pieces, so today's content and starting items keep working. `QuantityOf` and `TryRemove` stay stacks-only; `GearCountOf(itemId)` counts a base's pieces.
  - **B. Every item is an instance with a unique id** (consumables too). One shape for everything; but potions lose stacking, the inventory and saves grow, and ids need a generator in a deterministic Core.
  - **C. A rolled piece becomes a generated `ItemDefinition`** ("bronze_sword#3f2a") kept in a save-side catalog. The fewest code changes; but rolls are flattened into bonuses (no affix lines, no reroll), a catalog record stops being content (ADR 0011), and a changed base never reaches saved pieces.
  - **D. A piece is a base id and a seed; its rolls are worked out from the seed and content on load.** Tiny saves; but any change to the pool or the rarity table silently rewrites every owned piece, and crafting needs seeds that chain.
  - Also records: one row per piece, no cap (decision 345); a worn piece keeps its rolls wherever it moves (ADR 0003's move model); a saved affix is its id and value, its stat resolved from content on load; saves format 5 (ADR 0022).
- **0030 Loot rolls** (before D1).
  - **A. Rarity is a closed Core enum with each rolled tier's weight and affix count range in `rules.json`; affixes are a content pool (`affixes.json`: a stat kind, a range, the kinds it rolls on, a weight); drop tables are their own content file referenced by enemies; loot rolls in `GameSession.FinishBattle` with the session's `IRandom`, drawing only when uncertain, and returns beside the rewards (decided).**
  - **B. Rarities as content** (a list with names and colors). Open-ended; but the screens' colors and names, and the save shape, name each rarity anyway, as ADR 0027 found for damage types.
  - **C. Affixes in code** (an enum of affixes with ranges as constants). Type-checked; but every balance change is a rebuild, against ADR 0019.
  - **D. Loot rolled inside `BattleState`** (on each enemy's fall). Drops could show mid-battle; but battle draws would shift every seeded battle test, and `BattleState` (269 lines) grows a loot rule.
  - Also records: Unique is reserved and never rolls (decision 339); the four affix-only stats are stat kinds like the others (affix stats plan); drop luck acts on drop chances and rarity weights (decision 349).
- **None needed for:** the pages' layout (Game look), the save bump (ADR 0022 decides it), the test splits.
