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
