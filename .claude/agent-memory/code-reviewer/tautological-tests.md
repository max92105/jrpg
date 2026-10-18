---
name: tautological-tests
description: Recurring in Core/Data reviews - tests whose assertion can't fail (count==count, "same seed twice gives same result" that passes even with the shuffle removed)
metadata:
  type: feedback
---

Seen in slice 4b (`Select(...).ToList().Count == source.Count`) and 5a (`TurnOrder_Constructor_SameSeed_ProducesSameOrder` and `SeededRandom_..._SameSeed_ReturnsSameSequence` compare two runs with each other, so they pass with no shuffle at all and don't catch a .NET `Random` algorithm change).

**Why:** The plan lists "same seed -> same result" tests, and they read well, but they only prove the code is deterministic, not that it does what it should. The plan's Determinism risk says seeded expectations are a baseline, so pin them.

**How to apply:** For any seeded test, ask "would this pass if the randomness were removed or the algorithm changed?" If yes, Warning: pin golden values (a specific order/sequence for the seed). Related: [[records-with-lists-equality]].

Seen again in 5b (`Execute_SeededBattleLoop_EndsInVictory`): stats make the hero win under any seed, and the one-member party makes `EnemyTactics`' seed irrelevant, so "Seeded" in the name promises something the test doesn't check. Also an unbounded `while (!battle.IsOver)` hangs instead of failing on a turn-advance bug; ask for a turn cap.

Seen again in 6a (guard-clause variant): `ThrowIfIllegalTargets`' `Distinct` check, `CanUse(item)`'s party-side check and `ApplyToTargets`' `wasAlive` guard could each be deleted with every test still passing, and the revive test asserted only `events[1]`. For each deliberate rule the caller lists, ask "which test fails if I delete this clause?"

Seen again in slice 7 (identical-inputs variant): `Start_PartyAndEnemies_CreatesCombatantsOnBothSides` builds three identical goblins, so "in encounter order" can't be checked; and the simulation's only assertion ("every battle ends") would still pass if the party never acted, since win rate is 0 %. When a test claims an order, check the inputs are distinguishable.

Seen again in slice 8: `FieldItemUse.CanUse`'s `item.Effect is not null` clause can be deleted with every test passing (no test uses a menu-usable item without an effect, and the validator doesn't forbid one).

Seen again in part 2 slice 1b (converter): `TileConversion` rejects Sprite, Frames or CollisionPolygon but only CollisionPolygon is tested; `LegacyFileReader`'s strict options (unmapped members disallowed) have no test. Multi-clause `||` guards and serializer option flags are the usual blind spot: ask for a theory per clause.

Seen again in the 1b re-review (fixed the clauses above): `id <= EmptyTileId` tested only with 0, so `==` would pass; and `Convert_RepositoryAssets_WritesTilesAndReport` only asserts `File.Exists`. Boundary comparisons need a value past the boundary; file-writing tests need a content check. For generated content, ask for a drift test (convert to temp, compare to the committed `content/generated/`).

Seen again in 1c and 1d (converter error branches): each slice adds `ConversionException` throws "none exist today" (meta, duplicate legacy id, null object) and tests only some. In 1d raised as one Critical listing the untested throws. Grep every new `throw` in the diff and match it to a test message.

Seen again in part 2 slice 4a (fixture variant): `ContinueEvent_WarpOntoTouchCell_DoesNotFireArrivalCell` warps to town (3, 2), but the fixture's touch cell is (3, 3), so it never arrives on a touch cell. When a test name claims a scenario, trace the fixture coordinates and check the scenario actually happens.

Seen again in part 2 slice 5b: `Read_MissingRequiredField_ThrowsNamingTheField` renames a key, so it tests the unmapped-member error, not the missing field; `CheckNoNullEntries` (5 `||` clauses) tested on one list only.

Seen again in 11g (oracle variant): `ObjectBrowserViewModelTests` asserts the list equals the same `OrderBy(Category).ThenBy(Id)` the production code runs, so it passes for any sort bug shared by both. Pin a golden prefix instead (it did, with `Take(2)`).
