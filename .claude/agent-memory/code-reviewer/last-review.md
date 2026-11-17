---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG1 (map names its battle background; encounter's own wins).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): src/Jrpg.Data/Art/ArtReferenceCheck.Backgrounds.cs:15,21 — `BackgroundFile!` /
  `BattleBackgroundFile!` after a Where; a pattern-matching Select would drop the null-forgiving operators.
- Fixed (carried from IU17): StatusModifiersTests.cs:67 now names decision 886.
- Checked, no finding: PendingEncounter is set only by the random roll (ExplorationState.cs:265), so a map without
  random battles never reaches a null background; NewMapViewModel's `new MapSettings` has no table, so no background is
  needed; the interim editor gap (ticking random battles with no box) is BG2's problem line.
