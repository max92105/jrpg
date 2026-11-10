# Map fixes, area banner, map ids and folders: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Banner wait | `Game/Exploration/MapBanner.WaitTime` | 0.5 s | After entering (and after any event ends), before the fade in: the warp's fade from black finishes first. Placeholder. |
| Banner fade in | `MapBanner.FadeInTime` | 0.4 s | Placeholder. |
| Banner hold | `MapBanner.HoldTime` | 3 s | Fully shown. Placeholder. |
| Banner fade out | `MapBanner.FadeOutTime` | 0.8 s | Placeholder. |
| Banner place | `Ui/UiTheme.MapBanner*` | right edge 64 px in, top 56 px down at 1920 x 1080 | MB2 plain, MB3 look. Placeholder. |
| Banner sizes | `UiTheme.MapBannerTitlePixels`, `MapBannerLabelPixels`, `MapBannerNumberPixels` | 52 px title serif, 24 px label, 40 px number | MB3. Placeholder. |
| Banner slide | `UiTheme.MapBannerSlide` | 24 px from the right while fading in | MB3. Placeholder. |
| Level line text | `content/text/ui.json` `map_banner_level` | "Lv {level}" | MB2 (ADR 0010); MB3 may replace it by the shared "Lv" label. |
| Folder separator | `Core/Maps/MapFolderPath.Separator` | "/" | Decision 532; a format rule, so a Core constant. |

## Diagnosis: the save deadlock (item 1)

What the code does today:
- `MainViewModel.Saving.cs`: Save writes the catalogs when changed (`ContentWriter.WriteMapCatalogs`), then the shown map (`WriteMap(map, content.Current)`); Save all writes the catalogs, then each changed open map in id order, each with its own `WriteMap`, stopping at the first refusal (decision 500).
- `ContentWriter.WriteMap(map, catalog)` checks **every** map with this one in place, against `catalog.Maps` for the others. `catalog` is `CatalogDraft.Current`, whose maps are the **saved** files (`AddSavedMap` after each write). So every other map is checked as it is on disk, never as it is open.
- `MapEventValidator.FindStepProblem`: "warp cell (x, y) is outside map 'm'" is checked against the target map's layout in those references.

The deadlock: map R is resized, say from 40 x 30 to 30 x 40. Map W (saved) has a warp into R at (35, 2), outside the new R. Saving R checks W's saved file: refused, naming W's event. The developer opens W and moves the warp to (5, 35), inside the new R but outside the saved R. Now:
- saving W checks R's **saved** file (40 x 30): (5, 35) is outside it, refused;
- saving R checks W's **saved** file: (35, 2) is outside the new R, refused;
- Save all writes the lower id first and stops at its refusal, whichever it is.

Nothing can be saved, even with every open map correct. The same shape exists between the catalogs and a map: Save writes the catalogs first, checking every **saved** map against them, so a catalog change that a map's unsaved change makes valid (an object taken out of the catalog and off the map) is refused until the map is saved, which waits for the catalogs.

The fix (SV1, decision 518): every save writes what changed in one go, all or none; Save all checks each map with every open map as it is now. Data already has the all-or-none write (`WriteMapCatalogsAndMaps`, the purge's); SV1 adds its maps-only twin (`WriteMaps`) and gives both a catalog whose maps are the open drafts (`OpenMaps.ContentWith(saved)`). When a map is refused and the catalogs changed, the catalogs are still written alone when they pass on their own (as Ctrl+S did), so a refused map never holds back a painted tile. Ctrl+S only writes the shown map, so it checks the others as saved (the files on disk must stay loadable together); SV2 dry-runs the checks with every open map and, when they pass, says Save all would write them.

## Diagnosis: canopies flatten the layers (item 6)

What the code does today (game `Exploration/MapDrawList.Rebuild`, editor `Maps/MapDrawing.PartsInDrawOrder`, the same rule written twice):
- each tile layer is `DrawOrderKey.ForTile(z, index)`; each placed object `ForSprite(placed.Z, baseY, index)` with `baseY` its top plus height; the player `ForSprite(MapLayers.PlayerZ = 1, feet, objectCount)`;
- an object with above-player zones adds a canopy part at `ForSprite(MapLayers.CanopyZOf(placed.Z), baseY, index)`, where `CanopyZOf(z) = max(5, z)`;
- `DrawOrderKey.CompareTo`: Z, then tiles before sprites, then base, then insertion order.

So the object's own layer is honored for its body, but **every canopy lands on layer 5 and sorts by base only**. On the repository's maps the houses sit on z -4 and -5 and the trees on -3 and -4 (`content/map-layouts/start.json`), all under the player's layer. A house on -4 with a roof zone and a tree on -3 with a leaves zone, overlapping:
- bodies: the tree's (-3) draws over the house's (-4), as the developer meant;
- canopies: both on layer 5, sorted by base only: when the house's bottom edge is lower on screen than the tree's, the roof draws after the leaves, so the tree's leaves go **under the roof** although the tree is on the higher layer. That is "on a higher z level it was still under the house".
- a tree on -5 in front of the house (its base lower): its body is under the house's walls, but its leaves draw over the roof.
- same layer: bodies and canopies both sort by base, consistently: "on the same level it works really well".

The fix (DO2, decisions 521, 522): a sprite key also carries the layer it **comes from** (`ownerZ`: the object's Z, for a canopy too), compared after the tiles-before-sprites group and before the base. Bodies are unchanged (their owner is their own layer); canopies on layer 5 now order by their objects' layers, then by base.

What stays (decision 521, and DO2's hand check says so): a canopy is drawn over the player, so over every body below layer 5. Leaves are always over the player and walls always under; a tree **behind** a house (on a lower layer) still shows its leaves over the house's walls where they overlap, unchanged from today. Only roof against leaves, and body against body, follow the layers.

## Design overview

### What exists
- **Saving:** above. `ContentWriter`: `WriteMap(map, catalog)`, `WriteMapCatalogs(catalog)`, `WriteMapCatalogsAndMaps(catalog, mapIds)` (all or none, every map in `catalog.Maps` checked, the new game checked). `OpenMaps`: `Dirty` (id order), `LayoutsWith(saved)`, `EntriesWith(saved)`, `ReplaceFrom(written)`. `CatalogDraft.AddSavedMap`, `ReplaceAfterRewrite(written)` (raises `Rewriting` first: `CombinedHistory` clears the journal, `MainViewModel.ShowRewritten` rebuilds the clean open maps and sets `CurrentMap` to the shown id's map, or null).
- **Draw order:** above. `PlacedObjectHits.TopAt` builds its own `ForSprite` key per object (no canopy). `MapDrawing.Draw(layout, focusZ)` dims every part whose `MapPart.Z` differs from the focused layer; `LayerChoiceViewModel.AreOtherLayersDimmed` is the View menu entry and the tool bar's "Dim other layers" box.
- **Selecting:** `ObjectPlacingViewModel.SelectObjectAt` (Ctrl + click) selects `ObjectAt(point)`, the top object by body key; Shift + drag grabs `ObjectAt(point)` (`MainWindow.ObjectEditing.HandleObjectButton`); right click removes it; Delete removes the selection or the object under the mouse; Shift + double-click opens its shapes; the Placed object panel (`PlacedObjectEditing`, decision 513) edits the one selected object and opens its events. Events mode: `EventEditingViewModel.StartEditingAt(point)` takes the top object (`PlacedObjectHits.TopAt`), else `StartEditingCellAt(cell)`; a right click removes the cell's event only when no object is there (`MainWindow.EventEditing.HandleEventButton`).
- **Map settings:** `Core/Maps/MapSettings` (`Id`, `DisplayName` "shown to the player on entering the map", `MusicFile`, `RandomEncounters`); `Data/Maps/MapSettingsValidator` (its `InvalidSettings` theory); `AbsentWhenNullFilter` lists `musicFile`; the editor's `Maps/Settings/MapSettingsViewModel` (`ToSettings` builds a new record, `AreSame` compares) and `MapSettingsWindow`, applied by `MainViewModel.TryApplySettings` as a `SetMapSettingsEdit`. The display name shows today in the menu header (`MenuHeader`) and the save list (`SaveSlots`), not on the map.
- **Entering a map in the game:** `ExplorationScene.EnterCurrentMap` runs at the start and after each warp (the map reference changes); the warp's fade out and in belong to `EventPresenter`, busy throughout; the scene has 4 constructor parameters (`exploration`, `art`, `presenter`, `prompt`), built by `SceneFactory.CreateExploration` and the tests' `StartMapScenarios`.
- **Map ids:** the file names of `content/maps/{id}.json` and `content/map-layouts/{id}.json` (`MapFolderReader`, flat, paired); referenced by `WarpStep.MapId` (every map's object and cell events), `NewGameDefinition.StartLocation.MapId` (`content/new-game.json`), saves (`SaveGame` location, `WorldState`'s done events and swaps keyed by map id; `SaveGameRepair`/`WorldStateRepair` already move the party to the new game's start and forget the rest when a map is gone, with notes). Quests, shops, encounters and the music name no map. `Maps/Editing/MapIds.FromName` makes a new map's id from its name; `MapSettingsViewModel`'s and `MapIds`' summaries still say the id can't change (question 70). `Shell/MapNavigation` keeps the shown and previous map ids for Back.
- **The map list:** `MainWindow.xaml` lines 179-191 (filter box and `ListBox`), handlers in `MainWindow.MapSwitching.cs`, rows from `MapNavigation.ShownEntries` (`MapEntry(Id, Text)`).

### Core changes
- `Maps/MapDrawOrder` (static, DO1): fills a caller's list with the map's parts in draw order: `Fill(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf)` for the editor, `FillWithPlayer(into, layout, definitionOf, Point2D playerPosition)` for the game; `SpriteKeyOf(PlacedObject placed, ObjectDefinition definition, int order)` and (UO1) `CanopyKeyOf(...)` for the editor's hit test. No allocation once the list has grown (the game calls it every update).
- `Maps/MapDrawItem` (readonly record struct, DO1): one part: `Key`, `Kind`, `Index` (the layer's or object's), `Z` (the layer the part belongs to: a canopy's object's, from DO2).
- `Maps/MapDrawItemKind` (enum, DO1): `Layer`, `Object`, `Canopy`, `Player` (replaces the game's `DrawableKind` and the editor's `MapPartKind`).
- `Maps/DrawOrderKey` (DO2): a private owner layer; `ForCanopy(int objectZ, double baseY, int order)`; `CompareTo` adds the owner after the group.
- `Maps/MapSettings`: `AreaLevel` (int?, AL1), `ShowsBanner` (bool, true by default, AL2), `Folder` (string?, MF1).
- `Maps/MapFolderPath` (MF1): the folder path rule both Data's validator and the editor's dialogs read.

No event step kind, no save format change, no rule of play. `PlacedObject` is unchanged (the depth offset went to the backlog).

### Data changes
- `ContentWriter.WriteMaps(ContentCatalog content, IReadOnlyCollection<string> mapIds)` (SV1): the named maps of `content` written, every map in it and the new game checked first, all or none; the checks shared with `WriteMapCatalogsAndMaps` in one method, made public as `CheckMaps(ContentCatalog content)` in SV2 for the dry run.
- `ContentWriter.RenameMap(ContentCatalog content, string oldId, string newId)` (RN1), with `Maps/MapIdRename` (the content with a map's id and its references renamed).
- Validators: `areaLevel` (AL1) and `folder` (MF1) in `MapSettingsValidator`; `showsBanner` needs none (a bool).
- `AbsentWhenNullFilter` lists `areaLevel` and `folder`; a new `AbsentWhenTrueFilter` leaves `showsBanner` out while true (AL2).

### Saves
Untouched (ADR 0022): no field is added to `SaveGame`. A renamed map id makes a save naming it go through the repair already in place (decision 531).

### Structure and folder dependencies

New types (all `internal sealed` unless said):

| Where | Type | Responsibility | Slice |
|---|---|---|---|
| Core `Maps` | `MapDrawOrder` (public static) | The map's parts in the order every drawer draws them. | DO1 |
| Core `Maps` | `MapDrawItem` (public readonly record struct) | One part of the map to draw: its key, kind, index and layer. | DO1 |
| Core `Maps` | `MapDrawItemKind` (public enum) | What a draw item is: a layer, an object, a canopy or the player. | DO1 |
| Data `Content` | `AbsentWhenTrueFilter` (static) | Leaves a true-by-default field added later out of a file while it is true. | AL2 |
| Core `Maps` | `MapFolderPath` (public static) | The folder path rule: names joined by "/", their checks and nesting. | MF1 |
| Data `Maps` | `MapIdRename` (static) | The content with one map's id, and every reference to it, renamed. | RN1 |
| Data `Maps` | `RenamedContent` (record) | What a rename changed: the new content, the other maps whose layouts changed, whether the new game did. | RN1 |
| Editor `Shell` | `MapListPanel` (user control) | The map list and its filter box beside the map. | ML0 |
| Editor `Maps` | `MapRenamingViewModel` | The Rename map dialog's new id, prefilled, and its problem. | RN2 |
| Editor `Shell` | `MainViewModel.MapRenaming.cs` (partial) | Renaming the shown map's id: starting it, refusing it, writing it. | RN2 |
| Editor `Maps` | `MapRenameWindow` | The Rename map dialog. | RN3 |
| Editor `Maps` | `MapTreeNode` (observable class) | One row of the map tree as the tree shows it: a folder with its rows, or a map. | MF2 |
| Editor `Maps` | `MapTree` (static) | The map rows arranged into folders, narrowed by a filter. | MF2 |
| Editor `Maps` | `MapFolderChoiceViewModel` | The folder dialog's path, typed or picked, and its problem. | MF4 |
| Editor `Shell` | `MainViewModel.MapFolders.cs` (partial) | New folder, moving a map and renaming a folder. | MF4 |
| Editor `Maps` | `MapFolderWindow` | The dialog for Move to folder, New folder and Rename folder. | MF5 |
| Game `Exploration` | `MapBanner` | Which map's name and level the banner shows and how opaque it is now. | MB1 |
| Game `Exploration` | `MapBannerView` | Draws the banner at the top right, text measured when the map changes. | MB2 |
| Game `Exploration` | `FieldOverlays` (record) | What the exploration scene shows over the field besides events: the prompt, the banner and its view. | MB2 |

Folder edges (`->` = uses):

| Edge | New? | Why |
|---|---|---|
| Core `Maps -> Geometry` | exists | `Point2D` in `FillWithPlayer`; `Polygon` for UO1's zone test stays in the editor |
| Data `Maps -> Core Maps, Events, Content` | exists | `MapIdRename` rewrites `WarpStep`s and `NewGameDefinition` |
| Data `Content -> Maps` | exists | `ContentWriter.RenameMap` calls `MapIdRename` |
| Editor `Events -> Maps` | exists | `EventEditingViewModel` takes `LayerChoiceViewModel` (UO2) |
| Editor `Shell -> Maps` | exists | `MapListPanel` binds `MapNavigation`, `MapTree` |
| Editor `Maps -> Shell` | none, none added | `MapTree` takes `MapEntry` rows (in `Maps`), never `MapNavigation` (in `Shell`) |
| Game `Exploration -> Ui, Rendering` | exists | `MapBannerView` |
| Game `Flow -> Exploration` | exists | `SceneFactory` builds `FieldOverlays` |

No project reference, no package.

### Key decisions
1. **Every save writes in one go** (SV1, decision 518): `OpenMaps.ContentWith(ContentCatalog saved)` returns `saved` with each open map as it is now in place of its file (a new map added), and `ContentWith(saved, shown)` with the shown one only. Save: `WriteMapCatalogsAndMaps(ContentWith(saved, shown), [shown])` when the catalogs changed, else `WriteMaps(...)`. Save all: the same two calls with `ContentWith(saved)` and every dirty id. On a refusal with the catalogs changed, `WriteMapCatalogs(saved)` alone (it checks the saved maps), and the error line says the tiles and objects were saved and no map was. After a write, `AddSavedMap` and `MarkSaved` on each written map.
2. **The dry run** (SV2, decision 519): `ContentWriter.CheckMaps(content)` (SV1's shared checks, public) on `_openMaps.ContentWith(saved)`: when it passes and other open maps have changes, the refusal names them and points to Save all. It covers both directions: the map holding the warp refused by the other's saved file, and the resized map refused by the other's saved warp.
3. **One draw order** (DO1, decision 520): `MapDrawOrder` is the only place parts are made and sorted; `MapDrawList` keeps the game's storage and calls `FillWithPlayer`; `MapDrawing` calls `Fill`; `PlacedObjectHits` asks `SpriteKeyOf`. The player's feet half a cell below its position move into Core with the rule (`MapDrawOrder.PlayerFeetBelowPosition`).
4. **The owner layer** (DO2, decisions 521, 522): `DrawOrderKey` gains `_ownerZ`; `ForTile` and `ForSprite` set it to their Z, `ForCanopy` to the object's; `CompareTo`: Z, group, owner, base, order. `MapDrawItem.Z` of a canopy becomes its object's Z, so the editor's dimming (and UO2's lock) treat the canopy with its object.
5. **Hits follow the drawing** (UO1, decision 523): `PlacedObjectHits.AllAt(map, point, definitions)` lists the objects covering the point, topmost first: an object ranks by `MapDrawOrder.CanopyKeyOf` when the point lies inside one of its above-player zones (`Polygon.Contains` on the zone scaled to the object, as `CollisionShapes` scales outlines), else by `SpriteKeyOf`; `TopAt` is its first; `NextUnder(map, point, selectedUid, definitions)` is the one after the selected one, wrapping. The lock (UO2) filters these lists by Z.
6. **The banner waits for the field to be free** (MB1, MB2, decisions 528, 529): `MapBanner.Enter(MapDefinition map)` restarts it when the map's id differs from the last one entered, shown only when `map.Settings.ShowsBanner`; `Advance(TimeSpan delta)` runs its clock, called by `ExplorationScene.Update` only while `EventPresenter.IsBusy` is false; `Opacity`, `Shown` (the map) are read by the view. Nothing allocates per update.
7. **Rewrites across maps** (RN2, MF6, decisions 530, 533): refused (and their entries greyed) while `HasUnsavedChanges`; written through Data; then `CatalogDraft.ReplaceAfterRewrite(written)` (journal cleared, clean open maps rebuilt) and the shown map shown again by its (new) id without passing through "no map shown", so `MapNavigation`'s previous map survives (RN2).
8. **The folder is settings** (MF1-MF6, decision 532): `MapEntry` gains `Folder` (from the open draft when open); `MapTree.Of(entries, sessionFolders, filter, expandedFolders)` builds the rows; a move is `SetMapSettingsEdit` on the moved map, shown first (`TryOpenMap`) so Ctrl+Z reaches it (decision 503).

### Extension points
- **A depth offset** (backlog): a `PlacedObject` field added to `MapDrawOrder`'s base for sorting, one place.
- **A rename table for saves** (after the first release): `SaveGameRepair` looks an unknown map id up in it before falling back to the new game's start.
- **Folders for other catalogs:** `MapFolderPath` and `MapTree` take rows and paths, nothing map-only but the names.

### Replacement mapping

| Today | After this plan |
|---|---|
| Save all: map by map, stop at the first refusal (decision 500) | One write, all or none; the tiles and objects alone when a map is refused (SV1) |
| `WriteMap` checks the others as saved | Save checks the shown map's draft with the catalogs; Save all every open draft (SV1) |
| `Game/Exploration/Drawable`, `DrawableKind`, the sort in `MapDrawList.Rebuild` | `Core/Maps/MapDrawItem`, `MapDrawItemKind`, `MapDrawOrder.FillWithPlayer` (DO1) |
| `Editor/Maps/MapPart`, `MapPartKind`, `MapDrawing.PartsInDrawOrder` | `MapDrawOrder.Fill` (DO1) |
| `PlacedObjectHits.TopAt`'s own key | `MapDrawOrder.SpriteKeyOf` (DO1), `AllAt` with canopies (UO1) |
| Canopies sort by base only on layer 5 | By owner layer, then base (DO2) |
| Ctrl + click: the top object only | Again on the same spot: the next under (UO1) |
| "Dim other layers" | "Dim and lock other layers" (UO2) |
| Events mode: a cell under an object unreachable | Ctrl + click (UO3) |
| Display name in the menu header only | Also the entry banner, per map (MB2, AL2) |
| `ExplorationScene(..., InteractionPromptView prompt)` | `(..., FieldOverlays overlays)` (MB2) |
| Map ids fixed (question 70) | Map > Rename map... (RN3) |
| Map list in `MainWindow.xaml` | `Shell/MapListPanel` (ML0), a tree (MF3) |

## Risks
- **Save all writes more at once** (SV1): a refused map now stops every map's write, where before the maps before it were saved. The error line names the file and says no map was written; the tiles and objects still save on their own when they can. Accepted for the deadlock's end (Q1).
- **Ctrl+S still refuses the deadlock's maps one by one** (by design: a map file is only written when the files on disk stay loadable together); SV2's dry run points to Save all.
- **Draw order change shows on existing maps** (DO2): wherever two overlapping objects on different layers both have canopies and the lower layer's base is lower on screen, their canopies swap. DO2's throwaway test lists every such pair on the repository's maps, so the hand check looks at exactly those spots.
- **Leaves over walls** (DO2, unchanged): a canopy draws over the player, so over every body below layer 5: a tree behind a house shows its leaves over the house's walls where they overlap. No layer order fixes it without a per-object sort rule (the backlog's depth offset).
- **The lock hides what the developer forgot is ticked** (UO2): a click on a faint object does nothing. The panel's note says "Locked: only layer z 1 takes clicks" when a click hits only locked objects.
- **Hot files:** `MainWindow.xaml` ends about 288 (ML0 -11, RN3 +1); `ObjectPlacingViewModel` about 281; `ExplorationScene` constructor stays at 4 (`FieldOverlays`); `MainViewModel.cs` unchanged in size.
- **Renaming a map the repository tests name** (RN1-RN3): repository tests pin ids (`RepositoryShopTests`, `RepositoryInnTests`, `RepositoryForgeTests`, `RepositoryNewGameTests`, `RepositoryQuestTests` read `content`); a rename the developer makes later in `content/` breaks them, and the commit that renames re-pins them. The plan's own tests rename only copies of the test world (`EditorTestContent.CopyOfTestWorld`, `CopiedContentRoot`), never `tests/TestWorld` (ADR 0037).
- **`new-game.json` written by the writer for the first time** (RN1): if the file's layout differs from `ContentWriter`'s, a rename rewrites its whitespace. RN1 adds an identity test on the repository's file and, if it fails, re-lays `content/new-game.json` once as the writer writes it (content rule).
- **A rename leaves the old files on a crash between the writes and the deletes**: the loader then sees both maps (both valid); the developer deletes the old pair. Writes come first, deletes last, so content is never missing a map.
- **`showsBanner` true by default** (AL2): `System.Text.Json` keeps the property initializer when the field is absent; AL2 tests a file without it loading as true, so old files show the banner.
- **Determinism:** the banner's clock is the update's delta; no randomness, no wall clock.
- **Content format:** three optional settings fields (`areaLevel`, `showsBanner`, `folder`); old files write back byte for byte; an older build refuses a file holding them, naming it (strict serializer).
