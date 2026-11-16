---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU7 (Editor Items/ItemCatalogViewModel, ItemEditingViewModel,
ItemCategories.StartingRecord, CatalogDraft.SetItems; Data CatalogProblems.Items partial with OfItems + moved CheckItems).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): ItemEditingViewModelTests.Apply_NothingChanged_RecordsNoStep comment says ToItem puts a
  unique's fixed affixes "in a new list"; TypedRecord is `_source with {...}`, so the list is the same reference.
- Suggestion (open, optional): ItemCatalogViewModel.Insert/RemoveItem raise Problem explicitly after setting
  SelectedItem, whose [NotifyPropertyChangedFor(Problem)] already raised it: Data's whole check runs twice per Add/Remove.
- Checked, no finding: chosen-item-first order is the slice's Goal; sort test feeds a reversed Dictionary (category
  and id order both exercised); CheckItems moved verbatim; untouched Start/Apply carries FixedAffixes by reference,
  the new-list SequenceEqual path covered by CatalogDraftItemTests; problem line passes page order where the writer
  sorts by id, as DropTableCatalogViewModel does.
- Dropped (recorded elsewhere): RecordListProblem empty-id twice (backlog.md:330); IU6's optional suggestions (other slice).
