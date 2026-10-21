# Editor: design

Part of [the editor plan](README.md): the tunables, the design overview and the risks. Slices point here for the details they build on.

### Tunables and placeholders
Legacy values unless said otherwise. "Where" is where the value lives, so it changes in one place.

| Name | Where | Value | Meaning | Slice |
|---|---|---|---|---|
| new tile | `Catalogs/CatalogDraft.AddTile` | walkable; id one above the highest | Decision 114; tile ids aren't save keys. | E3d |
| tile size guess for a new sheet | `Tiles/SheetCutGuess` | 48, 32, 16, 64, 24 px: the first that divides both sides, else 48 | Legacy `GuessTileSize`; pre-fills the Cut panel's columns and rows. | E5b |
| new sheet id | `Tiles/TilesheetIds` | file name without extension in snake case, digits split off (`Grass3.png` -> `grass_3`, `RA_Jungle.png` -> `ra_jungle`); `_2`, `_3`... if taken | Decision 118. | E5b |
| sheet marks | `Tiles/TilesheetMarks` | grid `#50FFFFFF`; tile `#3000FF00` fill; blocked tile red cross `#C0FF4040`; chosen cell cyan 3 px; cell rectangle dashed cyan | Existing marks plus blocked and rectangle. | E3b, E4a, E12a |
| collision overlay | `Maps/CollisionDrawing` | blocked cell `#60FF3030` fill; collision outline `#7CFC7C` stroke, `#4600FF00` fill; above-player zone `#6EC8FF` stroke, `#500096FF` fill; strokes 2 screen px | Legacy `ObjectEditorWindow` colors; legacy "Show collision and zones". | E8 |
| shapes window | `Objects/ObjectShapesWindow`, `Objects/ShapeEditing` | points rounded to the object's map pixels (1 / (cells x 48)); handles 10 screen px, the last point yellow; a click within 8 screen px of an edge inserts there; snap 1/4 tile, off by default; inactive shapes at 35 %; opens with the object's longer side at 560 px, between 1x and 6x; wheel 1.2x per notch, 0.5x to 12x | Legacy `ObjectEditorWindow`; decision 126; pixel rounding from the review. | E9, E11a, E11b |
| NPC placeholder sprite | `content/art/field-sprites/npc_placeholder.png` | a copy of `player.png` | Decision 69, question 130. | N1 |
| NPC object | `Objects/NewNpcViewModel` (new NPCs), `content/objects.json` (`villager`) | 1 x 1 cell, category "NPC", outline (0.2, 0.5), (0.8, 0.5), (0.8, 1), (0.2, 1) | Questions 129, 131, 132. | N1, N3 |
| NPC frame in the editor | `Tiles/TileImages` | 48 px frames, the down row's first frame | Mirrors the game's `FieldSpriteFrame` (the editor can't reference the game); question 128. | N2 |
| editor theme | `Shell/EditorTheme.xaml` | window `#111111`, panels `#181818`, viewport and status `#202020`, borders `#333333`, text `#DDDDDD`, status message `#88CC88`, errors `#FF6B6B`, warnings `#E0A040` (not in legacy; replaces `DarkOrange`) | Legacy MapEditor; decision 121. | E13b |
| panel sizes | `Shell/MainWindow.xaml` resources | map list 200 px (min 140), side panel 420 px (min 300), both resizable | Today's 180 and 420, made resizable. | E13a |

## Design overview

### What exists, checked (2026-10-21)
- **Editor (part 3a, ADR 0012).** `Shell/MainViewModel` loads the catalog once (`Catalog`, replaced by a `with` copy when a map is saved), owns `Layer`, `Painting`, `Placing`, `EventEditing` and the open `MapViewModel`. `Shell/MainWindow` (473 lines, plus `MainWindow.ObjectEditing.cs` 179 and `MainWindow.EventEditing.cs` 98) turns mouse and keys into calls by `EditingMode`. `Maps/MapDrawing` and `Tiles/TileImages` are built once in the window's constructor from the loaded catalog and cache crops by tile id. `ObjectPlacingViewModel`, `ObjectBrowserViewModel` and `EventEditingViewModel` hold the catalog (or its objects) taken at startup. Edits are `IMapEdit`s in a per-map `EditHistory`. Saving writes the map only (`ContentWriter.WriteMap`). Painting onto a Z with no layer adds the layer (`MapDraft.SetTileId`); a layer left without tiles isn't saved.
- **Core.** `TileDefinition` (`Id`, `IsWalkable`, `SheetId`, `Column`, `Row`); `TilesheetDefinition` (`TileWidth`, `TileHeight`, `Margin`, `Spacing` in image pixels, `CellLeft`, `CellTop`); `ObjectDefinition` (`Width`, `Height`, `CollisionPolygon` and `AbovePlayerZones` in 0..1 over the object's cells, `Category`, `Tiles`). `PlacedObject` (`Uid`, `ObjectId`, `Position`, `Z`, `Events`); `MapEvent` (`Interact` or `Touch`, `Prompt`, `IsOnce`, steps such as `DialogueStep(Speech)` with an optional speaker and portrait). `InteractionFinder` finds an object's Interact event by facing its bounds. `WalkabilityGrid` (internal): a cell is walkable when it has at least one tile and all its tiles are walkable. `MapCollision` (internal): cells under the body walkable and the body's center outside every object's collision polygon. `MapLayers.AbovePlayerZ = 5`, `CanopyZOf`. Per-cell collision doesn't exist.
- **Data.** `ContentWriter` has only `WriteMap`. Validators: `TilesheetCatalogValidator`, `TileCatalogValidator` (sheet ids), `ObjectCatalogValidator` (tile ids, outlines 0..1 with 3+ points, one tile per cell, tiles not empty), `MapLayoutValidator`. `ArtReferenceCheck.FindProblems` (public) checks every tilesheet file exists, every tile's cell lies inside its image, and the player's field sprite exists; the editor treats any art problem as a load error and then shows no maps. `PngSize` (internal) reads a PNG's size from its header.
- **Game.** The player is drawn from `content/art/field-sprites/player.png`: 48 px frames, 8 per row, one row per facing (`WalkAnimation.RowOf`), inline in `ExplorationScene.DrawPlayer`. Objects are drawn from their tiles (`MapView.DrawObject`); canopies are baked from tiles and zones (`ObjectSpriteBaker`, `CanopyMask`). Tile layers are drawn by Z, so a layer above the player's Z already covers the player.
- **Content.** `tiles.json`: 283 tiles, every one walkable (legacy), only the sheet cells the legacy maps used. `tilesheets.json`: 10 sheets, all with tiles. `content/art/tilesheets/`: 34 PNGs, so 24 images have no sheet (`Floor.png` 64 px, `Wall.png`, `Grass3.png`, `Water3.png`, `Water4.png`, `RA_Jungle_Animations.png`, `RA_Jungle_Extras.png`, the other `tree0x_s_0y_animation.png`). `objects.json`: 12 objects in 3 categories (Chests, General, House); most have a collision outline, and most houses and trees have above-player zones. No NPC exists; `field-sprites/` holds only `player.png`.

### Why painting doesn't work (traced, confirmed by the developer)
Path: sheet click -> `TileBrowserView.OnSheetClicked` -> `TileBrowserViewModel.SelectTileAt(x, y)` -> `SheetTiles.FirstOrDefault(tile at column, row)`; map drag -> `MainWindow.HandlePaintButton` -> `StartDrag` -> `TilePaintingViewModel.StartPainting` -> `if (Browser.SelectedTileId is { } tileId)`.
1. **Only cells that are already tiles can be chosen.** `SelectTileAt` keeps the old selection when the cell isn't in `tiles.json` (test `SelectTileAt_CellThatIsNoTile_KeepsTheSelection` pins it). `tiles.json` only has the cells legacy maps used: `grass_1` and `grass_2` have 1 tile each, `ground` 6, `water_1` 3, `water_2` 2. The class comment says "making tiles from them comes with the tilesheet tools", which part 3c never built.
2. **Nothing says so.** With no tile chosen, `StartPainting` does nothing (pinned by `StartPainting_NoTileChosen_ChangesNothing`); "Fill layer", rectangle and fill do nothing either. The only hint is a faint 19 % green fill on tile cells.
3. **24 of 34 sheet images aren't listed at all**, since the browser lists `tilesheets.json` only.
With a tile that exists (e.g. picked with Alt + click on the map), brush, rectangle, fill and erase work: their tests pass, and the drawing path redraws on `MapViewModel.Map` changes. The developer's report (click a square on the left panel, then the map: nothing) is case 1.

### Why there is an empty strip at the top (traced)
`MainWindow.xaml` docks two error `TextBlock`s above the toolbar: `LoadError` with `Margin="8"` and `SaveError` with `Margin="8,0"` and `TargetNullValue=''`. A WPF `TextBlock` with empty text still measures one line high, so with no error they take about 8 + 16 + 8 + 16 = 48 px at 100 % scaling. A second, smaller waste: the layer list on the right (110 px) lists layers but has no selection binding, so clicking it does nothing.

### Gap list against the legacy MapEditor
| Feature | Legacy | Now | Plan |
|---|---|---|---|
| Map list, switching maps in one click | Open dialog | **Improved** (list, unsaved prompt) | E1 adds display names |
| New map, map settings (name, size) | Yes | **Improved** (encounters, validated) | kept |
| Events (object, cell) | Basic | **Improved** (presets, warp, dialogue, once) | kept; Talk preset (N2) |
| Undo/redo, grid, dim other layers, zoom/pan, status bar | Yes | Kept (map only) | undo covers catalogs (E4b) |
| Brush, rectangle, fill, sizes, fill/clear layer, eyedropper | Yes | **Broken in practice** (only pre-existing tiles) | E3a-E3d |
| Layer presets plus free Z | Yes | Kept; side layer list inert | E1 (choose, add a layer) |
| Place, move, select, delete objects | Yes | Kept | kept |
| Validation on save | No | **Improved** | extended to catalogs (E3c) |
| Menu bar, F1 controls help | Yes | **Missing** | E2 |
| Tileset Manager: list sheets, set cut (size, margin, spacing) | Yes (modal) | **Missing** | E5a, E5b, E6 |
| Import a sheet image | Yes | **Missing** | E7 |
| Make tiles from cells ("Add tile", "Add all") | Yes | **Missing** | E3d (on first paint, decision 113) |
| Tile walkable flag | Yes | **Missing** (all 283 tiles walkable) | E4a |
| Object collision outline editing | Yes | **Missing** (data exists) | E9, E11a, E11b |
| Above-player zones editing | Yes | **Missing** (drawn, not editable) | E10, E11a, E11b |
| "Show collision and zones" overlay | Yes | **Missing** | E8 |
| New object from a tile selection | Yes | **Missing** | E12a, E12b |
| Object name, category, NPC flag, NPC window | Yes | Missing | category in E12b; NPCs as field-sprite objects (N1-N3); object names: question 69 |
| Tile Z and Object Part Z modes, tile names, tile collision polygons | Yes | Missing | question 69 (rest), out of scope |
| Delete tile, Save As | Yes | Missing | out of scope (ids are keys; decision 70) |
| Dark theme, tool bars per mode, resizable bottom panel | Yes | Missing | E13a, E13b |

### Core changes
| Member | Why | Slice |
|---|---|---|
| `TilesheetDefinition.ColumnsIn(int imageWidth)`, `RowsIn(int imageHeight)`: whole cells only | The browser refuses partial cells; the cut panel shows the grid. One rule beside `CellLeft`/`CellTop`. | E3b |
| `WalkabilityGrid` public (`Build`, `IsWalkable`) | The overlay shows the game's own walkability rule instead of a copy. | E8 |
| `ObjectDefinition.FieldSpriteFile` (`string?`) | An NPC is an object drawn from a field sprite (ADR 0015). | N1 |

No other Core change: collision outlines, zones, events and dialogue are already Core's format; an NPC's talk is an Interact event.

### Data changes
| Member | Why | Slice |
|---|---|---|
| `Art/PngSize` public | The shell builds the image size lookup the browser and the cut panel take (no `BitmapSource` or file reads in view models). | E3b |
| `ContentWriter.WriteMapCatalogs(ContentCatalog catalog)` | Writes `tilesheets.json`, `tiles.json` and `objects.json` (sorted by id, as the files are today) after the three catalog validators, every map's `MapLayoutValidator` and `ArtReferenceCheck` on the new catalog; all three written aside first, then moved, as `WriteMap` does. | E3c |
| `ArtFolders.TilesheetImagesIn(string artRoot)` | The sheet list shows images that have no sheet yet. | E5a |
| `ContentWriter.ImportTilesheetImage(string sourcePath, ContentCatalog catalog)` and `ContentWriter.HasTilesheetImage(string fileName)` | Copies a PNG into `content/art/tilesheets/` (refuses a non-PNG via `PngSize`, and a replacement too small for the tiles cut from it, with the art check's cell rule). | E7 |
| `ObjectCatalogValidator`: tiles or a field sprite; no zones on a field-sprite object. `ArtReferenceCheck`: object field sprites exist | ADR 0015. | N1 |
| `ArtFolders.FieldSpriteImagesIn(string artRoot)` | The New NPC dialog lists the field sprites. | N3 |

### Editor structure added by this plan
```
Jrpg.Editor/
  Catalogs/  CatalogDraft (E3a; tiles E3d, walkability E4a, undo E4b, sheets E5b, objects E9), SheetCell (E3b),
             CatalogEdit, CatalogHistory, CatalogUsage (E4b; sheets E6, objects E12b), SheetCellRectangle (E12a)
  Tiles/     TileBrowserViewModel: SelectedCell, CellAt (E3b), sheet choices (E5a), Cut panel commands (E5b, E6),
             cell rectangle (E12a); WalkabilityMarking (E4a); SheetChoice (E5a); SheetCutViewModel, SheetCutGuess,
             TilesheetIds (E5b); SheetImport (E7)
  Maps/      LayerEntry (E1); CollisionShapes, CollisionDrawing (E8)
  Objects/   ObjectShapesViewModel, ObjectShapesWindow (E9; zones E10); ShapeEditing (E11a, E11b);
             ObjectIds, ObjectFromCells, NewObjectViewModel, NewObjectWindow (E12b); NewNpcViewModel, NewNpcWindow (N3)
  Shell/     MapEntry (E1); ControlsHelpWindow (E2); UndoJournal (E4b); EditorTheme.xaml (E13b)
```
No type is named after its folder (`CatalogDraft` in `Catalogs`). `TileBrowserViewModel` keeps one responsibility, choosing what to paint with: the cut's rules are `SheetCutViewModel`'s, marking is `WalkabilityMarking`'s, importing is `SheetImport`'s (review).

**Folder dependencies (Editor), checked edge by edge:**
| Folder | Depends on |
|---|---|
| `Viewport` | nothing in the editor |
| `Catalogs` (new) | Core, Data only |
| `Maps`, `Maps/Editing`, `Maps/Settings` | `Catalogs`; `Tiles` (existing edge: `MapDrawing` uses `TileImages`) |
| `Tiles` | `Catalogs`, `Maps` (existing: `MapViewModel`, `LayerChoiceViewModel`, `MapDrawing.CellPixels`) |
| `Objects` | `Catalogs`, `Maps` |
| `Events` | `Catalogs`, `Maps` |
| `Shell` | everything above |
`Maps` <-> `Tiles` is an existing cycle (follow-up below). This plan adds no edge between them; `Catalogs` points at nothing in the editor, so it can't join a cycle. `Shell` alone connects `Tiles` (cell selection) to `Objects` (new object, E12b), and the map's history to the catalog's (`UndoJournal`, E4b).

### Key decisions (ADR 0014, ADR 0015 and the developer's decisions)
1. **One live catalog.** `CatalogDraft` (an `ObservableObject`) holds `Current`, the whole `ContentCatalog` the editor works with. Catalog edits replace records in it (`Current = Current with { Tiles = ... }`), so every reader sees an immutable snapshot, as `MapViewModel.Map` does for maps. Every panel, the drawing and event editing read it (E3a); `MainViewModel.Catalog` becomes `Content.Current`.
2. **The selection is a sheet cell, not a tile id.** `SheetCell(SheetId, Column, Row)` is chosen in the browser; painting asks the draft for its tile, adding one on the first paint (decision 113). The eyedropper selects the tile's cell.
3. **Crops follow the cut.** `TileImages` caches crops by (sheet record, column, row), so a re-cut sheet (a different record) gets fresh crops without invalidation code (E6).
4. **Save order.** Ctrl+S: the catalogs if the draft is dirty (`WriteMapCatalogs`), then the open map (`WriteMap` against `Content.Current`). A map is never written before the tiles it uses.
5. **Columns and rows are an input, not a format** (decision 115). The cut panel turns columns and rows into `TileWidth`/`TileHeight` (with margin and spacing), shown read-only; an image that doesn't split evenly is a problem shown in the panel, not a rounding.
6. **The overlay asks Core.** Blocked cells come from `WalkabilityGrid.Build(layout, tiles)`; outlines and zones from the object definitions placed at their positions (`CollisionShapes`, pure).
7. **Shapes keep Core's format.** The shapes window edits points in 0..1 over the object's cells, rounded to its map pixels; it refuses an outline or zone with 1 or 2 points (OK disabled, with the reason), and puts the definition in the draft on OK.
8. **One undo order** (decision 117). Map steps stay in each map's `EditHistory`; catalog steps go to the draft's `CatalogHistory` as `CatalogEdit`s (undo and redo functions over the catalog, replacing records by id, so they compose with later changes). `Shell/UndoJournal` records where each step went; Ctrl+Z and Ctrl+Y follow it. Tiles made by painting aren't steps (harmless when unused). An undo that would remove a tile, sheet or object still in use (`CatalogUsage`, counting the open map's unsaved layout) is refused, naming the user.
9. **An NPC is an object drawn from a field sprite** (ADR 0015). `ObjectDefinition.FieldSpriteFile` names a file in `field-sprites/` laid out like the player's; it has no tiles and no zones. Placing, collision, interaction, events, saves and the shapes window work unchanged; the game and the editor draw its standing frame.

### Extension points (how the editor grows)
Written into `docs/map.md`'s editor rows as slices land, so the next feature starts from them:
- **A map change** is an `IMapEdit` in `Maps/Editing` (Apply and Revert), done through `MapViewModel.Do`; undo, redo, the journal and the unsaved state come for free.
- **A catalog change** is a `CatalogDraft` method that builds a `CatalogEdit` replacing records in `Current`; undo, the journal, saving (`WriteMapCatalogs`) and validation come for free; every panel and the map redraw from the draft's change. A new catalog file adds its write to the same method and its validator to the same check.
- **A command** is a `[RelayCommand]` on the view model that owns the state, a menu entry (E2), optionally a key in `Window.InputBindings` and a toolbar button in its mode's band (E13a), and a line in the controls help.
- **A panel** is a `UserControl` with its view model in its feature folder, hosted in the side panel's tabs (per editing mode) or as a window opened from the menu; a dialog follows `StartX()` (a view model or null) and `TryApplyX(viewModel)` / `ApplyX`, with `XProblem` properties and a `ToX()` that throws on a problem.
- **An editing mode** (what clicks on the map do) is an `EditingMode` value, a view model owned by `MainViewModel` (given the draft and the layer), a tab, a tool bar band, a `MainWindow.<Mode>Editing.cs` partial holding its mouse handling (objects and events already are), and a help section. Tile mode's handling still sits in `MainWindow.xaml.cs`; no slice here adds a map mode, so extracting a common `IMapTool` waits for a fourth mode (follow-up below).
- **An event preset** is a method in `Events/EventPresets` and a button in the events window (Talk, N2).
- **A look value** lives in `Shell/EditorTheme.xaml` (E13b) or the drawing class's named constants, never inline in a view.

### Legacy mapping
| Legacy | New | What changes | Slice |
|---|---|---|---|
| `TilesetManagerWindow` sheet list and cut fields | `Tiles/TileBrowserViewModel` sheet choices, `Tiles/SheetCutViewModel`, `SheetCutGuess`, `TilesheetIds` | In the Tiles panel, not a modal window; columns and rows as the input, tile size shown; validated and saved with Ctrl+S; undoable. | E5a, E5b, E6 |
| `TilesetManagerWindow` "Add tile", "Add all" | `CatalogDraft.AddTile` on first paint | No explicit step (decision 113); transparent-cell skipping dropped. | E3d |
| `TilesetManagerWindow` walkable box, `PropertyEditor` | `Tiles/WalkabilityMarking`: the Mark blocked toggle; blocked marks on the sheet | Only the toggle (decision 119); walkability seen on the sheet and on the map overlay. | E4a, E8 |
| `TilesetManagerWindow` import | `ContentWriter.ImportTilesheetImage`, `Tiles/SheetImport`, Tiles > Import | Copy through Data; asks before replacing, then checks the tiles cut from it (decision 125). | E7 |
| `ZoomableImageViewer` | `Tiles/TileBrowserView` (exists) | - | - |
| `ObjectEditorWindow`, `CollisionEditorWindow` | `Objects/ObjectShapesViewModel`, `ObjectShapesWindow`, `ShapeEditing` | Rules headless and tested; OK refuses 1-2 point shapes instead of dropping them; points rounded to pixels; opened from a placed object too; object name not ported (question 69). | E9-E11b |
| "Show collision and zones" | `Maps/CollisionShapes`, `CollisionDrawing` | Blocked cells from Core's rule; legacy drew tile polygons nothing used. | E8 |
| `NewObjectWindow`, `TileGroupObjectCreator`, selection mode | `Catalogs/SheetCellRectangle`, `Objects/ObjectFromCells`, `NewObjectViewModel`; Shift + drag in the browser | Then opens the shapes window. | E12a, E12b |
| `NpcEditWindow`, the object's NPC flag | `ObjectDefinition.FieldSpriteFile`, `Objects/NewNpcViewModel`, `EventPresets.Talk` | An NPC is a field-sprite object with a Talk event (ADR 0015), not a flag. | N1-N3 |
| Menu bar, F1 controls | `Shell/MainWindow.xaml` menu, `Shell/ControlsHelpWindow` | Help per mode. | E2 |
| `EditorMainWindow` styling, `ToolBarTray` bands | tool bars by mode, `Shell/EditorTheme.xaml` | - | E13a, E13b |
| `ZEditPopup`, `EditModeHelper` | - | Question 69 (rest); `EditingMode` exists. | - |

## Risks
- **A re-cut changes art on every map** (decision 116). Tiles keep their column and row, so a new tile size shows other pixels wherever they're used. E6 counts the tiles and maps touched and asks; refuses cuts leaving a tile outside the image; Ctrl+Z restores the old cut.
- **The art check guards the editor itself.** The editor refuses to show maps when `ArtReferenceCheck` finds a problem, so a catalog write or an image import that broke it would lock the developer out until the JSON is fixed by hand. `WriteMapCatalogs` runs the same check before writing anything (E3c); `ImportTilesheetImage` checks a replacement against its tiles before copying (E7).
- **First catalog write may reformat `objects.json`.** Points are doubles like `0.0520833333333333`; `System.Text.Json` writes the shortest round-trip form, which may differ from the converter's text. `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes` catches it in E3c; if it fails, the fix is a one-off reformat commit of the content, noted in that slice, not a custom number format.
- **Undo across two histories** (E4b). The journal must stay in step with both histories: a step recorded without the journal hearing of it would make Ctrl+Z skip it. Both histories raise one `StepRecorded` event, and the journal's tests pin the order. An undo across a save or a map switch can remove a record a saved map uses; `CatalogUsage` refuses it, and the save's validation is the backstop.
- **Unsaved catalog changes are global.** They survive switching maps and are lost only by closing without saving (which asks, from E4a). A crash loses them, as it loses an unsaved map.
- **Duplicate cells in `tiles.json`.** If two tiles share a sheet cell, `CatalogDraft.TileAt` returns the lowest id; painting stays correct, nothing is merged.
- **NPC content in repository tests.** N1 places a villager on `start`, which `StartMapScenarios` and the content tests walk; it is placed off their paths. The editor and the game read the field sprite's frame layout separately (the editor can't reference the game); both are pinned by a test or a named constant beside the tunables row.
- **Dark theme size** (E13b): WPF's ComboBox and scroll bar templates ignore background colors under the Windows theme; re-templating them may need a slice of its own.
- **Performance.** `MapDrawing.Draw` redraws the whole map on every change (existing); catalog changes and undo add one redraw each. The overlay builds a `WalkabilityGrid` per redraw: tens of microseconds on today's 30 x 20 maps. Sheet marks redraw on each click. Nothing per frame.
- **Coupling.** Every panel reads `CatalogDraft`; only `CatalogDraft` methods change catalogs, so a rule about catalog edits has one home.
- **Determinism and saves.** No randomness or time in the new code. No save format change (NPCs are placed objects; their events use the existing ids). One content format change: the optional `fieldSpriteFile` on objects (ADR 0015); old content loads unchanged.
- **Two apps, one content root** (part 3a risk): the game reads content at start, so catalog edits show on the next game start.

## Follow-ups (for `backlog.md`, not built here)
- `Shell/MainWindow.xaml.cs` is 473 lines and holds tile mode's mouse handling inline; move it to `MainWindow.TileEditing.cs` like the other modes, and consider an `IMapTool` per mode when a fourth mode arrives.
- `Maps` <-> `Tiles` folder cycle: `Maps/MapDrawing` uses `Tiles/TileImages`, and `Tiles` uses `Maps` (`MapViewModel`, `LayerChoiceViewModel`, `MapDrawing.CellPixels`). Moving `TileImages` and the drawing pieces into a `Drawing/` folder would break it.
- `docs/map.md` doesn't list the editor's `Hosting` folder, `MapDrawing`, `TileBrowserViewModel`, `TileImages` or `TilesheetMarks`.
- Importing field sprites (N3 copies them by hand); E7's import could take a target folder.
