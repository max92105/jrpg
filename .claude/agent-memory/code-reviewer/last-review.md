---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M3c (Dialogue steps)
Round: 1 (verdict: Ready to commit; build 0 warnings, format exit 0; Game test host flaked twice in 2 full runs (host crash "error: 1" with 866 total; then ExplorationSceneTests.Update_DuringWarpFadeIn_DoesNotWalk failed inside Start), 5 clean Game reruns of 98 tests)

- open (Suggestion): EventPresenter PagesOf default arm throws for an impossible step; could build pages in the Update switch arms (DialogueStep d => d.Speech.Lines) and drop the extra type test
- open (Suggestion): plan M3c slice note: tick checkbox, record BelowTalkingChest fixture (no map has a dialogue), extra tests CurrentPage_New_IsFirstPage / Confirm_AfterFinished_Throws (DoD 5)
- open (Suggestion): page-2 text after first Confirm is unobservable in tests (DialogueBox has no text accessor); covered only by SpeechPlayback unit tests
- follow-up: flaky Game test host now also fails in non-font tests (ExplorationSceneTests.Start) - native OOB read in FontLibraryTests corrupts the shared host
- follow-up: a long dialogue line overflows the 240 px box (no paging by height); M4 or later
- verified: consecutive steps reset _reading before ContinueEvent; first update never consumes Confirm; Dialogue folder has no Core dependency; validator rejects 0-line dialogue so CurrentPage is safe
