---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/LR1.md, slice LR1 (lasting resources carry between battles)
Round: 1 (verdict: Ready to commit)

- Suggestion: ResourceStarts.cs:18 one-line ternary ~150 columns mixing lasting and fixed starts; split into a guard `if (resource.IsLasting)`; open
- Suggestion: no test of a lasting amount at max plus a positive perBattleFought (clamp through CarryBack); open
- Follow-up: Data schema/loader for isLasting and perBattleFought belongs to LR3 (validation should forbid lasting + maxFromSpeed)
