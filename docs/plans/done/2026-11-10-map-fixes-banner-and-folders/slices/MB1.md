# MB1. `MapBanner`: what the entry banner shows, and how opaque

Game, about 185 lines (a presenter, no view type). Builds on AL1, AL2 and design.md, "Key decisions" 6. Decisions 528, 529 (Q9).

## Goal
A pure presenter, tested headless at 1/60 s steps, says which map the banner shows and its opacity now: entering a map other than the one entered last starts it when that map's settings show the banner (`ShowsBanner`); it waits, fades in, holds and fades out (design.md tunables); its clock moves only when the scene lets it (no event running, MB2). Not drawn yet (MB2).

## New types
- `Game/Exploration/MapBanner`: which map's name and level the banner shows and how opaque it is now.
  - `public static readonly TimeSpan WaitTime, FadeInTime, HoldTime, FadeOutTime` (placeholders, design.md).
  - `public void Enter(MapDefinition map)`: when `map.Id` differs from the last map entered, the banner stops whatever it showed and, when `map.Settings.ShowsBanner`, restarts on it (`Shown` = map, clock 0); a map that doesn't show it leaves `Shown` null. The same id changes nothing (a same-map warp). The first call always counts as another map.
  - `public void Advance(TimeSpan delta)`: moves the clock; past the fade out the banner is done (`Opacity` 0).
  - `public MapDefinition? Shown { get; }`, `public float Opacity { get; }` (0 to 1; linear in and out, as `PromptFade`).
  - No allocation in `Advance`.

## Changes
None outside the new type: the scene calls it in MB2.

## Tests
`Game.Tests/Exploration/MapBannerTests` (new; steps of exactly 1/60 s through a helper `StepFor(TimeSpan)`, `.claude/rules/tests.md`; maps built in the test):
- `Enter_FirstMap_ShowsIt`.
- `Advance_DuringTheWait_StaysTransparent`.
- `Advance_HalfwayThroughTheFadeIn_IsHalfOpaque` (within one step's share).
- `Advance_DuringTheHold_IsOpaque`.
- `Advance_AfterTheFadeOut_IsTransparent`.
- `Enter_SameMapAgain_DoesNotRestart` (opacity keeps falling after a same-id enter mid-fade-out).
- `Enter_AnotherMapMidHold_RestartsOnIt` (`Shown` is the new map, opacity 0 again).
- `Enter_BackToTheFirstMap_ShowsItAgain` (A, B, A: every entry to another map, Q9).
- `Enter_MapNotShowingTheBanner_ShowsNothing` (`ShowsBanner` false: `Shown` null, opacity 0 through the whole timeline).
- `Enter_HiddenMapMidHold_StopsTheBanner` (A shown and held, then B with the banner off: opacity 0 at once).
- `Enter_FromHiddenMapToShownMap_ShowsIt`.
- The hold while an event runs is the scene's (MB2 tests it through `ExplorationScene`).

- **Done, with deviations:** a twelfth test pins the fade out's halfway point, the other side of the fade in's; from the review, the same-map test checks both bounds (a restart would read 0) and a thirteenth enters the same map after the banner ended. `Advance` does nothing while nothing is shown, so the clock can't run on for a hidden map.

## Verify by hand
Nothing on screen yet; the tests are the check.
