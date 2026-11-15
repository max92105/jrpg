# FP2. The game zooms about the focus

Game, about 170 lines (about 100 of them tests; no view type added, so no 1.6x). Needs FP1. Decisions 748, 751, 752.
Design: "Why no black edge", "Key decisions", "Replacement mapping".

## Goal
In the game, a still with a focus zooms about that point of the screen: the point stays put while the picture grows
around it (or shrinks back to fit), never showing a black edge; a still without one zooms about the center exactly as
today. The frame a still shows at a time is built in one pure place the tests read.

## New types
- `Game/Ui/ScreenZoom` (internal readonly record struct `(float Scale, Vector2 Origin)`): a zoom over the virtual
  screen, its scale and the screen point that stays put.
  - `public Vector2 PositionOf(Vector2 point) => Origin + ((point - Origin) * Scale);` (summary: where a point of the
    unzoomed screen shows).

## Changes
- `Game/Exploration/StillFrame`: `(string ImageFile, float Opacity, ScreenZoom Zoom)` (the `Scale` parameter replaced;
  its doc says the zoom's origin is the still's focus on the screen, the center without one), and
  `public static StillFrame At(CinematicStill still, TimeSpan intoStill)`: the picture, `StillFade.OpacityAt(intoStill,
  still.Duration)`, and a `ScreenZoom` of `(float)still.ScaleAt(intoStill)` about `still.ZoomOrigin` times
  `VirtualScreen.Width` and `Height` (decision 751).
- `Game/Exploration/CinematicRun.Draw`: `view.Draw(batch, textures, StillFrame.At(still, moment.IntoStill))` replaces
  the inline construction.
- `Game/Ui/CoverFit.DrawZoomedOverScreen(SpriteBatch batch, Texture2D image, Color tint, ScreenZoom zoom)`: the cover
  crop as today, drawn from the cropped source's top-left: `origin` `Vector2.Zero`, `position`
  `zoom.PositionOf(Vector2.Zero)` (where the screen's top-left corner goes under the zoom), `scale` the fit (per axis,
  as today) times `zoom.Scale`. So the corner tests in `ScreenZoomTests` pin the exact point the draw uses, and the
  source center and screen center arithmetic leaves. Its summary says "about the zoom's origin" instead of "about the
  screen's center", and the `scale` param doc becomes the `zoom`'s ("1 covers the screen exactly; above 1 about an
  origin on the screen still covers it").
- `Game/Exploration/StillView.Draw`: passes `frame.Zoom`; its summary is unchanged (the drift by fractions of a pixel
  still holds).
- Grep "about the screen's center" and "about the center" in `Game/Exploration` and `Game/Ui` comments (the code
  quality rule on changed wording); `StillFrame`'s old `Scale` doc is the known one.
- `docs/map.md`: the Game `Ui` row names `ScreenZoom` (a zoom's scale and the screen point that stays put, decision
  752); the `Exploration` row's `StillView` entry says the still zooms about its focus (`StillFrame.At`).

## Tests
`ScreenZoomTests` (new, `Game.Tests/Ui`; Glob found none):
- `PositionOf_TheOrigin_StaysPut` (origin (480, 810), scale 1.5).
- `PositionOf_ScreenCenterAboutTheTopLeft_MovesOutByTheScale` ((960, 540) about (0, 0) at 1.5 is (1440, 810)).
- `PositionOf_ScaleOne_LeavesThePoint`.
- `PositionOf_ScreenCornersAboutAnOriginOnTheScreen_CoverTheScreen` (theory over origins (0, 0), (1920, 1080),
  (300, 900) at 1.5: the zoomed (0, 0), the very point `CoverFit` draws the picture's top-left at, is at or above and
  left of (0, 0); the zoomed (1920, 1080) at or below and right of (1920, 1080); both sides of the proof in the
  design).

`StillFrameTests` (new, `Game.Tests/Exploration`), on a 4 s still zooming 100 to 150 % built in the test:
- `At_StillWithoutFocus_ZoomsAboutTheScreenCenter` (origin (960, 540)).
- `At_StillWithFocus_ZoomsAboutItsPointOnTheScreen` (focus (25, 75) gives origin (480, 810)).
- `At_FocusAtTheBottomRightEdge_IsTheScreensCorner` (focus (100, 100) gives (1920, 1080)).
- `At_HalfwayThroughItsTime_ScalesByCoresZoom` (2 s gives 1.25).
- `At_IntoTheFadeIn_TakesCoresOpacity` (0.2 s equals `StillFade.OpacityAt(0.2 s, 4 s)`, strictly between 0 and 1).

Unchanged and passing: `CoverFitTests` (`SourceIn` only), `EventPresenterCinematic*Tests` (the timeline and the skip).

## Verify by hand
**The developer's check, left unticked by the implementer** with a Done note saying so: the implementer never edits
`content/cinematics.json` for it (the intro is the developer's content) and stops at `verify.ps1`. The steps, for the
developer: the repository's cinematics have no focus yet, so the check gives one locally and reverts it.
1. In `content/cinematics.json`, in `ragnir_intro`'s first still (`ragnir_intro/1_happy_life.png`), after
   `"voiceFile": "Placeholder.wav"`, add (with a comma after the voice line):
   ```json
   "zoom": {
     "startPercent": 100,
     "endPercent": 150
   },
   "focus": {
     "xPercent": 25,
     "yPercent": 25
   }
   ```
2. `dotnet run --project src/Jrpg.Game`. On the title, **New Game**; on the leader choice, move to **Ragnir** and
   press Confirm (the intro map's arrival event plays `ragnir_intro`).
3. As the intro's first still shows (the father on the left, his son running to him): over its 4 s the picture grows
   toward the father's head, which stays where it is on the screen while the rest of the picture slides away from it;
   no black edge appears at the top or the left.
4. Quit, change the focus to `"xPercent": 100, "yPercent": 100`, start again: the picture grows from the bottom-right
   corner, the corner itself never moving; no black edge.
5. Remove the `focus` block (keep the zoom), start again: it grows from the center as before this slice.
6. `git checkout content/cinematics.json` (the intro's real zoom and point are the developer's to set in the editor).

- **Done:** as planned; the hand check is the developer's and left unticked: the implementer edited no content file
  (steps 1 to 6 above give `ragnir_intro`'s first still a focus locally, then revert it).
