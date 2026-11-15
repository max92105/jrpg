# WS2. A page's session

**Stage 3.** Needs ADR 0060.

**Goal:** the Database mode's rules, headless: a list of catalog pages, the shown one started from the draft, Apply as
one undo step after which the page starts again on the same record, Revert, closing and reopening on the last page and
record. Nothing shows yet (WS5). Design: [design.md](../design.md), section 3 (signatures, the undo rule); decisions
918, 921, 922.

## New types (all in `Workspaces/`)
- `ICatalogEditing<TEdited>`: starts a page's edited catalog from the draft and applies it as one undo step.
- `CatalogPage`: one entry of the Database list: its title, and how its session starts (`Of<TEdited>(title,
  editing)`, `Start(chosenKey)` choosing the key when given).
- `PageSession`: one shown page's edited catalog with its apply.
- `DatabaseWorkspaceViewModel`: the Database mode's state: `Pages`, `ShownPage`, `Session`, `Open()`, `Close()`,
  `ApplyCommand` (greyed while `Session.Edited.Problem` is set; re-raised on the edited catalog's `Problem` change),
  `RevertCommand` (starts the session again from the draft, the same record chosen). It listens to `CatalogDraft`'s
  `Current` and, while a session is shown, starts it again with the old `ChosenKey`: its own Apply is such a change.

## Changes
- Every editing view model a page has (`ShopEditingViewModel`, `QuestEditingViewModel`, `DropTableEditingViewModel`,
  `EnemyEditingViewModel`, `EffectStripEditingViewModel`, `CinematicEditingViewModel`,
  `FieldSpriteEditingViewModel`, and those rows 7-10 added): declares `ICatalogEditing<TEdited>` (their `Start` and
  `Apply` already match; one line each).

## Tests
New file `tests/Jrpg.Editor.Tests/Workspaces/DatabaseWorkspaceViewModelTests.cs` (glob it first). Fixture built in the
test: a `CatalogDraft` over the test world with two shops built in the test (`armory`, `smithy`, items picked by id
from the draft) set through `SetShops`, and one page, `CatalogPage.Of("Shops", new ShopEditingViewModel(draft))`.
- `DatabaseWorkspaceViewModelTests: Open_FirstVisit_ShowsTheFirstPage`
- `DatabaseWorkspaceViewModelTests: Open_FirstVisit_StartsItsSessionFromTheDraft` (the edited list holds `armory`)
- `DatabaseWorkspaceViewModelTests: Apply_TypedName_IsOneUndoStep`
- `DatabaseWorkspaceViewModelTests: Apply_TypedNameThenUndo_PutsTheOldNameBack`
- `DatabaseWorkspaceViewModelTests: Apply_AddedShop_StartsAgainOnItWithItsIdFixed` (`IsNew` false, chosen key its id)
- `DatabaseWorkspaceViewModelTests: Apply_Unchanged_RecordsNothing`
- `DatabaseWorkspaceViewModelTests: ApplyCommand_ProblemShown_IsGreyed`
- `DatabaseWorkspaceViewModelTests: ApplyCommand_ProblemTyped_RaisesCanExecuteChanged`
- `DatabaseWorkspaceViewModelTests: Revert_TypedName_StartsFromTheDraftOnTheSameShop`
- `DatabaseWorkspaceViewModelTests: Close_Shown_DropsTheSession`
- `DatabaseWorkspaceViewModelTests: Open_AfterClose_ReopensOnTheChosenShop` (`smithy` chosen before closing)

## Size
About 120 lines of code, 110 of tests: about 230. Split point if needed: the restart on a draft change (with
`Apply_AddedShop_...`) second.

## Verify by hand
None: nothing shows yet. The run is the tests and `verify.ps1`.
