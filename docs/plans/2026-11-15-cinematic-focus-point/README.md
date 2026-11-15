# Cinematic focus point
Status: retro pending

Approved on 2026-11-15 by the coordinator, on the developer's delegation, after the devil's advocate's review (applied:
`CoverFit` draws from the crop's top-left at `zoom.PositionOf(Vector2.Zero)`; Play's existing black grid takes the
zoom's transform, no new grid; `Point2D`'s summary names its second use; a clamp row that needs the upper Y clamp; a
bad zoom and a bad focus together name the zoom; the visual hand checks of FP2 and FP4 are the developer's). The
defaults below stay his to overturn.

Drafted on 2026-11-15 from row 4 of the editor push's roadmap
([`2026-11-15-editor-push`](../2026-11-15-editor-push/README.md), design 3.10). The developer's words:

> For cinematic I want to be able to put a point to direct the zoom towards it.

Today a still zooms from a start to an end percent about its picture's center (decision 617, ADR 0046), by Core's
`CinematicStill.ScaleAt`, read by the game's `CinematicRun` and the Cinematics window's `CinematicPreview` (decision
629). This plan gives a still an optional **focus point**: the spot on its picture the zoom heads to.

- Numbers: decisions 746-757 (758-760 unused). No new ADR: an amendment paragraph in ADR 0046 (decision 757).
- Core: one record (`Cinematics/StillFocus`) and one rule (`CinematicStill.ZoomOrigin`). Data: the field read, checked
  and written. Game: the zoom drawn about the point. Editor: a click on the picture sets it, a marker shows it, Clear
  focus removes it, Play zooms about it. No save change.

## Goal
1. **A still may name a focus point** in `cinematics.json`:
   `"focus": { "xPercent": 30, "yPercent": 62 }`, left out when it has none (then the zoom stays about the center).
2. **The game zooms toward it**: the picture grows (or shrinks back) about the point, which stays where it is on the
   screen, as a mouse-wheel zoom keeps the spot under the pointer; never a black edge.
3. **The editor sets it with a click**: in Database > Cinematics..., a click on the chosen still's picture puts the
   point there, a ring marks it, a line names it ("Zoom heads to 30 %, 62 %"), **Clear focus** returns to the center,
   and **Play** zooms about it exactly as the game does.

## Order, and why
1. **FP1, the content field** (Core record and rule, Data's load, check and write): the rule lands with its first
   reader, the loader; every later slice reads the field.
2. **FP2, the game**: the truth the preview must match; its hand check (the developer's) writes a local focus and
   reverts it, and the implementer never edits `content/cinematics.json` for it. FP4's look check is the developer's
   too; the implementer runs `open-editor-windows.ps1` and may take a screenshot.
3. **FP3, the editor's rules** (the still row's focus, the click's rounding, Clear focus, the preview's zoom origin),
   then **FP4, the window** (the click, the marker, the button, Play's transform): editor slices stay near 200 lines, a
   panel's rules before its view.

## Guiding rules
- **One rule source** (decision 629, extended by 751): which point stays put while a still zooms is Core's
  `CinematicStill.ZoomOrigin`; the game applies it through `Ui/ScreenZoom`, the preview through WPF's
  `RenderTransformOrigin`. The editor never references Game.
- **One rule source for content** (ADR 0045): the focus's range is `CinematicCatalogValidator`'s; the window's problem
  line reaches it through `CatalogProblems.OfCinematics`, which already runs on every typed change.
- **Never a black edge**: the zoom never goes below 100 % (decision 617) and the point always lies on the shown picture
  (decision 747), so scaling about it always covers the screen (design, "Why no black edge").
- **No allocation per frame**: `ScreenZoom` and `StillFrame` are structs; `StillFrame.At` allocates nothing.
- **Determinism**: no clock, no randomness; times are the update's delta and the render tick's, as today.
- **Content written as the writer writes it**: the field is absent while null (`AbsentWhenNullFilter`), so
  `cinematics.json` writes back byte for byte until the developer sets a point.
- **Sizes:** editor slices about 200 lines with tests at about 10 lines each; no file nears 300 (checked 2026-11-15:
  `CinematicStillRow.cs` 161, `CinematicCatalogWindow.xaml` 156, `CinematicPreview.cs` 96, `CinematicRun.cs` 192,
  `StillView.cs` 79, `CinematicCatalogValidator.cs` 62, `AbsentWhenNullFilter.cs` 73).
- **Test files near 250, siblings named here:** `CinematicStillRowTests` (178) → `CinematicStillRowFocusTests` (new,
  FP3). Every other test file named below stays under 250 with its additions.

## Out of scope
- A pan bringing the point toward the screen's center (the other half of Ken Burns, decision 618 stays backlog); the
  extension point is named in the design.
- A focus that moves over the still's time (a start point and an end point).
- Typed X and Y boxes for the point; dragging the marker (a second click moves it).
- An in-window undo of a click: the window's Cancel drops the session and its OK is one undo step, as for every box.

## Slices

### Phase 1: the field
- [x] **FP1. A still's focus point in content** (Core, Data, about 180 lines) → [slices/FP1.md](slices/FP1.md)

### Phase 2: the game
- [x] **FP2. The game zooms about the focus** (Game, about 170 lines) → [slices/FP2.md](slices/FP2.md)

### Phase 3: the editor
- [x] **FP3. The still's focus as the window edits it** (Editor, about 210 lines) → [slices/FP3.md](slices/FP3.md)
- [x] **FP4. Click to set, marker, Clear focus, Play about it** (Editor, about 140 lines) →
  [slices/FP4.md](slices/FP4.md)

## Decisions
- 746: a still may name a **focus point**, the spot on its picture its zoom heads to: `CinematicStill.Focus`, a
  `StillFocus` (`xPercent`, `yPercent`); none keeps decision 617's zoom about the center. Written as
  `"focus": { "xPercent": 30, "yPercent": 62 }` after `zoom`, left out while none.
- 747: the point is a share of **the picture as the screen shows it**: the part that covers the 16:9 screen (the
  game's `CoverFit.SourceIn`, the preview's `UniformToFill`), measured from its top-left, not of the whole file. Why:
  a click on the 480 x 270 preview maps straight to it, and a point outside the shown part (which would bring a black
  edge in) can't exist. For the shipped stills (about 16:9, `ragnir_intro/1_happy_life.png` is about 1672 x 941) the
  two differ by a pixel or so. This sharpens design 3.10's "a share of the picture's width and height".
- 748: the zoom scales **about the point**: the point stays where it is on the screen while the picture grows around it
  (zooming in) or shrinks back to fit (zooming out), as a mouse-wheel zoom keeps the spot under the pointer. *(default
  taken while the developer was away; his to overturn)*
- 749: each share is a **whole percent from 0 to 100** (0 the left or top edge, 100 the right or bottom), as the zoom's
  percents are; one percent is about 19 px across the game's screen and 5 px across the preview. *(default taken while
  the developer was away; his to overturn)*
- 750: a focus is **kept on a still without a zoom**: it changes nothing there (a scale of 1 about any point is the
  fitted picture), Data accepts it, the editor keeps and writes it, so typing the zoom back to 100 and up again doesn't
  lose the click.
- 751: Core's `CinematicStill.ZoomOrigin` (the focus's shares, else the center's 0.5, 0.5, as a `Geometry/Point2D`)
  is the one rule the game and the preview read, as decision 629 has them read the scale; `CinematicStill.ScaleAt` and
  `StillZoom` are unchanged.
- 752: the game's zoom is a value, `Ui/ScreenZoom` (a scale and the screen point that stays put): `CoverFit`
  `.DrawZoomedOverScreen` takes it instead of a bare scale, and `Exploration/StillFrame.At(still, intoStill)` builds a
  still's frame (picture, opacity, zoom) from Core's rules, the seam the tests read; `CinematicRun.Draw` calls it.
- 753: in the Cinematics window, **a click on the chosen still's picture sets the point** where the click lands; a ring
  marks it over the picture; the line under the zoom boxes says "Zoom heads to 30 %, 62 %" or "Zoom heads to the
  center: click the picture to choose a point"; **Clear focus** (greyed without a point) returns to the center; while
  Play runs, a click does nothing (Play's picture covers the chosen still's and takes the click). *(default taken while
  the developer was away; his to overturn)*
- 754: setting or clearing the point is a typed change of the still: it stops Play and refreshes the problem line, as
  any box does (CP1); it is undone with the window's Cancel or, once OK'd, with the session's one undo step (ADR 0045),
  as the boxes are.
- 755: the marker is a 16 px ring with a cross in a new theme brush, `FocusMarkBrush` (a light gold), over a dark
  1 px outline so it shows on light and dark pictures; it never takes the click, so a click on it moves the point.
- 756: no save change: saves never name a cinematic; an event in progress is never saved.
- 757: no new ADR: ADR 0046 gains an "Amended 2026-11-15" paragraph (a still's optional focus; the zoom scales about it;
  the rule in Core, read by the game and the preview) and its index line names the focus. The pan stays decision 618's
  backlog line.

## Defaults to confirm
Taken while the developer was away; each is his to overturn at approval (the slice it changes is named).
- **748, zoom about the point (FP2, FP4):** the point stays put on screen. The alternative is a pan: the point drifts
  toward the screen's center as the zoom grows, as far as the edges allow (at 150 % a point at 20 % ends at about 30 %
  of the screen). It changes `ZoomOrigin`'s rule in Core and the preview's transform; about one more slice.
- **749, whole percents (FP1):** about 19 px on the game's screen. Permille instead changes only the record's ranges,
  the rounding and the line's text.
- **753, a click sets the point, Play doesn't take clicks, Clear focus returns to the center (FP4):** the other
  shapes are a drag of the marker, typed X and Y boxes, or a right click to clear.

## Open design questions (developer decides)
None beyond the defaults above.

## ADRs needed
- [x] **ADR 0046 amended** (decision 757), in FP1: write the paragraph from decisions 746-751; no new ADR number
  (0053 stays free).
