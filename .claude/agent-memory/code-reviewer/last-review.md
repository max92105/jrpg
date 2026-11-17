---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU11 (equipment bonus rows: StatBonusRow, ItemEquipmentViewModel Bonuses/
Add/Remove/StatChoices, SectionRowProblem.FirstInField overload with nameOf, Catalogs/ContentSpelling). 2026-11-16.
Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): an untouched round trip with several bonuses (`_armor`, given out of order); today only the axe's
  single Power bonus is round-tripped untouched.
- Suggestion (open): stale wording made by this slice: ItemEquipmentViewModelTests.cs:8 "the bonuses carried";
  ItemViewModel.cs:13-14 "Problem is only the typed id's and the number boxes'" (a stat named twice now reaches it).
- Checked, no finding: duplicate stat never reaches ToDictionary (ToEquipment, ItemViewModel.ToItem and the catalog's
  RecordListProblem all read Problem first); StatBonuses equality is order-independent; ContentSpelling body is the old
  expression verbatim, no other ConvertName copy left in src; TypedRow style is BasedOn; both bound sides and 0 tested.
- IU10 Suggestions: not carried (developer's call, round closed).
