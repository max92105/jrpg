# FL3. The painted levels marked; the picker widens with a drag bar

Builds on design.md, "Key decisions" 4, 6 and 7; decisions 491, 494 and 495. Needs FL2 (`LevelChoice`, the fixed list).

- **Done:** as planned.

## Goal
1. **The marks:** in the composer's Level box, every level holding a painted cell reads with a mark ("2 •", "0 (base) •", "-1 •"), the others plain. The marks follow every stroke, undo, redo and resize, and show a stacked object's levels as soon as Edit object opens it. The list itself never changes, so the chosen level stays chosen.
2. **The drag bar:** a thin bar between the composer's picker and its canvas side. Dragging it right widens the picker, which shows more of the sheet at the same zoom; its horizontal scroll bar goes once the whole sheet fits. The picker starts at 420 px each time the composer opens, never narrower than 300; the canvas side never narrower than 400, so its boxes row stays whole.

Merged here rather than its own slice: the bar is about ten lines of XAML with nothing headless to test, and this slice's hand check has the composer open anyway.

## New types
None.

## Changes
- `Editor/Objects/Composing/LevelChoice` becomes `internal sealed partial class LevelChoice(int level) : ObservableObject` (summary: ... marked while a cell is painted on it):
  - `[ObservableProperty] [NotifyPropertyChangedFor(nameof(Text))] public partial bool IsPainted { get; set; }`;
  - `string Text`: `Name`, then " •" while `IsPainted` (placeholder look).
- `Editor/Objects/Composing/ObjectCanvas`: `public bool HasCellsOn(int level)`: whether a cell is painted on the level.
- `Editor/Objects/Composing/CanvasPainting`: `partial void OnCanvasChanged(ObjectCanvas value)` sets each choice's `IsPainted` to `value.HasCellsOn(choice.Level)` (it runs for the start canvas too, `LevelChoices` being initialized before the constructor's body).
- `Editor/Objects/Composing/ObjectComposerWindow.xaml`:
  - the Level box's `DisplayMemberPath="Text"`; its tool tip ends "A • marks the levels painted on.";
  - the grid's columns: `<ColumnDefinition Width="420" MinWidth="300" />` (the side panel's minimum for the same view: below it the Cut panel's grid clips), `<ColumnDefinition Width="Auto" />`, `<ColumnDefinition Width="*" MinWidth="400" />` (the Columns, Rows, Erase, Undo and Redo row's width);
  - in column 1: `<GridSplitter Grid.Column="1" Width="5" Margin="4,0" HorizontalAlignment="Stretch" ResizeBehavior="PreviousAndNext" ToolTip="Drag to widen the sheet or the canvas." />`, themed by `EditorTheme.xaml`'s `GridSplitter` style as the main window's bars are.
- `Editor/Shell/ControlsHelpWindow.xaml`, Objects: the Level line gains "a • marks the levels painted on"; a new line: "In the composer, drag the bar between the sheet and the canvas to widen either."

## Tests
- `ObjectCanvasLevelTests: HasCellsOn_PaintedLevel_IsTrue` (a cell on -1); `HasCellsOn_EmptyLevel_IsFalse` (the base painted, level 1 asked: the other side).
- `CanvasPaintingLevelTests`:
  - `LevelChoices_PaintedUnderAndOverTheBase_MarksThoseLevelsOnly` (cells on -1 and 2: their entries read "-1 •" and "2 •", "0 (base)" plain).
  - `PaintAt_OnAnEmptyLevel_RaisesItsChoicesText` (the entry's `PropertyChanged` for `Text`, the relayed arm a bound box reads).
  - `Undo_OnlyStrokeOnALevel_ClearsItsMark` (the other side: the mark goes; FL2's `Undo_OnlyStrokeOnTheChosenLevel_KeepsTheLevelChosen` already pins the choice).
- `ObjectComposingLevelTests: StartEdit_StackedObject_MarksItsPaintedLevels` (the chest with a tile copied onto level 2: "0 (base) •" and "2 •" marked, "1" plain).
- The drag bar has no headless test (XAML only); the hand check covers it.

Size: about 25 lines of code and 12 of XAML, 6 tests (about 80): about 120.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/LevelChoice` gains "marked while painted"; `Composing/ObjectComposerWindow` gains "its picker widened with a drag bar".

## Hand check (yours)
1. Start the editor ("Game and editor" profile, F5).
2. Objects tab: choose FL2's `free_levels_test` (or any object with tiles on two levels); Objects > **Edit object...**. Open the Level box: "3 •", "0 (base) •" and "-1 •" are marked, the others plain; "0 (base) •" is chosen. Click it to close the list.
3. Choose "5". In the picker click any cell; click a canvas cell: the box now reads "5 •". Press Ctrl+Z: it reads "5", still chosen.
4. Move the mouse over the thin bar between the picker (left) and the canvas side: the pointer becomes a left-right arrow. Drag it to the right: the picker widens and the sheet shows more columns at the same zoom ("48 px" in the picker's bar is unchanged). Keep dragging until the whole sheet width shows: the horizontal scroll bar under the sheet goes.
5. Keep dragging right: the bar stops while the Columns, Rows, Erase, Undo and Redo row is still whole on the canvas side.
6. Drag the bar to the left: it stops at the picker's minimum, with the sheet box and the zoom bar still whole.
7. With the picker wide, click a picker cell and click a canvas cell: the cell lands where clicked. In the picker, Ctrl + wheel over the sheet: the zoom still keeps the cell under the mouse.
8. Maximize the composer window (double-click its title bar): the canvas side takes the new room, the picker keeps its width.
9. Press **Cancel**. Objects > **Edit object...** again: the picker is back at its starting width. Press **Cancel**.
10. In the picker of a New object, choose an image "(not set up)" in the sheet box (if any), press **Set up**: the Cut panel's Columns, Rows, Margin and Spacing boxes are whole at the picker's minimum width (drag the bar fully left first); press **Cancel** in the Cut panel.
11. Press F1: the Objects section has the "•" mark and the drag bar lines.
12. If you don't keep `free_levels_test`: choose it in the Objects tab, press **Remove** (after removing its placement from the map), then Ctrl+S.
