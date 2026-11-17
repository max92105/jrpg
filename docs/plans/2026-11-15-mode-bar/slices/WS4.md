# WS4. Undo, Redo, Save, close and the dialogs apply the page first

**Stage 3.** Needs WS3.

**Goal:** the rest of the undo rule's entry points: Undo and Redo, Save and Save all, the close question (and the
unexpected-error box, which reads the same unsaved line), and every `CatalogDialog` opened while Database is shown
apply the shown page first, or are refused naming why. After this slice no entry point reachable in Database mode can
change the draft under typed work. Design: [design.md](../design.md), section 3 (the undo rule's table); decisions 919,
920, 923, 924, 939.

## New types
None.

## Changes
- `Shell/CombinedHistory.cs` (221 → about 231): the constructor takes the `DatabaseWorkspaceViewModel` (param doc: "the
  Database page whose typed changes an undo or redo applies first"); `Undo` and `Redo` start with `if
  (!_database.TryApplyShown()) { Refusal = _database.Refusal; return; }`, before the journal check: an undo then takes
  back the step just applied; a redo with typed changes only applies them, the new step dropping the redo (decision
  920). The class summary names the second job.
- `Shell/MainViewModel.cs` (recount): `History` is built after `DatabaseWorkspace` (the line moves). While the content
  didn't load neither exists.
- `Shell/MainViewModel.Saving.cs` (197 → about 222): `Save` and `SaveAll` start with a shared `TryApplyShownPage()`
  that sets `SaveError` to the refusal plus " Nothing was written." and stops; `ConfirmClose` runs `TryApplyShown` once
  before the question (its result ignored: a page that can't apply is then listed), and Don't save closes without
  running it again; `UnsavedChanges` adds `DatabaseWorkspace.UnappliedPage` when set.
- `Workspaces/DatabaseWorkspaceViewModel`: `string? UnappliedPage` ("the Shops page, not applied: {problem or
  message}"; null with no session or when the last apply passed), kept from the last `TryApplyShown`.
- `Shell/MainWindow.Database.cs`: `OpenInDialog` returns at once when the Database workspace exists and its
  `TryApplyShown` is false (a dialog's OK changes the draft, which would start the shown page again); what it needs
  from the calling page is read before that call (decision 940). Removed in WS6 with the dialog.

## Tests
New file `tests/Jrpg.Editor.Tests/Shell/MainViewModelPageUndoTests.cs`; the shop `armory` built in the test and set on a
copied test world's draft, then Database shown and its name typed through `DatabaseWorkspace.Session.Edited`:
- `MainViewModelPageUndoTests: Undo_TypedName_TakesItBackAsOneStep`
- `MainViewModelPageUndoTests: Redo_AfterUndoOfTypedName_BringsItBack`
- `MainViewModelPageUndoTests: Redo_TypedChangesPending_AppliesAndDropsTheRedo` (an undone step first; after the redo
  the typed name is applied and `RedoCommand` greys)
- `MainViewModelPageUndoTests: Undo_PageWithProblem_RefusedNamingThePage`
- `MainViewModelPageUndoTests: Undo_PageWithProblem_KeepsWhatWasTyped`
- `MainViewModelPageUndoTests: Redo_PageWithProblem_RefusedNamingThePage`

New file `tests/Jrpg.Editor.Tests/Shell/MainViewModelPageSavingTests.cs` (copied test world, as
`MainViewModelSavingTests`); the throwing page is WS3's `ThrowingEditing`, built in the test and shown through
`DatabaseWorkspace.ShowPageCommand`:
- `MainViewModelPageSavingTests: Save_TypedName_WritesIt` (reads `shops.json` back)
- `MainViewModelPageSavingTests: Save_PageWithProblem_WritesNothingNamingThePage`
- `MainViewModelPageSavingTests: Save_ApplyThrows_WritesNothingNamingThePage`
- `MainViewModelPageSavingTests: SaveAll_PageWithProblem_WritesNothingNamingThePage`
- `MainViewModelPageSavingTests: UnsavedChanges_PageWithProblem_NamesThePage`
- `MainViewModelPageSavingTests: ConfirmClose_TypedName_AsksAndSaveAllWritesIt`
- `MainViewModelPageSavingTests: ConfirmClose_PageWithProblemAndSaveAll_StaysOpen`
- `MainViewModelPageSavingTests: ConfirmClose_PageWithProblemAndDiscard_Closes`
- `MainViewModelPageSavingTests: ConfirmClose_ApplyThrows_DiscardCloses`

The existing `CombinedHistory*Tests`, `MainViewModelUndoTests` and `MainViewModelSavingTests` pass unchanged (no
session is shown in World mode).

## Size
About 70 lines of code, 160 of tests (the saving tests write files): about 250 with the test fixtures shared from WS3.

## Verify by hand
Still nothing typable in Database mode (WS5). In Database mode Database > Shops... opens its dialog (the Shops page
in the workspace has nothing typed); undo, redo and save work as before in World mode. The run is the tests and
`verify.ps1`.
- **Note from WS1:** the pages' art imports (Import... on Enemies, Effect strips, Battle backgrounds and Character
  sprites; Strip from sheet cells... on Effect strips) are file copies, not draft steps (`Catalogs/ArtImageImport`,
  `MainViewModel.ImportCellStrip`), so they need no apply first; an imported effect GIF's pace and a background GIF's
  record land in the page's own session. `OpenInDialog` serves all thirteen Database entries and Character sprites
  until WS5, and `EditDropTables` the Enemies page's Edit...; the encounter undo refusal (EC4, decision 907) is one
  more catalog refusal an Undo meets after the apply.
