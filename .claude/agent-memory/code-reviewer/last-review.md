---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice NB2 (PlacedObject.Facing, NpcFacing.Of fallback, AbsentWhenNullFilter
row, MapLayoutValidator.FindFacingProblem). 2026-11-12. Round 1.
Verdict: ready to commit.

- Suggestion (open): MapLayoutValidatorTests.cs:126 private Validate helper sits between two tests and only the new
  tests use it; the other tests call MapLayoutValidator.Validate directly.
- Suggestion (open): NB2.md "Done: as planned" while two test names differ from the slice
  (Of_OwnEventRunning_IgnoresItsPlacedFacing, Validate_TileObjectWithFacing_ThrowsNamingTheUid).
- NB1 Critical (uid compare): fixed, NpcFacing uses ReferenceEquals. NB1 Suggestion (same-uid-after-swap test): fixed,
  now Of_AnotherObjectWithTheSameUid_FacesDown.
- Not raised: PlacedObjectEditing.cs:92 keeps Facing when the object id changes to a tile object; NB3.md already plans it.
