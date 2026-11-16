---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice TT (a damageEachTurn part may name a damageType; burning_1 is fire).
2026-11-15. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Fixed (from SW): StatusDefinition `Strengthened` doc no longer says "rounded down".
- Follow-up, open (from SW): EffectValidator.cs:97 groups grants by Status.Id, so guard_1 + guard_2 in one effect
  pass and the second refreshes the first; FamilyKey would catch it. Not in backlog.md.
- Fixed (from SW): loader test for an unknown status id exists (ContentCatalogLoaderStatusTests, "no status 'burning_9'").
- Checked: Calculate with NormalHit and ExtraTaken 0, no block/penetration; refresh (StrongerPart) and Strengthened
  (`with`) keep DamageType; family rule now forbids mixed types so refresh can't mix; no Core namespace cycle
  (Stats doesn't use Statuses); no Editor reader of StatusPart; test numbers 8->10, 8->4, 8, 3->1, 8 verified by hand.
