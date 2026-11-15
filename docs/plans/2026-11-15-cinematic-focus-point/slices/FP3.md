# FP3. The still's focus as the window edits it

Editor, about 210 lines (17 tests at about 10 lines, about 170, and about 45 of code). Needs FP1. Decisions 749, 750, 751, 753, 754. Design: "Changed
types", "Key decisions".

## Goal
The Cinematics window's view models hold a still's focus: a row keeps the record's point and writes it back, sets one
from a spot given as shares of the preview frame (rounded to whole percents and clamped to the picture), clears it,
names it in a line, and counts each change as a typed change (Play stops, the problem line refreshes). Play's preview
says which point of the frame its zoom keeps in place, by Core's rule. No window change yet (FP4): the slice is tested
headless.

## New types
None.

## Changes
- `Editor/Cinematics/CinematicStillRow` (161 lines, about 200 after):
  - `[ObservableProperty] [NotifyPropertyChangedFor(nameof(HasFocus), nameof(FocusText))]
    [NotifyCanExecuteChangedFor(nameof(ClearFocusCommand))] public partial StillFocus? Focus { get; private set; }`
    (summary: where the zoom heads, null for the center; set by a click, never typed, so never a problem: decision 750
    keeps it without a zoom).
  - `public bool HasFocus => Focus is not null;`
  - `public string FocusText`: "Zoom heads to 32 %, 62 %" with a focus; "Zoom heads to the center: click the picture to
    choose a point" without (decision 753).
  - `public void SetFocusAt(double xShare, double yShare)`: each share times 100, rounded half away from zero, clamped
    to `StillFocus.MinPercent`..`MaxPercent` (a click a pixel past the frame's edge is 100, decision 749); the shares
    are of the preview frame, which is the picture as the screen shows it (decision 747).
  - `[RelayCommand(CanExecute = nameof(HasFocus))] private void ClearFocus() => Focus = null;`
  - `partial void OnFocusChanged(StillFocus? value) => Changed?.Invoke(this, EventArgs.Empty);` beside the other
    hooks (decision 754: the list's relay stops Play and refreshes the problem line).
  - `From` copies `still.Focus`; `ToStill` sets `Focus = Focus` (the record is rebuilt with `new()`, so a missing copy
    would wipe it on OK).
  - The class summary names the focus.
- `Editor/Cinematics/CinematicPreview` (96 lines):
  - `[ObservableProperty] public partial Point ZoomOrigin { get; private set; } = new(0.5, 0.5);` (`System.Windows
    .Point`, shares of the frame; summary: the point Play's picture zooms about, by Core's `CinematicStill.ZoomOrigin`;
    the center while stopped).
  - `ShowMoment`: `ZoomOrigin = new Point(still.ZoomOrigin.X, still.ZoomOrigin.Y)`; `Stop`: back to (0.5, 0.5).
  - The class summary: "faded and zoomed about its focus by Core's rules".

## Tests
`CinematicStillRowFocusTests` (new, `Editor.Tests/Cinematics`; `CinematicStillRowTests` is at 178 lines), on a still
with a zoom and a focus (30, 62) built in the test:
- `From_StillWithFocus_KeepsIt`.
- `ToStill_WithFocus_CarriesIt` (the record's `Focus` is (30, 62), not the null default).
- `ToStill_NoFocus_IsNull`.
- `ToStill_FocusWithoutZoom_KeepsIt` (zoom boxes 100 and 100: `Zoom` null, `Focus` kept; decision 750).
- `SetFocusAt_SharesOfTheFrame_RoundsToWholePercents` (0.324, 0.616 gives (32, 62)).
- `SetFocusAt_PastTheEdges_ClampsToTheBounds` (theory, each side: (-0.01, 1.01) gives (0, 100), where 1.01 rounds to
  101 so the upper Y clamp is needed; (1.2, -3) gives (100, 0); and (0.5, 1.0008), a click a pixel past the frame's
  bottom, gives (50, 100)).
- `SetFocusAt_OnARow_RaisesChanged`.
- `ClearFocus_WithFocus_RemovesIt` (and `FocusText` says the center).
- `ClearFocusCommand_NoFocus_IsGreyed`.
- `ClearFocusCommand_FocusSet_RaisesCanExecuteChanged`.
- `FocusText_WithFocus_NamesThePercents` ("Zoom heads to 30 %, 62 %").
- `FocusText_FocusSet_RaisesPropertyChanged` (from a row without one, so the old text differs).

`CinematicPreviewTests` (99 lines, about 130 after): its `_intro` fixture's first still gains a focus (25, 75); the
second keeps none. The class comment ("a 2 s still zooming 100 to 110 %, then a 3 s one") names the focus: "a 2 s
still zooming 100 to 110 % toward 25 %, 75 %".
- `Advance_IntoTheFocusedStill_ZoomsAboutItsFocus` (`ZoomOrigin` (0.25, 0.75)).
- `Advance_IntoTheNextStillWithoutFocus_ZoomsAboutTheCenter` (after the focused one, so the old value differs).
- `Stop_AfterTheFocusedStill_ZoomOriginIsTheCenterAgain`.

`CinematicCatalogViewModelPlayTests` (93 lines):
- `SetFocusAt_WhilePlaying_StopsThePreview` (the chosen still's row; decision 754).
- `ClearFocus_RaisesTheProblemLine` (`PropertyChanged` for `Problem` when a set focus is cleared: the relay counts it
  as typed).

## Verify by hand
None: no window change. `verify.ps1` passes.
