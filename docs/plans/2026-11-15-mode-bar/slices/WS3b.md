# WS3b. The mode switch leaves the page through the workspace

**Stage 3.** Needs WS3 (split from it: WS3 is the workspace's leave rule, headless; this is the shell's wiring).

**Goal:** the main window builds the Database workspace with every page, and the mode switch goes through it: Database
opens the last page shown, World is shown only when the page applies, and Back to previous map stops when it is
refused. Design: [design.md](../design.md), section 3 (the undo rule's second row); decisions 919, 922.

## New types
None.

## Changes
- `Shell/MainViewModel.Workspaces.cs`: `DatabaseWorkspaceViewModel? DatabaseWorkspace` (null while the content didn't
  load, as the other editing view models), built by `CreateDatabaseWorkspace()` with one `CatalogPage.Of(title,
  editing)` per page in the list's order (see the note from WS1); `ShowDatabase` opens it; `ShowWorld` shows World
  only when `TryClose` passes.
- `Shell/MainViewModel.cs` (294 after WS1, recount): the constructor's line building `DatabaseWorkspace`, after the
  editing view models.
- `Workspaces/DatabaseWorkspaceView.xaml`: the placeholder also shows `DatabaseWorkspace.Refusal` in the problem brush
  (WS5 moves it into the bar).

## Tests
`tests/Jrpg.Editor.Tests/Shell/MainViewModelWorkspaceTests.cs` is 248 lines after WS1: these go in a sibling file
(glob it first), the shop `armory` built in the test and set on the draft first:
- `ShowDatabase_FirstVisit_ShowsTheFirstPage`
- `ShowDatabase_DatabaseShown_KeepsWhatWasTyped` (the WS2 review's guard)
- `ShowWorld_TypedName_AppliesIt`
- `ShowWorld_PageWithProblem_StaysInDatabase`
- `GoBackToMap_PageWithProblem_StaysOnTheMap` (the WS1 note's check)

## Verify by hand
Nothing typable in Database mode yet (WS5): Database, then World, both buttons and keys, still switch; the dialogs open
from the menu. `open-editor-windows.ps1` after the XAML line: no `ERROR`. The run is the tests and `verify.ps1`.
- **Note from WS1:** `CreateDatabaseWorkspace()` lists fourteen pages, titled as `Shell/DatabaseMenu.xaml`'s `Tag`s, in
  decision 917's groups: Story: Quests, Cinematics; Battle: Enemies, Encounters, Skills, Effects (the tiered effects of
  `effects.json`, not Effect strips), Statuses, Drop tables, Effect strips, Battle backgrounds; Items: Items, Affixes;
  Trade: Shops; Characters: Character sprites (the order inside Battle is a default, the developer's to change). WS1's
  `ShowWorld` already puts back `History.OpenMap` and `Placing.IsMapShown`: `TryClose` goes before them. Three callers
  show World first and must stop when it is refused: `OnNewMapExecuted` (`MainWindow.xaml.cs`), `OnGoToMapExecuted`
  (in `MainWindow.MapSwitching.cs`, not `MainWindow.xaml.cs`) and `GoBackToMapCommand`, a hand-built command in
  `MainViewModel.Workspaces.cs` (built lazily, since the constructor can't read `Navigation` without a nullable
  warning) whose action runs `ShowWorld()` then the navigation's Back with no check yet; it gets one, with a test.
  (Checked in WS3: the two window handlers already stop when `IsWorldShown` is false after `ShowWorldCommand`.)
- **Note from WS2 review:** `MainViewModel.ShowDatabase` has no guard for Database already shown: once it calls
  `DatabaseWorkspace.Open()`, pressing Database again would restart the session and drop what was typed but not
  applied. Return early (or grey the lit button) when Database is shown, with a test.
- **Done, with deviations:** `ShowDatabase` returns early while Database is shown (rather than greying the lit
  button); `CreateDatabaseWorkspace(content)` reads the fourteen editing view models with `!`, since the constructor
  calls it once they are built. 7 tests instead of 5, in the new `MainViewModelDatabaseWorkspaceTests` (119 lines):
  added `DatabaseWorkspace_ContentLoaded_ListsEveryPageInGroupOrder` (decision 917's order, the menu's titles) and
  `ShowWorld_FromDatabase_EndsThePageSession`. `MainViewModel.cs` 295, `MainViewModel.Workspaces.cs` 101,
  `DatabaseWorkspaceView.xaml` 15. `open-editor-windows.ps1`: every default entry opened, no `ERROR`, the test error
  caught by the net.
