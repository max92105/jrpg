# EW4. The step list beside the chosen step's fields

Editor, about 120 lines, XAML only. Design: section 1 (the sizes), section 3 (the "After" sketch and the warp row),
risk "Width". Decision 733. Needs EW2 (the single row of step buttons).

## Goal
The chosen event's steps sit in a list on the left and the chosen step's fields on the right, both the height of the
window, instead of the list squeezed over the fields. The windows open bigger and never shrink below that: the object
window at 1040 x 620 with a 260 px event list, the cell window at 760 x 560. The dialogue's lines box grows to 160 px.
A warp step's Pick... and Go to move to a line of their own, so the warp row fits the fields column (about 436 px at
the object window's minimum; the row was about 460 px).

## New types
None.

## Changes
- `Events/EventView.xaml`: under the id line and the conditions, the steps' heading row ("Steps, run in order", then
  **Add step ▾**, Remove, Up, Down); under it a `Grid` of two columns: the step list (280 px, design section 1) and,
  with a 12 px gap, the `EventStepEditor` in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"` and
  `HorizontalScrollBarVisibility="Auto"` (a long move path, a dialogue or a row wider than the column scrolls instead
  of being cut). Its comment says why both.
- `Events/EventStepEditor.xaml`: the lines box's `Height="160"`; the warp `StackPanel` becomes two lines: "Warp to",
  the map box, "at cell (", the two cell boxes, ")" on the first; **Pick...** and **Go to** on the second, under the
  map box (left margin as "Warp to"'s width). Their tooltips and `Click` handlers unchanged.
- `Events/ObjectEventsWindow.xaml`: `Width="1040" Height="620" MinWidth="1040" MinHeight="620"`; the event column's
  `Width="260"`.
- `Events/CellEventWindow.xaml`: `Width="760" Height="560" MinWidth="760" MinHeight="560"`.
- `Shell/ControlsHelpWindow.xaml`: nothing (no gesture changes).
- Greps for the old words in comments of `Events/` and `docs/` ("under the list", "820", "560"): none expected outside
  this plan; fix any found.

## Tests
None new: the slice moves XAML only. Every test passes unchanged. The script's captures before (EW3's run) and after
are compared side by side and not committed.

## Verify by hand
1. The implementer: `open-editor-windows.ps1 -EventWindows`: both captures show the step list left and the fields
   right, at the new sizes; no binding-error line beyond EW0's baseline.
2. The developer (F5): **Events** tab, click an NPC with a talk event, choose its dialogue step: the list keeps its
   rows, the lines box shows about eight lines.
3. Choose a warp step: the map box and cell boxes on one line, **Pick...** and **Go to** under them, nothing cut
   off. **Pick...** opens the picker; **Go to** shows the destination map (`interior_1` or another id with an
   underscore, named whole in the box).
4. Choose a Move NPC step and add twenty path steps with **Right**: the path text wraps and the fields scroll, the list
   stays.
5. Drag a corner of either window inward: it stops at its opening size.
