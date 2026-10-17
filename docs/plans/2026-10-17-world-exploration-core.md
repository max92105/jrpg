# Legacy demo migration, part 2: world and exploration core
Status: approved

## Goal
Port the legacy demo's world rules into `src/Jrpg.Core`, headless and deterministic, with the map, tile and object content converted from `Assets/` into validated `content/` JSON. At the end of this plan a test can: start a new game on the start map, walk the player with game-loop deltas and an analog direction, open the chest (and find the potion in the inventory), go through the door into the house and back, walk the forest until a seeded random encounter hands a `goblin_trio` to the battle code from part 1, save to a file, and load it back into an equivalent session, even after content has changed. Nothing is drawn: the MonoGame side is part 3.

Scope comes from part 1 ([2026-10-17-legacy-core-migration.md](2026-10-17-legacy-core-migration.md)): the "Follow-up plans" section, every inventory row marked P2, decision 20, and the deviation notes (`PartyState.MemberById` is deferred to save/load). After the review, cutscenes, event-started battles and render-only content move to part 3 (see "Moved to part 3").

## Guiding rule (same as part 1): port mechanisms, not design
- Port each **mechanism** (walking, collision, walkability, events, warps, encounters, saves) in its simplest working form.
- Every tunable number goes to `content/`, named so its meaning is obvious. A fixed technical limit is a named Core constant.
- Undesigned rules are marked **placeholder** in their slice and listed here.
- Game-design choices go to the developer (decisions 26 onwards).

Placeholders in this plan: walk speed 3 tiles/s, player radius 0.1 tiles, interaction reach 0.6 tiles, sliding along walls (question 45), an encounter "step" = entering a new cell, each map's encounter odds, encounter list and grace steps, a found item's extra copies being lost, and the save-repair rules of decision 40 (an unknown save map falls back to the new-game start location; a worn item that can no longer be worn returns to the inventory).

### Tunables
| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| `walkSpeed` | `rules.json` `exploration` | 3.0 | Tiles per second at full stick tilt or a held key. |
| `playerRadius` | `rules.json` `exploration` | 0.1 | Half the side of the player's collision square, in tiles. |
| `interactReach` | `rules.json` `exploration` | 0.6 | How far in front of the player interaction looks, in tiles. |
| `oneInSteps` | map settings `randomEncounters` | forest: 25 | Encounter chance per step, 1 in N. |
| `graceSteps` | map settings `randomEncounters` | forest: 3 | Steps without encounters after entering the map or after any fight. |
| `encounterIds` | map settings `randomEncounters` | forest: `goblin_trio` | Possible encounters, chosen uniformly. |
| `displayName` | map settings | converted legacy name | Shown when entering a map (part 3). |
| `InteractionFinder.InteractMargin` | Core constant | 0.1 | Slack around an object's bounds when looking for it. |
| `WarpArrival.SearchRadius` | Core constant | 8 | How far (in cells) arrival looks for a walkable cell. |
| stick deadzone | part 3 input settings | 0.2 | Below this tilt the stick counts as released. Input mapping, not a world rule. |
| acceleration | not implemented | - | Legacy starts and stops instantly; add only if the feel needs it. |

## Out of scope
- All rendering, input mapping, audio, camera, fades, dialogue box, prompt bubble, map-name banner, sprite and tile animation (part 3).
- The editor (part 4).
- Loading legacy saves (decision 20). A legacy save file is refused with a clear error, with a test.
- New mechanics: NPC movement, switches/variables, conditional branches, shops, gold.

### Moved to part 3 (to add to the part 3 plan)
- **Cutscenes** (decision 36: the intro stays a test hook). The design is ready: `Cutscenes/CutsceneDefinition` with step records (`FadeOutStep`, `FadeInStep`, `ShowBackgroundStep`, `ShowActorStep`, `MoveActorStep`, `CutsceneDialogueStep(Speech)`), a delta-driven `CutscenePlayback` (`Update(delta)` carrying leftover time into the next step, `ContinueDialogue()`, `Blackness`, `BackgroundFile`, `Actors`, `CurrentSpeech`, `IsFinished`), and the legacy intro moved verbatim from `CutsceneManager.StartIntroCutscene` to `content/cutscenes/intro.json`.
- **Event-started battles** (`StartBattleStep`): whether steps after a victory continue is undecided (question 30). The converter reports `Battle` actions/triggers it skips (none exist in the current data).
- **Render-only content:** tilesheets (`Tilesheets.json`, `TilesheetDefinition`, the 48 px default for unlisted sheets), tile sheet cells, object tiles and their layer offsets, `AbovePlayerZones`, `Category`, placed-object layers, tile layer `Z`, `DrawSortKey` and the layer constants. Part 2 doesn't keep them in `content/` because `Assets/` stays the source of truth for map geometry until part 4 (decision 42, ADR 0005), so part 3 extends the converter and re-runs it.
- **UI over Core state:** `SaveSlotSummary` (what a slot shows: map display name, play time, party), `ExplorationState.InteractionInFront` (prompt), `IsPlayerMoving` and `CurrentDefinitionOf` (sprites), the item-received message from `EventRun`'s receipt, the map-name banner (decision 33), save slot and autosave mapping (decision 34, question 44), the defeat flow (decision 31: the player chooses "load last save" or "main menu", with a loading screen between), and the stick deadzone.

## Design overview

### Current state checked
- Core has `Battle/`, `Content/`, `Effects/`, `Items/`, `Party/`, `Progression/`, `Randomness/`, `Session/`, `Skills/`, `Stats/`. `GameSession.StartNew(ContentCatalog)` builds party and inventory; `ContentCatalog` is a `required init` record; `GameRules(PartyRules Party, ExperienceTable Experience)` is positional.
- Data: `ContentCatalogLoader.Load(root)` reads each file through `ReadValidated`, with `ContentJsonOptions.Strict` (camelCase, string enums, unknown members and duplicates rejected), `ContentListValidator<T>` and `RangeProblems`. `InternalsVisibleTo`: Data -> Data.Tests; Core -> Core.Tests and Simulations.
- Test helpers: `Builders/ContentCatalogBuilder`, `Randomness/FixedSequenceRandom`.

### Legacy content facts that shape the design
Checked against the files, not assumed:
- `Assets/Tiles/Tileset.json`: 283 tiles, PascalCase, a dictionary keyed by the string id. **Every tile is walkable**; `Sprite`, `Frames`, `FrameDuration` and `CollisionPolygon` are `null` on all of them. Walls in the current maps come from object polygons and from cells with no tile.
- `Assets/Objects/Objects.json`: 12 objects with int ids. Polygons are `"x,y"` strings, normalized to the object's tile bounds. `Width`/`Height` are stale (the tree says 1×1 but spans 4×4 tiles); legacy computed the size from the tile group (`GetTileBounds`). `IsNpc` is false and `Meta` empty everywhere.
- Live maps (`Assets/Maps/*.json`): `start` (45×45, 55 objects, two warp tiles to `Forest.json` (1,1), the chest, the door), `Interior1` (15×20, two warp tiles back to `start.json` (5,5)), `Forest` (30×17, `randomEncounters: true`, no exits). They hold accidental computed fields (`usedTileIds`, `abovePlayerZoneZ`), id-0 placeholder tiles, and empty `meta`/`zOverrides`. Every object has a `uid`; none uses the old single `event`. Tile triggers are all `Warp`; object events use `GiveItem`, `SwapObject` and `Warp`.
- Backup maps (`Assets/Maps/Back/*.json`) use an **older map format**: tiles without `z`, objects as `{id, x, y, zIndex, hasCollision}` without `uid` or events, and `Test.json` embeds an old `objectDefinitions` block. `Back/start.json` (20×15) has a `Dialogue` trigger and a warp to `start2.json`; `Back/start2.json` (30×15) warps back to `start.json`; `Back/Test.json` (30×17) is tiles only. `Back/Custom Map.json` is an older copy of `Back/start.json` (same name, size, tiles and objects, but no triggers): skipped.

### Core feature folders added by this plan
```
Jrpg.Core/
  Geometry/    Point2D, GridCell, Facing, Polygon (internal)
  Maps/        TileDefinition, ObjectDefinition, PlacedObject, TileLayer, MapLayout, MapSettings,
               RandomEncounterTable, MapDefinition, CellEvent, MapLocation, WalkabilityGrid (internal)
  Dialogue/    Speech
  Events/      MapEvent, EventActivation, EventStep (+ DialogueStep, GiveItemStep, SwapObjectStep, WarpStep)
  World/       MapEventKey, ObjectSwap (public); WorldState (internal)
  Exploration/ MoveIntent, ExplorationState (public);
               PlayerWalker, ActiveMap, MapCollision, InteractionFinder, TriggeredEvent, EventRun, ItemReceipt,
               WarpArrival, RandomEncounters, ExplorationContext (internal)
  Party/       + PartyMemberProgress
  Saving/      SaveGame
  Content/     + ExplorationRules
```
**Folder dependencies, checked edge by edge (acyclic):**
| Folder | Depends on (new edges in bold) |
|---|---|
| `Geometry`, `Dialogue`, `World` | nothing |
| `Events` | **Geometry, Dialogue** |
| `Maps` | **Events, Geometry** |
| `Party` | Items, Skills, Progression, Stats (unchanged; `PartyMemberProgress` lives here) |
| `Content` | Battle, Items, Party, Skills, Progression, **Maps** |
| `Exploration` | **Maps, Events, World, Items, Battle, Content, Geometry, Randomness** |
| `Saving` | **Party, Maps, World, Items** |
| `Session` | Content, Items, Party, **Exploration, Saving, World** |
Nothing lower points back up: `Content` never references `Exploration` (so `ExplorationRules` lives in `Content/`), `Party` never references `Saving` (so the member snapshot `PartyMemberProgress` lives in `Party/`), and `Maps` never references `Battle` (encounter tables hold ids). Each slice's review re-checks this table.

### Visibility rule (as part 1)
- `public`: content records (`TileDefinition`, `ObjectDefinition`, `PlacedObject`, `TileLayer`, `MapLayout`, `MapSettings`, `RandomEncounterTable`, `MapDefinition`, `CellEvent`, `MapLocation`, `MapEvent`, `EventActivation`, the `EventStep` records, `Speech`, `ExplorationRules`), geometry values (`Point2D`, `GridCell`, `Facing`), what Data or the Data tests call (`ExplorationState`, `MoveIntent`, `SaveGame`, `PartyMemberProgress`, `MapEventKey`, `ObjectSwap`).
- `internal sealed`: `WalkabilityGrid`, `Polygon`, `PlayerWalker`, `ActiveMap`, `MapCollision`, `InteractionFinder`, `TriggeredEvent`, `EventRun`, `ItemReceipt`, `WarpArrival`, `RandomEncounters`, `ExplorationContext`, `WorldState` (and `GameSession.World`). Part 3 makes public what the renderer needs, when it needs it.
- Data: `ContentCatalogLoader`, `FileSaveStore`, `UnreadableSaveException` public; the rest internal. Data declares `InternalsVisibleTo` for `Jrpg.ContentConverter` (content options and writer).
- Tool: `tools/Jrpg.ContentConverter` declares `InternalsVisibleTo` for `Jrpg.ContentConverter.Tests`.

### Key design decisions in this plan
1. **`Assets/` stays the source of truth for map geometry until part 4** (decision 42, ADR 0005). The developer keeps editing maps in the legacy MapEditor, so the converter in `tools/Jrpg.ContentConverter` stays re-runnable. It owns `content/generated/` only and rewrites it completely on each run; it never writes anywhere else. This is a temporary, explicit exception to "`content/` is the source of truth", ending when the part 4 editor writes `content/` directly.
2. **What the legacy editor can't express lives in hand-written files.** Each map has a generated layout (`content/generated/maps/{id}.json` -> `MapLayout`) and a hand-written settings file (`content/maps/{id}.json` -> `MapSettings`: display name, random encounters). The loader composes them into `MapDefinition`. A re-run can't wipe hand-written values.
3. **Part 2 content holds only what part 2 uses.** Tiles carry `Id` and `IsWalkable`; objects carry `Id`, `Width`, `Height` and `CollisionPolygon`; layouts carry layers of tile ids, placed objects (uid, object id, position, events) and cell events. Render data is added by part 3 to the converter and schema (re-run from `Assets/`), so nothing is lost.
4. **Legacy inputs are converted only where the data has them.** Absent cases are conversion errors naming the file (old single `event`, a tileset key that disagrees with its inner `Id`, an action or trigger type with no step). Present cases are converted (missing uids in the Back maps, via the legacy rule `{id}@{x:0.###},{y:0.###}`). Content the plan can't express yet (`Battle`) is skipped and listed in a report naming file and object/cell (decision 39).
5. **One event system** (decision 20). A legacy warp or dialogue tile becomes a `CellEvent`: a `MapEvent` with `Touch` activation on a cell. Cell events are touch-only (validator). Objects keep their list of events. Both run through the same `EventRun` and remember "once" in the same `WorldState`.
6. **Event ids replace legacy event keys.** Every `MapEvent` has an `Id` unique within its map: `{uid}_{index}` for object events (`0bff6243_0`, `0bff6243_1`) and `cell_{x}_{y}` for cell events. Legacy saves aren't loaded, so the legacy `uid` / `uid#index` rule disappears.
7. **Events are a step cursor driven by `Continue()`** (ADR 0006). The run waits on steps the player must see (dialogue, item received) or that exploration completes (warp). A `GiveItemStep` applies when it becomes current and records what was actually added and lost in an `ItemReceipt` (like ADR 0004's actual amounts), so the message can tell the truth. A warp completes on `Continue()`, after the Game's fade-out. A swap applies and doesn't wait. "Once" is recorded when the event starts.
8. **A found item is added to the inventory** (decision 20); extra copies beyond `MaxStack` are lost (decision 29, placeholder).
9. **All maps are loaded and validated up front** in `ContentCatalog.Maps`. A warp to a missing map or an out-of-bounds cell is a load error, not a runtime failure.
10. **Every map entry goes through `WarpArrival`**: warps, new game and loading a save, as legacy `EnterMap` did.
11. **Movement takes an analog direction** (decision 37). `MoveIntent` is a vector of length 0..1: a stick gives partial speed, keys give full speed, and a diagonal is normalized (legacy moved √2 faster). Facing follows the dominant axis. The same input works for controller and keyboard; the Game maps devices to it (part 3).
12. **Encounters roll per step through one seam** (decision 26). `RandomEncounters.RollStep(table)` is the only place that draws. A step is entering a new cell. The step counter resets on entering a map and after any fight, so the first `graceSteps` steps after either can't start a battle (never back-to-back).
13. **Saves are snapshots, never runtime objects** (ADR 0002 consequence; format in ADR 0007). `GameSession.Capture(savedAt)` builds an immutable `SaveGame`; `GameSession.Restore(save, catalog, random)` rebuilds a session. Data's `FileSaveStore` writes JSON in a `formatVersion` envelope with atomic replacement, and on read repairs the save against current content (decision 40): what no longer exists is dropped, the rest is kept. Only structural corruption makes a save unreadable. The random state isn't saved (decision 43). Play time is summed from game-loop deltas and counts everything (decision 35).

### Content layout
| File | Written by | Holds | Slice |
|---|---|---|---|
| `content/generated/tiles.json` | converter | 283 tiles (`id`, `isWalkable`) | 1b |
| `content/generated/objects.json` | converter | 12 objects (snake_case ids, size, polygon) | 1c |
| `content/generated/maps/{id}.json` | converter | `start`, `interior_1`, `forest`, `back_start`, `back_start2`, `back_test` layouts | 1d, 3c |
| `content/generated/conversion-report.txt` | converter | what was skipped or changed, by file and object/cell | 1b onwards |
| `content/maps/{id}.json` | hand | display name, random encounters | 1d, 4b |
| `content/rules.json` | hand | gains `exploration` | 2, 3d |
| `content/new-game.json` | hand | gains `startLocation` | 4a |
The `id` inside each map file must equal its file name. Every layout needs a settings file and every settings file a layout.

### Legacy inventory: where every type and behavior goes
Slice numbers refer to this plan. P3 = rendering, P4 = editor.

**`JRPG.Core/Maps`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `Models/Map` | `Maps/MapLayout` + `Maps/MapSettings` -> `Maps/MapDefinition` | 1a, 1d | Immutable. Tiles become layer grids. `Name` -> `MapSettings.DisplayName` (hand-written, seeded from the legacy name). `RandomEncounters` bool -> `MapSettings.RandomEncounters` table (4b). Tile index and editing methods -> P4. `UsedTileIds` dropped. |
| `Models/Tile` | `Maps/TileLayer` cells | 1a | Layers are ordered by legacy z; `Z` itself -> P3. `Trigger` -> `CellEvent` (3c). Id 0 placeholders dropped. |
| `Models/TileDefinition` | `Maps/TileDefinition` (`Id`, `IsWalkable`) | 1a | Sheet fields -> P3. `Sprite`, `Frames`, `FrameDuration`, `CollisionPolygon` dropped (null on every tile). |
| `Models/AnimatedTileDefinition`, `IO/TilesetLoader` | removed | - | Unused. |
| `Models/TilesheetDefinition`, `IO/TilesheetDatabase` | P3 | P3 | Render-only. |
| `Models/ObjectDefinition` | `Maps/ObjectDefinition` (`Id`, `Width`, `Height`, `CollisionPolygon`) | 1a | String id. Size computed by the converter from the tile group (stale fields replaced). Tiles, `AbovePlayerZones`, `Category` -> P3/P4. `IsNpc`, `Meta` dropped. |
| `Models/TileGroupEntry` | P3 | P3 | Render-only. |
| `Models/PlacedObject` | `Maps/PlacedObject` (`Uid`, `ObjectId`, `Position`, `Events`) | 1a, 3a | Swaps no longer mutate it (`WorldState`). `Z`/`ZOverrides`/`GetTileZ`/`AbovePlayerZoneZ` -> P3. `Meta` dropped. `EventKey` -> event ids. `NewUid` -> P4. |
| `DrawSortKey`, `MapConstant` | P3; file paths -> Data constants | P3 | No Core user in part 2. |
| `Enums/TriggerType` | removed | 3a | Step records. |
| `IO/MapSerializer` | Data loader + converter | 1d | Uid derivation -> converter. |
| `IO/TileDatabase`, `ObjectDatabase` (static, mutable) | `ContentCatalog.Tiles`, `.Objects` | 1b, 1c | Immutable. Editing and save methods -> P4. |

**`JRPG.Core/Events`, `Triggers`, `Cutscenes`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `MapEvent` | `Events/MapEvent` | 3a | Gains `Id`. `Once` -> `IsOnce`; `OnlyWhenObjectId` -> string; `Actions` -> `Steps`; `IsActiveOn` -> `CanRunOn(objectId)`. `Describe`/`Clone` -> P4. |
| `EventAction` (flat) | `DialogueStep`, `GiveItemStep`, `SwapObjectStep`, `WarpStep` | 3a | Typed records with a `type` discriminator. `Battle` -> part 3 (`StartBattleStep`). |
| `EventActivation` | `Events/EventActivation` | 3a | |
| `EventActionType` | removed | 3a | |
| `Trigger*`, `TriggerJsonConverter` | converter -> touch `CellEvent` | 3c | `Warp` (live and Back) and `Dialogue` (Back) converted; `Battle` skipped with a report; `Item` has no data, so it's a conversion error. |
| `Cutscenes/*` | part 3 | P3 | Decision 36. |

**`JRPG.Core/Systems`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `WorldState` | `World/WorldState` (internal) | 3a | Keys are `MapEventKey`/`ObjectSwap` records. `ApplyTo` (mutating maps) -> `ActiveMap.CurrentDefinitionOf`. `LoadFrom` -> restore constructor. |
| `SaveGames/SaveGameManager` | `Data/Saving/FileSaveStore` | 5b | No interface (one implementation). `DateTime.Now` -> `savedAt` from the Game. `catch { IsCorrupt }` -> `UnreadableSaveException` with the reason. Slots are named by the caller (decision 34 lives in part 3). |
| `SaveGames/Models/SaveData` | `Saving/SaveGame` | 5a | Members by id as `PartyMemberProgress`; active order by id. `Gold`, `PartyAverageLevel`, `LocationName` dropped. |
| `SaveGames/Models/SaveDataPreview`, `GetLocationName` | P3 `SaveSlotSummary` (uses `MapSettings.DisplayName`) | P3 | |
| `SaveGames/Models/SavedInventoryEntry` | `Items/InventoryEntry` + `PartyMemberProgress.WeaponId`/`ArmorId` | 5a | Move model (ADR 0003). |
| `GameSystemManager` | `Session/GameSession` | 4a, 5a | Location from `ExplorationState`; play time from deltas. |
| `InventoryManager.GetSerializableItems`/`LoadFromSavedData`, `PartyManager.LoadFromSaveData` | `GameSession.Capture`/`Restore`, `PartyState.MemberById`, `PartyMember.Progress`/`Restore` | 5a | |

**`JRPG/` (WPF game)**
| Legacy | New | Where |
|---|---|---|
| `PlayerManager` movement, facing, clamping, cell + polygon collision | `Exploration/PlayerWalker`, `MapCollision`, `Geometry/Polygon`, `rules.json` | 2 |
| `PlayerManager` animation | P3 | P3 |
| `MapManager.IsCellWalkable` | `Maps/WalkabilityGrid` | 1a |
| `MapManager.ReadMap`/`SetMap` (+ `World.ApplyTo`) | `ContentCatalog.Maps` + `Exploration/ActiveMap` | 1d, 3a |
| `MapManager.GetTileTrigger` | cell events in `ExplorationState.Walk` | 3d |
| `MapManager` animated frames on the wall clock, sprite lookups | P3 | P3 |
| `GameManager.FindEventInFront`, `ObjectEvent` | `Exploration/InteractionFinder`, `TriggeredEvent` | 3d |
| `GameManager.RunEventAsync` | `Exploration/EventRun` + `ExplorationState.ContinueEvent` | 3a, 3d |
| `GameManager.UpdateObjectEvents`, `touchArmed`, `lastTriggeredX/Y`, `CheckForTriggers` | `ExplorationState.Walk`/`TryInteract` (key edges -> P3 input) | 3d |
| `GameManager.WarpAsync`, `EnterMap`, `FindArrivalPoint` | `ExplorationState` map entry, `Exploration/WarpArrival` | 4a |
| `GameManager.TryRandomEncounter`, static `Random`, `EncounterOneIn`, `encounterGrace` | `Exploration/RandomEncounters` | 4b, 4c |
| `GameManager` prompt, camera, fades, sprites | P3 | P3 |
| "(Unknown item …)", "(Can't go to map …)" messages | load-time validation | 3b |
| `TriggerHandler` | event steps; hardcoded goblins -> map encounter lists | 3a, 4b |
| `TriggerHandler` dialogue defaults `"???"` / `Fighter.png` | removed: speaker and portrait are optional (decision 32) | 3a |
| `TriggerHandler` item found: message box only | `GiveItemStep` adds to the inventory (bug fix) | 3a |
| `CutsceneManager`, `InputRouter` F1 | P3 | P3 |
| `SceneManager.StartNewGameAsync`: `"start.json"`, `(10, 3)` | `new-game.json` `startLocation` | 4a |
| `SceneManager.LoadGameAsync` | `GameSession.Restore` (scene flow P3) | 5a |

**`MapEditor/`**: unchanged and still in use (decision 42). It writes the legacy formats into `Assets/`; the converter reads them.

## Slices
Line counts are estimates of hand-written code plus tests; generated content isn't counted.

- [x] **ADR 0005: map content format and source of truth** (before 1a). Options: (A) copy legacy files and read them through permanent Data DTOs; (B) a re-runnable converter from `Assets/` into a new schema loaded straight into Core records; (C) convert catalogs, read maps as-is. Recommendation (decision 42): **B**, with these rules recorded:
  - `Assets/` + the legacy MapEditor are the source of truth for map geometry (tiles, objects, placements, legacy events) until part 4: a temporary, explicit exception to "`content/` is the source of truth".
  - The converter owns `content/generated/` and rewrites all of it on every run; it never writes elsewhere. Generated files aren't edited by hand.
  - Everything the legacy editor can't express (display name, encounter table, odds, grace steps) lives in hand-written `content/maps/{id}.json`, composed with the layout at load.
  - Generated content holds only what current code uses; later parts extend the converter and re-run it (render data in part 3). No lossless round trip is needed while `Assets/` is the source.
  - Tile ids stay ints (editor-generated, thousands of references); object and map ids become snake_case strings. Tiles are stored as one row grid per layer (0 = empty). Map ids equal file names. All maps are preloaded.
  - Inputs absent from the data are conversion errors naming the file; content the plan can't express yet is skipped and listed in `conversion-report.txt`.
  - Part 4 ends the exception: the new editor writes `content/` directly and the converter is deleted.

- [ ] **1a. Map model and walkability** (Core, about 200 lines)
  - **Goal:** tiles, objects and maps exist as immutable Core records, and a map answers which cells can be walked on.
  - **New types (Core):**
    - `Geometry/Point2D`: `readonly record struct Point2D(double X, double Y)`. A position in tile units.
    - `Geometry/GridCell`: `readonly record struct GridCell(int X, int Y)` + `static GridCell Containing(Point2D point)` (floor) and `Point2D Center`. One map cell.
    - `Maps/TileDefinition`: content record (`Id` int, `IsWalkable`). One tile type, as far as walking is concerned.
    - `Maps/ObjectDefinition`: content record (`Id`, `Width`, `Height` in tiles, `CollisionPolygon` as `IReadOnlyList<Point2D>` normalized 0..1 over the object, empty = no collision). One placeable object type.
    - `Maps/PlacedObject`: content record (`Uid`, `ObjectId`, `Position`). One object on a map. `Events` comes in 3a.
    - `Maps/TileLayer`: content record (`IReadOnlyList<IReadOnlyList<int>> Rows`) + `int TileIdAt(GridCell cell)` and `const int EmptyTileId = 0`. One layer of tiles.
    - `Maps/MapLayout`: content record (`Id`, `Width`, `Height`, `Layers`, `Objects`) + `bool Contains(GridCell cell)`. A map's geometry.
    - `Maps/MapSettings`: content record (`Id`, `DisplayName`). A map's hand-written settings; gains `RandomEncounters` in 4b.
    - `Maps/MapDefinition`: `record MapDefinition(MapLayout Layout, MapSettings Settings)` + `Id`. One map, as the game uses it.
    - `Maps/WalkabilityGrid` (internal): which cells can be walked on. `static WalkabilityGrid Build(MapLayout layout, IReadOnlyDictionary<int, TileDefinition> tiles)`, `bool IsWalkable(GridCell cell)`. Rule from `MapManager.IsCellWalkable`: inside the map, at least one tile on some layer, and every tile in the cell walkable.
  - **Tests (`tests/Jrpg.Core.Tests/Maps`, `/Geometry`; builders `Builders/MapLayoutBuilder`, `Builders/TileDefinitionBuilder`):**
    - `GridCell_Containing_NegativeFraction_RoundsDown`
    - `WalkabilityGrid_IsWalkable_AllTilesWalkable_ReturnsTrue`
    - `WalkabilityGrid_IsWalkable_BlockingTileOnUpperLayer_ReturnsFalse`
    - `WalkabilityGrid_IsWalkable_CellWithNoTile_ReturnsFalse`
    - `WalkabilityGrid_IsWalkable_OutsideMap_ReturnsFalse` (one cell at -1, one at `Width`)

- [ ] **1b. Converter tool and tile content** (tool + Data, about 280 lines)
  - **Goal:** `dotnet run --project tools/Jrpg.ContentConverter -- Assets content` regenerates `content/generated/tiles.json`, which loads and validates into `ContentCatalog.Tiles`.
  - **Projects:** `tools/Jrpg.ContentConverter` (console, `net10.0`, references Core and Data) and `tests/Jrpg.ContentConverter.Tests` (references the tool), both added to `Jrpg.slnx` (decision 41). `CLAUDE.md` gains the `tools/` line and the converter command. The test project links `Assets/Tiles/*.json`, `Assets/Objects/*.json`, `Assets/Maps/*.json` and `Assets/Maps/Back/*.json` as `legacy-assets/`, and `content/**` as `content/`.
  - **Tool types (internal unless stated):**
    - `Program`: parses the two paths, runs the conversion, prints the report.
    - `LegacyContentConversion`: converts everything into `{contentRoot}/generated/` (deletes and rewrites the folder) and writes `conversion-report.txt`. Grows per slice.
    - `ConversionReport`: collects skipped and changed items, each naming file and object or cell.
    - `Tiles/LegacyTileJson`: DTO with every field the file has. `Tiles/TileConversion`: keyed dictionary -> `TileDefinition` list; a key that disagrees with its inner `Id` is an error naming the key.
  - **Data:** `Content/ContentFileWriter` (internal): writes a content file with the content options, indented. `InternalsVisibleTo("Jrpg.ContentConverter")`. `Maps/TileCatalogValidator`: duplicate ids, id >= 1 (0 means empty). `ContentCatalog` gains `Tiles` (by int id); `ContentCatalogBuilder` gains a default.
  - **Tests:**
    - Tool: `TileConversion_Convert_KeyDisagreesWithInnerId_ThrowsNamingTheKey`, `TileConversion_Convert_RepositoryTileset_Converts283WalkableTiles`, `LegacyContentConversion_Convert_ExistingGeneratedFile_IsReplaced` (a stale extra file in `generated/` is gone after the run), `LegacyContentConversion_Convert_FileOutsideGenerated_IsUntouched`
    - Data: `TileCatalogValidator_Validate_TileIdZero_ThrowsNamingTheField`, `TileCatalogValidator_Validate_DuplicateId_ThrowsNamingTheId`, `ContentCatalogLoader_Load_RepositoryContent_Succeeds` (existing)

- [ ] **1c. Object content** (tool + Data, about 220 lines)
  - **Goal:** `content/generated/objects.json` is generated with snake_case ids and real sizes, and loads and validates.
  - **Tool:** `Objects/PointStringConverter` (`"x,y"`, invariant culture), `Objects/LegacyObjectJson`, `LegacyIds` (`"Chest Opened"` -> `chest_opened`, `"House1"` -> `house_1`, `"Door_Close"` -> `door_close`, `Interior1.json` -> `interior_1`; one rule for every id), `Objects/ObjectConversion` (size from the tile group's bounds; the stale `Width`/`Height` are reported when they differ; two names giving the same id is an error).
  - **Data:** `Maps/ObjectCatalogValidator`: duplicate ids, size >= 1, polygon has 0 or >= 3 points, every point within 0..1. `ContentCatalog` gains `Objects`.
  - **Tests:**
    - Tool: `PointStringConverter_Read_UnderFrenchCulture_ParsesDotAsDecimal` (sets the current culture to fr-FR), `LegacyIds_FromName_MixedSeparatorsAndDigits_ReturnsSnakeCase` (theory over the examples), `ObjectConversion_Convert_StaleSize_UsesTileGroupBoundsAndReports` (the tree: 4×4), `ObjectConversion_Convert_TwoNamesSameId_ThrowsNamingBoth`
    - Data: `ObjectCatalogValidator_Validate_TwoPointPolygon_ThrowsNamingTheObject`, `ObjectCatalogValidator_Validate_PointOutsideUnitSquare_ThrowsNamingTheObject`

- [ ] **1d. Map content** (tool + Data + content, about 300 lines)
  - **Goal:** the three live maps are generated as layouts, their hand-written settings exist, and both load, validate and compose into `ContentCatalog.Maps`. Back maps and events come in 3c.
  - **Tool:** `Maps/LegacyMapJson`, `Maps/LegacyPlacedObjectJson` (including the accidental `usedTileIds` and `abovePlayerZoneZ`, and `events`/`event`/`trigger`, which this slice skips: `TODO(plan 2026-10-17-world-exploration-core, slice 3c)`), `Maps/MapConversion`: tile list -> one row grid per distinct z in ascending order (id 0 dropped; a second tile on the same cell and layer is an error naming the cell); objects with their uid and string object id; map id from the file name. A missing `uid` here is an error (live maps all have one; Back maps are handled in 3c).
  - **Data:** `Maps/MapLayoutValidator` with an internal `MapReferences` record (tile ids, object definitions): id equals the file name, size >= 1, each layer has `Height` rows of `Width` ids, unknown tile ids, unknown object ids, duplicate uids. `Maps/MapSettingsValidator`: id equals the file name, non-empty display name. `Maps/MapFolderReader` (internal): reads both folders in ordinal file-name order and fails on a layout without settings or settings without a layout, then composes `MapDefinition`s. `ContentCatalog` gains `Maps`.
  - **Content (hand-written):** `content/maps/start.json`, `interior_1.json`, `forest.json` with `displayName` seeded from the legacy map names (`start`, `Interior1`, `Forest`; placeholders, decision 33).
  - **Tests:**
    - Tool: `MapConversion_Convert_TileIdZero_IsDropped`, `MapConversion_Convert_TwoTilesOnSameCellAndLayer_ThrowsNamingTheCell`, `MapConversion_Convert_LayersOrderedByZ`, `MapConversion_Convert_LiveObjectWithoutUid_ThrowsNamingTheFile`, `LegacyContentConversion_Convert_RepositoryAssets_ProducesLoadableContent` (converts `legacy-assets` into a temp copy of `content/` and loads it with `ContentCatalogLoader`)
    - Data: `MapLayoutValidator_Validate_IdDiffersFromFileName_ThrowsNamingTheFile`, `MapLayoutValidator_Validate_RowCountDiffersFromHeight_ThrowsNamingTheLayer`, `MapLayoutValidator_Validate_UnknownObjectId_ThrowsNamingTheUid`, `MapFolderReader_Read_LayoutWithoutSettings_ThrowsNamingTheMap`

- [ ] **2. Walking and collision** (Core + rules content, about 300 lines)
  - **Goal:** the player walks with an analog direction and game-loop deltas, at partial speed for a partial tilt and never faster diagonally, and is stopped by unwalkable cells and object collision polygons.
  - **New types (Core):**
    - `Geometry/Facing`: `Up`, `Down`, `Left`, `Right`.
    - `Geometry/Polygon` (internal static): `bool Contains(IReadOnlyList<Point2D> vertices, Point2D point)` (even-odd rule; legacy's `+ 0.00001` guarded a division the edge test already excludes).
    - `Content/ExplorationRules`: content record, `required init` (`WalkSpeed`, `PlayerRadius`); gains `InteractReach` in 3d. `GameRules` gains `Exploration`.
    - `Exploration/MoveIntent`: `readonly record struct` with `static MoveIntent FromAxes(double x, double y)` (vectors longer than 1 are scaled to length 1, so a keyboard diagonal (1, 1) and a stick pushed into a corner both give full speed, never √2), `X`, `Y`, `Magnitude`, `static MoveIntent None`. Where and how hard the player is pushing; the Game maps keys and sticks to it (deadzone in part 3).
    - `Exploration/ActiveMap` (internal): the map the player is on and its objects' definitions. `ActiveMap(MapDefinition map, IReadOnlyDictionary<string, ObjectDefinition> objects, WalkabilityGrid walkability)`, `Definition`, `Walkability`, `ObjectDefinition CurrentDefinitionOf(PlacedObject placed)` (gains world swaps in 3a).
    - `Exploration/MapCollision` (internal): whether a body fits at a point. `MapCollision(ActiveMap map)`, `bool CanStandAt(Point2D center, double radius)`: every cell under the body's square is walkable, and for each object the center is first checked against its bounding box, then transformed into the object's 0..1 space and tested against its polygon. No allocation per call (`game.md` rule for per-frame code). Legacy also tested only the center against polygons.
    - `Exploration/PlayerWalker` (internal): the player's position, facing and movement. `PlayerWalker(Point2D position, Facing facing, ExplorationRules rules)`, `Position`, `Facing`, `IsMoving` (true while pushing, even against a wall; touch events rely on it), `Cell`, `void PlaceAt(Point2D position)`, `void Walk(MoveIntent intent, TimeSpan delta, MapCollision collision)`. Moves `WalkSpeed × Magnitude × delta` along the intent; if the full move is blocked, tries the X part alone, then the Y part alone (sliding along walls, placeholder, question 45; legacy stopped dead). Facing follows the larger axis, vertical on a tie (legacy). Clamped to `[radius, size - radius]`.
  - **Content:** `rules.json` gains `"exploration": { "walkSpeed": 3.0, "playerRadius": 0.1 }`; `GameRulesValidator` checks both > 0.
  - **Tests:**
    - Core: `Polygon_Contains_PointInConcaveNotch_ReturnsFalse`, `Polygon_Contains_PointLevelWithHorizontalEdge_ReturnsExpectedSide`, `MoveIntent_FromAxes_KeyboardDiagonal_HasLengthOne`, `MoveIntent_FromAxes_HalfTilt_KeepsHalfMagnitude`, `MapCollision_CanStandAt_BodyOverlapsBlockedNeighbourCell_ReturnsFalse`, `MapCollision_CanStandAt_CenterInsideObjectPolygon_ReturnsFalse`, `MapCollision_CanStandAt_InsideBoundsButOutsidePolygon_ReturnsTrue`, `PlayerWalker_Walk_FullTiltForHalfSecond_MovesSpeedTimesDelta`, `PlayerWalker_Walk_HalfTilt_MovesHalfAsFar`, `PlayerWalker_Walk_DiagonalIntoWall_SlidesAlongIt`, `PlayerWalker_Walk_IntoBlockedCell_KeepsPositionButIsMoving`, `PlayerWalker_Walk_PastMapEdge_ClampsToRadius`, `PlayerWalker_Walk_EqualAxes_FacesVertical`, `PlayerWalker_Walk_NoIntent_StopsMovingAndKeepsFacing`
    - Data: `GameRulesValidator_Validate_ZeroWalkSpeed_ThrowsNamingTheField`

- [ ] **ADR 0006: map event flow** (before 3a). Options: (A) a step cursor: Core waits on steps the player must see or that exploration completes, and the Game calls `Continue()`; (B) run the whole event at once and return field events, like battle (breaks on a warp mid-event); (C) the Game iterates steps and applies effects (legacy; rules in the Game). Recommendation: **A**, recording: a `GiveItemStep` applies when it becomes current and exposes an `ItemReceipt` (item, added, lost), so the message reports what actually happened, as ADR 0004 does for amounts; a warp completes on `Continue()`, after the Game's fade-out; a swap applies without waiting; "once" is marked when the event starts; event-started battles are out until question 30 is answered.

- [ ] **3a. Map events and world state** (Core, about 300 lines)
  - **Goal:** a map event runs step by step: the found item lands in the inventory, an object swap persists in the world state, and a "once" event is remembered.
  - **New types (Core):**
    - `Dialogue/Speech`: `record Speech(string? Speaker, string? PortraitFile, IReadOnlyList<string> Lines)`. One speaker's lines; speaker and portrait are optional (decision 32).
    - `Events/EventActivation`: `Interact`, `Touch`.
    - `Events/EventStep` (abstract record, `private protected` constructor) and `DialogueStep(Speech Speech)`, `GiveItemStep(string ItemId, int Quantity)`, `SwapObjectStep(string ObjectId)`, `WarpStep(string MapId, GridCell Cell)`. One thing an event does.
    - `Events/MapEvent`: content record (`Id`, `Activation`, `Prompt`, `IsOnce`, `OnlyWhenObjectId`, `Steps`) + `bool CanRunOn(string objectId)`. What happens when the player interacts with or touches an object or cell.
    - `Maps/CellEvent`: `record CellEvent(GridCell Cell, MapEvent Event)`. A touch event on a map cell (legacy tile trigger).
    - `PlacedObject` gains `Events` and `MapLayout` gains `CellEvents`, both non-required `init` with empty defaults, so the content generated in 1d still loads until 3c regenerates it.
    - `World/MapEventKey` (`MapId`, `EventId`) and `World/ObjectSwap` (`MapId`, `ObjectUid`, `ObjectId`): public records, saved in 5a.
    - `World/WorldState` (internal): what the player changed on maps. `WorldState()`, `WorldState(IEnumerable<MapEventKey> completed, IEnumerable<ObjectSwap> swaps)`, `bool IsEventDone(MapEventKey key)`, `string? SwappedObjectId(string mapId, string objectUid)`, `CompletedEvents`, `ObjectSwaps`, and `internal` mutators `MarkEventDone`, `SwapObject`.
    - `Exploration/TriggeredEvent` (internal): `record TriggeredEvent(MapEvent Event, PlacedObject? Source)`. An event about to run and its object (null for a cell).
    - `Exploration/ItemReceipt` (internal): `record ItemReceipt(ItemDefinition Item, int Added, int Lost)`. What a give-item step actually did.
    - `ActiveMap` gains the map id and a `WorldState`: `CurrentDefinitionOf` applies swaps; `SwapObject(PlacedObject placed, string objectId)`; `IsDone(MapEvent mapEvent)`; `MarkDone(MapEvent mapEvent)`.
    - `Exploration/EventRun` (internal): one running map event (ADR 0006). `EventRun(TriggeredEvent triggered, ActiveMap map, Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items)` marks a "once" event done, then advances to the first waiting step. `EventStep? Current`, `ItemReceipt? CurrentReceipt`, `bool IsFinished`, `void Continue()` (throws once finished). Advancing applies swaps without waiting, applies a give-item step and waits on it, and waits on dialogue and warp steps. Placeholder: copies beyond `MaxStack` are lost (decision 29).
  - **Tests (`tests/Jrpg.Core.Tests/Exploration`, `/World`; builder `Builders/MapEventBuilder`):**
    - `EventRun_Constructor_OnceEvent_MarksDoneBeforeFirstStep`
    - `EventRun_Constructor_SwapThenDialogue_AppliesSwapAndWaitsOnDialogue`
    - `EventRun_Constructor_OnlySwapSteps_IsFinished`
    - `EventRun_Continue_ReachesGiveItem_AddsItemToInventory` (the legacy bug)
    - `EventRun_Continue_GiveItemBeyondMaxStack_ReceiptReportsLostCopies`
    - `EventRun_Continue_LastStep_Finishes`
    - `EventRun_Continue_AfterFinished_Throws`
    - `WorldState_IsEventDone_SameEventIdOnOtherMap_ReturnsFalse`
    - `ActiveMap_CurrentDefinitionOf_SwappedObject_ReturnsSwappedDefinition`
    - `MapCollision_CanStandAt_AfterSwapToObjectWithoutPolygon_ReturnsTrue`
    - `MapEvent_CanRunOn_OnlyWhenOtherObject_ReturnsFalse`

- [ ] **3b. Event content loading** (Data, about 260 lines)
  - **Goal:** map files with events load through polymorphic steps, and broken event content fails at load time naming the map, event and step. Tested with hand-written fixtures; the repository maps get their events in 3c.
  - **Data:**
    - `Content/ContentTypeDiscriminators` (internal): a type-info modifier added to `ContentJsonOptions` that registers `EventStep`'s derived types under a `"type"` discriminator (`dialogue`, `giveItem`, `swapObject`, `warp`), unknown types rejected. Core stays attribute-free (ADR 0002).
    - `Maps/MapEventValidator` (internal), called by `MapLayoutValidator`: event ids unique per map; `Interact` events have a prompt; steps non-empty; dialogue has lines; item ids exist and quantity >= 1; swap and `onlyWhenObjectId` object ids exist; swaps only in object events; cell events are `Touch`; warp maps exist and the cell is inside the target map. `MapReferences` gains item ids and map sizes; layouts validate after all are read.
  - **Tests (Data):**
    - `ContentFileReader_Read_EventStepWithUnknownType_ThrowsNamingTheFile`
    - `MapEventValidator_Validate_GiveItemUnknownItem_ThrowsNamingTheEventAndStep`
    - `MapEventValidator_Validate_WarpCellOutsideTargetMap_ThrowsNamingTheEvent`
    - `MapEventValidator_Validate_DuplicateEventId_ThrowsNamingTheId`
    - `MapEventValidator_Validate_SwapStepInCellEvent_ThrowsNamingTheCell`
    - `MapEventValidator_Validate_InteractCellEvent_ThrowsNamingTheCell`
    - `MapEventValidator_Validate_InteractEventWithoutPrompt_ThrowsNamingTheEvent`

- [ ] **3c. Event conversion and backup maps** (tool + content, about 300 lines)
  - **Goal:** the chest, the door and the warp tiles are in the generated maps as events, and the Back maps are converted where the plan can express them, with a report for the rest.
  - **Tool:**
    - `Events/LegacyMapEventJson`, `Events/LegacyEventActionJson`, `Events/LegacyTriggerJson` DTOs. `Events/EventConversion`: flat actions -> typed steps (int object ids and map file names -> new ids; warp targets resolve relative to the map's own folder); event ids `{uid}_{index}` and `cell_{x}_{y}`; `Warp` and `Dialogue` triggers -> touch `CellEvent`s; `Battle` actions/triggers skipped and reported (decision 39); the single legacy `event` field and `Item` triggers are errors naming the file (no data has them). Empty prompts become `"Check"` (the legacy display default, now content).
    - `Maps/BackMapConversion`: the older format (tiles without `z` -> layer 0; objects `{id, x, y, zIndex, hasCollision}` without uid -> legacy-derived uid; `zIndex` and `hasCollision` reported as ignored, since collision now comes from the object definition; `Test.json`'s embedded `objectDefinitions` reported as ignored). Ids `back_start`, `back_start2`, `back_test`. `Back/Custom Map.json` is skipped as an older copy of `Back/start.json` (reported).
    - The 1d `TODO` goes; the converter is re-run.
  - **Content (hand-written):** `content/maps/back_start.json`, `back_start2.json`, `back_test.json` with display names. **Verify by hand first:** open the three Back maps in the legacy MapEditor; they predate the current tileset format, so if their tile ids no longer show the intended art, drop them from the conversion and record that as a deviation.
  - **Tests (tool):**
    - `EventConversion_Convert_WarpTrigger_BecomesTouchCellEventWithWarpStep`
    - `EventConversion_Convert_DialogueTrigger_KeepsSpeakerPortraitAndLines`
    - `EventConversion_Convert_SecondEventOnObject_GetsIndexedId`
    - `EventConversion_Convert_BattleAction_IsSkippedAndReported`
    - `EventConversion_Convert_LegacySingleEvent_ThrowsNamingTheFile`
    - `BackMapConversion_Convert_ObjectWithoutUid_DerivesLegacyUid`
    - `BackMapConversion_Convert_TileWithoutZ_GoesToLayerZero`
    - `LegacyContentConversion_Convert_RepositoryAssets_ChestGivesPotionOnce` (the chest on `start`: one `giveItem` of `potion`, then a swap to `chest_opened`, `isOnce` true)

- [ ] **3d. Interaction and touch** (Core + rules content, about 300 lines)
  - **Goal:** pressing interact in front of the chest starts its event, walking onto a warp cell or into the open door starts theirs, and nothing fires twice by accident.
  - **New types (Core):**
    - `Exploration/InteractionFinder` (internal): finds the object event the player can start. `const double InteractMargin = 0.1`. `InteractionFinder(ActiveMap map, ExplorationRules rules)`, `TriggeredEvent? FindInFront(Point2D position, Facing facing, EventActivation activation)`: probe point = position + facing × `InteractReach`; objects whose bounds plus the margin contain it, last placed first; on an object, the first event with that activation that `CanRunOn` its current definition and isn't a done "once" event. Objects only; cell events are touch events found by cell.
    - `Exploration/ExplorationContext` (internal): `record ExplorationContext(ContentCatalog Catalog, Inventory Inventory, WorldState World)`. What exploration reads and changes outside itself (keeps constructors within 4 parameters; gains `Random` in 4b).
    - `Maps/MapLocation`: `record MapLocation(string MapId, Point2D Position, Facing Facing)`. Where the player is.
    - `Exploration/ExplorationState` (public): the player on the current map and the event in progress. `internal ExplorationState(ExplorationContext context, MapLocation start)`; `MapLocation Location`, `EventStep? CurrentEventStep`, `bool IsEventRunning`, `void Walk(MoveIntent intent, TimeSpan delta)` (throws while an event runs), `bool TryInteract()`, `void ContinueEvent()`.
    - Rules in `Walk`, from `GameManager`: entering a new cell fires that cell's touch event (edge-triggered, so standing on it doesn't refire and the arrival cell stays quiet); otherwise a touch event on the object in front fires while the player is moving, armed only once the player has stood still since entering the map or since the last touch event (decision 38). An event that finishes at once (swap only) leaves nothing running.
    - Until 4a, `ContinueEvent` on a `WarpStep` throws `NotSupportedException` naming slice 4a. No caller exists outside tests before then.
  - **Content:** `rules.json` `exploration` gains `interactReach: 0.6` (validator: > 0).
  - **Tests (Core, maps built with builders):**
    - `InteractionFinder_FindInFront_FacingChest_ReturnsItsInteractEvent`
    - `InteractionFinder_FindInFront_TwoOverlappingObjects_ReturnsLastPlaced`
    - `InteractionFinder_FindInFront_OnceEventDone_ReturnsNextEventOnObject`
    - `InteractionFinder_FindInFront_OnlyWhenObjectNotCurrent_ReturnsNull` (the closed door has no touch event)
    - `ExplorationState_TryInteract_NothingInFront_ReturnsFalse`
    - `ExplorationState_TryInteract_SwapOnlyEvent_LeavesNoEventRunning`
    - `ExplorationState_ContinueEvent_ChestEvent_AddsPotionAndOpensChest`
    - `ExplorationState_TryInteract_OpenedChest_ReturnsFalse`
    - `ExplorationState_Walk_EnterCellWithTouchEvent_StartsIt`
    - `ExplorationState_Walk_WithinSameCell_DoesNotRestartCellEvent`
    - `ExplorationState_Walk_KeepPushingIntoObjectAfterTouchEvent_DoesNotRefireUntilStopped`
    - `ExplorationState_Walk_WhileEventRunning_Throws`

- [ ] **4a. Warps and the new game's start location** (Core + content, about 300 lines)
  - **Goal:** warp steps move the player to another map, landing on the nearest walkable cell when the target is blocked, and a new game starts on `start` at (10, 3) through the same map entry.
  - **New types (Core):**
    - `Exploration/WarpArrival` (internal static): where the player lands. `const int SearchRadius = 8`. `static Point2D Find(WalkabilityGrid grid, Point2D target)`, from `FindArrivalPoint`: keep `target` if inside the map and its cell is walkable; else clamp to the map and search rings 1..`SearchRadius` for the walkable cell center with the smallest squared distance (first found wins ties, legacy scan order); if none, the clamped cell's center.
    - `ExplorationState`: one private map-entry path for the constructor and warps: build the `ActiveMap` (world swaps applied), place the walker with `WarpArrival`, set the last cell to the arrival cell, disarm object touch. `ContinueEvent` on a `WarpStep` enters the target cell's center, then continues the event.
    - `Content/NewGameDefinition` gains `StartLocation` (`MapLocation`).
    - `Session/GameSession` gains `World` (internal) and `Exploration` (`ExplorationState`), started at `StartLocation`.
  - **Data:** `NewGameValidator` checks the start map exists and the position is inside it.
  - **Content:** `new-game.json` `"startLocation": { "mapId": "start", "position": { "x": 10, "y": 3 }, "facing": "down" }` (legacy values, test data).
  - **Tests:**
    - Core: `WarpArrival_Find_WalkableTarget_KeepsExactPoint`, `WarpArrival_Find_BlockedTarget_ReturnsCenterOfNearestWalkableCell`, `WarpArrival_Find_EdgeAndCornerInSameRing_PrefersEdge`, `WarpArrival_Find_TargetOffMap_ClampsBeforeSearching`, `WarpArrival_Find_NothingWithinRadius_ReturnsClampedCellCenter`, `ExplorationState_ContinueEvent_WarpStep_EntersTargetMapAtCellCenter`, `ExplorationState_ContinueEvent_WarpOntoTouchCell_DoesNotFireArrivalCell`, `ExplorationState_ContinueEvent_WarpBackToMapWithOpenedChest_ChestStaysOpen`, `ExplorationState_ContinueEvent_OnceEventWithWarp_IsDoneOnSourceMap`
    - Data: `NewGameValidator_Validate_StartMapUnknown_ThrowsNamingTheField`, `NewGameValidator_Validate_StartPositionOutsideMap_ThrowsNamingTheField`, `GameSession_StartNew_RepositoryContent_StartsOnStartMap`, `GameSession_RepositoryContent_DoorLeadsIntoInterior` (open the door, walk in, end on `interior_1`)

- [ ] **4b. Encounter rolls and per-map tables** (Core + Data + content, about 250 lines)
  - **Goal:** a map's settings say how often random battles happen, which, and how many grace steps follow a map entry or a fight; one roll per step decides.
  - **New types (Core):**
    - `Maps/RandomEncounterTable`: content record (`OneInSteps`, `GraceSteps`, `EncounterIds`). A map's random battles (decision 26). `MapSettings` gains `RandomEncounters` (null = none).
    - `Exploration/RandomEncounters` (internal): the single seam for encounter rolls. `RandomEncounters(IRandom random)`, `string? RollStep(RandomEncounterTable? table)`: counts the step; no table, or a step count within `GraceSteps`, returns null without drawing; otherwise `NextBelow(OneInSteps) == 0`, then `NextBelow(EncounterIds.Count)` picks one (uniform). `void ResetSteps()`: called on map entry and after any fight (decision 26).
  - **Data:** `MapSettingsValidator` checks `OneInSteps >= 1`, `GraceSteps >= 0`, a non-empty list, known encounter ids.
  - **Content:** `content/maps/forest.json` gains `"randomEncounters": { "oneInSteps": 25, "graceSteps": 3, "encounterIds": ["goblin_trio"] }` (legacy ≈ 1/500 per update × ~20 updates per tile; grace ≈ legacy 1 second at 3 tiles/s; placeholders). `start` and `interior_1` have none (legacy `false`); Back maps none.
  - **Tests:**
    - Core: `RandomEncounters_RollStep_NoTable_DoesNotDraw` (a `FixedSequenceRandom` with no values throws if drawn), `RandomEncounters_RollStep_WithinGraceSteps_DoesNotDraw`, `RandomEncounters_RollStep_FirstStepAfterGrace_Draws`, `RandomEncounters_RollStep_RollZero_ReturnsEncounterPickedBySecondDraw`, `RandomEncounters_RollStep_RollNonZero_ReturnsNull`, `RandomEncounters_ResetSteps_AfterGrace_GraceAppliesAgain`
    - Data: `MapSettingsValidator_Validate_UnknownEncounterId_ThrowsNamingTheMap`, `MapSettingsValidator_Validate_ZeroOneInSteps_ThrowsNamingTheField`

- [ ] **4c. Pending encounters and the encounter-rate simulation** (Core + simulation, about 250 lines)
  - **Goal:** walking rolls once per new cell; a hit hands an `EncounterDefinition` to the caller, who runs the part 1 battle and reports back, which resets the step counter.
  - **Core:** `ExplorationContext` gains `Random`; `GameSession.StartNew(ContentCatalog catalog, IRandom random)` (callers in tests updated). `ExplorationState`: entering a new cell with no cell event calls `RollStep`; map entry calls `ResetSteps`; `EncounterDefinition? PendingEncounter`; `void FinishEncounter()` clears it and resets the steps (any fight; event battles in part 3 will call it too). `Walk` and `TryInteract` throw while an encounter is pending. What happens after a defeat is the Game's (decision 31, part 3).
  - **Simulation:** `tests/Jrpg.Simulations/Exploration/ForestEncounterRateSimulation`: 1,000 seeds on `forest`, from the warp arrival cell, walking in a straight line and turning to a random walkable direction when blocked, capped at 1,000 steps per seed (a seed hitting the cap fails the run). Prints average and median steps to the first encounter. Asserts the average lies within 20% of `graceSteps + oneInSteps`. It checks the roll rate end to end; the grace rule itself is covered by unit tests, since a missing grace (3 of 28 steps) is within the bound.
  - **Tests:**
    - Core: `ExplorationState_Walk_EnterNewCellOnEncounterMap_SetsPendingEncounter`, `ExplorationState_Walk_WithinSameCell_DoesNotRoll`, `ExplorationState_Walk_EnterCellWithTouchEvent_DoesNotRoll`, `ExplorationState_Walk_WhileEncounterPending_Throws`, `ExplorationState_FinishEncounter_NextStepsWithinGrace_DoNotRoll`, `ExplorationState_ContinueEvent_WarpToEncounterMap_ResetsSteps`
    - Simulation: `ForestEncounters_StraightWalk_AverageStepsNearConfiguredOdds`

- [ ] **ADR 0007: save format and versioning** (before 5a). Options for the shape: (A) immutable snapshot records in Core (`SaveGame`) serialized by Data; (B) Data DTOs mapped from runtime state; (C) serialize runtime classes (excluded by ADR 0002). Options for versioning: a `formatVersion` envelope with step-by-step `JsonNode` migrations vs. a version per record vs. none. Recommendation: **A + envelope**, recording: a format change ships a migration and a test that loads the old format (CLAUDE.md); atomic replacement on write; on read, structural problems (bad JSON, missing fields, unknown or missing version, legacy files) make the save unreadable, while content mismatches are repaired by dropping what no longer exists (decision 40); what's derived from content (max HP/MP, stats) isn't saved until stat growth exists.

- [ ] **5a. Session snapshot and play time** (Core, about 300 lines)
  - **Goal:** a running session becomes a `SaveGame` and back, with party progress, equipment, inventory, location, world state and play time intact. Play time is summed from game-loop deltas.
  - **New types (Core):**
    - `Party/PartyMemberProgress`: `record` (`Id`, `Level`, `Experience`, `Hp`, `Mp`, `WeaponId`, `ArmorId`). What changes about a member during play. `PartyMember.Progress()` returns it; `internal static PartyMember Restore(PartyMemberDefinition definition, IReadOnlyDictionary<string, SkillDefinition> skills, PartyMemberProgress progress)` rebuilds the member (worn items applied by `GameSession`).
    - `Saving/SaveGame`: snapshot record, `required init` (`SavedAt` (`DateTimeOffset`), `PlayTime`, `Location` (`MapLocation`), `Members` (`PartyMemberProgress`), `ActiveMemberIds`, `Inventory` (`InventoryEntry`), `CompletedEvents` (`MapEventKey`), `ObjectSwaps` (`ObjectSwap`)). Everything a save restores. No random state (decision 43).
    - `PartyState.MemberById(string id)` (deferred from part 1).
    - `GameSession` gains `PlayTime`, `void AddPlayTime(TimeSpan delta)` (throws for a negative delta; the Game adds every update's delta, menus, battles and pause included; decision 35), `SaveGame Capture(DateTimeOffset savedAt)` (throws while an event or encounter is in progress), `static GameSession Restore(SaveGame save, ContentCatalog catalog, IRandom random)` (the save is assumed repaired by Data). Restore builds the `ExplorationState` with the restored `WorldState` and the saved `MapLocation`, so loading enters the map through `WarpArrival` like a new game.
  - **Tests (Core):**
    - `GameSession_CaptureThenRestore_RestoresLevelsExperienceAndVitals`
    - `GameSession_CaptureThenRestore_EquippedItemsAreWornNotInInventory`
    - `GameSession_CaptureThenRestore_ActiveOrderFollowsSavedIds`
    - `GameSession_CaptureThenRestore_OpenedChestStaysOpenAndDone`
    - `GameSession_CaptureThenRestore_PlayerAtSavedLocation`
    - `GameSession_CaptureThenRestore_KeepsPlayTime`
    - `GameSession_AddPlayTime_TwoDeltas_SumsThem`
    - `GameSession_AddPlayTime_NegativeDelta_Throws`
    - `GameSession_Capture_WhileEventRunning_Throws`
    - `GameSession_Capture_WhileEncounterPending_Throws`
    - `PartyState_MemberById_UnknownId_Throws`

- [ ] **5b. Save files** (Data, about 250 lines)
  - **Goal:** saves are written atomically to and read from files in a versioned format, and a legacy or newer save is refused with a clear message.
  - **New types (Data):**
    - `Saving/SaveJsonOptions` (internal): strict camelCase options for saves, separate from content options so content changes never change the save format.
    - `Saving/SaveFile` (internal): the envelope `{ "formatVersion": 1, "game": { … } }`. A DTO because the file holds more than the model (ADR 0002 option C).
    - `Saving/UnreadableSaveException` (public): names the slot and the reason.
    - `Saving/FileSaveStore` (public, concrete; one implementation, no interface): `FileSaveStore(string directory)`, `void Write(string slotName, SaveGame save)`, `SaveGame? Read(string slotName)` (null when there's no file). Slot names are `[a-z0-9_]+` and map to `{slotName}.json`; which names exist (20 manual slots, the autosave) is part 3's (decision 34, question 44). `Write` writes a temp file and moves it over the old one. `Read`: no `formatVersion` (legacy saves) -> "not a save from this version of the game"; a version above 1 -> "made by a newer version (N)"; malformed JSON or a missing field -> unreadable, naming the slot and path.
  - **Tests (Data, temp directories):**
    - `FileSaveStore_WriteThenRead_ReturnsEquivalentSave` (every field, lists element by element)
    - `FileSaveStore_Read_NoFile_ReturnsNull`
    - `FileSaveStore_Read_LegacySaveFile_ThrowsUnreadable` (sample with the legacy `SaveData` shape)
    - `FileSaveStore_Read_NewerFormatVersion_ThrowsNamingTheVersion`
    - `FileSaveStore_Read_MalformedJson_ThrowsNamingTheSlot`
    - `FileSaveStore_Read_MissingRequiredField_ThrowsNamingTheField`
    - `FileSaveStore_Write_ExistingSlot_ReplacesIt`
    - `FileSaveStore_Write_InvalidSlotName_Throws`

- [ ] **5c. Save repair against current content** (Data, about 300 lines)
  - **Goal:** a save made before content changed still loads: whatever no longer exists or no longer fits the rules is dropped or brought back into range, the rest is kept, and the player can continue (decision 40).
  - **New types (Data):**
    - `Saving/SaveGameRepair` (internal): fixes a save so it fits the current catalog. `static RepairedSave Repair(SaveGame save, ContentCatalog catalog)`. Rules, each with a test:
      - unknown member ids: dropped from members and the active order;
      - no active member left: the save is unreadable (nothing to play);
      - more active members than `PartyRules` allows: the extra move to the reserve;
      - unknown item ids in the inventory: dropped; quantities above `MaxStack`: clamped; below 1: dropped;
      - worn item unknown: dropped; worn item that exists but can't be worn in that slot or by that class: returned to the inventory (placeholder);
      - level above the experience table: clamped to the maximum with 0 experience; experience at or above its level's threshold: clamped to just below it (part 1's rule);
      - `Hp` outside 0..`MaxHp` and `Mp` outside 0..`MaxMp`: clamped;
      - completed events and swaps naming unknown maps, events, object uids or object ids: dropped;
      - unknown location map: the new-game start location (placeholder); a position outside the map is left to `WarpArrival`.
    - `Saving/RepairedSave` (internal): `record RepairedSave(SaveGame Save, IReadOnlyList<string> Changes)`. The fixed save and one line per change, for a log or a message in part 3.
    - `FileSaveStore(string directory, ContentCatalog catalog)`: `Read` repairs before returning.
  - **Tests (Data):**
    - `SaveGameRepair_Repair_UnknownMember_DropsItFromMembersAndActiveOrder`
    - `SaveGameRepair_Repair_NoActiveMemberLeft_ThrowsUnreadable`
    - `SaveGameRepair_Repair_TooManyActiveMembers_MovesExtraToReserve`
    - `SaveGameRepair_Repair_UnknownItem_DropsEntry`
    - `SaveGameRepair_Repair_QuantityAboveMaxStack_Clamps`
    - `SaveGameRepair_Repair_UnknownWornItem_DropsIt`
    - `SaveGameRepair_Repair_WornItemNoLongerWearable_ReturnsItToInventory`
    - `SaveGameRepair_Repair_ExperienceAtThreshold_ClampsBelowIt`
    - `SaveGameRepair_Repair_HpAboveMax_Clamps`
    - `SaveGameRepair_Repair_CompletedEventOnUnknownMap_DropsIt`
    - `SaveGameRepair_Repair_SwapToUnknownObject_DropsIt`
    - `SaveGameRepair_Repair_UnknownLocationMap_UsesNewGameStart`
    - `SaveGameRepair_Repair_ValidSave_ReportsNoChanges`
    - `GameSession_SaveToFileAndRestore_RepositoryContent_ContinuesWhereSaved` (new game, open the chest, walk, save, read, restore; compare inventory, location and world)

## Decisions (developer, 2026-10-17)
Numbering continues from part 1 (1-25; decision 25 is now decided there: menu items can target reserve members).
26. **Random encounters:** each map defines a chance per step (1 in N), a list of possible encounters (uniform choice until weights are added) and its grace steps. A step is entering a new cell (placeholder, tunable). The step counter resets on entering a map and after any fight, random or not, so battles never come back-to-back. `RandomEncounters.RollStep` is the single seam.
27. **Encounter values:** forest 1 in 25 with 3 grace steps on `goblin_trio`; other maps none. Placeholders to tune.
28. **Grace:** per map, reset as in 26.
29. **Found item that doesn't fit:** extra copies are lost (placeholder).
30. *(Open)* **Battle inside an event:** whether steps continue after a victory is undecided. Event-started battles move to part 3.
31. **Defeat:** the player chooses "load last save" or "main menu", with a loading screen between. Game/UI, part 3; Core only needs `BattlePhase.Defeat` and saves.
32. **Dialogue:** speaker and portrait are optional; no `"???"`/`Fighter.png` defaults.
33. **Map display names:** every map has a display name in its settings, shown briefly on entering the map or under a future minimap (part 3).
34. **Save slots:** 20 manual slots plus 1 autosave. Slot mapping lives in part 3; part 2's store takes a slot name.
35. **Play time:** counts everything, including menus, battles and pause.
36. **Intro cutscene:** stays a test hook; cutscenes move to part 3.
37. **Movement:** parameters are named and live in `rules.json`; movement is tight on a joystick and works with controller and keyboard/mouse. `MoveIntent` is an analog vector (0..1) with diagonals normalized. Deadzone in part 3's input settings; acceleration only if needed.
38. **Touch events that don't warp:** re-arm only after the player stops.
39. **Back maps:** converted where feasible (tiles, objects, dialogue); what the plan can't express (`Battle`) is skipped and reported by file and object/cell. `Custom Map.json` is an older copy of `Back/start.json` and is skipped.
40. **Saves after content changes:** drop what isn't valid anymore, keep the rest, keep it clean. Not a strict refusal.
41. **Tool project:** `tools/Jrpg.ContentConverter` is approved. No NuGet package is needed.
42. **Map source of truth until part 4:** the developer keeps editing maps in the legacy MapEditor. `Assets/` stays the source of map geometry, the converter stays re-runnable and only overwrites its own generated files, and hand-written map settings live where it never writes (ADR 0005).
43. **Random state:** not saved.

## Open design questions (developer decides)
30. **Battle inside an event** (see decisions): do the remaining steps run after a victory? What happens on escape? Blocks event-started battles (part 3).
44. **Autosave slot:** one shared by all playthroughs, or one per playthrough? Part 3; part 2's store takes any slot name, so it doesn't depend on the answer.
45. **Sliding along walls:** slice 2 makes a blocked diagonal slide along the wall (each axis tried alone), as part of "tight" movement; legacy stopped dead. Confirm, or keep the legacy stop.

## Risks
- **Two sources until part 4.** Map geometry is edited in `Assets/` and converted; hand-written settings live in `content/maps/`. Editing a generated file by hand is lost on the next run; the folder name `generated/` and ADR 0005 say so. A map renamed in the legacy editor gets a new id, which orphans its settings file (the loader fails loudly) and saved progress keyed by that map id (repaired by dropping, decision 40).
- **Stable keys in saves.** Object uids and event ids (`{uid}_{index}`) are save keys. Reordering an object's events in the legacy editor changes their ids, so a "once" event can run again after loading an older save. Documented in ADR 0005; the part 4 editor must keep ids stable.
- **Back maps may not render as intended.** They predate the current tileset and object formats; slice 3c verifies them in the legacy editor before keeping them.
- **`ExplorationState` growing into a god object.** It only orchestrates; each rule has its own type (`PlayerWalker`, `MapCollision`, `InteractionFinder`, `EventRun`, `WarpArrival`, `RandomEncounters`). Reviews should push logic out of it.
- **Floating-point movement.** Positions are doubles advanced by `TimeSpan` deltas: deterministic for the same inputs on the same runtime. Tests compare with a tolerance; the fixed 60 Hz step stays the only source of deltas.
- **Seeded order.** Encounter tests depend on the order of `IRandom` calls (roll, then pick); changing it re-baselines them. The random state isn't saved (decision 43), so reloading changes later encounters.
- **Save format and stat growth.** Max HP/MP, attack and defense come from content because levels don't grow stats yet. Stat growth needs save format 2 and a migration with a test loading format 1.
- **Preloading every map.** Fine for six small maps; hundreds of large ones would need a map source seam. The catalog shape keeps that change contained.
- **Collision cost.** `MapCollision` checks every object's bounding box each update (55 objects on `start`); polygons only on a bounds hit, no allocation. A spatial index can come later if a map gets dense.
- **Generated content vs. strict reading.** The converter writes through the same content options the loader reads with; the end-to-end converter test (1d) and the "all of `content/` loads" test catch mismatches.
- **Analyzers.** CA1724: no type named after a folder (`Maps`, `Events`, `Saving`, `World`, `Dialogue`; `WorldState`, `Speech` are deliberate). CA1716 on member names. CA1002/CA2227 on content records. Namespace = folder in the tool project too.

## ADRs needed
Each is a checkbox step in the slice list, accepted before the slices that depend on it.
- **0005 Map content format and source of truth** (before 1a): converter vs. read-as-is vs. hybrid; `Assets/` as the temporary geometry source until part 4; generated vs. hand-written files; what generated content holds; tile id type and layer grids; preloading; error vs. report rules; part 4 exit.
- **0006 Map event flow** (before 3a): step cursor with `Continue()` vs. run-and-report vs. Game-driven; give-item receipts; warps after the fade; "once" at start.
- **0007 Save format and versioning** (before 5a): snapshot records vs. DTOs; `formatVersion` envelope and migrations; unreadable vs. repaired; atomic writes; what isn't saved.
