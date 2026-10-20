---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B2a.md, B2a (backdrop, formation, background check)
Round: 1 (verdict: Ready to commit; verify all pass, 1161 tests)

- open (Suggestion): ArtReferenceCheck.CaseMismatch can report a false mismatch on a case-sensitive file system when both "Forest.png" and "forest.png" exist; check for an exact ordinal match first.
- open (Suggestion): BattleScene.SpotsOf (combatant -> spot pairing, enemy slot = index - partyCount) is untested and EnemySpot(0)'s absolute position is not pinned; could move to BattleLayout with a test.
