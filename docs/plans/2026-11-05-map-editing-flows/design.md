# Map editing flows: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Unsaved mark | `Maps/MapEntry.Of` | " *" after the text | A map with unsaved changes in the map list (decision 498). |
| Save all key | `Shell/MainWindow.xaml` key binding | Ctrl+Shift+S | File > Save all (decision 500). |
| Map filter key | `Shell/MainWindow.xaml` key binding | Ctrl+M | Focuses the map list's filter box (decision 504). |
| Back key | `Shell/MainWindow.xaml` key binding | Alt+Left | Map > Back to previous map (decision 505). |
| Nudge step | `Objects/ObjectPlacingViewModel.NudgeCells` | 1 cell | An arrow key's move of the selection (decision 509). |
| Fine nudge step | `Objects/ObjectPlacingViewModel.FineNudgeCells` | 1/16 cell | Shift + arrow: one Mana Seed pixel, 3 screen pixels at 48 px; exact in binary. |
| Snap unit | `Objects/GridSnap` | 1 cell | Where a snapped top-left lands (decision 511). |
| Snap key, snap toggle | `Shell/MainWindow.ObjectDragging.cs`, `ObjectEditing.cs` | Alt held; G | Decision 511. |
| Paste point from the menu | `Shell/MainWindow.ObjectCopying.cs` | the view's center | Edit > Paste has no mouse on the map (decision 512). |
| Picker's starting cell size | `Events/WarpDestinationViewModel.Zoom` | 32 px (a `CellZoom` step) | A 40 x 30 map is 1280 x 960 at the start; the bar goes from 16 to 192. |
| Picker window | `Events/WarpDestinationWindow.xaml` | 1100 x 720, resizable | As the composer window. |
| Destination mark | `Shell/MainWindow.xaml`, `DestinationMark` | gold, dashed | The cell Go to arrived on (WP4). Placeholder look. |
| Zoom after Go to | `Shell/MainWindow.MapSwitching.cs` | 100 % | Decision 508. |
| Levels under and over the chosen layer | `Objects/Composing/MapSelectionCanvas` | 7 each | `-ObjectTile.MinLevel` and `ObjectTile.MaxLevel`, not new numbers (decision 514). |
| Largest map selection made into an object | `Objects/Composing/MapSelectionCanvas` | 32 x 32 cells | `ObjectCanvas.MaxCells`, not a new number. |

## Design overview

### What exists
- **One open map at a time** (`MainViewModel.CurrentMap`, a `MapViewModel` with its own `EditHistory`): `TryOpenMap` builds a new `MapViewModel` from the saved catalog each time, after asking "Discard them?" (`Func<string, bool> confirm`, `MainWindow.Ask`) when the open one is dirty; the old one and its history are dropped. `CombinedHistory.OpenMap` forgets the old map's journal steps and every redo (`UndoJournal.ForgetMapSteps`). Ctrl+S (`Save`) writes the catalogs when dirty, then the open map; with no map open it returns without writing (C9). Closing asks yes/no. The purge (`CatalogDraft.Rewriting`) rebuilds the one open map from the written content and refuses to start while it or the catalogs are dirty (`SheetRemoval.AskPurge`).
- **Usage checks** (`Catalogs/CatalogUsage`: `UsersOfTile`, `OfSheet`, `UsersOfObject`, `UsersOfQuest`) read the saved maps with one `MapLayout? openLayout` in place of its saved version; callers: `CombinedHistory` (undo refusals), `ObjectBrowserViewModel` (Remove), `SheetRemoval` (Remove sheet), `TileBrowserViewModel` (the re-cut warning), each through a `Func<MapLayout?>` or `Func<MapViewModel?>` of the shown map.
- **The map list** (`MainWindow.xaml`, `MapList`): `MainViewModel.MapEntries` (`Shell/MapEntry(Id, Text)`, "Display name (id)", saved maps only, decision 122); a new map is not listed until saved. No filter, no shortcut.
- **Warp steps** (`Core/Events/WarpStep(MapId, GridCell Cell)`; editor `Events/EventStepViewModel`: `MapId` from `StepChoices.MapIds`, `CellXText`, `CellYText` typed) in both modal event dialogs (`ObjectEventsWindow`, `CellEventWindow`), whose view models carry `StepChoices`, built by `EventEditingViewModel.Choices()` on each opening.
- **Placed objects** (`Core/Maps/PlacedObject`: `Uid`, `ObjectId`, `Position` (top-left, free `Point2D` in cells), `Z`, `Events`): events live inside them. Event ids are `{uid}_{n}` (`EventIds`), unique over the whole map, cell events included (`MapEventValidator.FindRepeatedEventId`); uids are `{objectId}_{n}` (`PlacedObjectUids`). Saves remember objects by uid and "once" events by id. No event names a uid or another event's id.
- **Object mode** (`MainWindow.ObjectEditing.cs`, 257 lines): click places centered on the mouse (free); Shift + drag moves one object (`MoveObjectEdit`, already one undo step); right click removes; Ctrl + drag selects (a Ctrl + click selects every object whose bounds hold the point); Delete removes. Arrow keys pan (`HandleViewportKey`, after `HandleObjectKey`). Alt is free in Objects mode. Missing: changing a placement's object, typing a position, changing its layer, opening its events from Objects mode.
- **The composer** (`ObjectComposing.StartNew`, `ObjectCanvas` keyed by `CellOnLevel`, levels -7 to 7, `TryApply` one catalog step). A map cell holds a tile id per layer; every tile has its `SheetCell`, so each map layer maps onto one canvas level.
- **Pictures:** `Maps/MapDrawing.Draw(layout, focusZ)` and `DrawGrid`; `Maps/CollisionShapes` (blocked cells from Core's `WalkabilityGrid`, object outlines); Core's `Polygon.Contains`; `Viewport/CellZoom` and `CellZoomBar`; `Viewport/ViewportTransform` (no "center on" yet).

### Core changes
None. Core's records (`PlacedObject`, `MapEvent`, `WarpStep`, `ObjectTile`, `MapLayers`) hold everything; Data's validators already refuse a repeated uid or event id.

### Structure and folder dependencies

New types (all `internal sealed` unless said):

| Where | Type | Responsibility | Slice |
|---|---|---|---|
| `Shell` | `MainViewModel.MapSwitching.cs` (partial) | Which maps are open and shown: the map list, opening and making maps. | MS0 |
| `Shell` | `MainWindow.MapSwitching.cs` (partial) | The map list's handlers, the filter box's keys, Go to's arrival. | MS0 |
| `Shell` | `MainWindowStyles.xaml` | The main window's styles and panel sizes. | MS0 |
| `Maps` | `OpenMaps` | The maps open in this session, each with its unsaved changes and history. | MS1 |
| `Maps` | `MapEntry` (moved from `Shell`) | One row of a map list: the id and "Display name (id)", marked while unsaved. | MS1 |
| `Shell` | `UndoStep` (record: `Target`, `MapId`) | One step in the undo order: the catalogs, or which open map. | MS3 |
| `Shell` | `MainViewModel.Saving.cs` (partial) | Save, Save all and the close question. | MS4 |
| `Shell` | `UnsavedChangesAnswer` (enum: SaveAll, Discard, Cancel) | The developer's answer when closing with unsaved changes. | MS4 |
| `Shell` | `SaveChangesWindow` | The dark Save all / Don't save / Cancel question. | MS4 |
| `Shell` | `MapNavigation` | The map list as filtered and the map shown before, for Back. | MS5 |
| `Events` | `WarpMaps` | The maps a warp can lead to: their rows and layouts, and the catalog saying where the player stands. | WP1 |
| `Events` | `WarpDestinationViewModel` | The destination picker's map and cell, its warning and its problem. | WP1 |
| `Events` | `EventStepViewModel.Warp.cs` (partial) | A warp step's destination: picked, and followed by Go to. | WP2 |
| `Events` | `WarpDestination` (record: `MapId`, `Cell`) | Where a warp step leads, once its fields are valid. | WP2 |
| `Events` | `WarpDestinationWindow` | The picker: the map list beside the chosen map's picture, a click choosing the cell. | WP3 |
| `Shell` | `MainWindow.ObjectDragging.cs` (partial) | The object drags: the selection rectangle, the moved objects and the placing ghost. | MO0 |
| `Maps/Editing` | `MoveObjectsEdit` | Moves several placed objects as one step, each with its exact from and to. | MO1 |
| `Objects` | `GridSnap` (static) | Where a top-left lands on the grid. | SN1 |
| `Maps/Editing` | `PlacedObjectCopies` (static) | Copies of placed objects for a map: fresh uids and event ids, moved to a new top-left. | CP1 |
| `Maps/Editing` | `PlaceObjectsEdit` | Places several objects on top of the others as one step. | CP1 |
| `Objects` | `ObjectCopying` | The objects copied and pasting them on the shown map. | CP2 |
| `Shell` | `MainWindow.ObjectCopying.cs` (partial) | The Copy, Cut and Paste command bindings in Objects mode. | CP2 |
| `Objects` | `PlacedObjectEditing` | The selected placed object's position and layer (PO1) and object (PO2) as typed, applied as one step. | PO1 |
| `Maps/Editing` | `ReplacePlacedObjectEdit` | Replaces one placed object's record, keeping its place in the order. | PO1 |
| `Objects` | `PlacedObjectPanel` (user control) | The panel showing `PlacedObjectEditing` under the Objects panel's list. | PO3 |
| `Shell` | `MainWindow.TilePainting.cs` (partial) | The Tiles mode handlers and the brush outline. | OM0 |
| `Maps` | `MapCellRectangle` (record struct) | A rectangle of map cells: its top-left, columns and rows, and its cells. | OM1 |
| `Objects/Composing` | `MapSelectionCanvas` (static) | The canvas a map rectangle makes, every layer on its level around the chosen one, and why it can't. | OM2 |
| `Shell` | `MainViewModel.ObjectFromMap.cs` (partial) | Objects > New object from map selection: the composer (OM3) and the replacement (OM4). | OM3 |
| `Maps/Editing` | `ReplaceCellsWithObjectEdit` | Erases cells on some layers and places an object, as one step. | OM4 |

Folder edges (`->` = uses):

| Edge | New? | Why |
|---|---|---|
| `Shell -> Maps, Events, Objects, Tiles, Viewport, Catalogs` | exists | |
| `Tiles, Objects, Events -> Maps` | exists | `OpenMaps`, `MapEntry`, `MapDrawing`, `MapCellRectangle` |
| `Catalogs -> Maps` | none; `CatalogUsage` takes plain `MapLayout`s (Core) | `Maps` uses `Catalogs` (`MapDrawing`), so `Catalogs` must not read `Maps`: the callers turn `OpenMaps` into layouts. |
| `Events -> Viewport` | **new** | `CellZoom`, `CellZoomBar` in the picker; `Viewport` uses no editor folder. |
| `Objects/Composing -> Maps` | **new** (`MapCellRectangle`) | `Maps` uses nothing of `Objects`. |
| `Maps -> Shell, Events, Objects` | none, and none added | `MapEntry` moves *into* `Maps` so `Events` reads it without reading `Shell` (`Shell -> Events` exists). |

No project reference, no package.

### Key decisions
1. **Open maps are a list the shell owns** (MS1, decision 497): `Maps/OpenMaps` holds every `MapViewModel` opened (`Find(id)`, `Add`, `Layouts`, `Dirty` in id order, `DirtyChanged` when any map's `IsDirty` changes, `ReplaceFrom(written)` for the purge). `MainViewModel.CurrentMap` stays the shown map, so every panel's `OpenMap` wiring is unchanged; `TryOpenMap(id)` shows the open one or opens it and never asks. `MapEntries` are the saved maps plus the new open ones, each `MapEntry.Of(map, isDirty)`, rebuilt on `DirtyChanged`. A new map's id must be free among the saved and the open maps (`StartNewMap`).
2. **Usage reads layouts, not view models** (MS2, decision 502): `CatalogUsage`'s `MapLayout? openLayout` becomes `IReadOnlyCollection<MapLayout> openLayouts` (each replaces its saved version; a new map's is added); `Catalogs` keeps reading only Core types. The providers become `Func<IReadOnlyCollection<MapLayout>>` from `OpenMaps.Layouts`: `CombinedHistory(catalog, openMaps)`, `ObjectPlacingViewModel(catalog, layer, openMaps)`, `SheetRemoval(catalog, browser, openMaps, writer)`; `TilePaintingViewModel` takes its `TileBrowserViewModel` built by `MainViewModel` (it only built it from its images and question), which brings it from 4 parameters to 3.
3. **The journal knows each step's map** (MS3, decision 503): `UndoJournal` records `UndoStep(UndoTarget Target, string? MapId)`; `CanUndo(shownMapId)`, `Undo(shownMapId)` and their redo twins take the newest step that is the catalogs' or the shown map's, leaving the others in place; `Record` still clears every undone step. `ForgetMapSteps` goes; `Clear` stays for the purge. `CombinedHistory` asks with the shown map's id and sends a map step to the shown map's `UndoCommand`.
4. **Saving is one partial file** (MS4, decisions 499-501): `Save` writes the catalogs when dirty, then the shown map when there is one; `SaveAll` writes the catalogs when dirty, then each of `OpenMaps.Dirty`, stopping at the first refusal (`SaveError`). `ConfirmClose` asks `Func<string, UnsavedChangesAnswer> askToClose` (`SaveChangesWindow.Ask`, given by `App`) only when something is unsaved; Save all closes only when `SaveAll` leaves `SaveError` null. `confirm` (yes/no) keeps the resize, import and purge questions.
5. **The map list is filtered by `MapNavigation`** (MS5, decisions 504, 505): `Filter`, `ShownEntries`, `PreviousMapId`, `GoBackCommand` (enabled while the previous id is in the entries); `MainViewModel` holds it in its MapSwitching partial and notes each shown map.
6. **The picker works on `WarpMaps`, a value built when an event dialog opens** (WP1, WP2, decisions 506, 507): `EventEditingViewModel(catalog, openMaps).Choices()` adds the listed maps' rows and layouts (each open map's draft in place), and the catalog for walkability, outlines and objects. The warning checks Core's `WalkabilityGrid`, each object's outline (`CollisionShapes.Outlines` with `Polygon.Contains` at the cell's center) and walk-into events (a cell event with `Touch`, or a `Touch` event on an object whose bounds hold the cell).
7. **The step editor asks its dialog** (WP2-WP4): `EventStepEditor` raises bubbling routed events (`PickWarpDestination`, `GoToWarpDestination`) with the step as data context; both dialogs handle them and take a `MapDrawing`; Go to closes with OK and leaves `FollowedWarp` for `MainWindow.EventEditing`, which shows the map (`TryOpenMap`) and centers on the cell (`ViewportTransform.CenterOn`).
8. **Copies are fresh records, made by one rule** (CP1, CP2, decision 512): `PlacedObjectCopies.Make(copied, target, topLeft)`; the clipboard (`ObjectCopying.Copied`) holds the records as copied; `CanPaste` also asks that every copied object id be in `CatalogDraft.Current.Objects`, re-raised on every catalog change.
9. **Moves and snapping** (MO1, MO2, SN1, decisions 509-511): `MoveObjectsEdit(IReadOnlyList<MoveObjectEdit>)`; `GridSnap.TopLeft` read by the view model and the ghost alike; two methods per gesture (`PlaceObjectAt`/`PlaceObjectOnGridAt`), the view choosing by `IsGridSnapOn != Alt held`.
10. **The panel applies all its fields as one step** (PO1, PO2, decision 513): `PlacedObjectEditing` follows the selection (exactly one), the shown map's `Map` (an undo resets the fields or empties the panel) and the catalog (the object list); Apply does one `ReplacePlacedObjectEdit`.
11. **An object from the map is a canvas from the map** (OM2, OM3, decisions 514, 516): `MapSelectionCanvas.Of(layout, cells, baseZ, tiles)` and `ProblemOf`, `baseZ` being the layer chosen in the tool bar (the object's level 0 and, after a replacement, its layer); `TakenLayersOf` gives each taken layer's z with its level, read by `Of`, the warning (`DrawingWarningOf`, decision 516) and the replacement.
12. **The replacement is one map step after the catalog step** (OM4, decision 515): after `ObjectComposing.TryApply`, `ReplaceCellsWithObjectEdit` erases, on each taken layer from the base up (levels 0 and higher; the layers under it are never erased), the rectangle's cells whose saved object still holds the same sheet cell on that layer's level, and places the object at the rectangle's top-left on `baseZ`.

### Warp destinations: the alternatives
How other editors set where a door leads, weighed at planning (decision 506 chose the first):
1. **A transfer dialog with the destination map's picture** (RPG Maker MZ's Transfer Player). Content unchanged; the picker reads maps the editor already holds and draws them with `MapDrawing`; Go to answers "where does this door lead?". Weakness: a cell is a number, so reshaping the destination later can leave it on a wall; the warning shows it when the step is next opened.
2. **Named destinations** (Tiled's object references, Godot's `NodePath`, Unity's scene references): survive edits to the destination, but cost a content record, a validator, a Core lookup, a placing tool, every warp converted, and an ADR.
3. **Pick on the target map itself** (an eyedropper across maps): chains a modal dialog through a map switch and a picking mode; the picker window does the same with none of that state.
4. **Linked door pairs**: a later convenience on top of 1 (out of scope).
5. **Typing, helped by the status bar** (before this plan).

### Extension points
- **Closing one open map:** `OpenMaps.Remove(id)` with the shell's question; the journal's steps for that id dropped (`UndoStep.MapId`).
- **The system clipboard:** `ObjectCopying.Copied` serialized; `PlacedObjectCopies` stays the renaming rule.
- **Snapping to half cells:** a unit parameter on `GridSnap`.

### Replacement mapping

| Today | After this plan |
|---|---|
| One `CurrentMap`, rebuilt on every switch | `OpenMaps`, `CurrentMap` the shown one (MS1) |
| "Discard them?" on switch and on new map | Nothing asked (MS1) |
| `CatalogUsage(..., MapLayout? openLayout)` | `IReadOnlyCollection<MapLayout> openLayouts` (MS2) |
| `TilePaintingViewModel(catalog, layer, images, confirm)` | `(catalog, layer, browser)` (MS2) |
| `UndoJournal.Record(UndoTarget)`, `ForgetMapSteps` on switch | `Record(UndoStep)`, steps kept per map (MS3) |
| Save returns with no map open | Writes the tiles and objects (MS4) |
| Close: "Discard them?" yes/no | Save all / Don't save / Cancel (MS4) |
| `Shell/MapEntry`, saved maps only | `Maps/MapEntry.Of(map, isDirty)`, new open maps too (MS1) |
| `MapList.ItemsSource = MapEntries` | `Navigation.ShownEntries` (MS5) |
| Warp cell typed in two boxes | Still typed, plus Pick... and Go to (WP2, WP3) |
| Ctrl + click selects every object under the point | The top one (MO1) |
| Arrow keys always pan | Nudge the selection in Objects mode, pan otherwise (MO1) |
| Placing and moving free only | Free, or snapped with Alt, the box or G (SN1) |
| Tile handlers in `MainWindow.xaml.cs` | `MainWindow.TilePainting.cs` (OM0) |
| Ctrl + drag in Tiles mode paints | Selects map cells (OM1) |

## Risks
- **Memory with many open maps:** each open map holds its draft and history (a 100 x 100 map's layers are a few hundred kilobytes); a session opens tens of maps at most. The view's place isn't kept per map (switching back fits the map), a backlog line.
- **Save all stops halfway:** the maps written before a refused one are saved, the rest not; the error line names the refused file and the list's marks say which are still unsaved. One developer, files in git: accepted over an all-or-nothing write across files.
- **Catalog undo against hidden maps:** decision 502 makes every usage check read every open map; between MS1 and MS2 the save's validation is the guard (README, "Order").
- **The purge with maps open:** it already refuses while anything is unsaved; MS1 widens that to every open map, so `ReplaceFrom(written)` only ever rebuilds clean maps (a new map is dirty from its creation, so the purge waits for it to be saved).
- **Alt + Shift switches the keyboard layout** on a Windows with two layouts (Left Alt + Shift): a snapped Shift + drag may switch it; the Snap to grid box (or G) avoids holding Alt.
- **Alt alone and the menu:** the main menu is `IsMainMenu="False"` already, so letting go of Alt never focuses it.
- **Arrow keys change meaning** in Objects mode with a selection: Esc clears it; the Objects tab's tooltip and F1 say so.
- **Ctrl+C, Ctrl+X, Ctrl+V inside text boxes** stay the text box's: the window binds `ApplicationCommands`, which a focused `TextBox` handles first.
- **Pasted ids against cell events:** `PlacedObjectCopies` checks every event id on the map, cell events included (a CP1 test).
- **Copies whose warps lead to their own map** still lead where they led; Go to shows it.
- **Swapping a placed object's object:** an event gated on the old object id never runs again unless a swap step leads back; the panel warns, and Data's shadowing rule (ADR 0033) still checks the order at save.
- **The warp warning is advice:** outlines are tested at the cell's center, so a cell an outline only clips at a corner isn't warned about; the game's collision (`MapCollision`) is the truth, the hand check walks it.
- **Replacing the ground leaves holes:** with every layer taken (decision 514), the ground under a tree is on level -1 and a replacement erases it from the map, leaving cells with no tile, which block the player and show black where the object doesn't cover them. Decision 515 therefore never erases the layers under the base: the ground stays on the map, drawn under the object's own copy of it (the same tiles, so it looks the same); erasing that level in the composer only drops it from the object.
- **The replacement changes how the cells draw:** tiles draw by layer, an object by depth (`DrawOrderKey`), and the object starts with no collision outline, so erased blocked tiles stop blocking until the Shapes window (opened after Save) draws one; decision 516's warning names the layers most affected.
- **Determinism, saves, content:** no randomness, no time; no save or content format change.
- **File sizes:** every split is planned in the slice before the growth (README, "Guiding rules"); `MainWindow.xaml` ends about 294, the closest.
