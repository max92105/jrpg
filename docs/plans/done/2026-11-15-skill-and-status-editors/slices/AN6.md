# AN6. Strip from sheet cells: the window, picking and the preview, opened from the Effect strips page

Editor + script, about 220 lines (XAML about 90, code-behind about 80, the script about 15, about 30 of tests).
Decisions 976, 990, 996. Design: design.md, section 2.7 ("Strip from sheet cells"). Builds on AN5, AN1b and row 5's
SW3 (`Sprites/SequenceMarks`).

## Goal
A "Strip from sheet cells..." button on the Effect strips page opens the tool over it: Open sheet... reads a PNG from
anywhere; the sheet shows zoomable with its grid and its column and row numbers; with Pick on, a click adds the cell
under it, numbered in pick order on the sheet, and a click on a row's number adds the whole row; the shared preview
plays the picked cells at the typed pace. OK isn't there yet (AN7 writes the strip): he can already see which cells of
a six-effect sheet make one effect, and at which pace. The smoke script opens the tool through the page.

## New types
- `Editor/Animation/StripFromCellsWindow` (`Window`, about 1000 x 640, `EditorWindow` style): a plain modal window (it
  edits no catalog, so it is no page): left, Open sheet... and the file line over the sheet in a
  `Viewport/ZoomedScroller` with `Sprites/SpriteSheetMarks` (grid and numbers) and `Sprites/SequenceMarks` (the picks
  numbered) over it; right, Columns, Rows, the cut line, Pick (a `ToggleButton` `BasedOn` the theme's button), Remove
  last, Clear, the picks line, a `FramePreview` (320 x 360) with `FrameMilliseconds` bound one-way, Frame time, Strip
  name, the problem line, Cancel.
  - The sheet is read under one catch naming every refusal of the read and its decoding (`InvalidDataException`,
    `IOException`, `UnauthorizedAccessException`, `NotSupportedException`), the refusal on the file line
    (`.claude/rules/editor.md`); a click on the image goes to `PickCellAt` in image pixels (`e.GetPosition(SheetImage)`,
    the image sized in its own pixels under the zoom, as SW3's), and a click on a row's number (the area
    `SpriteSheetMarks` draws it in, through its new `RowAt(Point point, double scale)`) to `PickRow`.
  - On `PicksChanged`: the preview's frames `PreviewFrames.OfCells(sheet, cellSize, picks)` and the marks redrawn.

## Changes
- `Editor/Animation/PreviewFrames`: `OfCells(BitmapSource sheet, (int Width, int Height) cell,
  IReadOnlyList<SpriteCell> cells)`: each picked cell cut (`CroppedBitmap`), a cell outside the sheet skipped.
- `Editor/Sprites/SpriteSheetMarks`: `int? RowAt(Point point, double scale)`: the row whose number is drawn under the
  point (the same rectangle it draws the number in), null elsewhere.
- `Editor/Battle/EffectStripCatalogPage`: a "Strip from sheet cells..." button under Import..., tooltip "Pick cells
  of any sheet, in order, and make them a strip of this folder with its pace"; a constructor parameter `Action<Window>
  openStripFromCells` (the shell's opener, given the page's window as owner; AN7 turns it into a write that returns the
  import's result).
- `Editor/Shell/MainWindow.Database.cs`: the Effect strips registration passes the opener (the window over the owner).
- `Editor/Shell/ControlsHelpWindow.xaml` (114): one line after the Effect strips line: "Database > Effect strips...,
  Strip from sheet cells...: Open sheet, set columns and rows, then with Pick on click cells in the order they play,
  or a row's number for the whole row; the preview plays them."
- `.claude/scripts/open-editor-windows.ps1`: a nested entry form, `'Database>Effect strips...>Strip from sheet
  cells...'`: the page's dialog opened as today, then the button of that name found under it
  (`AutomationElement.NameProperty`) and invoked, the nested window captured and closed before its owner (about 15
  lines); the entry joins the default list (decision 996). There is no Database menu entry for the tool.

## Tests
- `Editor.Tests/Animation/PreviewFramesTests`: `OfCells_TwoPicksOfOneCell_CutsItTwice`,
  `OfCells_CellOutsideTheSheet_Skipped` (on `StaThread`, a sheet built with `BitmapSource.Create`).
- `Editor.Tests/Sprites/SpriteSheetMarksTests` (glob first: extend the file if it exists, else new):
  `RowAt_PointOnRowTwosNumber_IsRowTwo`, `RowAt_PointInsideACell_IsNull`.

## Verify by hand
The repository's `content/art/effects/FireExplosion.png` (8 frames in a row, generated art an agent may view) serves
as the sheet.
1. Database > Effect strips..., Strip from sheet cells..., Open sheet..., `content/art/effects/FireExplosion.png`:
   Columns 8, Rows 1, "8 x 1 cells of 443 x 443 px"; the problem line asks for picks.
2. Click cells 8, 7, 6, 5, 4, 3, 2, 1: they show 1 to 8 in pick order; the preview plays the explosion backwards; type
   60 in Frame time: faster. Clear; click the row's number: cells 1 to 8 picked in order. Remove last works; a click
   with Pick off adds nothing.
3. Ctrl + wheel zooms; the numbers keep their size and a click still picks the cell under the mouse. Cancel.
4. Open a file that isn't a PNG (a text file renamed `.png`): the file line names the refusal; the window stays open.
5. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Effect strips...>Strip from sheet cells...: opened", no
   `ERROR` or binding-error line.
- **Done, with deviations:** code 287 lines (window 214; `SpriteImageFile.Read` the one PNG read, TileImages too); tests
  72; the event-window run moved to `open-event-windows.ps1`. `RowAt` also takes the heights; Pick is a CheckBox (no
  ToggleButton style can base on Button's); help line under a new heading; window 1000 x 680.
