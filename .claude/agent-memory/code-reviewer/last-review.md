---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK6 (Effects/TierChoice, Skills/SkillEffectSection, Effect section on
SkillCatalogPage; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit.

- Confirmed (implementer's question): untouched open/OK adds no undo step. Effect.Equals compares Tier by value and
  Statuses by sequence; ToEffect only zeroes power on kinds Data lets carry one (grantStatus/readyExtraTurn), and every
  repository skill has power 0 there; Apply_Untouched_RecordsNoStep pins it for bolt.
- Probed (PowerShell -STA): nullable-enum Damage type box shows blank for null, no write-back on context switches or
  close; Tier box (SelectedValuePath over the page's shared list) writes nothing back on switches.
- Suggestion (open): greyed Damage type keeps showing the old type after Kind leaves damage (written null).
- Suggestion (open): IsTiered public with no binding; CanTypeKind is the same value.
- Suggestion (open): SkillCatalogViewModel._firstDamageTier recomputes from content what Choices.Tiers lists.
- SK5 items: ResourceChoice and SkillUsers id order fixed; Apply_Untouched_RecordsNoStep added (fixed); ListOrder
  follow-up in backlog, dropped.
