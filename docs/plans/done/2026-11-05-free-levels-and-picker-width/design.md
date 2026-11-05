# Free levels and the picker's width: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Lowest level | `Core/Maps/ObjectTile.MinLevel` | -7 | The content check's lower bound and the Level box's last entry. Technical limit, not design. |
| Highest level | `Core/Maps/ObjectTile.MaxLevel` | 7 (unchanged) | The content check's upper bound and the Level box's first entry. |
| Painted mark | `Editor/Objects/Composing/LevelChoice.Text` | " •" after the name | Shows which levels hold a painted cell. Placeholder look. |
| Picker's starting width | `ObjectComposerWindow.xaml`, column 0 | 420 px (unchanged) | Where the drag bar starts each time the composer opens. |
| Picker's minimum | `ObjectComposerWindow.xaml`, column 0 `MinWidth` | 300 px | As the main window's side panel (`SidePanelMinWidth`), which shows the same view: the Cut panel's grid (about 240 px) and the zoom bar fit. |
| Canvas side's minimum | `ObjectComposerWindow.xaml`, column 2 `MinWidth` | 400 px | The Columns, Rows, Erase, Undo and Redo row is about 375 px; 300 would clip it. |

## Design overview

### What exists
- **The format** (`Core/Maps/ObjectTile(X, Y, TileId, Level = 0)`, `MaxLevel = 7`): `Data/Maps/ObjectCatalogValidator.FindSingleTileProblem` refuses a level outside 0 to 7 ("levels run from 0 to 7."); `FindTileListProblem` walks the list once with `previousLevel` **starting at 0**, refusing a tile below the previous one's level and a second tile on one (x, y, level). `AbsentWhenZeroFilter` leaves `level` out only while 0, so a negative level is written. Nothing else in `src/` reads `ObjectTile.Level` or `MaxLevel` but the editor's composer (grep, 2026-11-05).
- **Every drawer draws the list in order** (`Game/Exploration/MapView.DrawObject`, `Game/Rendering/ObjectSpriteBaker.CanopyPixels` composing "over", `Editor/Maps/MapDrawing.DrawObjectAt`): a list sorted lowest level first draws a negative level under the base with no change.
- **The composer** (`Editor/Objects/Composing/`):
  - `ObjectCanvas`: `Cells` keyed by `CellOnLevel`; `Stamp(level, at, cells)` and `Erase(level, at)` throw outside 0 to `MaxLevel` (`ThrowIfOutsideTheLevels`); `HighestLevel` (-1 when blank, read only by `CanvasPainting`); `Of(definition, tiles)` keys each tile by its level with no range check; `TilesOf` orders by level ascending, then row, then column.
  - `CanvasPainting`: `Level` is an **index into `LevelChoices`** (which happens to equal the level while the list starts at 0), a hand-written setter ignoring a value outside the list; `LevelChoices` is a list of strings rebuilt in `OnCanvasChanged` to end one above `HighestLevel`, which also clamps `Level` after an undo.
  - `ObjectCanvasDrawing.Draw(canvas, chosenLevel, imageOf, gridLineWidth)`: orders `Cells` by level and fades `at.Level > chosenLevel`: already right for negative levels.
  - `ObjectComposerWindow.xaml`: the Level box binds `ItemsSource` to `Painting.LevelChoices` and `SelectedIndex` to `Painting.Level`; the window's grid is `420 | 12 | *` with the picker (`TileBrowserView`) in column 0 and the canvas side in column 2; the window is resizable (no `ResizeMode` set), 1100 x 720.
- **The picker's sheet** sits in a `ScrollViewer` with both bars `Auto` (`Tiles/TileBrowserView.xaml`): the horizontal bar already shows only while the sheet is wider than the view.
- **The main window's splitters** (`Shell/MainWindow.xaml`): an `Auto` column holding `GridSplitter Width="5" HorizontalAlignment="Stretch" ResizeBehavior="PreviousAndNext"`, themed by `EditorTheme.xaml`'s `GridSplitter` style.

### Core changes
- `Core/Maps/ObjectTile`: `public const int MinLevel = -7`; the summary says a level runs from `MinLevel` to `MaxLevel`, 0 the base, a lower level drawn under a higher one (ADR 0040 and its amendment). `MaxLevel`'s summary loses "far past a wall, a window and a sign" in favour of one line covering both bounds.

No rule moves into Core: the range is content shape; its check is Data's.

### Structure and folder dependencies

New types:

| Where | Type | Responsibility |
|---|---|---|
| `Editor/Objects/Composing` | `LevelChoice` | One entry of the composer's Level box: a level and how the box shows it (FL2), marked while painted (FL3). |

Changed: `Core/Maps/ObjectTile`, `Data/Maps/ObjectCatalogValidator`, `Editor/Objects/Composing/ObjectCanvas` (the range; `HighestLevel` becomes `HasCellsOn`), `CanvasPainting` (`Level` a level number, `LevelChoices` fixed), `ObjectComposerWindow.xaml` (the box bound by value, the drag bar), `Editor/Shell/ControlsHelpWindow.xaml` (the Level line, the drag bar line), `docs/decisions/0040-object-tile-levels.md` and `docs/decisions/README.md` (the amendment), `docs/map.md`, `docs/plans/backlog.md`.

Folder edges: none new. `Objects/Composing -> Core.Maps` exists (`ObjectTile.MinLevel`, `MaxLevel`); `Data/Maps -> Core.Maps` exists. No package, no project reference.

### Key decisions
1. **The range is two named constants, and the order check starts at the lowest one.** `FindSingleTileProblem` refuses `tile.Level is < ObjectTile.MinLevel or > ObjectTile.MaxLevel` ("levels run from -7 to 7.", both numbers from the constants). `FindTileListProblem` starts `previousLevel` at `ObjectTile.MinLevel`: at 0, a list starting on -1 would be refused as "listed after a tile on level 0", which no tile is. The range check runs first, so the order check only ever compares levels inside the range.
2. **`CanvasPainting.Level` is the level itself, a plain observable property** (`[ObservableProperty] public partial int Level { get; set; }`, 0 at the start). The index mapping goes: with -7 to 7 an index can no longer equal the level, and -1 is a level, so the old -1 guard would refuse the developer's first request. No guard replaces it: the box can only push one of its fifteen levels (below), and `ObjectCanvas.Stamp` still throws outside the range.
3. **The Level box is bound by value to a fixed list.** `CanvasPainting.LevelChoices` is `IReadOnlyList<LevelChoice>`, made once per painting by `LevelChoice.TopDown()` (7 first, -7 last), never replaced. The box binds `SelectedValue="{Binding Painting.Level}"` with `SelectedValuePath="Level"` and `DisplayMemberPath` (`Name` in FL2, `Text` in FL3). A list that never changes never clears the box's selection, so nothing is pushed back to `Level` but a level from the list. Chosen over `SelectedIndex` with a mapping (index = `MaxLevel - level`): the mapping would be a second rule beside the list's order, and the marks would still need the list rebuilt or the items observable.
4. **The marks are a property of each entry, not a rebuilt list** (FL3). `LevelChoice` becomes an `ObservableObject` with `IsPainted` (`[NotifyPropertyChangedFor(nameof(Text))]`) and `Text` (`Name`, then " •" while painted). `CanvasPainting.OnCanvasChanged` sets each entry's `IsPainted` from `ObjectCanvas.HasCellsOn(level)`, after every stroke, undo, redo, resize and at the start (Edit object's stack). The box's shown entry follows its `Text` change through the binding.
5. **No clamp, no derived choices.** `OnCanvasChanged`'s rebuild and clamp leave in FL2 (they come back in FL3 as the marks only); an undo or redo leaves `Level` as chosen, even on a level the undo emptied.
6. **`ObjectCanvas.HighestLevel` becomes `HasCellsOn(int level)`.** Its only reader was the rebuild (FL2 removes it, so FL2 removes `HighestLevel` and its two tests); `HasCellsOn` arrives in FL3 with its reader. `ThrowIfOutsideTheLevels` checks `ObjectTile.MinLevel` instead of 0.
7. **The drag bar is the main window's splitter** (FL3): column 1 becomes `Width="Auto"` holding `<GridSplitter Width="5" Margin="4,0" HorizontalAlignment="Stretch" ResizeBehavior="PreviousAndNext" />` (13 px, today's gap within a pixel), column 0 keeps `Width="420"` and gains `MinWidth="300"`, column 2 (`*`) gains `MinWidth="400"`. No view model: the width is the view's, not remembered. The picker's sheet view is already a scroll viewer with `Auto` bars, so a wider column shows more of the sheet and drops the horizontal bar once the sheet fits.

### Extension points
- **More levels:** change `ObjectTile.MinLevel`/`MaxLevel`; the box's list follows (`LevelChoice.TopDown` reads them).
- **A level shown or hidden:** a flag on `LevelChoice` beside `IsPainted`, read by `ObjectCanvasDrawing`; no format change.
- **The picker's width remembered:** the editor has no settings store yet; it would hold column 0's width.

### Replacement mapping

| Today | After this plan |
|---|---|
| Levels 0 to 7 (`ObjectCatalogValidator`, `ObjectCanvas.ThrowIfOutsideTheLevels`) | -7 to 7 (`ObjectTile.MinLevel`, FL1 and FL2) |
| `FindTileListProblem`'s `previousLevel = 0` | `ObjectTile.MinLevel` (FL1) |
| `CanvasPainting.Level`: an index into the choices, -1 ignored | The level number, plain observable (FL2) |
| `CanvasPainting.LevelChoices`: strings, rebuilt to one above the highest | Fifteen `LevelChoice` entries, fixed (FL2), marked (FL3) |
| `OnCanvasChanged`: rebuild and clamp | Nothing (FL2), then the marks (FL3) |
| `ObjectCanvas.HighestLevel` | Removed (FL2); `HasCellsOn(level)` (FL3) |
| The box bound by `SelectedIndex` | By `SelectedValue`/`SelectedValuePath="Level"` (FL2) |
| Picker column `420`, gap `12` | `420` at least `300`, an `Auto` column with a `GridSplitter`, canvas side at least `400` (FL3) |

## Risks
- **A negative level in an older build:** the strict check refuses it at load, naming the file. Accepted: one developer, one build.
- **The binding pushing a stray value:** a `SelectedValue` binding pushes back only when the selection changes; the list never changes, so it can't clear. Were it ever cleared, a null can't convert to `int` and WPF leaves `Level` as it was (a binding error in the Output window), never -1. FL2's hand check strokes right after a Ctrl+Z, as OL4's did.
- **Fifteen entries in the drop-down:** about 300 px at the theme's row height, under the combo box template's `MaxDropDownHeight` (a third of the screen), so no scroll inside the list on a 1080-line screen.
- **Two places say what a painted level is** (the drawing's faded cells and the marks): both read `ObjectCanvas.Cells`; the marks are a query, never stored apart from the canvas.
- **The window narrower than both minimums** (300 + 13 + 400 + the 24 px margin = 737 px): a window dragged narrower clips the canvas side, as any WPF grid; the composer opens at 1100.
- **Determinism, saves:** no randomness, no time; saves untouched. **Content format:** one wider range, old files unchanged (the identical-bytes writer tests are the guard).
- **File sizes:** all well under 300 (README, "Guiding rules"); `ObjectCatalogValidatorTests` ends about 255, still under 300 and so no sibling file.
