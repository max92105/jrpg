# CG4. Hold Confirm 2 s to skip a cinematic

Game + content, about 220 lines (about 120 of them tests). Needs CG2 (CG3 for the voice's silence on a skip). Decisions
623, 624, 625; tunable "Hold to skip".

## Goal
During a cinematic step (its loading, lead-in and stills), holding Confirm for 2 s skips the whole cinematic: the event
runs past its presentation steps (ADR 0016), what follows shows at once, already black if it's a warp. Letting go before
2 s resets the hold and the cinematic carries on; a single press does nothing; Cancel does nothing; a hold counts only
from a press made while the step is current, so a Confirm still held from the press that started the event never skips.
The hint reads "Hold E to skip" (the device's key, as today). The ring comes in CG5; until then the hint is the plain
text. The `still` step keeps today's press to skip (decision 625).

## New types
- Game `Exploration/SkipHold` (internal sealed): how far the hold to skip has come: grows while Confirm is held, back to
  nothing on release, done at 2 s.
  - `public static readonly TimeSpan HoldTime = TimeSpan.FromSeconds(2)`, with the comment for decision 624 (no content
    place for UI timings; presentation timings are named Game constants).
  - `void Update(TimeSpan delta, InputFrame input)`: armed by `IsPressed(Confirm)`; while armed and `IsHeld(Confirm)`,
    the held time grows by the delta (capped at `HoldTime`); not held: back to zero and unarmed.
  - `float Progress` (0 to 1), `bool IsDone`.

## Changes
- Game `Exploration/CinematicRun`: a `SkipHold`, updated every `Show` from the step's second update (the first only
  starts the loading, as the lead-in did); `IsDone` calls `SkipEventPresentation()` and clears the run, the lead-out not
  played (as today's skip). `Show` takes the `InputFrame` again. `float SkipProgress` for the view.
- Game `Exploration/StillView`: a second hint, measured once per device as the first: `cinematic_hold_skip` for a
  cinematic, `cinematic_skip` for a still step (the still view is told which run draws).
- Game `Ui/MessageIds` (`.Field` partial) and `content/text/ui.json`: `cinematic_hold_skip`: "Hold {button} to skip".
- `docs/map.md`: Game `Exploration` names `SkipHold`.

## Tests
- `SkipHoldTests` (new, `tests/Jrpg.Game.Tests/Exploration/`), at `StartMapScenarios.OneUpdate` steps:
  - `Update_PressThenHeldTwoSeconds_IsDone` (120 updates held after the press).
  - `Update_HeldJustUnderTwoSeconds_IsNotDone` (119: the other side of the bound).
  - `Update_ReleasedBeforeTwoSeconds_ResetsProgress` (progress 0 after a release at 1 s).
  - `Update_HeldAgainAfterRelease_StartsFromZero` (1 s, release, 1.5 s: not done).
  - `Update_HeldWithoutAPress_DoesNotCount` (held from before: progress stays 0).
  - `Update_SinglePress_IsNotDone`.
  - `Progress_HalfwayHeld_IsHalf`.
- `EventPresenterCinematicTests` (CG2's file, or a sibling past 250):
  - `Update_ConfirmHeldTwoSeconds_SkipsToTheDialogue`.
  - `Update_ConfirmReleasedAtOneSecond_CinematicCarriesOn`.
  - `Update_CancelPressed_DoesNothing`.
  - `Update_ConfirmHeldFromTheEventsStart_DoesNotSkip` (the press that started the event held on: still playing after
    3 s).
  - `Update_HeldSkip_SilencesTheVoice` (a voiced still: the channel is silent after the skip).
- `EventPresenterStillsTests`: unchanged and passing (the still step keeps its press).
- `UiMessagesTests` (or the ids test): `cinematic_hold_skip` exists with its `{button}`.

## Verify by hand
1. New game with Ragnir: the bottom right reads "Hold E to skip" (a pad's Confirm when a pad was last used).
2. Tap E during still 2: nothing happens. Hold E about one second and let go: the cinematic carries on. Hold E two
   seconds: the milestone shows (through black), the voice stops, the town track comes.
3. New game with Zanyr (still steps until CR1): a tap of E still skips, as today.

- **Done, with deviations:** `SkipProgress` waits for CG5, its first reader. The view picks its hint by the message id each run passes to `Follow` (`cinematic_hold_skip` for a cinematic, `cinematic_skip` for still steps), not by being told which run draws. The presenter tests are a sibling, `EventPresenterCinematicSkipTests`, and `InputFrames.Holding` is new. The hold tests run whole updates to a time, so they don't depend on 1/60 s's tick rounding. Test added: `Progress_HeldLongerThanTheHoldTime_StaysAtOne`. `MessageIdsTests` needs nothing new: its check over every id covers `cinematic_hold_skip`.
  CG4's review: the hint each run shows is tested (`Update_DuringACinematic_HintAsksForAHold`, `Update_DuringAStillStep_HintAsksForAPress`, through `EventPresenter.SkipHint`, added in CG5 once the hint had its own view); `Show`'s summary says the hold counts only from a press made during the cinematic.
