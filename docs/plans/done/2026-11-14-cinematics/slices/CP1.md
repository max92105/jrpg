# CP1. Play in the Cinematics window

Editor, about 230 lines (about 90 of them tests). Needs CE6, CG1 (`MomentAt`, `StillFade`), CG6 (`ScaleAt`).
Decisions 620, 629. Silent: no audio in the editor (decision 620; CE7 shows the voices' lengths instead).

## Goal
Under the preview, **Play** plays the chosen cinematic from the chosen still (from still 1 when none is chosen) on the
preview's own clock: each still for its seconds, faded in and out by Core's fade, zoomed by Core's zoom, its subtitle
under the picture, a "still 3 of 6, 2.4 s" line; **Stop** (or the end, or another cinematic chosen, or any box typed)
stops it and shows the chosen still again. The boxes as typed are what plays (a box that can't be read stops it with
the problem shown; the preview never calls `ToCinematic()` while a problem shows).

## New types
- `Editor/Cinematics/CinematicPreview` (internal sealed partial, `ObservableObject`): the preview's moment on its own
  clock: the still shown, its opacity, scale and subtitle.
  - `Play(CinematicDefinition cinematic, int fromStill)` (elapsed starts at the sum of the earlier stills' seconds),
    `Stop()`, `Advance(TimeSpan delta)` (moves the clock; past the end stops), `IsPlaying`.
  - `ShownImage`, `Opacity`, `Scale`, `Caption`, `TimeLine` ("Still 3 of 6, 2.4 s"), raised only when they change.
- No new window type: `CinematicCatalogWindow` gets the Play and Stop buttons and draws `ShownImage` with an
  `Opacity` binding and a `ScaleTransform` about the center (`RenderTransformOrigin` 0.5, 0.5) in a clipped 480 x 270
  border, with the subtitle under it.

## Changes
- `Editor/Cinematics/CinematicCatalogViewModel`: `Preview` (a `CinematicPreview`), `PlayCommand` (greyed while the
  chosen cinematic shows a problem; `CanExecute` re-raised on every typed change, the tests rule) and `StopCommand`;
  any typed change or another cinematic chosen stops it.
- `CinematicCatalogWindow.xaml.cs`: while `Preview.IsPlaying`, `CompositionTarget.Rendering` advances it by the render
  time's delta (unsubscribed on stop and on `Closed`); the picture cache (CE6) serves the shown image.
- `docs/map.md`: the Editor `Cinematics` row names the preview.

## Tests
`CinematicPreviewTests` (new), on a cinematic built in the test (stills 2 s with zoom 100 to 110, then 3 s, captions):
- `Play_FromStillTwo_StartsAtItsBeginning` (`ShownImage` is the second's, `Opacity` 0).
- `Advance_IntoTheFirstStill_FadesItIn` (`Opacity` from `StillFade`, compared to Core's value).
- `Advance_HalfwayThroughTheFirst_ScalesItByItsZoom` (1.05).
- `Advance_PastTheEnd_Stops` (`IsPlaying` false, the chosen still shown again).
- `Stop_WhilePlaying_ShowsTheChosenStillAtFullOpacity`.
- `TimeLine_InTheSecondStill_NamesItAndTheTimeInto`.
`CinematicCatalogViewModelTests` (or a sibling past 250):
- `PlayCommand_ChosenCinematicHasProblem_IsGreyed`; `PlayCommand_CanExecute_RaisedWhenATypedSecondsChanges`.
- `TypingSeconds_WhilePlaying_StopsThePreview`.

## Verify by hand
1. Database > Cinematics..., choose `ragnir_intro`, choose still 1, click **Play**: the stills fade in and out in order,
   still 1 creeping in by its zoom, the subtitles under the picture, the time line counting; at the end it stops on
   still 1.
2. Choose still 4, click **Play**: it starts at still 4.
3. Click **Play**, then type in a **Seconds** box: it stops. Click **Play**, then **Stop**: it stops on the chosen
   still.
4. Close the window while playing: no error; reopen it: nothing plays.

- **Done, with deviations:** the playing picture is a second image over black above the chosen still's, shown while `Preview.IsPlaying`, so the stopped preview needs no state of its own (`ShownImage` null). The clock starts and stops with `IsPlaying` (`CompositionTarget.Rendering`), and stops when the window closes. The window's Play tests are a sibling, `CinematicCatalogViewModelPlayTests`, with tests added for Play from the chosen still, another cinematic chosen, Stop, and `Advance` while stopped. Hand check (the look) is the developer's.
  CP1's review: Play is greyed while Data refuses the chosen cinematic too (a still's seconds past the game's clock would overflow the preview's; `PlayCommand_SecondsPastTheGamesClock_IsGreyed`), and Play, Stop and the time line sit outside the still's panel, so they work with no still chosen.
