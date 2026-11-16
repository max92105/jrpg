---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU6 (Editor Items/ItemCategory, ItemCategories.Of, ItemViewModel with
From/New/Duplicate/ToItem, SaleLine, Problem, Changed; tests ItemFixtures, ItemViewModelTests, ItemCategoriesTests).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): theory `Shows_ByCategory_AreTheCategorysBoxes` names no member; rename
  `ShowsBoxes_EachCategory_MatchesItsFields` or split per property.
- Suggestion (open, optional): ItemFixtures.Catalog's ToDictionary throws a TypeInitializationException for every
  Items test if test-world ever gains an id like "ore"/"tonic"; prefix the fixtures' ids or document it.
- Follow-up (open): RecordListProblem reports two new records with empty ids as "X  is listed twice." (empty id
  compared equal) before their own "id must not be empty"; skip empty ids in the twice check.
- Checked, no finding: taken-ids skipped as Enemy/DropTable/Quest (Skills/Effects/Statuses pass TakenIds); IU7's
  RecordListProblem catches a new id equal to a saved one since new items list after saved ones; ToItem untouched
  equals source (ItemDefinition.Equals SequenceEqual on FixedAffixes); SaleLine is Core's floor share of the base
  price; a unique's affix gold is IU13's "Price as a piece" line (decision 871), not IU6's.
- Dropped (recorded elsewhere): AsUnique builder (backlog.md:328); CheckItems/CheckAffixes order (IU15 note).
