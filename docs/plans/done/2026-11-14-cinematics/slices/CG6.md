# CG6. The slow zoom, and every still drawn smoothly

Core + Game, about 140 lines (about 70 of them tests). Needs CG2. Decisions 617, 627; risk "Sampler state".

## Goal
A still with a zoom drifts from its start scale to its end scale over its whole time, linearly, about the screen's
center, smoothly (float scale): 100 to 108 % creeps in, 108 to 100 % creeps out; a still without a zoom stays still.
Every still, zoomed or not, of a cinematic or a still step, is drawn with linear sampling in its own batch (decision
627).

## Changes
- Core `Cinematics/StillZoom`: `double ScaleAt(double share)` (share 0 to 1 of the still's time; the start percent at 0,
  the end percent at 1, linear between, as a factor: 108 % is 1.08; throws outside 0..1).
- Core `Cinematics/CinematicStill`: `double ScaleAt(TimeSpan intoStill)` (1 without a zoom; else the zoom at
  `intoStill / Duration`, clamped to 1 at the end).
- Game `Exploration/StillView.Draw` takes a `float scale` too: it ends the scene's batch, begins its own with
  `SamplerState.LinearClamp`, draws black and the picture covering the screen (`CoverFit.SourceIn` as today) with a
  float destination scaled about the screen's center (the drift moves by fractions of a pixel), ends it and begins the
  scene's batch again with the scene's own state, then draws the subtitle band, the hint and the ring. Read the
  exploration scene's `Begin` first to restore it exactly.
- Game `Exploration/CinematicRun.Draw`: passes `ShownStill.ScaleAt(moment.IntoStill)`; `StillRun` passes 1.
- `docs/map.md`: Core `Cinematics` names `ScaleAt`.

## Tests
- Core `StillZoomTests` (new):
  - `ScaleAt_Start_IsStartPercent` (100 to 108: 1.0).
  - `ScaleAt_End_IsEndPercent` (1.08).
  - `ScaleAt_Half_IsMidway` (1.04).
  - `ScaleAt_ZoomOut_Shrinks` (108 to 100: 1.04 at half, 1.0 at the end).
  - `ScaleAt_ShareOutside_Throws` (both sides: -0.01 and 1.01).
- Core `CinematicStillTests` (new): `ScaleAt_NoZoom_IsOne`; `ScaleAt_PastItsTime_StaysAtTheEnd`.
- Game: none (draw-only); the hand check, with captures before and after.

## Verify by hand
1. F12 capture of still 1 of Ragnir's intro before the slice (in its hold, no zoom set).
2. Database > Cinematics..., `ragnir_intro`: still 1 zoom 100 to 110, still 2 zoom 110 to 100; OK; Ctrl+S.
3. New game, Ragnir: still 1 creeps in smoothly over its seconds (no stepping, no shimmer on fine lines), still 2
   creeps out; the others don't move; the subtitle, hint and ring stay still and sharp.
4. F12 capture of still 3 (no zoom) at the same moment as step 1's: smoother edges than before, no blur on the subtitle.
5. New game with Zanyr (still steps): its placeholder stills draw smoothly too.

- **Done, with deviations:** `StillView.Draw` takes a `StillFrame` (picture, opacity, scale), not a fifth parameter; the zoomed draw is `CoverFit.DrawZoomedOverScreen` beside `DrawOverScreen` (a float scale about the screen's center). The picture's batch draws the black too; the scene's batch (`VirtualScreen.BeginBatch`, as `ShatterView` restores it) comes back for the subtitle, the hint and the ring. Test added: `CinematicStillTests.ScaleAt_HalfItsTime_IsMidway`. Hand check (the look) is the developer's.
