# CR2. The test world's intros play cinematics

Tests only, about 150 lines. Needs CR1. Decision 621; design risk "The test world"; ADR 0037.

## Goal
No test depends on the `still` step any more, so CR3 can delete it: the frozen test world's intro map plays cinematics
from a frozen `cinematics.json` of its own (the live file is the developer's and changes every day), and the tests that
build still steps build cinematic steps. Tests of the `voice` step stay (it stays, decision 621).

## Changes
- `tests/TestWorld/cinematics.json` (new, frozen): one placeholder cinematic per leader the test world's intro gates
  (CR1's eight shapes plus a placeholder Ragnir one, never the developer's `ragnir_intro`).
- `tests/TestWorld/TestWorld.props`: `cinematics.json` joins the excluded live files and the frozen one is copied in;
  `tests/TestWorld/README.md` names it among the frozen files.
- `tests/TestWorld/map-layouts/intro.json`: each intro event's three `still` steps become one `cinematic` step; its
  `voice` step stays.
- Tests building still steps move to cinematic steps (Grep `StillStep|"still"|BelowChestShowingStills|IsShowingStill`
  in `tests/` first; on 2026-11-14 the wider still-or-voice grep found 87 lines in 19 files):
  - `StartMapScenarios.BelowChestShowingStills` and its users (`EventPresenterStillsTests`, `EventPresenterSoundTests`,
    `EventPresenterFadeSoundTests`, `EventPresenterTests`): a case `EventPresenterCinematicTests` already covers (lead-in,
    lead-out, warp after) is deleted with `EventPresenterStillsTests`; the others are rewritten on
    `BelowChestPlayingCinematic` (the warp out of a cinematic's black stays silent, decision 434). The press-to-skip
    cases go with the still step (decision 625).
  - `ExplorationArrivalTests`, `ExplorationMoveNpcTests`, `EventRunQuestTests`, `MapFolderReaderTests`,
    `CellEventViewModelTests`, `EventListViewModelTests`: their still steps become cinematic steps, or another
    presentation step where the test only needs one that waits; their voice steps stay.
  - `EventRunTests`' still cases: those `EventRunCinematicTests` covers are deleted; the voice cases stay.
  - `TestWorldTests` (Data): the frozen-files list names `cinematics.json`.
- The tests about the still step itself (`MapEventValidatorTests`' still rows, `EventStepViewModelTests`' still cases,
  `StillPlaybackTests`, `ArtReferenceCheckTests`' still-step case) stay until CR3 deletes them with their code.

## Tests
Runner totals per project before and after, and the moved test names, in the slice's notes (a test-move guard).

## Verify by hand
None: tests only.

- **Done, with deviations:** the frozen `tests/TestWorld/cinematics.json` (nine placeholder records, Ragnir's on the placeholder stills too) and the intro map's cinematic steps are written by the same script as CR1's. `ContentCatalogLoaderCinematicTests` and `ContentWriterCinematicTests` add their cinematics beside the frozen ones (the test world's intro plays them, and a save checks every map). `EventPresenterStillsTests` is deleted with the still step's press to skip (decision 625; its lead-in, lead-out and warp cases are in `EventPresenterCinematicTests`) and `StartMapScenarios.BelowChestShowingStills` with it; the intro tests in `EventPresenterTests` and `EventPresenterFadeSoundTests` mark the pictures loaded (a headless test never draws), and those and `GameFlowNewGameTests` skip with the hold (which works while the pictures load). CR2's review: `ExplorationArrivalTests` names its fixture's cinematics as such. `MapFolderReaderTests`' still case stays for CR3: it reads the still step itself. New: `TestWorldTests.Load_TestWorld_PlaysItsOwnFrozenCinematics`. Totals: Core 1303 → 1303, Data 895 → 896, Game 1481 → 1472 (the nine still-press tests), Editor 1549 → 1549.
