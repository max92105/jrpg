---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/RG1.md, slice RG1 (speed stat, rogue readiness charges from speed)
Round: 1 (verdict: Ready to commit)

- Warning: ResourceDefinitionValidator `speedPerCharge < 1` branch has no test; it guards a DivideByZeroException in ReadinessFormula.Charges at battle start; open
- Suggestion: no test pins ResourceMeters.Change clamping to the start's max (not the definition's Max 0) for a speed-scaled resource; open
- Suggestion: ResourceRowsTextTests has no case where MaxOf differs from definition.Max (speed-scaled "3/3" row); open
- Follow-up: omitting `startsAt`/`max` on a fixed resource now loads as 0 instead of failing the required-parameter check (defaults added); validator catches max only
