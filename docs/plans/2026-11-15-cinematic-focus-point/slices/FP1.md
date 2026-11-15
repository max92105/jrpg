# FP1. A still's focus point in content

Core and Data, about 180 lines (about 110 of them tests). Decisions 746, 747, 749, 750, 751, 757. Design: "Core
changes", "Key decisions".

## Goal
`cinematics.json` may give a still `"focus": { "xPercent": 30, "yPercent": 62 }`: loaded into `CinematicStill.Focus`,
each percent checked from 0 to 100 with a message naming the cinematic and the still, written back after `zoom` and
left out while none. Core says which point of the shown picture stays put while the still zooms
(`CinematicStill.ZoomOrigin`): the focus, else the center. Nothing draws it yet (FP2); no repository content changes.

## New types
- `Core/Cinematics/StillFocus` (public sealed record): where on a still's shown picture its zoom heads, as two whole
  percents.
  - `public required int XPercent { get; init; }`, `public required int YPercent { get; init; }`.
  - `public const int MinPercent = 0;`, `public const int MaxPercent = 100;` (summary: the shown picture's left or top
    edge to its right or bottom, decision 747).
  - `public static StillFocus Center { get; } = new() { XPercent = 50, YPercent = 50 };`
  - `public Point2D Share => new(XPercent / 100.0, YPercent / 100.0);`

## Changes
- `Core/Cinematics/CinematicStill`:
  - `public StillFocus? Focus { get; init; }` after `Zoom` (the writer writes fields in declaration order); summary:
    where the zoom heads, null for the center (decision 746); kept without a zoom, where it changes nothing (750);
    kept when the still's picture changes (a share of whatever picture shows, decision 747), so a new picture keeps
    the point until another click moves it.
  - `public Point2D ZoomOrigin => (Focus ?? StillFocus.Center).Share;` with a summary: the share of the shown picture
    that stays put while the still zooms, read by the game and the editor's preview (decision 751).
  - The class summary names the focus.
- `Core/Geometry/Point2D`: its summary, which says only "a position on a map, in tile units", becomes "A position in
  two dimensions: tile units on a map; shares of a still's shown picture (`CinematicStill.ZoomOrigin`)." (its
  second job, as the code quality rule asks when a type gains a caller).
- `Core/Cinematics/StillZoom`: the summary's "about the center" becomes "about the still's `ZoomOrigin`".
- `Data/Cinematics/CinematicCatalogValidator`:
  - **Extract first** (the conditional chain is near CA1502): `FindOptionalFieldProblem(CinematicStill still)` takes
    the caption, voice and zoom arms out of `FindStillProblem`, which ends `: FindOptionalFieldProblem(still)`.
  - Then the focus arm, last in it: the caption's and the voice's arms as today, then
    `(still.Zoom is { } zoom ? FindZoomProblem(zoom) : null) ?? (still.Focus is { } focus ? FindFocusProblem(focus) : null)`,
    so a still with a bad zoom names the zoom first and a focus is checked with or without a zoom (decision 750).
  - `FindFocusProblem(StillFocus focus)`: `RangeProblems.Within("focus xPercent", ...)` `?? ...("focus yPercent", ...)`
    over `StillFocus.MinPercent` and `MaxPercent`.
  - The class summary: "a subtitle, voice, zoom and focus that are absent or well formed".
- `Data/Content/AbsentWhenNullFilter`: `(typeof(CinematicStill), "focus")` beside `zoom`; the comment: "a still may
  have no subtitle, voice, zoom or focus".
- `docs/decisions/0046-cinematics-as-content-records.md`: an "Amended 2026-11-15" paragraph at the end of
  Consequences, written from decisions 746-751 (a still's optional focus point, a share of the shown picture; the zoom
  scales about it, the center without one; the rule `CinematicStill.ZoomOrigin` in Core, read by the game and the
  preview; a pan stays decision 618's backlog). `docs/decisions/README.md`'s 0046 line: "...seconds, subtitle, voice,
  zoom and an optional focus point (amended 2026-11-15)...".
- `docs/map.md`, Core `Cinematics` row: `StillFocus` (where a still's zoom heads, decision 746) and
  `CinematicStill.ZoomOrigin` (the point that stays put, read by the game and the preview, decision 751).

## Tests
Every focus below is a `new StillFocus { XPercent = x, YPercent = y }` built in the test, written "(x, y)".

`StillFocusTests` (new, `Core.Tests/Cinematics`; Glob found none):
- `Share_TwentyFiveAndSeventyFive_IsAQuarterAndThreeQuarters`.
- `Center_IsHalfAndHalf` (`Share` is (0.5, 0.5)).

`CinematicStillTests` (32 lines, extended), on its `_zooming` fixture:
- `ZoomOrigin_NoFocus_IsTheCenter`.
- `ZoomOrigin_WithFocus_IsItsShare` (`_zooming with { Focus = (20, 70) }` gives 0.2, 0.7, away from the center).
- `ScaleAt_WithFocus_IsUnchanged` (the focus moves the origin, never the scale: 1.05 at 2 s).

`CinematicDefinitionTests` (119, extended), on its `_home` fixture:
- `Equals_StillFocusDiffers_IsFalse`.

`CinematicCatalogValidatorTests` (113, extended):
- Four rows in `InvalidCinematics` through a new `WithFocus(int x, int y)` helper over `_second`:
  (-1, 50) "stills[1]: focus xPercent must be within 0..100 (was -1).", (101, 50) the same "(was 101)", (50, -1) and
  (50, 101) for `focus yPercent`.
- `Validate_FocusAtBothBounds_Passes` (theory: (0, 100) and (100, 0)).
- `Validate_FocusWithoutZoom_Passes` (`_second with { Zoom = null, Focus = (30, 62) }`, decision 750).
- A fifth `InvalidCinematics` row, `_second with { Zoom = null, Focus = (101, 50) }`, "stills[1]: focus xPercent must
  be within 0..100 (was 101).": the focus is checked on a still without a zoom too (the other side of the `??`).
- A sixth row, a bad zoom and a bad focus together (`Zoom = (100, 151), Focus = (101, 50)`), expecting the zoom's
  message, "stills[1]: zoom endPercent must be within 100..150 (was 151).": the zoom is named first.

`ContentCatalogLoaderCinematicTests` (108, extended), JSON written in the test beside the test world's cinematics:
- `Load_StillWithFocus_ReadsIt` (`"focus": { "xPercent": 30, "yPercent": 62 }` after a zoom; equal to
  `new StillFocus { XPercent = 30, YPercent = 62 }`).
- `Load_FocusMissingYPercent_ThrowsNamingTheFile` (`StartsWith("cinematics.json")`).
- `Load_StillWithoutOptionalFields_ReadsThemAsNull`: its tuple gains `still.Focus` (an added assertion, none removed).

`ContentWriterCinematicTests` (158, extended):
- `WriteMapCatalogs_StillWithEveryField_WritesThem`: its still gains `Focus = (30, 62)` (it loads back equal).
- `WriteMapCatalogs_StillWithFocus_WritesItsTwoPercentsAfterZoom` (the still's keys end `zoom`, `focus`; the focus's
  keys are `xPercent`, `yPercent`).
- Unchanged and now guarding the filter: `WriteMapCatalogs_StillWithoutOptionalFields_LeavesThemOut` (without the
  filter line it writes `"focus": null` and fails).

## Verify by hand
None: nothing draws the focus yet. `verify.ps1` passes; `git status` shows no content file changed.
