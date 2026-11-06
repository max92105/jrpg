# SN1. Snap to grid while placing or moving

Builds on design.md, "Key decisions" 9 and Risks (Alt + Shift); decision 511; C8. Needs MO0, MO1 and MO2 (`MainWindow.ObjectDragging.cs`, `MoveSelectedObjects`).

## Goal
Objects stay free by default. Holding Alt while placing (click) or moving (Shift + drag, the whole selection too) lands the object's top-left on the nearest whole cell; the ghost shows where it will land as the mouse moves. A **Snap to grid (Alt, G)** check box in the Objects tool bar makes snapping the default; with it on, holding Alt places freely. G toggles the box in Objects mode (outside a text box), so a snapped Shift + drag never needs Alt + Shift. A snapped group moves so the grabbed object lands on whole cells, the others keeping their offsets from it.

## New types
- `Editor/Objects/GridSnap` (`internal static class`): where a top-left lands on the grid.
  - `public static Point2D TopLeft(Point2D topLeft)`: each coordinate to the nearest whole cell, a half going right and down (`Math.Floor(x + 0.5)`, the same for negative positions an object centered near the map's left edge has).

## Changes
- `Objects/ObjectPlacingViewModel` (about 210 after MO2, ends about 235):
  - `[ObservableProperty] public partial bool IsGridSnapOn { get; set; }` (off at the start: free placing stays the default); `[RelayCommand] void ToggleGridSnap()` (G).
  - `public void PlaceObjectOnGridAt(Point2D center)`: as `PlaceObjectAt`, the top-left through `GridSnap.TopLeft`; both share a private `Place(ObjectDefinition, Point2D topLeft)` (the off-map check stays on the center).
  - `public void MoveObjectOnGrid(string uid, Point2D to)`: `MoveObject(uid, GridSnap.TopLeft(to))`.
  - `public void MoveSelectedObjectsOnGrid(string grabbedUid, Point2D grabbedTo)`: the offset that lands the grabbed object on the grid, applied to the whole selection (`MoveSelectedObjects`).
  - Two methods per gesture, no flag argument (code-quality.md).
- `Shell/MainWindow.ObjectDragging.cs`:
  - `private bool IsSnapping => Placing.IsGridSnapOn != Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);` (the box, inverted by Alt; read on each mouse move and on release).
  - `ShowObjectDragAt` places the ghost at `GridSnap.TopLeft(...)` of its free top-left while snapping; the selection marks' travel too.
  - `FinishObjectDrag` calls the `...OnGrid` methods while snapping.
- `Shell/MainWindow.ObjectEditing.cs`:
  - `HandleObjectButton`: the move branch accepts Shift with or without Alt (`(Keyboard.Modifiers & ~ModifierKeys.Alt) == ModifierKeys.Shift`); a plain or Alt click places, through `PlaceObjectOnGridAt` while snapping. Ctrl stays exact (Ctrl + Alt is AltGr on some layouts).
  - `HandleObjectKey` (a switch since MO1): `Key.G` runs `ToggleGridSnapCommand` (Objects mode, no text box focused, its guard already); `HandleToolKey` (B, R, F, [, ]) doesn't take G, so nothing clashes.
- `Shell/MainWindow.xaml`, Objects tool bar: `<CheckBox Content="Snap to grid (Alt, G)" IsChecked="{Binding Placing.IsGridSnapOn}" ToolTip="Placed and moved objects land on whole cells. Holding Alt does the opposite of this box; G ticks or unticks it." />` (+1 line, ends about 279).
- `Shell/ControlsHelpWindow.xaml`, Objects: "Alt, held while clicking or Shift + dragging: snap the object's top-left to the grid (the ghost shows where). Snap to grid in the Objects tool bar, or G: snap by default; Alt then places freely."
- The ghost follows Alt as the mouse moves (the developer's gesture holds Alt while moving); pressing Alt with the mouse still shows the snapped ghost at the next move. No key handler is added, so `MainWindow.xaml.cs` doesn't change.

## Tests
New `Objects/GridSnapTests`:
- `GridSnapTests: TopLeft_JustUnderHalf_RoundsDown` ((3.49, 2.2) → (3, 2)).
- `GridSnapTests: TopLeft_Half_RoundsToTheNextCell` ((3.5, 2.5) → (4, 3)).
- `GridSnapTests: TopLeft_NegativeOverMinusHalf_RoundsToZero` ((-0.4, 0) → (0, 0)); `TopLeft_NegativeUnderMinusHalf_RoundsToMinusOne` ((-0.6, 0) → (-1, 0)).
- `GridSnapTests: TopLeft_WholeCell_StaysPut`.

New `Objects/ObjectPlacingSnapTests` (the test world's forest, a 2 x 2 object chosen):
- `ObjectPlacingSnapTests: IsGridSnapOn_AtTheStart_IsOff`.
- `ObjectPlacingSnapTests: ToggleGridSnap_Twice_TurnsItOnThenOffAndRaisesIt`.
- `ObjectPlacingSnapTests: PlaceObjectOnGridAt_FreeCenter_PlacesOnWholeCells` (center (5.3, 6.8) → top-left (4, 6)).
- `ObjectPlacingSnapTests: PlaceObjectOnGridAt_CenterOffTheMap_PlacesNothing`.
- `ObjectPlacingSnapTests: MoveObjectOnGrid_FreeTarget_LandsOnWholeCells`.
- `ObjectPlacingSnapTests: MoveSelectedObjectsOnGrid_GrabbedLandsOnTheGrid_OthersKeepTheirOffsets` (the other one at a free (7.25, 3.5), moved by the same offset).
- `ObjectPlacingSnapTests: MoveSelectedObjectsOnGrid_Undo_PutsAllBackInOneStep`.

Size: about 10 lines for `GridSnap`, 33 in the view model, 28 in the window, 3 in XAML and F1; 12 tests (about 130): about 205.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `GridSnap` (where a top-left lands on the grid; Alt or the Snap to grid box).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Objects tab; choose an object in the list. The Objects tool bar shows **Snap to grid (Alt)**, unticked.
2. Move the mouse over the map: the ghost follows freely. Hold Alt and move: the ghost jumps cell by cell, its top-left on the grid lines. Click with Alt held: it lands where the ghost was.
3. Hold Shift + Alt and drag a placed object: it moves cell by cell; let go. (If your keyboard layout switches, this is Windows' Alt + Shift shortcut: tick the box instead and drag with Shift only.)
4. Tick **Snap to grid**: plain clicks land on the grid; Alt + click lands freely. Press G (the map focused): the box unticks; G again ticks it. Shift + drag with the box ticked moves cell by cell without Alt.
5. Ctrl + drag two objects, untick the box, Shift + Alt + drag one of them: the grabbed one lands on the grid, the other keeps its distance to it. Ctrl+Z puts both back.
6. F1: the Objects section has the Alt and G line. Type a "g" in the map list's filter box: it is typed, the box doesn't change.
