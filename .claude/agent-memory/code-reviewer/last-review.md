---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/M2b.md, slice M2b (the mage's spells cost mana; the list shows it)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: BattleSkillChoice.Actor has no doc and is only read for its Resources; document it or expose only what the panel needs.
- Suggestion, open: SkillCostText.CostOf shows only the first negative change; a skill paying two resources (or mana plus MP) shows half its cost. Fine until such a skill exists.
- Follow-up, open: M2a's repeated-resourceId-per-skill validator warning is still unaddressed.
