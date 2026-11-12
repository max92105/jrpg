# Character sprites and NPCs: design

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| `FieldSpriteSheet.MaxScale` | Core `Sprites/FieldSpriteSheet` | 8 | The largest whole-number scale; a technical bound. |
| `MoveNpcStep.MaxSteps` | Core `Events/MoveNpcStep` | 64 | The longest path a step may hold (decision 578); a technical bound. |
| Player walk | `content/field-sprites.json`, `player.png` | 8 frames, 100 ms | Today's numbers (decision 86), now content. |
| Placeholder NPC walk | same, `npc_placeholder.png` | 8 frames, 100 ms | As the player's (decision 130). |
| NPC walk speed | `rules.json` `exploration` | the player's walk speed | A cutscene NPC walks as fast as the player (decision 573). |
| `FrameCutGuess.Columns` | Editor `Sprites/FrameCutGuess` | 4 | The columns a new sheet's cut starts with. |
| New walk time | Editor `Sprites/FieldSpriteSetupViewModel` | 150 ms | A new setup's frame time. |
| New walk rows | same | down 0, left 1, right 2, up 3 | `player.png`'s order. |
| Scale guess | Editor `Sprites/SpriteScaleGuess` | 48 / the most common tile width of the set-up sheets; 1 when none divides | 3 for Mana Seed. |

## Design overview

### What exists
- `Core/Maps/ObjectDefinition.FieldSpriteFile` (ADR 0015) and `NewGameDefinition.FieldSpriteFile`.
- `Data/Art/FieldSpriteLayout` (48 px, 8 walk columns, 4 rows), read by `ArtReferenceCheck`, `Game/Exploration/FieldSpriteFrame`, `WalkAnimation` (8 frames, 100 ms, `RowOf`) and `Editor/Tiles/TileImages.FieldSpriteFrameOf`.
- The game draws the player in `ExplorationScene.DrawPlayer`, NPCs in `MapView.DrawObject` (facing down); `PromptPlacement.BoxFor` puts the prompt over the object's top cell.
- `Data/Content/ContentWriter.WriteMapCatalogs` writes every catalog file (`CheckedCatalogWrites`) whenever the draft is dirty (`MainViewModel.Saving`), from the editor's loaded snapshot: a hand edit made while the editor is open is reverted by the next save (the review's blocker, CW1).
- The tile flow: `ContentWriter.ImportTilesheetImage`, `Tiles/SheetImport`, `Tiles/SheetCutViewModel` (its private `TileLengthAlong`, `SpanAlong`, `ProblemAlong`), `Tiles/SheetCutGuess`, `Tiles/TileAnimationTime`, `Maps/TileAnimation.FrameAt`.
- Events: `Exploration/EventRun` (`ApplyAndCheckWait` near CA1502's limit; `SkipPresentation` runs past `IsPresentation` steps), `MapEventStarter.Run`, `TriggeredEvent(Event, Source)`; `ActiveMap` is built anew on each map entry (`ExplorationState` line 214); `MapCollision.IsInsideAnObject` and `InteractionFinder` read `placed.Position`.

### Core changes
New folder `Core/Sprites` (a leaf over Geometry and Maps):
- `FieldSpriteSheet` (sealed record): one image's layout: `File`, `FrameWidth`, `FrameHeight`, `Scale`, `FeetPixels`, `Walk`; `StandingCell(Facing)`, `WalkCellAt(Facing, TimeSpan walked)`, `ColumnsNeeded`, `RowsNeeded`, `FrameLeft`, `FrameTop`, `AreaOver(int cellPixels)`.
- `FacingCycle` (sealed record): a loop per facing: `Rows` (`FacingRows`), `Frames` (`Maps/TileAnimation`).
- `FacingRows` (sealed record): the row each facing reads; `RowOf(Facing)`.
- `SpriteCell`, `SpriteArea` (readonly record structs): a frame's column and row; a drawn rectangle relative to the cell.

Elsewhere:
- `Content/ContentCatalog.FieldSprites` (init, empty by default).
- `Geometry/Facings.Opposite(Facing)`; `Geometry/Facings.Step(GridCell, Facing)` (MV1).
- `Maps/PlacedObject.Facing` (`Facing?`, NB2).
- `Events/MoveNpcStep(string ObjectUid, IReadOnlyList<Facing> Path)` (MV1): `IsPresentation` true.
- `Exploration/NpcWalk` (MV1): one NPC walking its path: position at the time walked, the step's facing, `IsFinished`, `End` (the last cell and facing).
- `Exploration/NpcPose` (readonly record struct, MV1): where an NPC stands and looks now and how long it has walked (`Position`, `Facing`, `Walked`, zero when standing).
- `Exploration/NpcFacing` (NB1, extended in NB2 and MV1): decision 577's order.
- `Exploration/ActiveMap` (MV1): the visit's walks and their ends by uid; `PositionOf(placed)`, `StartWalk`, `AdvanceWalks(delta)`, `FinishWalks()`, `IsAnyNpcWalking`.
- `Exploration/ExplorationState`: `NpcFacingOf` (NB1) becomes `NpcPoseOf(placed)` (MV1); `AdvanceNpcWalks(delta)` continues the event when the walk arrives (MV1).

The drawn area (decision 562): `width = FrameWidth * Scale`, `height = FrameHeight * Scale`, `left = (cellPixels - width) / 2`, `top = cellPixels - (FrameHeight - FeetPixels) * Scale`; `player.png` gives (0, 0, 48, 48), today's square.

### Content format
`content/field-sprites.json`, sorted by file, in the writer's layout:

```json
[
  {
    "file": "npc_placeholder.png",
    "frameWidth": 48,
    "frameHeight": 48,
    "scale": 1,
    "walk": {
      "rows": {
        "down": 0,
        "left": 1,
        "right": 2,
        "up": 3
      },
      "frames": {
        "frameCount": 8,
        "frameMilliseconds": 100
      }
    }
  }
]
```

A move step in an event: `{ "type": "moveNpc", "objectUid": "npc_3", "path": [ "left", "left", "down" ] }` (written one value per line by the writer).

### Catalog writes (CW1, decision 559)
`Data/Content/CatalogFileStamps`: per catalog file, the hash of its bytes on disk and the hash of its content as the writer would write it, both taken when the editor loads the content and renewed for each file written. A save serializes each catalog in memory; a catalog whose content hash equals its stamp is skipped (so a file the editor didn't change keeps its hand edits, and a hand-laid file isn't relaid by accident); a changed one is written only if the file's bytes on disk still match the stamp, else the whole save is refused naming the file ("objects.json changed on disk since the editor read it: close the editor and reopen it to edit it, or undo the changes to it."). Maps are out of this rule (they are written only when changed already; a hand edit to an open map is the developer's to avoid; noted in "Risks").

### Structure and folder dependencies

| From | To | Why |
|---|---|---|
| Core `Sprites` | `Geometry`, `Maps` | `Facing`; `TileAnimation` reused |
| Core `Content` | `Sprites` | `ContentCatalog.FieldSprites` |
| Core `Exploration` | `Geometry`, `Maps`, `Events`, `Content` | `NpcFacing`, `NpcWalk` (the walk speed from `GameRules`); no `Sprites` edge |
| Core `Maps` | `Geometry` | `PlacedObject.Facing` |
| Data `Sprites` | Core `Sprites`, Data `Content` | the validator (as every Data feature folder) |
| Data `Art` | Core `Sprites` | the art check |
| Game `Exploration` | Core `Sprites`, `Exploration`, Game `Rendering` | frames, poses, `MapArt.FieldSprites` |
| Editor `Sprites` (new) | `Catalogs`, `Tiles` | the draft; `TileAnimationTime`, `TileImages`, `GridCut` |
| Editor `Tiles`, `Maps` | Core `Sprites` | the standing frame and its area |

### Key decisions
1. **Keyed by file, exact case** (decision 567): objects and the new game need no change; the test world gets a frozen `field-sprites.json` so a record changed in `content/` never moves a test.
2. **One record shape for player and NPCs** (decisions 560, 561): `MapArt.PlayerSprite` is the new game's record; `WalkAnimation` keeps the walked time, the sheet gives the cell.
3. **The walk reuses `TileAnimation`** (decision 566); its summary says it serves sprites too.
4. **Facing and cutscene positions are never saved** (decisions 571-574): derived from the running event, the visit's walks and the placed facing.
5. **Core walks the NPC, the Game drives the time** (MV1, MV2): `ExplorationScene.Update` calls `AdvanceNpcWalks(delta)` once per update; Core moves the NPC, and on arrival continues the event itself, so the presenter has nothing to do for the step but not throw (one kind added to an existing `case` line).
6. **Data's rules show live in the editor** (SI1's `CatalogProblems`): the setup's problem line ends with the loader's own message.
7. **One grid-cut rule** (SI2): `Editor/Tiles/GridCut` (the length a cut's cells get along one side, and why a count doesn't split it), extracted from `SheetCutViewModel` and read by it and the sprite setup.
8. **One art import** (SI1): `ContentWriter.ImportArtImage(artFolder, sourcePath, catalog)` with the folder's own check (field sprites: the record's frames; effects: whole square frames; battlers, portraits, backgrounds: a PNG); plan 2's enemy art reuses it.

### Wandering, later (not this plan)
A wandering NPC needs: collision against the player while it moves (`MapCollision` testing NPCs against the player and the player against moving NPCs); a walk chosen through the session's `IRandom` (determinism), within a radius or along a content route; a pause while an event runs; the draw order keyed on runtime positions; and either saved positions (a `formatVersion` bump with a refusal test, ADR 0022) or NPCs back at their start on load. Its own ADR. ADR 0044's runtime positions in `ActiveMap` are the place it would extend.

### Replacement mapping

| Today | Replaced by |
|---|---|
| `Data/Art/FieldSpriteLayout` | `Core/Sprites/FieldSpriteSheet` from content; deleted in FS5 |
| `WalkAnimation.RowOf`, its 100 ms and 8 frames | `FacingRows.RowOf`, the record's `walk.frames` (FS3) |
| `FieldSpriteFrame.SourceOf(Facing, int)` | `SourceOf(FieldSpriteSheet, SpriteCell)` (FS3) |
| `MapView.ScreenAreaOf`, `PromptPlacement.BoxFor`'s arithmetic | `Game/Exploration/ObjectScreenArea.Of` (FS4) |
| `TileImages.FieldSpriteFrameOf(file)` | `FieldSpriteFrameOf(file, facing)` from the record (FS5, NB3) |
| `NewNpcViewModel`'s folder listing | the set-up records (SI5) |
| `SheetCutViewModel`'s private cut arithmetic | `Tiles/GridCut` (SI2) |
| `CheckedCatalogWrites` writing every file | only changed files, against `CatalogFileStamps` (CW1) |
| decisions 127 and 128 (stand still, face down) | `NpcFacing`, `NpcWalk`, `MoveNpcStep` (NB1, NB2, MV1) |
| `MapCollision`, `InteractionFinder` reading `placed.Position` | `ActiveMap.PositionOf` (MV2) |

### Saves
Nothing saved changes; `formatVersion` stays. A save made after a cutscene walk and loaded puts the NPC back at its placed cell (decision 574).

## Risks
- **Player regression** (FS3): rectangles and area pinned; a hand check walks the four facings.
- **A moved NPC's draw order**: `MapDrawOrder` keys objects on `placed.Position`, so an NPC that walked past the player's row sorts as if it stood at its start (its head over or under the player wrongly when they overlap). Cutscenes rarely overlap the two; accepted, listed out of scope, with the fix (a position lookup into `MapDrawOrder`, which the game, the editor and the hit test share) for the wandering plan.
- **Tall sprites and the hit test**: a sprite's head above its cell isn't clickable in the editor.
- **Collision after a walk** (MV2): the NPC blocks at its runtime cell for the rest of the visit; one that walked into a doorway blocks it until the player leaves the map: content's care (decision 574), named in MV2's hand check.
- **CW1 and the editor's own writes**: the purge and the map rename write through the writer, which renews the stamps; a file changed on disk is refused rather than merged, so the developer reopens the editor after hand edits to a catalog it changed. A hand edit the save keeps is never checked against what it writes (a tile removed by hand while an object the editor saves still uses it): the game's startup refuses it then, not the save; accepted, since the only cure is reloading every kept file at each save.
- **Test world**: its frozen `field-sprites.json` lists `npc_placeholder.png` and `player.png`; the two PNGs come from live content.
- **Art sources** (ADR 0038): every imported sprite fails `ArtSourcesTests` until its line is added; each import hand check says so.
- **Bought images** (decision 438): no agent opens them; the tests write their own PNGs.
- **`TileAnimation` reused**: a later change to tile animations reaches the walk too (wanted).
- **Coupling with plan 2**: plan 2 needs CW1 and SI1 (`CatalogProblems`, `ImportArtImage`, `ContentWriter.ArtImports.cs`).
