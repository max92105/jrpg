---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice FX6 (Editor Statuses/StatusCatalogPage, Statuses/TimingButtonConverter
+ 4 tests; Shell wiring StatusEditing, OpenStatuses, DatabaseMenu "Statuses..."; smoke script list; map row).
2026-11-16. Round 1. Verdict: Ready to commit.

- Warning (open): OnTypedRowFocused is the third copy of the row-chooses-on-focus handler (Loot OnEntryRowFocused,
  Skills, Statuses); proposed one attached property shared by the three styles.
- Suggestion (open): the Family box's XAML comment credits text search with keeping the chosen item the typed family;
  while typing it is a prefix match; the real guard is SelectedStatus raised before Families + exact match on a set Text.
- Suggestion (open): part Kind box shows StatusPartKind's C# names (DamageEachTurn), same as the FX2b Effects Kind box.
- FX5 Suggestions: both fixed (Families_FamilyTyped_NotRaised; OfStatuses content param doc names items).
- FX2b carried Follow-up (open, developer's call): Effects page Kind box lists EffectKind enum values (RestoreHp),
  not the file's word; not in backlog.
