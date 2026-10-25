---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/WL1.md, slice WL1 (warlock instability, gain/need skill row text)
Round: 1 (verdict: Ready to commit)

- Suggestion: SkillCostText.CostOf / class name now also return gains ("+2 Instability"); consider `RowTextOf` / `SkillResourceText` when next touched; open
- Suggestion: BattleStateResourceTests.cs:23 `_bolt` declaration ~150 columns; break it like `_voidRift`; open
- Closed (RG4): the RG4 findings were left with the developer at commit fbef16e. Don't re-raise them.
