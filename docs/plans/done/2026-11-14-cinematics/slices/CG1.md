# CG1. The fade and the timeline in Core

Core + Game, about 160 lines (about 90 of them tests; the fade's moved). Needs CM1. Decision 629; tunable "Still fade".

## Goal
A still's opacity at a time into it, and which still a cinematic shows at a time, are Core rules (`Cinematics/`), so
the game (CG2) and the editor's preview (CP1) show the same fades at the same moments. Nothing on screen changes:
today's `StillPlayback` reads the moved fade.

## New types
- Core `Cinematics/StillFade` (public static): a still's opacity at a time into it: rising from black over the fade,
  holding, falling over the fade, the fade at most half the still. `public static readonly TimeSpan FadeTime` (0.8 s,
  placeholder, decision 159's "appear and disappear"); `public static float OpacityAt(TimeSpan intoStill, TimeSpan
  duration)` (0 to 1; throws `ArgumentOutOfRangeException` on a negative time, as `TileAnimation.FrameAt`).
- Core `Cinematics/StillMoment` (public readonly record struct `(int StillIndex, TimeSpan IntoStill)`): which still a
  cinematic shows at a time, and how far into it.

## Changes
- Core `Cinematics/CinematicDefinition`: `TimeSpan Duration` (the stills' durations summed; Data bounds the total, CM1)
  and `StillMoment? MomentAt(TimeSpan elapsed)` (walks the stills in order, no allocation; null at or past `Duration`;
  throws on a negative time). Summary: "the game and the editor's preview ask it, so a still's place in time has one
  rule" (decision 629). Its first callers are CG2 and CP1.
- Game `Exploration/StillPlayback`: `ShotFade` leaves; `Opacity => StillFade.OpacityAt(_elapsed, still.Duration)`; its
  summary points to the rule.
- `docs/map.md`: a Core `Cinematics` row line for `StillFade`, `StillMoment`, `MomentAt`.

## Tests
- `StillFadeTests` (new, `tests/Jrpg.Core.Tests/Cinematics/`): the opacity cases of `StillPlaybackTests` move here as
  `OpacityAt_...` (start 0, after the fade 1, halfway down the fade-out, a still shorter than two fades fading over
  half, the end 0), plus `OpacityAt_Negative_Throws`.
- `StillPlaybackTests` keeps `IsOver` and one `Opacity_AfterTheFade_IsOne` (the delegation's guard).
- `CinematicDefinitionTests` (CE1's), on `CinematicFixtures.TwoStills` (2 s then 3 s):
  - `Duration_TwoStills_SumsTheirSeconds` (5 s).
  - `MomentAt_Zero_IsFirstStillAtItsStart`.
  - `MomentAt_JustBeforeTwoSeconds_IsStillTheFirst` and `MomentAt_TwoSeconds_IsSecondAtItsStart` (both sides of the
    edge).
  - `MomentAt_Duration_IsNull`; `MomentAt_Negative_Throws`.

## Verify by hand
None: the same fades. Optional: a placeholder intro plays as before.

- **Note from CM1:** `CinematicStill.Duration` (`TimeSpan.FromSeconds(Seconds)`) was left for this slice, its first caller.

- **Done, with deviations:** `CinematicDefinition.Duration` sums in a loop, not LINQ (the game reads it every frame). Tests added: `OpacityAt_End_IsZero`, `MomentAt_InTheSecondStill_CountsFromItsStart`. `CinematicStill.Duration` lands here (CM1's note).
  CG1's review: the timeline tests build their 2 s then 3 s cinematic in the file (`TwoSecondsThenThree`), not from a shared `CinematicFixtures`.
