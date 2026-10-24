# K2c. The map shows a still step

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "The intro as a map cutscene", tunable `ShotFade`. ADR 0016 (option B).

- **K2c. The map shows a still step: covering the screen, fading in and out, Confirm or Cancel skipping** (Game, about 250 lines; decisions 159, 171, 172)
  - **Goal:**
    - While a still step runs, its image covers the screen over the map, fading in and out over `ShotFade` (at most half its seconds), with its caption as plain text at the bottom.
    - After its seconds, the event continues: the next still follows, or whatever step comes next.
    - Confirm or Cancel skips: the event runs past the presentation steps (`ExplorationState.SkipEventPresentation`). The press is the same one the player would use to read a dialogue, so a dialogue after the stills isn't completed by it.
    - The look (lead-in, caption band, skip hint) comes in K4.
  - **New types (Game):** `Exploration/StillPlayback` (pure): where a shown still is in its time.
    - `StillPlayback(StillStep still)`, `Update(TimeSpan delta)`.
    - `Opacity` (0 to 1, fading in and out over `ShotFade`, clamped to half the seconds); `IsOver`.
    - The fade tunable is a named constant with its reason.
  - **Changes:**
    - `Exploration/EventPresenter`:
      - a `StillStep` case starts a `StillPlayback` on the step's first update, measures the caption once, and continues the event when it's over;
      - Confirm or Cancel while a still shows calls `SkipEventPresentation`;
      - `Draw` covers the screen with the still (`CoverFit.DrawOverScreen`, smooth sampling), tinted by its opacity, then the caption.
    - The summary of `EventPresenter` lists stills.
  - **Tests:**
    - `StillPlaybackTests`:
      - `Opacity_AtStart_IsZero`
      - `Opacity_AfterShotFade_IsOne`
      - `Opacity_StillShorterThanTwoFades_PeaksAtHalfway`
      - `IsOver_ExactlyItsSeconds_IsTrue` (the boundary belongs to the next step)
    - `EventPresenterTests` (a test map whose interact event holds two stills, then a dialogue):
      - `Update_PastFirstStillsSeconds_ShowsTheSecond`
      - `Update_ConfirmDuringStill_SkipsToTheDialogue`
      - `Update_ConfirmThatSkipped_DoesNotTurnTheDialoguePage`
  - **Verify by hand:** covered by K3, the first content that uses stills.
  - **Done, with deviations:**
    - Black is drawn under each still for its whole step, so the map never shows while a still fades. K4 adds the lead-in, lead-out and caption band.
    - The subtitle is plain text 120 px above the bottom, fading with its still.
    - The skip test is `Update_ConfirmRightAfterTheSkip_DoesNotEndTheDialogue`: the dialogue that follows starts on the next update, which ignores the press as dialogue already does. `IsOver_JustBeforeItsSeconds_IsFalse` and `Opacity_HalfwayThroughTheFadeOut_IsHalf` were added.
    - `StartMapScenarios.WithChestSaying` takes several steps, for the new `BelowChestShowingStills`.
    - Stills are drawn through the game's point sampling, so the 640x360 placeholders show blocky at 3x. Real 1920x1080 stills draw 1:1. If real stills come at other sizes, shrink them smoothly as `UiTextures` does for portraits.
    - Review (Warning fixed): when a still ended, the next one only started on the following update, so one frame showed the bare map between stills. The next still now starts in the same update, and `Update_PastFirstStillsSeconds_ShowsTheSecond` asserts `IsShowingStill`. Also: `StillPlayback.Still` is renamed `Step`, and a still too short to fade shows fully instead of dividing by zero. The reviewer's carried-over items were already fixed in K2a and K2b.
