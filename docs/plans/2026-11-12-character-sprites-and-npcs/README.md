# Character sprites and NPCs
Status: approved

Drafted on 2026-11-12 from the developer's request (lightly cleaned up):

> "The really next step I want to take to elevate everything is to animate NPCs. I have bought sprites [...]. I want to be able to set it all in the editor, push the NPC behavior further and set up animation. Maybe be able to select a sheet, import it, cut it and set up animation for movement. I don't know how it is usually done. I want it simple and streamlined. I don't know either if an NPC should be under object or its own thing [...]. I want to be able to set up dialogue, quest, store and the smith."

The bought sheet, in words (decision 438: agents never open it): one character per sheet, square cells, about 4 columns and 7 rows; the top four rows are a walk cycle, one row per facing, 4 frames each; the rows under them hold extra poses of two frames each, with an empty row between groups. It doesn't follow the fixed field sprite layout (48 px frames, 8 walk columns, 4 facing rows, `Data/Art/FieldSpriteLayout`) that `player.png` and `npc_placeholder.png` follow.

Approved on 2026-11-12 with the developer's changes: the idle loops moved to "Later"; a phase for an NPC walking in a cutscene (a `moveNpc` event step) replaces them, since the introduction needs it; the player's sheet is untouched beyond moving its drawing to the record with today's numbers; the devil's advocate's blocker (a save rewriting every catalog file, reverting hand edits) fixed first by CW1; every other question as recommended.

This plan comes first; [`2026-11-12-shops-and-enemy-database`](../2026-11-12-shops-and-enemy-database/README.md) depends on its CW1 (catalog writes) and SI1 (`CatalogProblems`, `ImportArtImage`). The class and skill editors plan drafted beside them was dropped (backlog: balancing stays in JSON).

Builds on ADR 0006 (map event flow), ADR 0009 (art folders), ADR 0011 (content the only source of truth), ADR 0012 and 0014 (editor, catalog undo), ADR 0015 (an NPC is an object with a field sprite: kept, amended), ADR 0016 (cutscenes as map events, skipping presentation steps), ADR 0037 (the frozen test world), ADR 0038 (art sources), ADR 0039 (a frame count and a time, rightward: reused for the walk), ADR 0041 (one draw order).

- Numbers: decisions 559-579, ADRs 0043 and 0044.
- Core: `Sprites/` (new folder: a sheet's layout as content), `Geometry/Facings`, `Events/MoveNpcStep`, `Exploration/NpcFacing`, `NpcWalk`, `NpcPose`, `Maps/PlacedObject.Facing`. No save format change (design.md, "Saves").

## Goal
1. **A bought character sheet works as it is.** Each field sprite image gets a record in a new `content/field-sprites.json`: frame size, scale, feet line, which row is which facing, the walk's frames and time. The player's sheet moves to the same record with today's numbers: no visible change.
2. **It is set up in the editor.** Objects > Character sprites... lists the sprite images, imports new ones through Data, cuts them (columns and rows in, frame size out), names each facing's row and the walk, copies a setup from another sheet, and plays the four facings side by side. New NPC... picks a set-up sprite; Change NPC sprite... swaps one on an existing NPC.
3. **NPCs turn and walk in cutscenes.** An NPC faces the player while its own event runs; each placed NPC has its own standing facing; a `moveNpc` event step walks a placed NPC cell by cell along a path, through its walk frames, while the event waits.

What an NPC *does* (dialogue, quest, shop, smith) is already event steps and presets (`Events/EventPresets`); plan 2 adds the Shops window and a quest giver preset.

## How this is usually done, and the answer here (the developer asked)
- **NPC as an object or its own thing.** Engines keep the *actor* placed on the map (position, collision, triggers) apart from the *sprite asset* (sheet, frame size, animations): RPG Maker's events pick a "character image" from a sheet; Godot and Unity put an animated sprite on the placed node, its frames in a separate asset. Here the object already is the actor (placing, collision, events, saves, the shapes window), so the answer is to keep ADR 0015's object and add the sprite asset, a per-image record the object names by file (decision 560).
- **Import, cut, animate.** Aseprite, Tiled and Godot's SpriteFrames editor all do: pick the image, give the grid, name animations as runs of frames on rows with a time per frame. The tile flow has the first two (Import sheet images, the Cut panel); the sprite window reuses their shape and ADR 0039's "frame count and time, rightward".
- **Behavior.** Facing the player is a draw-time rule over the running event. A cutscene walk is a step the event waits on, its position kept for the visit only (as RPG Maker's "set move route" in an event). Wandering needs collision with the player, `IRandom` and saved positions: a later plan with its own ADR.

## Order, and why
1. **Phase 1: sheets as content (FS1-FS5).** The record and its loader with the two existing sprites in today's numbers, then the art check, the game and the editor read it, and the fixed layout goes. Ends with a hand-written record for the bought sheet showing on a map.
2. **Phase 2: set up in the editor (CW1, SI1-SI5).** CW1 first: a save writes only the catalog files that changed and refuses one edited on disk since it was read, so a hand edit (such as FS3's hand-written record) is never reverted; plan 2 depends on it. Then the save path, the setup rules, the list, the window, New NPC and Change NPC sprite.
3. **Phase 3: NPC behavior (NB1-NB3, MV1-MV3).** Facing the player, the placed facing in content and in the editor, then the `moveNpc` step: Core, then the game and the content rules, then the editor's step.

Phase 3 needs FS3; MV2 needs NB1 (the view's NPC lookup); MV3's hand check needs SI5. Between FS1 and FS3 the records exist only in content and tests; between MV1 and MV3 the step exists only in tests and hand-written content.

## Guiding rules
- **One rule, read by both shells** (ADR 0001): which frame a facing shows at a time, the frames a sheet needs, where a sprite stands over its cell, where a walking NPC is: Core methods; the game, the editor's drawing, the setup preview and the art check read them.
- **The player must not change** (decision 561): FS1 writes `player.png`'s record with today's numbers (48 px, scale 1, rows down 0, left 1, right 2, up 3, 8 frames at 100 ms); FS3 pins its source rectangles and drawn area against today's values.
- **Agents never open a bought sprite image** (decision 438): tests use `npc_placeholder.png`, `player.png` and PNGs written by the tests; every look check is the developer's.
- **Undo covers every edit** (ADR 0014): the sprite window's OK, Change NPC sprite, a placed facing and a move step are undo steps; an import is a file copy and isn't undone.
- **The editor never locks out on missing art** (decision 442).
- **A new step kind touches every switch over kinds** (MV1-MV3 name each one's room): Data's step check, Core's `EventRun.ApplyAndCheckWait`, the Game's `EventPresenter` (294 lines: the kind joins an existing `case` line), the editor's `EventStepKind`, `ToStep` and summary.
- **Slice sizes:** Editor slices about 200 lines, tests counted as about as many lines as the code; a new window is two slices; Game slices state their raw size.
- **Files that must not grow past 300** (verify refuses a changed file crossing it):
  - `Shell/MainWindow.xaml` (293; the backlog says its next control goes in its own user control): SI4 first moves the three status lines into `Shell/StatusBand.xaml` (about 11 out, 1 in), then adds one menu line; SI5 a menu line and a button (286).
  - `Shell/MainViewModel.cs` (260): new members in `MainViewModel.FieldSprites.cs` (SI4); one constructor line.
  - `Catalogs/CatalogDraft.cs` (269): `SetFieldSprites` in `CatalogDraft.FieldSprites.cs` (SI1).
  - `Data/Content/ContentWriter.cs` (235): CW1 puts the stamps in their own type; SI1 moves the image imports into `ContentWriter.ArtImports.cs`.
  - `Game/Exploration/EventPresenter.cs` (294): MV2 adds the kind to the `case null or OpenForgeStep or OpenShopStep` line, no new line.
  - `Events/EventStepViewModel.cs` (265): MV3's members in `EventStepViewModel.MoveNpc.cs`, as `.Warp`.
  - Test files past 250 lines take no new test: `ArtReferenceCheckTests` (279), `ContentCatalogLoaderTests` (272), `ContentWriterTests` (254); new tests go in the siblings each slice names.

## Out of scope (follow-ups)
- **Replacing the player's sheet** with a bought one: its own plan, with help (backlog line); this plan only moves the player's drawing to the record with today's numbers.
- Wandering NPCs and routes, and an NPC blocking the player while it walks: a later plan and ADR (design.md, "Wandering, later").
- A moved NPC's draw order against the player: it still sorts by its placed row (design.md, "Risks").
- Mirrored right rows, frames not starting at column 0, per-frame times; other loops than the walk (emotes, sitting).
- The editor's map animating NPCs or showing a move step's path.
- Clicking an NPC's head above its cell to select it.
- The import writing the art source line (`art-sources.json`, ADR 0038): still by hand.

## Notes from the review (not now)
- A direction-fix flag on a placed NPC (a statue or a sleeper that never turns toward the player): add it when such an NPC exists.
- A facing kept on an NPC swapped to a tile object (a swap step) is unread and harmless: the game draws a tile object from its tiles, and Data refuses a facing only on the placed object's own object.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a slice is done and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: sheets as content
- [x] ADR 0043: field sprite sheets as content (amends ADR 0015). Write it from the entry under "ADRs" below (decisions 560-567).
- [x] **FS1. Field sprite sheets load from `field-sprites.json`: `FieldSpriteSheet`, `FacingRows`, `FacingCycle`; every named sprite needs a record; the test world freezes its copy** (Core + Data + content, about 260 lines) → [slices/FS1.md](slices/FS1.md)
- [x] **FS2. The startup art check reads each sheet's own layout** (Core + Data, about 180 lines) → [slices/FS2.md](slices/FS2.md)
- [x] **FS3. The game draws the player and NPCs from their records, scaled and standing on their cell; the player unchanged** (Core + Game, about 250 lines) → [slices/FS3.md](slices/FS3.md)
- [x] **FS4. The interaction prompt clears a tall NPC's head: one object screen area for the view and the prompt** (Game, about 140 lines) → [slices/FS4.md](slices/FS4.md)
- [x] **FS5. The editor draws NPCs from their records; `FieldSpriteLayout` deleted** (Editor + Data, about 170 lines) → [slices/FS5.md](slices/FS5.md)

### Phase 2: set up in the editor
- [x] **CW1. A save writes only the catalog files that changed, and refuses one edited on disk since the editor read it (`CatalogFileStamps`)** (Data + Editor, about 240 lines) → [slices/CW1.md](slices/CW1.md)
- [x] **SI1. Field sprites save and import through Data: `field-sprites.json` written with the catalogs, `ImportArtImage`, `CatalogDraft.SetFieldSprites`, `CatalogProblems`** (Data + Editor, about 240 lines) → [slices/SI1.md](slices/SI1.md)
- [x] **SI2. The sprite setup's rules, the grid cut shared with the tile Cut panel** (Editor, about 230 lines) → [slices/SI2.md](slices/SI2.md)
- [x] **SI3. The sprite list, import and Copy setup from...; OK as one undo step** (Editor, about 220 lines) → [slices/SI3.md](slices/SI3.md)
- [x] **SI4. The Character sprites window (the status lines moved out of `MainWindow.xaml` first)** (Editor, about 230 lines) → [slices/SI4.md](slices/SI4.md)
- [x] **SI5. New NPC picks a set-up sprite and shows it walking; Change NPC sprite...** (Editor, about 190 lines) → [slices/SI5.md](slices/SI5.md)

### Phase 3: NPC behavior
- [x] ADR 0044: NPC facing and cutscene movement (amends ADR 0015's decisions 127 and 128). Write it from the entry under "ADRs" below (decisions 571-578).
- [x] **NB1. An NPC faces the player while its own event runs, and turns back after** (Core + Game, about 190 lines) → [slices/NB1.md](slices/NB1.md)
- [ ] **NB2. Each placed NPC's standing facing: `PlacedObject.Facing` in the layout file, refused on a non-NPC** (Core + Data + Game, about 170 lines) → [slices/NB2.md](slices/NB2.md)
- [ ] **NB3. The placed facing in the editor: a Facing box in the Placed object panel** (Editor, about 190 lines) → [slices/NB3.md](slices/NB3.md)
- [ ] **MV1. The `moveNpc` step in Core: an NPC walks its path cell by cell for the visit, the event waiting until it arrives** (Core, about 260 lines) → [slices/MV1.md](slices/MV1.md)
- [ ] **MV2. The step in content and on screen: Data's rules, the NPC drawn walking at its runtime position, the prompt and collision following it** (Data + Game, about 240 lines) → [slices/MV2.md](slices/MV2.md)
- [ ] **MV3. The step in the editor: the NPC picked from the map, the path built from direction buttons** (Editor, about 210 lines) → [slices/MV3.md](slices/MV3.md)

## Later (not in this plan)
- **NB4, NB5: idle loops** (slice files kept as drafts: [NB4](slices/NB4.md), [NB5](slices/NB5.md)). If they come back: every loop on the map's clock would bob in step on a whole street of NPCs, so each placed NPC needs an offset from its uid (a fixed hash, no randomness) added to the time.

## Decisions
- 559: a save writes only the catalog files whose content differs from what was last read or written, and refuses (writing nothing) when such a file's bytes on disk changed since then, naming the file; the stamps are the file's bytes and its content as written (CW1).
- 560: an NPC stays an object with a field sprite (ADR 0015); the sheet's layout is a separate record keyed by the image's file name, so `objects.json` and `new-game.json` keep their `fieldSpriteFile`.
- 561: `player.png` gets a record like any other sheet, with today's numbers; nothing about the player changes on screen; replacing the player's sheet is a later plan.
- 562: a frame is drawn at a whole-number `scale`, centered on its cell, its feet line on the cell's bottom edge; the editor guesses the scale the set-up tilesheets are drawn at (48 / their tile size).
- 563: `feetPixels` (sheet pixels between a frame's bottom and the feet), 0 by default and left out at 0.
- 564: a sheet stands on its walk's first frame (column 0 of the facing's row).
- 565: frames have a width and a height (`frameWidth`, `frameHeight`).
- 566: the walk is ADR 0039's shape reused: `frames` is a `Maps/TileAnimation` (`frameCount` from 2, `frameMilliseconds` from 1) running rightward from column 0 on each facing's row; rows may repeat.
- 567: `ContentCatalog.FieldSprites` is keyed by file name, exact case; a sprite named without a record is refused at load, naming the object or the new game.
- 568: the window shows rows numbered from 1 (stored from 0) and the facing rows are chosen in boxes beside the image.
- 569: the sprite window applies its session as one undo step; a saved record can't be removed from it.
- 570: Copy setup from... copies another set-up sheet's frame size, scale, feet, rows, frames and time onto the chosen image; a problem shows when the image doesn't split into that frame size.
- 571: an NPC faces the player (the opposite of the player's facing) while an event of its own runs, whatever its trigger; nothing saved.
- 572: the standing facing is per placed NPC (`PlacedObject.Facing`, down when absent, left out when down).
- 573: `moveNpc` names a placed NPC on the event's own map by uid and a path of one-cell steps (`up`, `down`, `left`, `right`); the NPC walks them in order at the player's walk speed (`rules.json` `exploration`) through its walk frames, facing each step's way; the step waits until it arrives.
- 574: the NPC's runtime position and facing live in `ActiveMap` for the current visit: not saved, lost on leaving the map or loading; a cutscene that must leave an NPC elsewhere for good swaps it or ends with a once event. Collision and the interaction finder read the runtime position.
- 575: the walk ignores walls, objects and the player (a cutscene's freedom); the player can't move while the event runs.
- 576: `moveNpc` is a presentation step: a skip pressed on a still runs past it with the NPC placed at its path's end facing its last step; the walk itself has no skip of its own (the presenter is at 294 lines; adding skip input there is a later change).
- 577: what an NPC faces, first rule that applies: its walk's step while walking; the player while its own event runs; its last walk's facing this visit; its placed facing.
- 578: the path is relative to where the NPC stands; it is checked at load from the placed cell: from 1 to 64 steps, every cell on the map (walls allowed), the uid a field-sprite object on the same map; at run time a step that would leave the map is dropped.
- 579: the editor builds the path with four direction buttons and Remove last, the path shown as text ("left, left, down").

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- **0043, Field sprite sheets as content** (amends ADR 0015's decisions 130 and 132 and its "game and editor each read the layout" consequence). Options: the fixed layout kept; a per-image record in `field-sprites.json` (chosen); layout fields on each object; frames as tiles (ADR 0015's option C). Decisions 560-567.
- **0044, NPC facing and cutscene movement** (amends ADR 0015's decisions 127 and 128). Options: always down and standing still (today); a draw-time facing rule and a cutscene step whose positions last for the visit (chosen); runtime facing and positions saved (a format bump, ADR 0022). Decisions 571-578. Not covered: wandering, an NPC blocking the player while it walks, saved NPC positions, a moved NPC's draw order against the player.
