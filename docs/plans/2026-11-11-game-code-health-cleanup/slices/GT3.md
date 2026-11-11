# GT3. `EventPresenterTests` split: eight stills tests into `EventPresenterStillsTests`

Game tests, about 100 lines moved, about 15 new, no production change. Builds on decision 556. After GD1, so the moved tests already build their presenter with `EventPresenters.Over`.

## Goal
`tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (306 lines before GD1) keeps the pages, the warp, the voice and the intro; the cutscene stills that start from a chest (their lead-in, skip and hold) get their own class, beside `EventPresenterQuestTests` and the others.

## Changes
- New `tests/Jrpg.Game.Tests/Exploration/EventPresenterStillsTests.cs` (about 115): `[Collection(RepositoryScreenText.CollectionName)]` like its sibling (FontStashSharp's font cache is not thread-safe, and the totals guard can't see a lost attribute; decision 556). Summary: "Cutscene stills over the map: the lead-in from black, each still's time, Confirm's skip and the hold before the dialogue." Moved, with `StartStills` and their comments:
  `Update_PastFirstStillsSeconds_ShowsTheSecond`, `Update_ConfirmDuringStill_SkipsToTheDialogue`, `Update_ConfirmRightAfterTheSkip_DoesNotEndTheDialogue`, `Update_ConfirmOnFirstStillStep_StartsTheLeadIn`, `Update_ConfirmDuringLeadIn_SkipsToTheDialogue`, `Update_DuringLeadIn_ShowsNoStill`, `Update_FirstStillOver_SecondStartsAtOnceWithNoBlackHold`, `Update_LastStillOver_HoldsBlackForTheLeadOutBeforeTheDialogue`.
- Stay in `EventPresenterTests`, with `StartIntro`: `Update_AfterLastStillsLeadOut_ContinuesToTheWarp` and `Update_SkipBeforeTheWarp_StaysBlackIntoIt` (they call `StartIntro`), and the voice tests.
- The five moved tests that read the private `_confirmPressed` use `InputFrames.Pressing(GameButton.Confirm)` instead (the same frame), not a copy of the field; the field stays for the tests left behind.
- `tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (about 300 after GD1 → about 205).
- `docs/plans/backlog.md`:
  - delete the class mechanics plan's `EventPresenterTests` line;
  - the quests retro's line naming `ContentWriterTests` and `EventPresenterTests` keeps Data's `ContentWriterTests` (as the sibling plan's D2 left it) and drops `EventPresenterTests`;
  - delete the composer plan's line, which after GT1 names only `EventPresenterTests`.

The edit inside moved bodies is the one above (`_confirmPressed` → `InputFrames.Pressing(GameButton.Confirm)`); nothing else changes.

## Tests
None new. The runner's totals per project equal before and after; the Done note lists the eight moved names.

## Verify by hand
None: tests only.
