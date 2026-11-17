# WS3. Leaving a page applies it first

**Stage 3.** Needs WS2.

**Goal:** showing another page or World mode applies the shown page first; a page whose problem stops Apply, or whose
Apply throws, refuses the move and names why. A draft change while a page is shown starts it again from the draft. The
main window builds the Database workspace with every page, and the mode switch goes through it. Design:
[design.md](../design.md), section 3 (the undo rule's first two rows); decisions 919, 921, 922, 939.

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
- `Shell/MainViewModel.Workspaces.cs`: `DatabaseWorkspaceViewModel? DatabaseWorkspace` (null while the content didn't
  load, as the other editing view models), built by `CreateDatabaseWorkspace()` with one `CatalogPage.Of(title,
  editing)` per page in the list's order (Quests, Cinematics, Enemies, Drop tables, Effect strips, rows 7, 8 and 10's,
  row 9's, Shops, Character sprites, row 12's); `ShowDatabase` opens it; `ShowWorld` shows World only when `TryClose`
  passes.
- `Shell/MainViewModel.cs` (286 on 2026-11-15, recount): the constructor's line building `DatabaseWorkspace`, after the
  editing view models.
- `Workspaces/DatabaseWorkspaceView.xaml`: the placeholder also shows `DatabaseWorkspace.Refusal` in the problem brush
  (WS5 moves it into the bar).

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

`tests/Jrpg.Editor.Tests/Shell/MainViewModelWorkspaceTests.cs` (WS1's, about 100 → 130); the shop `armory` built in
the test and set on the draft first:
- `MainViewModelWorkspaceTests: ShowDatabase_FirstVisit_ShowsTheFirstPage`
- `MainViewModelWorkspaceTests: ShowWorld_TypedName_AppliesIt`
- `MainViewModelWorkspaceTests: ShowWorld_PageWithProblem_StaysInDatabase`

## Verify by hand
Nothing typable in Database mode yet (WS5): Database, then World, both buttons and keys, still switch; the dialogs open
from the menu. The run is the tests and `verify.ps1`.
- **Note from WS1:** `CreateDatabaseWorkspace()` lists fourteen pages, titled as `Shell/DatabaseMenu.xaml`'s `Tag`s, in
  decision 917's groups: Story: Quests, Cinematics; Battle: Enemies, Encounters, Skills, Effects (the tiered effects of
  `effects.json`, not Effect strips), Statuses, Drop tables, Effect strips, Battle backgrounds; Items: Items, Affixes;
  Trade: Shops; Characters: Character sprites (the order inside Battle is a default, the developer's to change). WS1's
  `ShowWorld` already puts back `History.OpenMap` and `Placing.IsMapShown`: `TryClose` goes before them. Three callers
  show World first and must stop when it is refused: `OnNewMapExecuted` (`MainWindow.xaml.cs`), `OnGoToMapExecuted`
  (in `MainWindow.MapSwitching.cs`, not `MainWindow.xaml.cs`) and `GoBackToMapCommand`, a hand-built command in
  `MainViewModel.Workspaces.cs` (built lazily, since the constructor can't read `Navigation` without a nullable
  warning) whose action runs `ShowWorld()` then the navigation's Back with no check yet; it gets one, with a test.
- **Note from WS2:** the restart on a `Current` change is in (WS2's `OnDraftChanged`), with WS2's tests
  `DraftChanged_Closed_StartsNothing` (taken from this list) and `MarkSaved_TypedName_KeepsTheSession`; WS3 adds the
  undo tests. Closing keeps `ShownPage` and remembers each page's chosen key in `_chosenKeys` (decision 1007), so
  `ShowPage` reads it. `Close`, `Apply` and `Revert` assume a shown session (`Session!`): `TryClose` and
  `TryApplyShown` take "nothing shown" from there.
- **Note from WS2 review:** `MainViewModel.ShowDatabase` has no guard for Database already shown: once it calls
  `DatabaseWorkspace.Open()`, pressing Database again would restart the session and drop what was typed but not
  applied. Return early (or grey the lit button) when Database is shown, with a test.
