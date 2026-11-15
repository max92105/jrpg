# SW3. The window: Shows boxes, picking on the image, the marks and the pace line

Editor, about 220 lines (draw and wiring, plus about 20 lines of tests). Decisions 771-775. Builds on SW2.

## Goal
In the Character sprites window each facing's line reads "Down: [Shows] [what]": Row shows the row box, Mirror of the
box of the three other facings, Cells the picked cells as text with Pick (a toggle), Remove last and Clear. While a
facing picks, a click on the image adds the cell under it; the image numbers the picking facing's cells in pick order
at each frame's bottom-right (a repeated cell shows each number, "2, 4") and numbers the columns from 1, centered on
each column's top edge (decision 773). "Walk frames" becomes "Frames per row", greyed while no facing reads its row.
Beside the Frame time box a pace line reads "150 ms a frame, 6.7 frames a second", following the box as he types (the
preview already walks at the typed frame time). The setup panel scrolls when the window is short. The F1 list says
how. After this slice the developer can set the merchant's Right as "Mirror of Left" (decision 775).

## New types
- `Editor/Sprites/SequenceMarks` (internal static): drawn over the sheet: the picking facing's cells, each outlined
  and numbered in pick order at its frame's bottom-right, at the shown scale (lines and numbers keep their screen
  size, as `SpriteSheetMarks`). `Draw((int Width, int Height) imageSize, (int Width, int Height) frame,
  IReadOnlyList<SpriteCell> cells, double scale)`; `PickNumberPixels` (design.md, "Tunables").
- `Editor/Sprites/SameItemConverter` (internal sealed, `IMultiValueConverter`): true when its two values are the same
  object; a line's Pick toggle is checked when the line's facing is `Walk.PickingFacing`.

## Changes
- `Editor/Sprites/FieldSpriteSetupViewModel`: `PaceText` (empty while the frame time is below 1; frames a second to
  one decimal, invariant culture), raised with `Walk.FrameMilliseconds`.
- `Editor/Sprites/SpriteSheetMarks`: each column's number from 1, centered on its column's top edge, in the row
  numbers' brush and size (the method extracted for it keeps `DrawGrid` short).
- `Editor/Sprites/FieldSpriteWindow.xaml`:
  - `Window.Resources` gains `<BooleanToVisibilityConverter x:Key="VisibleWhenTrue" />` and `SameItemConverter`;
  - the `Grid.Column="3"` setup panel sits in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"`;
  - the "Walk rows" block becomes four facing lines: label, Shows box (`FacingSourceChoice.All` by `SelectedValue`),
    then by the `Is...` flags through `VisibleWhenTrue` the row box, the mirror box, or the cells line (the
    `CellsText` with `TextWrapping="Wrap"`, a `ToggleButton` "Pick" whose `IsChecked` is a one-way `MultiBinding` of
    the line's facing and `Walk.PickingFacing` through `SameItemConverter` and whose `Command` is
    `Walk.TogglePickingCommand` with the line's facing, "Remove last" and "Clear" bound to the commands); local styles
    stay `BasedOn` the theme's;
  - "Walk frames" relabelled "Frames per row", enabled by `Walk.IsAnyFacingOnRow`, tooltip "The frames a facing on its
    row walks through, from column 1"; the pace line beside "Frame time (ms)";
  - a third image `PickMarks` over `SheetMarks` in `ImageView`; the image's tooltip "With Pick on, click a frame to add
    it".
- `Editor/Sprites/FieldSpriteWindow.xaml.cs`: `MouseLeftButtonDown` on `ImageView` → `_setup.PickCellAt` with
  `e.GetPosition(SheetImage)` (image pixels: the image is sized in its own pixels under the zoom's layout transform);
  `PickMarks` redrawn on a setup change and in `DrawSheet` (zoom), cleared with no picking facing; the summary names
  the picking.
- `Editor/Shell/ControlsHelpWindow.xaml` (114 lines): one line after "NPCs > New NPC": "NPCs > Character sprites...:
  each facing's Shows box (Row, Cells, Mirror of); with Cells, Pick then click frames on the image to add them in
  order; Remove last, Clear. The preview walks at the Frame time typed."

## Tests
`FieldSpritePaceTests` (new, `Editor.Tests/Sprites/`; a setup made with `ForNew` on a size given in the test):
`PaceText_150Milliseconds_ReadsSixPointSevenFramesASecond` (asserts the whole line); `PaceText_FrameTimeZero_IsEmpty`.
The rest draws and wires (the plan lessons' draw-only rule); SW2's tests hold the rules.

## Verify by hand
The window shows the sheets, so steps 1-8 are the developer's; the implementer runs step 9 and reads only its lines.
1. `dotnet run --project src/Jrpg.Editor`, NPCs > Character sprites..., choose `npc_placeholder.png`.
2. Up's Shows box: Cells. Pick is on and Up's row run is numbered 1-8 on row 4 of the image, each number at its
   frame's bottom-right; columns are numbered 1-8 centered along the top.
3. Clear; click the frames at columns 1, 2, 3, 2 of row 4: they show 1, 2, 3 and "2, 4"; Up's text reads
   "4 cells: (1, 4) (2, 4) (3, 4) (2, 4)"; the preview's Up figure walks 1, 2, 3, 2.
4. Ctrl + wheel to zoom: the numbers keep their size and sit on their frames; a click on a zoomed frame still adds
   the frame under the mouse. Remove last: "3 cells".
5. Left: Cells, then Up's Pick: Left's Pick turns off, Left's numbers go, Up's show. Up back to Row: row 4 again, and
   Pick is off everywhere.
6. Right: Mirror of, Left: the third figure walks as the second, flipped. Every facing off Row: "Frames per row"
   greys.
7. The pace line reads "100 ms a frame, 10.0 frames a second"; type 250 in Frame time: the figures slow at once and
   it reads "250 ms a frame, 4.0 frames a second".
8. Make the window as short as it goes: the setup panel scrolls and the cells text wraps. Cancel. F1: the new line is
   under "NPCs > New NPC". Then, his content call (decision 775): choose `npc merchant A v00.png`, set Right (and
   Up if its row is wrong) as he sees fit, OK, File > Save all, and check the milestone map's merchant in the game.
9. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
