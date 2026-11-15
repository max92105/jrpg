# Cinematic focus point: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Focus range | `Core/Cinematics/StillFocus.MinPercent`, `MaxPercent` | 0, 100 | Each share's bounds: the shown picture's left or top edge to its right or bottom (decision 749). |
| The center | `Core/Cinematics/StillFocus.Center` | 50, 50 | Where a still without a focus zooms about (decision 617). |
| Marker size | `Editor/Cinematics/FocusMarkerMarginConverter.MarkerSize` | 16 px (a double, as XAML sizes are) | The ring's width and height over the preview; the converter centers it on the point. |
| Marker brush | `Editor/Shell/EditorTheme.xaml` `FocusMarkBrush` | a light gold (`#FFD45A`, placeholder) | The ring and cross; outlined with the theme's `WindowBackgroundBrush` (decision 755). |

No content value is a tunable here: the point is the developer's per still.

## Design overview

### What exists
- `Core/Cinematics/CinematicStill`: picture, seconds, caption, voice, `Zoom` (a `StillZoom`, 100 to 150 %), and
  `ScaleAt(intoStill)`, the scale at a time (decision 617, 629). `StillZoom.ScaleAt(share)` is linear.
- `Data/Cinematics/CinematicCatalogValidator`: a still's problems in one conditional chain, `FindStillProblem`; the zoom
  through `RangeProblems.Within`. `Data/Content/AbsentWhenNullFilter` leaves the still's `caption`, `voiceFile` and
  `zoom` out while null.
- `Game/Exploration/CinematicRun.Draw` builds a `StillFrame(ImageFile, Opacity, Scale)` inline from Core's fade and
  scale; `StillView.Draw` draws it through `Ui/CoverFit.DrawZoomedOverScreen(batch, image, tint, scale)`, which places
  the cover-cropped picture's center at the screen's center and scales it there.
- `Editor/Cinematics/CinematicStillRow` (one still's boxes, `From`/`ToStill`, `Changed` relayed to the problem line,
  which also stops Play), `CinematicPreview` (Play's `ShownImage`, `Opacity`, `Scale`, `Caption`, `TimeLine`), and
  `CinematicCatalogWindow.xaml`: a clipped 480 x 270 black frame holding the chosen still's `Image`
  (`UniformToFill`, centered), and over it, while playing, a black grid holding Play's `Image` with a `ScaleTransform`
  about `RenderTransformOrigin` 0.5, 0.5.

### Core changes (FP1)
- `Cinematics/StillFocus` (public sealed record): where on a still's shown picture its zoom heads, as two whole
  percents.
  - `required int XPercent`, `required int YPercent`; `const int MinPercent = 0`, `MaxPercent = 100`.
  - `static StillFocus Center { get; }` (50, 50).
  - `Point2D Share => new(XPercent / 100.0, YPercent / 100.0)`.
- `CinematicStill.Focus` (`StillFocus?`, init, declared after `Zoom` so the writer writes it after `zoom`).
- `CinematicStill.ZoomOrigin` (`Point2D`): `(Focus ?? StillFocus.Center).Share`, the share of the shown picture that
  stays put while it zooms (decision 751).
- `CinematicDefinition.Equals` compares the stills by record equality, so a different focus is a different cinematic
  with no change there (a test pins it).

### Why no black edge
The picture at scale 1 covers the screen `[0, W] x [0, H]`. Scaled by `s` about a point `O` on the screen, the covered
area becomes `[O.x (1 - s), O.x + s (W - O.x)] x [...]`. With `s >= 1` (decision 617's 100 % floor) and
`0 <= O.x <= W` (decision 747: the point is on the shown picture), the left edge is at or below 0 and the right at or
past `W`; the same holds vertically. So the zoom covers the screen for every allowed point and scale, the corners
included; `ScreenZoomTests` pins both sides.

### Structure and folder dependencies

| From | To | New? | Why |
|---|---|---|---|
| Core `Cinematics` | Core `Geometry` | new | `StillFocus.Share` and `ZoomOrigin` are `Point2D`. Geometry reads nothing; no cycle. |
| Data `Cinematics` | Core `Cinematics` | exists | The validator checks the focus's range. |
| Game `Exploration` | Game `Ui` | exists | `StillFrame` holds a `ScreenZoom`; `StillView` already draws through `CoverFit`. |
| Game `Ui` | Game `Rendering` | exists | `ScreenZoom` is plain `Vector2` math; `CoverFit` already reads `VirtualScreen`. |
| Editor `Cinematics` | Core `Cinematics` | exists | The row holds a `StillFocus`; the preview reads `ZoomOrigin`. |

New types and where they go:
- `Core/Cinematics/StillFocus` (FP1): where on a still's shown picture its zoom heads.
- `Game/Ui/ScreenZoom` (FP2, internal readonly record struct): a zoom over the virtual screen, its scale and the screen
  point that stays put. `PositionOf(Vector2 point)`: where a point of the unzoomed screen shows. In `Ui/` beside
  `CoverFit`, its only caller's home.
- `Editor/Cinematics/FocusMarkerMarginConverter` (FP4, internal sealed, `IValueConverter`): a still's focus to the
  marker's margin in the preview frame.

Changed types:
- `Game/Exploration/StillFrame` (FP2): `(string ImageFile, float Opacity, ScreenZoom Zoom)` replaces `Scale`, and
  gains `static StillFrame At(CinematicStill still, TimeSpan intoStill)`.
- `Game/Ui/CoverFit.DrawZoomedOverScreen(SpriteBatch, Texture2D, Color, ScreenZoom)` (FP2): the cropped source drawn
  from its top-left, origin `Vector2.Zero`, at `zoom.PositionOf(Vector2.Zero)`, scaled by the fit times `zoom.Scale`,
  so the corner the tests pin is the point the draw uses.
- `Editor/Cinematics/CinematicStillRow` (FP3): `Focus`, `HasFocus`, `FocusText`, `SetFocusAt(double xShare, double
  yShare)`, `ClearFocusCommand`; `From` and `ToStill` carry the focus.
- `Editor/Cinematics/CinematicPreview` (FP3): `ZoomOrigin` (`System.Windows.Point`, shares; the center while stopped).

### Key decisions
- **The point is of the shown picture, not the file** (decision 747): no crop arithmetic in Core, the editor or the
  game; the click is `position / frame size`. Both shells already crop the same centered 16:9 part.
- **Core gives the origin, each shell scales about it** (decision 751): the game through `ScreenZoom`, the preview
  through WPF's `RenderTransformOrigin` on Play's existing black grid, which is exactly the frame's size (the `Image`
  itself may be larger than the frame under `UniformToFill`, so the transform moves off it). Both are the same affine
  map, `O + s (p - O)`; the hand checks compare them side by side.
- **The validator's chain is at its complexity limit**: FP1 extracts `FindOptionalFieldProblem` (caption, voice, zoom,
  focus) out of `FindStillProblem` before adding the focus's branch.
- **No row problem for the focus**: the row's point comes from a clamped click or a loaded (checked) record, so it is
  always in range; Data's range check is the only one.

### Extension points
- **A pan** (decision 618's backlog): only `CinematicStill.ZoomOrigin`'s rule changes (a point drifting toward the
  center over the still's time, clamped by the edges) into a time-dependent `ZoomOriginAt(intoStill)`; the game's
  `StillFrame.At` and the preview's `ShowMoment` already read it per moment.
- **A moving focus** (a start and an end point): a second `StillFocus` on the still, read by the same method.

### Replacement mapping

| Today | After |
|---|---|
| `StillFrame.Scale` (float) | `StillFrame.Zoom` (`ScreenZoom`: scale and origin) |
| `CinematicRun.Draw` building the frame inline | `StillFrame.At(still, moment.IntoStill)` |
| `CoverFit.DrawZoomedOverScreen(..., float scale)` about the screen center | `(..., ScreenZoom zoom)` about the zoom's origin |
| Preview: `ScaleTransform` on Play's `Image`, `RenderTransformOrigin` 0.5, 0.5 | on Play's black grid (the frame's size), `RenderTransformOrigin` bound to `CinematicPreview.ZoomOrigin` |
| `CoverFit` drawing the crop about its center at the screen's center | drawn from its top-left at `zoom.PositionOf(Vector2.Zero)` |
| `Point2D`'s summary: a map position in tile units | also shares of a still's shown picture (`ZoomOrigin`) |
| `CinematicCatalogValidator.FindStillProblem`'s caption, voice and zoom arms | `FindOptionalFieldProblem`, with the focus arm |

## Risks
- **Complexity limits:** `CinematicCatalogValidator.FindStillProblem` is near CA1502 (FP1 extracts first, named).
  `CinematicRun.Draw` gets simpler. No editor hot method (`HandleObjectKey` and kin) is touched.
- **XAML:** the transform moved from Play's `Image` to its black grid must keep the picture centered and cropped as
  today (the developer's hand check compares a still without a focus before and after); `MarkerSize` is a double (an `x:Static` size reads a double, or the
  window throws as it opens); FP4 runs `.claude/scripts/open-editor-windows.ps1` (the window is already in its default
  list; no new window).
- **Coverage at the edges:** a point at 0 or 100 scales about the screen's edge or corner; the proof above and the
  `ScreenZoomTests` corner theory guard it. The editor's click is clamped, so a click a pixel past the frame's edge
  (WPF can report 480.4) gives 100.
- **Content format:** an additive optional field, absent while null; `cinematics.json` and the test world's copy are
  unchanged, so the writer's byte-for-byte tests hold. A hand-written focus missing one percent is refused naming the
  file (the strict serializer's required members); FP1 pins it.
- **Saves:** none touched (decision 756).
- **Determinism and performance:** no clock, no randomness; `ScreenZoom` and `StillFrame` are structs, computed once
  per draw, no allocation.
- **Parallel plans:** no other editor-push row touches `Cinematics/`, `StillView`, `CoverFit` or the Cinematics window
  (checked against the roadmap's rows 2, 3 and 5 on 2026-11-15); `AbsentWhenNullFilter` gains one line, a likely merge
  neighbor of any plan adding an optional field (independent lines).
