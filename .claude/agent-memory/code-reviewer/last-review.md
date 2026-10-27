---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/P2.md, slice P2 (StatRules, StatFormula.MaxHp, Vitals.ResizeMax, PartyMember.RestoreProgress)
Round: 1 (verdict: Ready to commit)

- W1 docs/ui-reference: four PNG deletions unrelated to P2 in the working tree; keep out of the slice commit; open
- W2 src/Jrpg.Core/Party/PartyMember.cs:48 new public Primaries has no reader and no test asserting it (recruit/restore/level-up); open
- S1 src/Jrpg.Core/Stats/Vitals.cs:72-73 "since the maximum is" reads unclearly; open (suggestion)
- F1 PartyMember.LevelUp raises by MaxHpOf(after) - Vitals.MaxHp: once P4's gear resizes MaxHp the "before" includes gear; follow-up for P4
