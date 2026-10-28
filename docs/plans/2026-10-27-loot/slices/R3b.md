# R3b. Dropped gear rolls its rarity and affixes

Part of [the loot plan](../README.md). ADR 0030, decision 344. Design: "Loot". After S1b (saves keep and repair rolls before rolled pieces appear in play).

- **R3b. Dropped gear rolls its rarity and affixes** (Core + content + tests, about 180 lines: about 120 for the slice, about 60 budgeted for the seeded tests the new draws shift)
  - **Goal:** a piece of gear that drops is rolled through `GearRoll` with the content's pool and table; starting items and map gifts stay plain (decision 344). Rarity isn't shown in color until P1a; the Equipment comparison already shows what the affixes give.
  - **Changes:**
    - `Loot/DropRoll` becomes an instance built with the items, a `GearRoll` and the random (`Roll(tables)`), so the affix stats plan's drop luck fits as a second parameter; an equipment entry gives `gearRoll.Roll(item)` per copy.
    - `Session/GameSession`: builds the `GearRoll` and the `DropRoll` once from `catalog.Affixes`, `catalog.Rules.Loot` and the session's random, and rolls at `FinishBattle`.
  - **Tests:**
    - `DropRollTests: Roll_Equipment_RollsThroughGearRoll` (a Rare-only table: the dropped piece has two or three affixes)
    - `GameSessionDropTests: FinishBattle_GearDrop_HasRarityAndAffixes` (a `FixedSequenceRandom` pinned through the drop chance, the rarity, the count and the affixes)
    - `RepositoryLootTests: GoblinGearDrop_RollsUnderContentRules` (seeded; asserts the piece's affixes all roll on its kind, not a total)
  - **Call sites:** `DropRoll.Roll` in `DropRollTests` (6, D1): build the `DropRoll` with a `GearRoll` over a Common-only table, so they stay plain.
  - **Seeded tests to recheck** (the budgeted 60 lines): the Core and Game tests D2 fixed for drops after a repository victory (`GameSessionBattleTests`, `BattleScenarios` users, the `GameFlow` battle tests): a gear drop now adds rarity, count and affix draws after the drop chance. Each fix asserts what the test needs rather than re-pinning a later draw; write the list in the Done note.
  - **Verify by hand:** win until a piece drops; on the Equipment page its Changes band shows its affixes' stats (a Magic Bronze Sword with +3 Power shows Power and the damage it raises). Save, quit, load: the same piece is there.
- **Note from R2 review:** `ContentCatalog.Affixes` is a dictionary, whose `Values` order is not the file's: give `GearRoll` the pool in a fixed order (sorted by id), so a seed always picks the same affixes.
- **Done, with deviations:**
  - `GameSession` builds its `DropRoll` once with a `GearRoll` over the pool sorted by id (the R2 review's note) and the content's rarity table, both on the session's random.
  - `RepositoryLootTests.GoblinGearDrops_EachHasAffixesForItsKind` checks the content instead of a seeded roll: `GearRoll` is internal to Core and the Data tests can't see it. It asserts what the seeded test was for: every gear item the goblin drops has pool affixes for its kind.
  - The test catalog's rarity table stays Common-only; the session test that wants rolled drops copies the catalog with a Magic-only table and an affix. `DropRollTests`' six cases go through a Common-only `GearRoll`.
  - No seeded test needed fixing: no repository test wins a battle with a gear drop and then reads the random again.
  - Tests 2698 → 2701.
