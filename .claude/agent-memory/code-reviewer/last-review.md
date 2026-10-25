---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/M2a.md, slice M2a (skills can spend a class resource)
Round: 1 (verdict: Ready to commit)

- Warning, open: SkillCatalogValidator.cs FindResourceChangeProblem — one skill listing the same resource twice (mana -3, mana -2) passes; CanPay checks each change alone, so with 4 mana the skill is usable and costs 4 (clamped) instead of being unusable. Reject a repeated resourceId per skill.
- Suggestion, open: SkillPaymentTests — no test that a gain at max reports no ResourceChanged (the `applied != 0` branch).
