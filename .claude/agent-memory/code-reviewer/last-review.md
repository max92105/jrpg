---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/MP1.md, slice MP1 (skills and effects stop using MP)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: CommandMenuTests' Update_UnaffordableSkill_IsGreyedAndSaysCannotAfford (Fireball) and Update_ConfirmMeteorWithThreeMana_SaysNotEnoughMana now set up the same case; fold the IsUsable/Targets asserts into one.
- Suggestion, open: docs/map.md still says SkillPayment covers "MP" and SkillCostText is "a class resource over MP".
- Closed (CL1): the "change and report when non-zero" step is one method, `Combatant.ChangeResource`; TurnUpkeep, SkillPayment and ResourceTriggers call it.
- Closed (M2a): `SkillCatalogValidator` rejects a resourceId repeated in one skill's `resourceChanges`; repeated requirements are left allowed on purpose (each is checked alone).
