# GT1. `GameFlowTests` split: the new game's tests into `GameFlowNewGameTests`

Game tests, about 110 lines moved, about 20 new, no production change. Builds on decision 556.

## Goal
`tests/Jrpg.Game.Tests/Flow/GameFlowTests.cs` (321 lines) keeps the title, session and battle round trips; the steps from the title's New game to the first map move to their own class.

## Changes
- New `tests/Jrpg.Game.Tests/Flow/GameFlowNewGameTests.cs` (about 125): `[Collection(RepositoryScreenText.CollectionName)]` like its sibling, summary "From the title's New game through the leader choice and the intro to the start map." Moved: `Update_NewGameConfirmedWholeFade_HasNoSession`, `Update_CancelOnChoice_ReturnsToTitleWithoutSession`, `Update_LeaderConfirmedFadeReachesBlack_StartsLedByThem`, `Update_LeaderConfirmedFadeNotYetBlack_HasNoSession`, `Update_LeaderConfirmedFadeReachesBlack_StartsOnTheStartMap`, `Update_TestWorldIntroSkipped_LandsOnTheStartMap`, `Update_LeaderConfirmedThenOneUpdateLongerThanTheFade_StillStartsNewGame`, `Update_IntroSkipped_VoiceStops`, with their comments.
- `tests/Jrpg.Game.Tests/Flow/GameFlowTests.cs` (321 → about 215): its private `_catalog` is deleted. `StartMapScenarios.TestWorld` already loads the test world's catalog, and the class already imports `Jrpg.Game.Tests.Exploration`. `_holdingRight` and the save directory stay with the tests that use them; if a moved test reads either, it moves to the class that uses it. `FlowStarts` doesn't change.
- `docs/plans/backlog.md`:
  - the new-game flow's "Test files over 300 lines" line keeps Core's `PartyStateTests` (322; the sibling plan keeps it) and drops `GameFlowTests`;
  - the composer plan's line naming `GameFlowTests` and `EventPresenterTests` drops `GameFlowTests` only (GT3 deletes the rest).

The mechanical edit, in both classes: `_catalog` → `StartMapScenarios.TestWorld`; nothing else in a moved body.

## Tests
None new. The runner's totals per project equal before and after; the Done note lists the eight moved names.

## Verify by hand
None: tests only.

- **Done:** as planned; `GameFlowTests` ends at 214 lines (its `_catalog` field and the `Jrpg.Core.Content` and `Jrpg.Data.Content` usings gone), `GameFlowNewGameTests` at 119 with the collection attribute. The moved tests: `Update_NewGameConfirmedWholeFade_HasNoSession`, `Update_CancelOnChoice_ReturnsToTitleWithoutSession`, `Update_LeaderConfirmedFadeReachesBlack_StartsLedByThem`, `Update_LeaderConfirmedFadeNotYetBlack_HasNoSession`, `Update_LeaderConfirmedFadeReachesBlack_StartsOnTheStartMap`, `Update_TestWorldIntroSkipped_LandsOnTheStartMap`, `Update_LeaderConfirmedThenOneUpdateLongerThanTheFade_StillStartsNewGame`, `Update_IntroSkipped_VoiceStops`; the one mechanical edit is `_catalog` → `StartMapScenarios.TestWorld` in both classes; none of the moved tests reads `_holdingRight` or the save directory. The runner's totals: Game.Tests 1432 before and after (4610 in all). The two backlog lines changed as planned.
