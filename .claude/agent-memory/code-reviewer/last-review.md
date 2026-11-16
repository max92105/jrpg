---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice WK (Weakened: damageDealtChange, DamageModifierPermille,
DamageFormula.ModifiedFlat, Enfeeble). 2026-11-16. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: DamageFormula.ModifiedFlat summary/param (and map.md) say "the basic attack's"; it also serves
  every untiered damage skill (EffectScaling.DamageOf).
- Suggestion, open: ContentCatalogLoaderStatusTests Load_RepositoryEnfeeble_GrantsWeakened comment says "the warlock's
  Enfeeble" though it is off the warlock's skillIds.
- Suggestion, open: ModifiedFlat has no MaxPower hold, unlike TieredAmount (decision 818); harmless until row 9 adds a
  positive gear modifier.
- Fixed (from ST): StatusCatalogValidator now compares ActsAt within a family (line 107).
- Fixed (from ST): PlaybackBeatKind.Tick summary says start or end.
- Checked: ModifiedFlat at modifier 0 equals StatFormula.Scaled (same floor); enemies get it via CurrentStats in
  AttackEffect.Of and UseSkill; items bypass ScaledFor (LandFromItem); Int-strengthened Weakened past -100 clamps to 0
  then Calculate's minimum 1; all test arithmetic re-derived.
