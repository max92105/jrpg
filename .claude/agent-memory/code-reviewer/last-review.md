---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/CL1.md, slice CL1 (the cleric's faith: requirements; drops when an ally falls)
Round: 1 (verdict: Ready to commit)

- Warning, open: "Change a resource, report ResourceChanged when non-zero" is now in three places (TurnUpkeep.ApplyPerTurnGains, SkillPayment.Pay, ResourceTriggers.Gain); share one method.
- Suggestion, open: ClassCatalogValidator's (SkillId: id, ResourceId: resourceId) SelectMany line is long; a named projection or line break reads better.
- Follow-up, open: BattleState.CanAfford's doc says "the MP and the resources skill costs"; it now also checks requirements.
- Follow-up, open: M2a's repeated-resourceId-per-skill validator warning is still unaddressed (requirements may also repeat a resource id).
