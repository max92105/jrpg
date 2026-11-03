# Mana Seed reset: design

## Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Mana Seed cut | the Cut panel, `Editor/Tiles/SheetCutGuess` | 16 px square | Every Mana Seed sheet is cut at 16 x 16; a prop bigger than a cell (the 80 x 112 trees, the 16 x 32 and 48 x 32 props) is an object of 16 px cells (`Objects/ObjectFromCells`). |
| Tile scale | `Game/Exploration/MapView.CellPixels`, `Editor/Maps/MapDrawing.CellPixels` | 48 px cell, 3x | Unchanged (decision 444). |
| Pack folder | outside the repository | `C:\git_perso\art-packs\mana-seed\<pack>\` (suggested) | Where bought packs live (decision 439); copied to both PCs by hand. |
| Art manifest | `content/art/art-sources.json` | | Sources and files (ADR 0038). |
| Art credits | `content/art/CREDITS.md` | | One section per source; placeholders listed apart. |
| Allowed art licenses | `Data.Tests/Art/ArtSourcesTests` | `CC0-1.0`, `CC-BY-3.0`, `CC-BY-4.0`, `OFL-1.1`, `LicenseRef-ManaSeed`; `LicenseRef-Unrecorded` and `LicenseRef-Generated` on placeholder sources only | A new bought publisher adds its `LicenseRef-` here. |
| Missing art mark | `Editor/Tiles/MissingArtMark` | magenta with a dark diagonal hatch, one cell | What the editor draws for a tile or NPC it can't show (decision 442). Placeholder look. |
| Test world | `tests/TestWorld/` | frozen at T1's commit | The maps the mechanism tests walk (ADR 0037). |

## Design overview

### What exists
- **The startup art check** `Data/Art/ArtReferenceCheck.FindProblems` names every missing or unreadable file the content names, and every tile cell outside its sheet. The editor shows its list instead of the maps while it isn't empty (`Shell/MainViewModel`'s constructor sets `LoadError`, and builds no panel), and `ContentWriter.WriteMapCatalogs` runs it before writing the catalogs (`WriteMap` doesn't). The game's startup refuses on it.
- **Drawing in the editor.** `Tiles/TileImages` loads each sheet image once (`SheetImageOf`) and crops tiles (`TileImageOf`, a `CroppedBitmap`), and NPC frames (`FieldSpriteFrameOf`); a missing file or a cell past the image throws. `Maps/MapDrawing` draws layers, objects and canopies through it; `Tiles/TileBrowserView` shows the chosen sheet through `SheetImageOf`; `TileBrowserViewModel` reads image sizes through `SheetImages.SizeOf` (`PngSize.Read`) to hit-test and to set up a cut. `TileImages.Forget` drops a file's cache after an import.
- **Sheets in the editor.** `ArtFolders.TilesheetImagesIn` lists the top-level PNGs (so the pack's subfolders were never listed); `TileBrowserViewModel.ChoicesOf` lists one choice per image, with its sheets, so a sheet whose image is gone isn't listed at all. `Tiles/SheetImport` copies PNGs through `ContentWriter.ImportTilesheetImage`, which refuses an image too small for the tiles cut from it and asks before replacing one. `SheetCutGuess.TileSizeFor` tries 48, then 32, 16, 64, 24; `TilesheetIds.FromFile` gives `autumn_trees__bare__80_x_112_` for "autumn trees (bare) 80x112".
- **Tiles are drawn into square cells** (`MapView.DrawTile`, `ObjectSpriteBaker`, `TilesheetMarks.DisplayScaleOf`); `TilesheetCatalogValidator` accepts any size.
- **Objects** are built from cells (`ObjectFromCells.Build`: tiles, no outline, no zones), replaced by id through `CatalogDraft.SetObject` (one undo step), shaped in `ObjectShapesViewModel`. A placed object keeps its uid when moved (`MoveObjectEdit`); its events live on the placement.
- **An empty cell blocks** (`Core/Maps/WalkabilityGrid`).
- **What uses a record** (`Catalogs/CatalogUsage`): `UsersOfTile`, `OfSheet` (`SheetUsage`), `TilesCutFrom`, `UsersOfObject` (maps placing it, not events swapping to it), `UsersOfQuest`; the open map is read as it is on screen in place of its saved file. `CombinedHistory.RefusalOfCatalogUndo` refuses an undo that would remove a record in use. `CatalogDraft` has `WithoutTile`, `WithoutObject`, `WithoutTilesheet`; `NewTileAt` takes the highest current id plus one, and `AddTile` (a tile made by painting) is outside the undo history.
- **Saves:** `SaveGameRepair.RepairLocation` sends a save whose map left to the new game's start (decision 186); a position on a kept map id isn't checked.
- **Tests and content:** every test project copies `content/**` into its output with `PreserveNewest`. `Game.Tests/Exploration/StartMapScenarios` builds about 15 start-map scenarios by cell (about 100 files), `Editor.Tests/Shell/EditorTestContent` and `Data.Tests/Content/CopiedContentRoot` copy it. `MainViewModelTests` and `MainViewModelUndoTests` open `Grass3.png`, which the working tree has deleted (S0 restores it). The Game and the Editor stamp the repository's content folder as `AssemblyMetadata("ContentRoot")` in Debug (`Jrpg.Game.csproj` line 25, `Jrpg.Editor.csproj` line 30).

### Core changes
None. No rule, state, event kind or save format changes.

### Structure and folder dependencies

New files and types:

| Where | Type or file | Responsibility |
|---|---|---|
| `tests/TestWorld/` | data, `README.md`, `TestWorld.props` | The frozen maps, catalogs, new game and tilesheet images; the props copy the live content without those, then the frozen files, into `test-world/`. |
| `Data.Tests/TestWorld/TestWorldTests` | test class | The test world loads and passes the art check. |
| `Data.Tests/Session/TestWorldNewGameTests` | test class | The new game's map mechanisms on the frozen world, moved from `RepositoryNewGameTests`. |
| `content/art/art-sources.json`, `content/art/CREDITS.md` | content | Where every art file came from; the credits. |
| `Data.Tests/Art/ArtSourcesManifest` | test records | The manifest as the test reads it. |
| `Data.Tests/Art/ArtSourcesTests` | test class | Files, entries, sources, licenses and credits in step, read from the source tree. |
| `Editor/Catalogs/MissingArt` | record | Which sheets' images and which NPC sprites the catalog names but the art folder lacks, worked out from a file-exists lookup; asked by drawing, the browser and the status. |
| `Editor/Shell/ArtStatus` | view model | The art problems as the status band's warning, refreshed when the sheets change or an import or a save happens; the cursor status's note for a tile of a missing sheet. |
| `Editor/Tiles/MissingArtMark` | static | The marked cell's picture, built once and frozen. |
| `Editor/Tiles/SheetChoices` | static | The sheet box's list: each image with its sheets, then each sheet whose image is missing (moved out of `TileBrowserViewModel.ChoicesOf`). |
| `Editor/Tiles/SheetImageSizes` | class | Each tilesheet image's size, read from its header once (moved out of `MainViewModel` in X1). |
| `Editor/Tiles/SheetRemoval` | view model | Takes the shown sheet out of the catalog: with its tiles when none is used (E3a), or, its image missing, with everything drawn from it after a question (E3c); says what it did or why it refused. |
| `Editor/Catalogs/SheetPurge`, `SheetPurgeResult` | static, record | The catalog and the maps without a sheet and everything drawn from it (its tiles, their cells, the objects made of them and their placements), with the counts; or the reason it can't (an event swaps to one of those objects). |
| `Editor/Tiles/SheetPurgeQuestion` | record | What the purge's question says and the purge it would apply (the dialog pattern's `StartPurge` / `TryApplyPurge`). |
| `Editor/Objects/ObjectReskin` | static | An object with its cells replaced by a sheet rectangle, keeping its id, category, outline and zones. |
| `Editor/Objects/ObjectReskinning` | view model | Whether the chosen object and the chosen cells can be combined, and the replacement as one undo step. |

Changed: `Catalogs/CatalogDraft` (`RemoveObject`, `RemoveTilesheet`, a tile id high-water mark, `ReplaceAfterRewrite` for E3c), `Tiles/SheetImportResult` (`Summary`, moved from `MainViewModel`), `Catalogs/CatalogUsage` (`UsersOfObject` counts swap steps), `Objects/ObjectBrowserViewModel` (Remove), `Tiles/TileImages` (marks instead of throwing), `Tiles/TileBrowserViewModel` (listing moved out, guards for a missing image), `Tiles/SheetCutGuess`, `Tiles/TilesheetIds`, `Tiles/SheetCutViewModel`, `Shell/MainViewModel` (X1 moves out the image-size cache and the import status, about 25 lines; the art check becomes `ArtStatus`; it holds `Art`, `Removal` and `Reskinning`; net growth over the plan below zero), `Data/Content/ContentWriter` (catalogs saved with art missing; maps written with the catalogs for E3c), `Data/Maps/TilesheetCatalogValidator` (non-square refused).

Folder edges (all exist already): Editor `Shell -> Catalogs`, `Shell -> Tiles`, `Tiles -> Catalogs`, `Objects -> Catalogs`, `Objects -> Tiles` (`SheetCellRectangle` is in Catalogs, so none new), `Catalogs -> Core.Events`. `Catalogs/MissingArt` takes a lookup, so it reads no file and needs no Data reference beyond what Catalogs has. Data `Content` gains no edge.

### Key decisions
1. **A frozen test world, overlaid on live content** (decision 446, ADR 0037). Only the map side is frozen (maps, layouts, tiles, sheets, objects, tilesheet images), so the mechanism tests stay on the live systems. `new-game.json` is frozen with them: `NewGameValidator.FindStartLocationProblem` only checks the live start lies inside the intro's bounds, but the Game tests need the start on the frozen intro's arrival cell (a live start moved within the intro would skip the cutscenes they play through) and count on the starting items, gold and leader choices; the cost is that a change to those reaches only `RepositoryNewGameTests`. The overlay is MSBuild only: `test-world/` is a whole content root, so the loader and the art check need no change.
2. **Repository tests pin ids, never cells** (decision 447). The start map, the forest and the interior keep their ids, and the NPCs, chest and doors keep their uids, so the shop, Forge, inn and quest tests keep reading them; only the tests pinning a cell move or are restated (R1). An interior is told from the wild by the way in: a door is an object's interact event, an edge exit is a cell's touch event, and the target has no random-encounter table.
3. **Removal is "unused only"** (decision 443). `CatalogUsage` answers "who uses this" with the open map as it is on screen in place of its saved file; a removal is a `CatalogEdit` whose revert adds the records back, so Ctrl+Z covers it with no refusal (adding is safe). A redo can't remove something used since: a new step drops the redo, and a painted tile raises `RedoDropped`. **Tile ids are never reused within a session**: the draft keeps the highest id it has seen (loaded, painted or restored) beside `Current`, and `NewTileAt` goes above it; otherwise undoing a sheet's removal after painting a new tile would overwrite that tile (E3a).
4. **A swap step naming an object is a use** (E1a). `MapEventValidator` refuses a swap to an unknown object; `CombinedHistory`'s undo refusal reads the same method, so it gains the case too.
5. **Missing art is the editor's warning, the game's error** (decision 442). `MissingArt` is worked out from the catalog and a file-exists lookup (`ContentWriter.HasTilesheetImage` for sheets; the field-sprite folder for NPCs), never from `ArtReferenceCheck`'s text. `TileImages` returns `MissingArtMark`'s picture for a tile of a missing sheet, a cell past its image, or an NPC without a sprite, so the map, objects and canopies draw it with no other change. The writer drops the art check from `WriteMapCatalogs` and keeps every content check; `ArtStatus` shows what's missing after a save. An import replacing a missing file needs no question (nothing to replace) and goes through `ImportTilesheetImage`'s size check against the tiles cut from it, then `TileImages.Forget` drops the marks.
6. **The purge (E3b worked out, E3c in the editor) is the one cascade, and a file operation** (decision 442, planner's pick on undo). A sheet whose image is missing can't be repaired by erasing what can't be seen one map at a time without help, so its removal takes its tiles, erases their cells on every map, removes the objects made of them and their placements, after a question that counts all of it. It is refused while the catalogs or the open map have unsaved changes, and while an event swaps to an object it would remove (named). It writes the catalogs and every changed map at once through one all-or-none `ContentWriter` call, then the draft takes the written content with a fresh history and the open map is reopened from it. Not undoable, like Import; git is the backup.
7. **Non-square tiles are refused in Data and in the Cut panel**: the game draws every tile into a square cell. A size that doesn't divide 48 isn't refused (Data doesn't know the cell); the 16 px convention covers it.
8. **The guess prefers 16 px**; **ids from pack names** collapse runs of underscores and trim them at the ends; the file keeps its pack name.
9. **Re-skinning keeps an object's identity** (decision 449). `ObjectReskin` keeps id, category, outline and zones (in the object's own 0-1 coordinates, so they stretch with a new size until the Shapes window adjusts them) and takes width, height and tiles from the rectangle; `CatalogDraft.SetObject` makes it one undo step; cells not yet tiles become tiles first, as `CreateObject` does.

### Extension points
- An import step that writes the manifest entry (decision 448) would be a Data writer for `art-sources.json` and a source choice in the import.
- Later plans' characters or monsters add their sources and files to the same manifest.
- Map removal in the editor would reuse the "refused while in use" shape: a warp into it, the new game's start.
- Removing a sheet's unused tiles (decision 450) is `SheetRemoval` plus a `CatalogDraft.RemoveTiles`.

### Replacement mapping

| Today | After this plan |
|---|---|
| Hand-edited `tiles.json`, `objects.json`, `tilesheets.json` to take content out | E1b, E3a, E3c in the editor |
| The editor locked out by a missing image (`MainViewModel.LoadError`) | `ArtStatus`'s warning, marked cells (X1, X2) |
| `ContentWriter.WriteMapCatalogs` refusing on missing art | content checks only; the game's startup keeps the art check |
| `TileBrowserViewModel.ChoicesOf` | `Tiles/SheetChoices`, missing sheets listed |
| A new object to change an object's art (losing its events) | `ObjectReskin` (O1) |
| `StartMapScenarios.RepositoryContent`, `EditorTestContent.RepositoryContent`, `CopiedContentRoot` reading `content/` for maps | the same helpers reading `test-world/` |
| `RepositoryNewGameTests`' cell-pinned tests | `TestWorldNewGameTests`, and R1's restatements |
| `ForestEncounterRateSimulation` on the repository forest's cell (28, 7) | the same on the test world's forest |
| `ContentCatalogLoaderTests`' `tree` outline and villager pins | the same on the test world |
| The 48 px sheets (City, Grass1, Grass2, Ground, RA_*, Water1, Water2, tree01_s_01_animation, FG_Forest_Fall if dropped) | Mana Seed sheets cut at 16 px |

## Risks
- **Test churn.** T2a-T2c touch many files; each is mechanical and guarded by the test count before and after.
- **The frozen world names live content.** Renaming a quest stage, a shop, an item, a member, an encounter, a music file, a still or a voice the frozen maps name fails `TestWorldTests` by file; the fix is in the frozen file, by hand.
- **Stale test outputs.** `PreserveNewest` never deletes, so a file removed from `content/` stays in each test output's copy until a clean build. `ArtSourcesTests` reads the source tree (AS1); other tests reading the copy (`SoundSourcesTests`) keep the backlog's line.
- **License.** The imported sheets are committed; a public repository would redistribute them (Q6, before C1). The "No GenAI" clause is the developer's accepted risk (decision 438); agents never open Mana Seed images (AS1's rule).
- **Saves on rebuilt maps.** A save made before a rebuild loads at its old position, maybe in a wall or off a shrunk map. Delete the development saves after C2 (C3's step; backlog line on repairing a position).
- **Scale mismatch.** The 48 px player and NPC sprites look small beside 3x Mana Seed props until the characters plan.
- **A map being rebuilt can't be walked** where its layers are erased (an empty cell blocks). Rebuild one map at a time and save it painted.
- **Saving with missing art** writes content the game refuses to start with (decision 442's exception): the status band's warning and the game's startup message both name the file. The hand checks open the game after each phase.
- **The purge (E3c) is not undoable** and erases cells on maps that aren't open; the question counts the cells, objects and placements, and git holds the previous content.
- **Re-skinning to another size** moves an object's footprint: a placement near a map edge may then overhang it, and its outline stretches until edited. The hand check re-skins the chest at the same size.
- **Removing against a dirty open map.** A tile or object unused by the open map's unsaved state but used by its saved file can be removed; if the map's changes are then discarded, the next catalog save is refused, naming the map.
- **File sizes.** `Shell/MainViewModel` (502) doesn't grow: X1 moves about 25 lines out before X2, E3a, E3c and O1 add about 11. `Tiles/TileBrowserViewModel` (310) loses `ChoicesOf` (X2) before X2's guards. `TileBrowserViewModelTests` (410) and `MainViewModelTests` (449) take no new test: new tests go in new files. `CatalogDraft` (228) takes about 45 lines over E1b, E3a and E3c; if it nears 280, the removals move into a partial file in the slice that crosses.
- **Determinism, performance, save format:** unchanged. `MissingArt` checks one file per sheet and sprite when the sheets change, not per frame.
