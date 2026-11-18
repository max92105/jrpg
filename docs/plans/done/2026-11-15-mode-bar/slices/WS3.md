# WS3. Leaving a page applies it first

**Stage 3.** Needs WS2.

**Goal:** showing another page or World mode applies the shown page first; a page whose problem stops Apply, or whose
Apply throws, refuses the move and names why. A draft change while a page is shown starts it again from the draft.
Design: [design.md](../design.md), section 3 (the undo rule's first two rows); decisions 919, 921, 922, 939.
- **Split:** the main window building the workspace and the mode switch going through it moved to
  [WS3b](WS3b.md), with the notes from WS1 and the WS2 review; this slice is the workspace's rule, headless.

## New types
None.

## Changes
- `Workspaces/DatabaseWorkspaceViewModel`:
  - `bool TryApplyShown()`: applies the shown session (nothing shown: true); with a problem, false and `Refusal` set
    to "The {title} page has a problem: {problem} Fix it or press Revert."; when Apply throws
    `InvalidOperationException` or `ContentValidationException`, false and "The {title} page can't apply: {message}"
    (decision 939); `Refusal` is cleared by the next move that passes.
  - `[RelayCommand] ShowPage(CatalogPage page)`: `TryApplyShown`, then the page shown on its remembered key. It takes
    any page (the list passes its own; tests show pages built in the test).
  - `Close()` becomes `bool TryClose()`: refused as above, else the session ended and its key remembered.
  - `[ObservableProperty] string? Refusal`.

## Tests
New file `tests/Jrpg.Editor.Tests/Workspaces/DatabaseWorkspaceLeavingTests.cs`. Fixture built in the test: WS2's draft
with `armory` and `smithy`, the Shops page, and two pages built in the test over a small `IEditedCatalog` test class
(`NotesCatalog`, whose `Problem` the test sets): `NotesEditing`, whose applies are counted, and `ThrowingEditing`,
whose `Apply` throws `InvalidOperationException("broken")`.
- `DatabaseWorkspaceLeavingTests: TryApplyShown_TypedName_AppliesIt`
- `DatabaseWorkspaceLeavingTests: TryApplyShown_Problem_RefusesNamingThePageAndItsProblem` (asserts the whole text)
- `DatabaseWorkspaceLeavingTests: TryApplyShown_ApplyThrows_RefusesNamingItsMessage` (asserts the whole text)
- `DatabaseWorkspaceLeavingTests: TryApplyShown_NothingShown_Passes`
- `DatabaseWorkspaceLeavingTests: ShowPage_TypedName_AppliesItFirst`
- `DatabaseWorkspaceLeavingTests: ShowPage_Problem_KeepsTheShownPage`
- `DatabaseWorkspaceLeavingTests: ShowPage_AfterARefusal_ClearsTheRefusal`
- `DatabaseWorkspaceLeavingTests: TryClose_Problem_KeepsTheSession`
- `DatabaseWorkspaceLeavingTests: DraftChanged_UndoWhileShown_StartsAgainOnTheSameShop`
- `DatabaseWorkspaceLeavingTests: DraftChanged_ChosenShopUndoneAway_ChoosesTheFirst`
- `DatabaseWorkspaceLeavingTests: DraftChanged_Closed_StartsNothing`

## Verify by hand
None: nothing calls the workspace yet (WS3b). The run is the tests and `verify.ps1`.
- **Note from WS2:** the restart on a `Current` change is in (WS2's `OnDraftChanged`), with WS2's tests
  `DraftChanged_Closed_StartsNothing` (taken from this list) and `MarkSaved_TypedName_KeepsTheSession`; WS3 adds the
  undo tests. Closing keeps `ShownPage` and remembers each page's chosen key in `_chosenKeys` (decision 1007), so
  `ShowPage` reads it. `Close`, `Apply` and `Revert` assume a shown session (`Session!`): `TryClose` and
  `TryApplyShown` take "nothing shown" from there.
- **Done, with deviations:** split (decision 1008): the shell's wiring, its three `MainViewModelWorkspaceTests` and the
  notes from WS1 and the WS2 review moved to [WS3b](WS3b.md). One test page instead of two: `NotesEditing` counts its
  applies and throws its settable `Thrown` (so a refusal can be followed by a move that passes with nothing else
  changed); `ThrowingEditing` not written. `Refusal` is also cleared by a problem change and a new session (decision
  1008). 16 tests instead of 10 here (`DraftChanged_Closed_StartsNothing` was WS2's): added
  `TryApplyShown_Problem_AppliesNothing`, `TryApplyShown_ContentRefused_RefusesNamingItsMessage` (the second caught
  type), `ShowPage_PageLeftBefore_ReopensOnItsChosenShop`, `TryClose_NothingShown_Passes`,
  `ProblemChanged_AfterARefusal_ClearsTheRefusal`, `Revert_AfterARefusal_ClearsTheRefusal`. WS2's `Close` tests call
  `TryClose` (`Close_Shown_...` renamed `TryClose_Shown_...`). `DatabaseWorkspaceViewModel` 166 lines,
  `DatabaseWorkspaceLeavingTests` 228, `NotesCatalog` 24, `NotesEditing` 26.
