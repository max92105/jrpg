---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice EN2 (Editor/Battle: TypedNumber, EnemyViewModel,
EnemyCatalogViewModel, EnemyEditingViewModel; numbers kept as typed strings, CanApply as the Shops window).
2026-11-13. Round 1. Verdict: fix the Critical, then commit.

- EN1 Warnings (CatalogProblems try/catch copies, AbsentWhen* filter summaries): fixed (FirstProblem at
  CatalogProblems.cs:70; filter summaries name both reasons). EN1 Suggestions not carried (committed slice).
- Critical (open): EnemyViewModel.cs:60 DuplicateOf calls chosen.ToEnemy(), which throws while the chosen enemy has a
  Problem; DuplicateEnemy's CanExecute only checks a selection, so Add then Duplicate (empty id) or an emptied number
  box then Duplicate throws InvalidOperationException; the editor has no unhandled-exception handler. Fix: copy the
  chosen view model's typed fields over its _original; add a test.
- Suggestion (open): EnemyCatalogViewModel.cs:97 reacts to every PropertyChanged (Label, Problem too), rebuilding the
  list through OfEnemies 2-3 times per keystroke; filter on typed properties as ShopCatalogViewModel.OnShopChanged.
- Suggestion (open): TypedNumber is generic but sits in Battle/; same parse as PlacedObjectEditing.Layer (Follow-up to fold).
