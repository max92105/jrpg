# SI4. The Character sprites window

Editor, about 230 lines (window code and XAML, and the status lines' move; the rules are SI2 and SI3's). Builds on SI3. Decision 568.

## Goal
Objects > **Character sprites...** opens a window: the sprite list on the left with **Import...** and **Copy setup from...** under it; in the middle the chosen image, zoomable (`Viewport/ZoomedScroller`, `CellZoomBar`), its frame grid drawn and its rows numbered from 1 on the left edge; on the right the setup boxes (Columns, Rows, frame size shown, Down / Left / Right / Up rows, Walk frames, Frame time (ms), Scale, Feet (px)), the problem line, and the four facings walking side by side, each at its scale over a 48 px cell outline so the feet line can be set by eye. OK applies (greyed while a problem shows), Cancel drops the session.

## New types
- `Editor/Shell/StatusBand` (`.xaml` + `.xaml.cs`, a `UserControl` on the main view model): the load error, art warning and save error lines moved out of `MainWindow.xaml` with no change, first (the backlog's rule: the window's next control goes in its own user control).
- `Editor/Sprites/FieldSpriteWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the window above. The preview redraws from `TileAnimationTime.Elapsed` on WPF's render tick (`CompositionTarget.Rendering`), only when a facing's `PreviewCellAt` changed; the handler is removed on `Closed`. Copy setup from... opens a small list of `CopySources`.
- `Editor/Shell/MainWindow.FieldSprites.cs` (partial): `OnCharacterSpritesClicked` (start, show, apply) and the Import... file dialog (PNG filter), as `MainWindow.SheetFiles`.
- `Editor/Shell/MainViewModel.FieldSprites.cs` (partial): `FieldSpriteEditing` and `CanEditFieldSprites`; the constructor gains one line.

## Changes
- `Shell/MainWindow.xaml` (293): the status lines become `<local:StatusBand DockPanel.Dock="Top" />` (283), then one line under Objects: `Character sprites...` with a tool tip ("Import, cut and animate character sheets; New NPC uses them.") (284).
- `docs/plans/backlog.md`: the line on `MainWindow.xaml` at 293 updated to its new count.

## Tests
None new: window code; SI2 and SI3 test the rules; the status band move is checked by hand.

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`: rename an art file the content names, start: the art warning shows as before; put it back.
2. Objects > Character sprites...: `npc_placeholder.png` and `player.png`, set up; choose `player.png`: 8 columns, 4 rows, 48 x 48, rows 1 2 3 4, 8 frames, 100 ms, scale 1; the preview walks the four facings as in the game.
3. **Import...**, pick the bought sheet in `C:\git_perso\art-packs\...`, Open: listed "(not set up)" and chosen; 4 columns, 7 rows, the grid lines between the frames (fix Columns / Rows if not), Scale 3.
4. Set Down, Left, Right, Up to the walk's rows as numbered on the image; the four characters walk the right way. Move Feet until the feet touch the outline's bottom.
5. Type Columns 3: the problem line names the pixels, OK greys; set it back.
6. Import a second character sheet of the same pack, **Copy setup from...** the first: every box filled, the preview walks.
7. OK. Ctrl+Z: the records go; Ctrl+Y. Ctrl+S: `git diff content/field-sprites.json` shows the new records, no other catalog file changed.
8. Add each image's line to `content/art/art-sources.json` and its source to `content/art/CREDITS.md` (ADR 0038), or `verify.ps1` fails `ArtSourcesTests`; or revert with `git checkout -- content` and delete the copied PNGs.

- **Done, with deviations:** the walk rows are combo boxes over `RowChoices` numbered from 1, the box's index being the row (SI2's `RowChoices` now lists 1..N); the marks and the walking preview are two small static drawers (`SpriteSheetMarks`, `WalkPreview`) beside the window; Import... runs through `MainViewModel.ImportFieldSprites`, which returns the result for the window to relist and raises the shell's `ImagesImported` (the tilesheet import's event, renamed) so the replaced pictures are dropped and redrawn as for tiles; the window takes four arguments, no tile images; the help window gained no line (a menu entry, no gesture). The review: the walking preview is one `WalkPreview` per window (no static state), the record built once per change and only the walk cells compared on the render tick; the walk row boxes bind through `RowIndexConverter`, which drops the -1 a combo box writes back while its list is rebuilt; the SI2 Warning on an image with spare pixels (`From` dividing down) is accepted and noted in the backlog.
