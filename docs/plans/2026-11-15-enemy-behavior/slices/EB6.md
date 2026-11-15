# EB6. The Enemies page keeps and applies an enemy's rules

Editor, about 150 lines (about 50 of code, 6 new tests, about 30 lines of re-pinned or deleted tests). Needs EB5, row
6's EP and the mode bar's WS7 (all three edit the enemy's view model or its page): re-read `EnemyViewModel.cs`,
`EnemyCatalogViewModel.cs` and the page's XAML first. Design: design.md, sections 2.7 (the replacement), 6 (test churn,
equality).

## Goal
Each enemy on the Enemies page holds its rules as edited: applying writes them, Duplicate copies them as typed, the
problem line names a rule's box, and a session that changed nothing still records nothing. The Stats tab's "Tactics and
phases: edited in enemies.json" line goes.

## Changes
- New partial `Editor/Battle/EnemyViewModel.Tactics.cs` (`EnemyViewModel.cs` is at 188 plus EP's lines): `public
  EnemyTacticsViewModel Tactics { get; }` built from the record, its `Changed` relayed as
  `OnPropertyChanged(nameof(Tactics))` plus `Problem`; `private void CopyTacticsFrom(EnemyViewModel chosen)`.
- `Editor/Battle/EnemyViewModel.cs`:
  - the constructor builds `Tactics`; `DuplicateOf` calls `CopyTacticsFrom` (the boxes as typed, so a half-typed rule
    duplicates too);
  - `Problem` ends with `?? Tactics.Problem` (after `Art.Problem`);
  - `ToEnemy` sets `Tactics = Tactics.ToTactics(), Phases = Tactics.ToPhases()`;
  - `TacticsSummary` goes; the class summary drops "What no form edits yet (its tactics and phases) is carried from the
    record" (grep `_original with`: what is still carried is what the art's `ApplyTo` doesn't set); the duplicate's
    summary says "its rules as typed".
- `Editor/Battle/EnemyCatalogViewModel.OnEnemyChanged`: unchanged (a `Tactics` change reaches it as a typed field);
  its comment names the rules.
- The Enemies page's XAML (WS7's `Battle/EnemyCatalogPage.xaml`): the `TacticsSummary` line goes (EB7 adds the tab).
- `docs/map.md`, Editor `Battle` row: "what no form edits carried from its record" loses tactics; `EnemyViewModel`'s
  `.Tactics` partial.

## Tests
- `Editor.Tests/Battle/EnemyViewModelTests` (165 lines, about 175 after):
  - re-pin `From_*` (lines 40-42): `Assert.Same(Chief.Tactics, ...)` becomes `Assert.Equal` over the actions, with a
    comment: the rules are now rebuilt from the rows;
  - delete `TacticsSummary_ThreeActionsOnePhase_CountsThem` (the line is gone);
  - `DuplicateOf_GoblinChief_CopiesTacticsAndPhases` stays, now through the rows;
  - `ToEnemy_RuleWeightTyped_BuildsTheNewWeight` (a weight unlike the record's);
  - `Problem_RuleWeightNotANumber_NamesTheRule`;
  - `DuplicateOf_HalfTypedRule_CopiesItAsTyped`.
- `Editor.Tests/Battle/EnemyCatalogViewModelTests`: `Problem_RuleTyped_IsRaised` (a rule's weight typed on the chosen
  enemy raises the catalog's `Problem` and `CanApply`).
- `Editor.Tests/Battle/EnemyEditingViewModelTests` (64 lines, as WS7 leaves it): `Apply_UntouchedSession_RecordsNothing`
  (start, apply at once: `CanUndo` false, every rule rebuilt from its rows); `Apply_RuleTargetChanged_IsOneUndoStep`.

## Verify by hand
1. Run the editor, Database > Enemies, choose `goblin_chief`, leave the page or apply it unchanged: nothing shows as
   unsaved, and Ctrl+Z takes back whatever was done before, not the page.
