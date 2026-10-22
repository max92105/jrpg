# Editor: tilesheets, collision, above-player zones and NPCs
Status: approved, in progress

The editor plan of part 3 (the "part 3c" notes at the end of `done/2026-10-18-shell-cutover-and-map-editing.md`, slices 11k-11n and 12). Names and question numbers are reused from there where still valid (69, 70, 71).

Developer's request (2026-10-21, lightly translated): "Next gap to plan and fix is the editor. It runs right now, but there is unused space at the top, and it is a step back feature-wise compared to legacy. For example, painting tiles doesn't work. I want to be able to select a sheet and set the x/y number of columns for the cut, in the app. I want to be able to set the collision and the above layer like I could in the legacy editor. [...] I really like some changes you made, though, like being able to go from map to map easily. I want the editor to be 'top notch', because I will be in it a lot to make maps and everything. So it needs to be user friendly, and we need to be able to add features and tweak things easily, so I can adapt it to my future needs."

Developer's clarifications (2026-10-21):
- Painting failed when they clicked a square in the left panel, then the map: the "only cells already in `tiles.json`" diagnosis (design.md).
- "The above layer" means the map's Z layers: ground lowest, then flora, then details and so on. The editor must let them choose an existing layer, add a layer at a chosen Z, and paint into any of them. Object above-player zones (E10) stay, for legacy parity.
- A tile is, for them, a full-square texture (grass) painted over most of the map, with layers built on top.

## Goal
At the end of this plan the developer can build a map from a fresh tilesheet without leaving the editor:
1. **The window uses its space.** No empty strip at the top; a menu bar holds every command, F1 lists the controls; the layer list chooses a layer and adds one at any Z; maps are listed by name and switch with one click, as today.
2. **Painting works with any sheet.** Every PNG in `content/art/tilesheets/` is listed. Any whole cell of a set-up sheet can be chosen and painted, on any layer; it becomes a tile the first time it is painted.
3. **Sheets are set up in the app.** A new image is cut by columns and rows (margin and spacing editable, tile size shown); a sheet already in use can be re-cut with a warning; an image from outside `content/` can be imported.
4. **Collision.** Tiles are marked blocked in the Tiles panel. An object's collision outline is drawn point by point in an object shapes window. A view toggle shows on the map what blocks the player, by the game's own rule.
5. **Above the player.** Tile layers above the player's Z draw over the player (they already do in the game); the shapes window draws an object's above-player zones (canopies, roofs), as legacy did.
6. **New objects** are made from a rectangle of sheet cells, then given their shapes.
7. **NPCs.** An NPC is a placed object drawn from a field sprite (one shared placeholder for now) with a Talk event; the developer makes, places and scripts them in the editor, and talks to them in the game.
8. **Undo covers everything.** Ctrl+Z undoes the newest change, to the map or to the catalogs.
9. **It looks like a tool you live in:** a dark theme, tool bars that follow the editing mode, resizable panels.

Everything is saved through Data (`ContentWriter`), checked by the loader's validators and the art check first, in CRLF JSON, so the editor can't write content the game (or the editor itself) would refuse.

## Order, and why
1. **The window (E1).** Small fixes the developer sees at once, layer choice included.
2. **ADR 0014, then painting (E3a-E3d).** Painting is the reported breakage. E3a gives the editor one live catalog (the seam every catalog edit needs, every panel reading it); E3b lets any cell be chosen; E3c is the Data writer for the catalogs; E3d makes a cell a tile on first paint and saves it.
3. **The menu bar (E2),** once painting works, so every later command has a home.
4. **Walkability and catalog undo (E4a, E4b1, E4b2).** E4a is the first catalog change that doesn't touch the map; E4b1 makes Ctrl+Z undo it (decision 117) and E4b2 refuses an undo that would remove a tile in use, before more kinds of catalog change arrive.
5. **Sheets (E5a-E7).** Listing every image first, then setting up new sheets (no tile can change under them), then re-cutting sheets, then importing images from outside `content/`.
6. **Collision and zones (E8-E11b).** The overlay first, so every later shape edit is checked on the map; then the shapes window with a plain behavior (collision, then zones), then its tools, then its look.
7. **New objects (E12a, E12b)** from sheet cells, ending in the shapes window.
8. **NPCs (ADR 0015, N1-N3)** reuse objects, events and the shapes window: first in the game, then in the editor.
9. **The look (E13a, E13b),** last, over a window whose behavior is settled; then the session check (Z).

## Guiding rules
- **Rules stay in Core (ADR 0001).** What blocks the player is Core's `WalkabilityGrid` and `MapCollision` rule; the overlay draws Core's answer, it never re-implements it. Shapes keep Core's 0..1 coordinates over the object's cells.
- **Content only through Data (ADR 0011, 0012).** Catalogs (`tilesheets.json`, `tiles.json`, `objects.json`) are written by one `ContentWriter.WriteMapCatalogs`, which runs the loader's validators, every map's layout validator and the art check before writing anything. Images are copied in by `ContentWriter.ImportTilesheetImage`, checked against the tiles cut from them first. The editor never writes a file itself.
- **Ids are kept.** Map ids, object uids, event ids, sheet ids and object ids never change once saved. Tile ids aren't save keys, but tiles are never renumbered either: maps refer to them.
- **Headless logic, thin views (ADR 0012).** Every rule of a panel or window (cuts, ids, shapes, selections, undo order) is in a view model or pure class tested in `tests/Jrpg.Editor.Tests`; views get a hand checklist. View models hold no `DispatcherObject`. One responsibility per view model: the cut panel, walkability marking and import each have their own type beside `TileBrowserViewModel`.
- **Behavior first, look second.** Panels and windows land with a plain WPF look; zoom, handles' look and the theme come in their own slices (E11b, E13b).
- **Keyboard and mouse both.** Every command has a menu entry; frequent ones also a key and a toolbar button; F1 lists them all, per mode.
- **One place per tunable** (design.md table); legacy values are placeholders.

## Out of scope (follow-ups)
- **The rest of question 69** (still open): per-tile and per-object-part Z overrides (legacy "Tile Z" and "Object Part Z" modes), tile names, tile collision polygons (the legacy game never read them).
- **NPC behavior beyond standing and talking:** movement, turning to the player, schedules (questions 127, 128).
- **Deleting** tiles, sheets or object definitions, and renaming ids.
- **A multi-cell stamp brush** (decision 123). E12a's rectangle selection of sheet cells is the piece it would reuse.
- **Per-cell collision on a map** (decision 120): not built.
- **Animated tiles** (the `*_animation` sheets are cut and painted as still frames).
- **Editing other catalogs** (items, enemies, skills, encounters, dialogue): their own plans.
- **Code health** found while planning (design.md, "Follow-ups"): `MainWindow.xaml.cs` at 473 lines, and the existing `Maps` <-> `Tiles` folder cycle.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the window

- [x] **E1. The window: no empty strip, a layer list that chooses and adds layers, maps by name** (Editor, about 190 lines) → [slices/E1.md](slices/E1.md)

### Phase 2: painting

- [x] **ADR 0014: Editing the shared catalogs** (before E3a) → [slices/ADR-0014.md](slices/ADR-0014.md)
- [x] **E3a. One live catalog in the editor** (Editor, refactor, about 190 lines) → [slices/E3a.md](slices/E3a.md)
- [x] **E3b. Any whole cell of a sheet can be chosen** (Core + Data + Editor, about 200 lines) → [slices/E3b.md](slices/E3b.md)
- [x] **E3c. The catalog writer** (Data, about 170 lines) → [slices/E3c.md](slices/E3c.md)
- [x] **E3d. A chosen cell becomes a tile when painted; Save writes the catalogs** (Editor, about 200 lines) → [slices/E3d.md](slices/E3d.md)
- [x] **E2. Menu bar and controls help** (Editor, about 170 lines) → [slices/E2.md](slices/E2.md)

### Phase 3: walkability and catalog undo

- [x] **E4a. Blocked tiles: Mark blocked** (Editor, about 180 lines) → [slices/E4a.md](slices/E4a.md)
- [x] **E4b1. Ctrl+Z undoes catalog changes** (Editor, about 320 lines) → [slices/E4b1.md](slices/E4b1.md)
- [x] **E4b2. An undo that would remove a used tile is refused** (Editor, about 150 lines) → [slices/E4b2.md](slices/E4b2.md)

### Phase 4: sheets

- [x] **E5a. Every sheet image is listed** (Data + Editor, about 120 lines) → [slices/E5a.md](slices/E5a.md)
- [x] **E5b1. Setting up a sheet with the guessed cut** (Editor, about 280 lines; split from E5b) → [slices/E5b1.md](slices/E5b1.md)
- [x] **E5b2. The Cut panel: columns and rows** (Editor, about 260 lines; split from E5b) → [slices/E5b2.md](slices/E5b2.md)
- [x] **E6a. Re-cutting a set-up sheet** (Editor, about 200 lines; split from E6) → [slices/E6a.md](slices/E6a.md)
- [x] **E6b. Warning before re-cutting a used sheet** (Editor, about 170 lines; split from E6) → [slices/E6b.md](slices/E6b.md)
- [x] **E7a. Importing a sheet image: the Data writer** (Data, about 150 lines; split from E7) → [slices/E7a.md](slices/E7a.md)
- [x] **E7b. Importing a sheet image in the editor** (Editor, about 210 lines; split from E7) → [slices/E7b.md](slices/E7b.md)

### Phase 5: collision and above-player zones

- [x] **E8. Collision and zones shown on the map** (Core + Editor, about 220 lines) → [slices/E8.md](slices/E8.md)
- [x] **E9a. Collision outline editing: the rules and the catalog step** (Editor, about 260 lines; split from E9) → [slices/E9a.md](slices/E9a.md)
- [x] **E9b. The object shapes window** (Editor, about 220 lines; split from E9) → [slices/E9b.md](slices/E9b.md)
- [x] **E9c. Opening the shapes window from the map** (Editor, about 120 lines; split from E9) → [slices/E9c.md](slices/E9c.md)
- [x] **E10. Above-player zones** (Editor, about 200 lines) → [slices/E10.md](slices/E10.md)
- [x] **E11a. Point handles: drag, insert, remove** (Editor, about 200 lines) → [slices/E11a.md](slices/E11a.md)
- [x] **E11b. Rectangle, snap, zoom and dimming** (Editor, about 190 lines) → [slices/E11b.md](slices/E11b.md)

### Phase 6: new objects

- [x] **E12a. A rectangle of sheet cells** (Editor, about 140 lines) → [slices/E12a.md](slices/E12a.md)
- [x] **E12b1. A new object from the rectangle: the rules and the catalog step** (Editor, about 290 lines; split from E12b) → [slices/E12b1.md](slices/E12b1.md)
- [x] **E12b2. The New object window** (Editor, about 140 lines; split from E12b) → [slices/E12b2.md](slices/E12b2.md)

### Phase 7: NPCs

- [x] **ADR 0015: NPCs on maps** (before N1) → [slices/ADR-0015.md](slices/ADR-0015.md)
- [x] **N1. An NPC in the game** (Core + Data + Game + content, about 250 lines) → [slices/N1.md](slices/N1.md)
- [x] **N2. NPCs in the editor: drawn, and a Talk preset** (Editor, about 200 lines) → [slices/N2.md](slices/N2.md)
- [x] **N3. A new NPC** (Data + Editor, about 220 lines) → [slices/N3.md](slices/N3.md)

### Phase 8: the look

- [x] **E13a. Layout: splitters and tool bars per mode** (Editor, about 150 lines) → [slices/E13a.md](slices/E13a.md)
- [ ] **E13b. Dark theme** (Editor, about 230 lines) → [slices/E13b.md](slices/E13b.md)
- [ ] **Z. Editor session check** (manual) → [slices/Z.md](slices/Z.md)

## Decisions
Developer's answers (2026-10-21). Earlier decisions that apply: 47 `CommunityToolkit.Mvvm` (ADR 0012); 72, 73 the legacy MapEditor is reference only, under `legacy/`; placeholders still in use from part 3a: 70 (map ids fixed at creation), 71 (legacy layer presets plus a free Z).
- **113. Cells become tiles on the first paint.** A tile is a full-square texture painted over the map, with layers on top; no separate "Add tile" step, no unused tiles in `tiles.json` from choosing alone (E3d).
- **114. A new tile is walkable** (legacy).
- **115. The cut is entered as columns and rows and stored as tile size.** Set once per sheet and remembered in `tilesheets.json` as `tileWidth`/`tileHeight` (no format change). The tile size is shown read-only; there is no tile-size input and no two-way sync (E5b).
- **116. Re-cutting a sheet that has tiles keeps each tile's column and row,** with a warning naming the tiles, maps and objects touched; a cut leaving a tile outside the image is refused (E6).
- **117. Ctrl+Z also undoes catalog changes.** Ctrl+S saves the catalogs and the open map together (E4b; ADR 0014).
- **118. A new sheet's id comes from its file name in snake case** (`Grass3.png` -> `grass_3`).
- **119. Walkability is marked only with the "Mark blocked" toggle** in the Tiles panel; no Walkable box (E4a).
- **120. Collision is per tile type and per object outline only;** no per-cell override on maps.
- **121. A dark theme and a menu bar, like legacy** (E2, E13b).
- **122. The map list reads "Display name (id)", sorted by display name** (E1).
- **123. The multi-cell stamp brush is out of scope.**
- **124. New objects from sheet cells:** typed id and category; the shapes window opens right after; fully transparent cells are kept (E12b).
- **125. Importing an image whose name is taken asks before replacing** (then checks it against the tiles cut from it, E7).
- **126. "Snap to 1/4 tile" checkbox, off by default,** as legacy `ObjectEditorWindow` (E11b).
- **69 (part). NPCs are in this plan.** No NPC art yet: one shared placeholder sprite in `content/art/` that NPCs reference by file name, so real art later is a file swap (ADR 0015, N1-N3).
- **ADR 0015 (2026-10-22): an NPC is an object drawn from a field sprite** (option A), placed with an Interact event.
- **127. NPCs stand still;** movement would be a later plan with its own ADR.
- **128. A standing NPC always faces down** (the standing frame of the down row).
- **129. NPC collision: a default outline over the lower half of the cell,** editable in the shapes window.
- **130. The placeholder sprite is a plain silhouette** in `player.png`'s frame layout, `npc_placeholder.png` (not a copy of the player).
- **131. One NPC object per character** (`villager`, `guard`), each naming its sprite file.
- **132. NPCs are one cell, 48 px frames** in the player sheet's layout (8 frames by 4 facings).

## Open design questions (developer decides)
Each slice uses the placeholder until answered. Questions and decisions share numbers.
- **69 (rest, still open):** drop per-tile and per-object-part Z overrides, tile names and tile collision polygons?

## ADRs needed
- **0014 Editing the shared catalogs** (checkbox step before E3a). The main question: **how catalog edits (sheets, tiles, objects) are held, saved and undone.**
  - **A. A catalog draft in the main window, saved with Ctrl+S, undone with Ctrl+Z (recommended, with decision 117).** `Catalogs/CatalogDraft` holds the loaded catalog plus unsaved sheet, tile and object changes; every panel and the map drawing read it. Ctrl+S writes the catalogs (if changed) then the open map. Closing asks if anything is unsaved; switching maps keeps catalog changes, since they belong to no map. Catalog changes are steps in a catalog history; a shell journal records whether each step went to the map or the catalogs, so Ctrl+Z undoes the newest whichever it is. An undo that would remove a tile, sheet or object still in use is refused, naming the user.
    - Pros: one Save and one Undo for everything, as the developer works today; panels stay in the main window (no modal "manager"); one writer path, validated together; map histories stay per map.
    - Cons: a second kind of unsaved state beside the map's; the journal is one more piece to keep in step.
  - **B. Modal manager windows with their own Save (legacy Tileset Manager and Object Editor).** Each window edits a copy and writes at once on Save.
    - Pros: simple to reason about; proven by legacy.
    - Cons: two saves to think about; the sheet and its cut sit away from where tiles are chosen; the map can't show a change until the window closes; no undo after closing.
  - **C. Every catalog change written at once.** Pros: nothing unsaved. Cons: a file write per click, a validation failure mid-gesture, no undo.
  - **D. One undo history for map and catalog edits.** Pros: one stack. Cons: map histories are per map and catalog edits are global; switching maps would have to keep or drop the other map's steps inside one stack.
  - The ADR also records decisions 113 (tiles made on first paint, not undo steps of their own), 115 (the cut stored as tile size, columns and rows computed in the editor) and 117, and the writer rule: one `ContentWriter.WriteMapCatalogs` writes all three catalogs after the loader's validators, every map's layout check and the art check, all three or none.
- **0015 NPCs on maps** (checkbox step before N1). The question: **how an NPC is modelled in content.**
  - **A. An object drawn from a field sprite (recommended).** `ObjectDefinition` gains an optional `FieldSpriteFile` (a file in `content/art/field-sprites/`, the player sprite's layout); such an object has no tiles. An NPC is that object placed on a map with an Interact event holding a Dialogue step. Pros: placing, collision outlines, interaction, events, "once" saves and the shapes window all work unchanged; one optional field. Cons: an object can't move; movement (question 127) would need runtime positions and a new ADR.
  - **B. A separate NPC catalog and placed NPCs** (`npcs.json`, `MapLayout.Npcs`). Pros: room for names, schedules and movement. Cons: duplicates placing, collision, interaction and events today, in Core, Data, Game and the editor.
  - **C. No format change: the placeholder art as a tilesheet, NPCs as tile objects.** Pros: no code outside the editor. Cons: the field sprite's frames don't fit tile cells; real walk sheets wouldn't drop in; facing (question 128) impossible later without redoing it.
