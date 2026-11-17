---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU12 (consumable form: Effects/EffectKindFields shared with
SkillEffectSection, Items/ItemEffectViewModel, Items/ItemForms, ItemViewModel.EffectFile, catalog EffectImages and
MissingStripNote, page Effect section). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): ItemEffectViewModel.ToEffect writes power 0 for any kind without a power, so a hand-edited
  grantStatus (or rest) record with a nonzero power, which Data accepts, differs from its source after an untouched OK.
- Suggestion (open): ItemEffectViewModel.TakesPower's doc comment speaks of a rest only; a status grant also takes none.
- Checked, no finding: SkillEffectSection behaviour unchanged (Kinds still omits rest, no skill test changed, no test
  relied on the old throw); untouched Firebomb ToItem equals source (Statuses list carried by `with`, EffectFile kept);
  KindChoices Union appends a hand-edited kind; strip box outside the Effect DataContext binds EffectFile on the item;
  unbased StackPanel style is the file's existing pattern (no theme StackPanel style).
- IU11 Suggestions: not carried (round closed; ItemViewModel summary wording now fixed by this slice).
