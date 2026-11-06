# Map editing flows
Status: approved

Drafted on 2026-11-05 from the developer's list of what slows down building maps in the editor every day (cleaned up by the coordinator):

> Edit a placed object; move a placed object (drag, and probably arrow keys) as one undo step; make an object from a rectangle of painted cells on the map; copy and paste a placed object with its events, within a map and across maps; "Save changes before leaving?" with Save / Don't save / Cancel when switching maps, and a quick way to switch; pick a warp's destination on the target map instead of reading coordinates and typing them; hold a key to snap an object to the grid while placing or moving it.

Approved on 2026-11-05 with the devil's advocate's changes (B1, B2, C1, C4, C5, C7, C8, C9 and the nitpicks) and one design change of the developer's, which replaces the leave question:

> "I don't want a save pop-up anymore when I switch maps. I want to edit several maps at the same time without saving every time I change map."

Builds on ADR 0012 (editor architecture: headless view models, `IMapEdit` undo), ADR 0014 (one undo order over the map and the catalogs), ADR 0040 (object tile levels, -7 to 7), the object composer plan (`done/2026-11-03-object-composer-and-animated-tiles`, decisions 455-462) and the free levels plan (`done/2026-11-05-free-levels-and-picker-width`, decisions 488-495).

- Numbers: decisions 497-516; no ADR.
- Editor only: no Core rule, no content format and no save format changes (design.md, "Core changes").

## Goal
1. **Several maps open at once.** Every map opened stays open with its unsaved changes and its undo history; switching never asks; the map list marks the maps with unsaved changes. Ctrl+S saves the shown map, Ctrl+Shift+S (File > Save all) every changed map; closing the editor asks once: **Save all**, **Don't save** or **Cancel**.
2. **Switching maps is quick.** A filter box over the map list (Ctrl+M) narrows it as you type and Enter opens the first match; Map > Back to previous map (Alt+Left) returns to the map shown before.
3. **A warp's destination is picked, not typed.** The warp step's **Pick...** button opens a picker: the map list beside a zoomable picture of the chosen map, where a click sets the arrival cell (a cell the player can't stand on, or one running a walk-into event, is warned about). **Go to** shows the destination in the main window, centered on the cell and marked.
4. **Placed objects move precisely.** Ctrl + click selects one object; arrow keys nudge the selection (a cell, or one art pixel with Shift), Shift + drag on a selected object moves the whole selection; each move is one undo step.
5. **Snap while placing or moving.** Holding Alt snaps the object's top-left to the grid, the ghost showing where it lands; a **Snap to grid** check box (G) makes snapping the default, which Alt then frees.
6. **Copy and paste placed objects with their events.** Ctrl+C, Ctrl+X and Ctrl+V in Objects mode, also in the Edit menu; the copies keep their events with fresh uids and event ids, on this map or any other.
7. **A placed object is edited in a panel.** Selecting one object shows its uid, object, position and layer under the Objects panel's list; Apply changes them as one undo step; **Edit events...** opens its events.
8. **An object is made from the map.** Ctrl + drag in Tiles mode selects a rectangle of map cells; Objects > New object from map selection opens the composer on every layer's cells there, the chosen layer as level 0; Save can replace the cells with the new object, placed where they were.

## Order, and why
1. **Phase 1: several maps open (MS0-MS5).** The developer switches maps all day. The moves first (MS0), then open maps kept (MS1), every usage check reading them (MS2) and undo following each map (MS3) before Save all and the close question (MS4), then the filter and Back (MS5).
2. **Phase 2: warps (WP1-WP4).** The developer's most tedious flow, and the start map still needs its door into `interior_1` (backlog, decision 454). The picker's rules, the step's side, the window, then Go to (kept: with maps staying open, going to the destination loses nothing).
3. **Phase 3: moving and snapping (MO0-MO2, SN1).** The drag code's move first (MO0); Ctrl + click selection lands in MO1, which phases 4 and 5 build on.
4. **Phase 4: copy and paste (CP1, CP2).** Rules first (ids), then the commands; paste snaps with SN1's rule.
5. **Phase 5: the placed object panel (PO1-PO3).** Two rules slices (position and layer, then the swap: together they pass 300 lines), then the panel.
6. **Phase 6: objects from the map (OM0-OM4).** The handlers' move, the selection, the canvas rule, the menu entry, then the replacement; last because the developer can already paint an object in the composer.

Dependencies across phases: MS0 before any slice adding to `MainWindow.xaml` (the styles' move) or to `MainViewModel`; MS2 before WP2 (`OpenMaps.Layouts`); MS3 and MS5 before WP4 (Go to's undo story and arrival); MO1 before CP2 and PO1 (Ctrl + click selection); SN1 before CP2 (snapped paste). Between MS1 and MS2 a hidden open map's unsaved use of a record isn't checked by an undo (the save refuses it, naming the file); between MS1 and MS3 a map shown again keeps its changes but Ctrl+Z can't reach its earlier steps. Both gaps close within the phase.

## Guiding rules
- **Undo covers every edit** (the developer's rule): every move, nudge, paste, cut, panel change and replacement is one map undo step; an object made from the map is one catalog step, its replacement of the cells a second map step after it (undone first, so the catalog undo is never refused for its placement). Each open map keeps its own steps (decision 503).
- **Headless rules, thin windows** (ADR 0012): each window or panel is planned as its rules with tests, then the view with a hand check (`.claude/rules/editor.md`).
- **A modifier gesture gets a visible control and an F1 line** (`.claude/rules/editor.md`): Alt snap has the Snap to grid check box; the map cell selection is named by the tooltips of the menu entry and tool bar button that use it (OM3); Ctrl + click on an object shows the Placed object panel (PO3) and the Objects tab's tooltip names it; every new key has its menu entry with its gesture text; each slice adds its line to `Shell/ControlsHelpWindow.xaml`.
- **A panel or command holding a placed object or a catalog record re-checks it on every change** (undo, redo): PO1 tests a map undo, PO2 and CP2 a catalog undo, with the panel or command live.
- **Pure moves are their own slices** (C1): MS0, MO0 and OM0 move code into new partial files with no behavior change, the existing tests the guard.
- **Agents never open a Mana Seed image** (decision 438): tests use the test world (`tests/TestWorld`, ADR 0037) and objects built in code; hand checks are the developer's.
- **Slice sizes:** Editor slices about 200 lines, tests counted about as many lines as the code; MS1, MS4, WP1, CP2 and OM2 run 225-240, each over 200 by short one-assert tests or re-pinned ones, under 300 with them.
- **Files that must not grow** (the verify script refuses a changed file crossing 300 lines):
  - `Shell/MainViewModel.cs` (484) only shrinks: MS0 moves the map list and opening into `MainViewModel.MapSwitching.cs`; MS4 moves Save and the close question into `MainViewModel.Saving.cs`; OM3 adds `MainViewModel.ObjectFromMap.cs`.
  - `Shell/MainWindow.xaml.cs` (571) never grows: MS0 moves the map list handlers into `MainWindow.MapSwitching.cs`, OM0 the tile painting handlers into `MainWindow.TilePainting.cs` (the backlog's split).
  - `Shell/MainWindow.xaml` (296): MS0 moves its styles into `Shell/MainWindowStyles.xaml` (about 32 lines out, net); the plan adds about 30 after (ends about 294; each slice states its count).
  - `Shell/MainWindow.ObjectEditing.cs` (257): MO0 moves the drag and ghost code into `MainWindow.ObjectDragging.cs`; CP2 adds `MainWindow.ObjectCopying.cs`.
  - `Objects/ObjectPlacingViewModel` (163) ends about 250 after PO1: copying, the panel and the snap rule are their own types.
  - `Events/EventStepViewModel` (263): WP1 puts the warp members in a partial `EventStepViewModel.Warp.cs`.
  - `Catalogs/CatalogDraft.cs` (298): not touched.
  - Test files past 250 lines take no new test: `MainViewModelTests` (404, MS1 moves its switching and closing tests out), `EventStepViewModelTests` (264), `ObjectComposingTests` (261), `TileBrowserViewModelTests` (496); new tests go in sibling files named in each slice.

## Out of scope (follow-ups for `docs/plans/backlog.md` at the retro)
- Closing one open map (dropping its changes) without closing the editor; the view's place kept per map (switching back fits the map again).
- The system clipboard (pasting into a second editor or a text file): the clipboard is the editor's own, in memory (design.md, "Key decisions" 8).
- A "make the return warp" button creating the reverse door at the destination; a list of the warps leading into a map.
- Named warp destinations (spawn points) in content (decision 506 chose cells).
- Snapping to half cells or to other objects; snapping in the Shapes window (it has its own quarter-tile snap).
- Moving placed objects to another layer by drag; rotating or flipping objects.
- An object from more than 7 layers under or over the chosen one (OM2 refuses it, naming the count).
- The map list sorted by most recent use.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: several maps open
- [x] **MS0. Moves only: the map list and opening into `MainViewModel.MapSwitching.cs`, its handlers into `MainWindow.MapSwitching.cs`, the window's styles into `MainWindowStyles.xaml`** (Editor, about 170 lines moved, no behavior change) → [slices/MS0.md](slices/MS0.md)
- [x] **MS1. Open maps stay open (`Maps/OpenMaps`): switching and new maps never ask, unsaved maps marked in the list, new maps listed; `MapEntry` moves to `Maps/`** (Editor, about 240 lines, 40 of them re-pinned tests) → [slices/MS1.md](slices/MS1.md)
- [x] **MS2. Usage checks read every open map: `CatalogUsage` takes the open layouts; undo refusals, removals and the re-cut warning see hidden unsaved maps; the purge rebuilds every open map** (Editor, about 175 lines, 33 test sites mechanical) → [slices/MS2.md](slices/MS2.md)
- [x] **MS3. Undo follows each open map: journal steps carry their map, Ctrl+Z reaches the shown map's and the catalogs' steps, switching forgets nothing** (Editor, about 180 lines) → [slices/MS3.md](slices/MS3.md)
- [x] **MS4. Save all (Ctrl+Shift+S), Save with no map open writes the tiles, and the close question: Save all / Don't save / Cancel in a dark window listing what is unsaved; Save moves into `MainViewModel.Saving.cs`** (Editor, about 240 lines besides a 45-line move) → [slices/MS4.md](slices/MS4.md)
- [x] **MS5. The map list's filter box (Ctrl+M, Enter opens the first match) and Back to previous map (Alt+Left), enabled only while the previous map is listed** (Editor, about 215 lines) → [slices/MS5.md](slices/MS5.md)

### Phase 2: warps
- [x] **WP1. `WarpDestinationViewModel` and `WarpMaps`: the picker's map, cell, warning (no tile, blocked, inside an outline, a walk-into event) and problem** (Editor, about 235 lines) → [slices/WP1.md](slices/WP1.md)
- [x] **WP2. The warp step's side: `StepChoices.WarpMaps` from every open map, the step starting the picker, taking its result and naming its `WarpDestination`** (Editor, about 165 lines) → [slices/WP2.md](slices/WP2.md)
- [x] **WP3. The warp destination picker window: the map list, the zoomable map picture with its grid and the chosen cell; the warp step's Pick... button in both event dialogs** (Editor, about 185 lines) → [slices/WP3.md](slices/WP3.md)
- [x] **WP4. Go to: the warp step's button keeps the dialog's edits and shows the destination centered on its cell, marked** (Editor, about 90 lines) → [slices/WP4.md](slices/WP4.md)

### Phase 3: moving and snapping
- [x] **MO0. Moves only: the object drags into `MainWindow.ObjectDragging.cs`** (Editor, about 75 lines moved, no behavior change) → [slices/MO0.md](slices/MO0.md)
- [x] **MO1. Ctrl + click selects one object, Esc clears the selection, arrow keys nudge the selected objects as one step each (`MoveObjectsEdit`)** (Editor, about 195 lines) → [slices/MO1.md](slices/MO1.md)
- [x] **MO2. Shift + drag on a selected object moves the whole selection as one step, its outlines following the mouse** (Editor, about 115 lines) → [slices/MO2.md](slices/MO2.md)
- [x] **SN1. Snap to grid: `GridSnap`, Alt held while placing or moving, the ghost where it lands, the Snap to grid check box and G** (Editor, about 205 lines) → [slices/SN1.md](slices/SN1.md)

### Phase 4: copy and paste
- [x] **CP1. `PlacedObjectCopies`: copies of placed objects for a map, with fresh uids and event ids, moved to a new top-left; `PlaceObjectsEdit`** (Editor, about 180 lines) → [slices/CP1.md](slices/CP1.md)
- [x] **CP2. `ObjectCopying`: Ctrl+C, Ctrl+X and Ctrl+V in Objects mode and the Edit menu, across maps, pasted at the mouse (snapped with Alt) and selected; greyed while a copied object is missing** (Editor, about 225 lines) → [slices/CP2.md](slices/CP2.md)

### Phase 5: the placed object panel
- [x] **PO1. `PlacedObjectEditing`: the one selected placed object's position and layer as typed, their problems, applied as one step (`ReplacePlacedObjectEdit`), following the selection and every map change** (Editor, about 205 lines) → [slices/PO1.md](slices/PO1.md)
- [x] **PO2. The panel's object swap: every catalog object offered, the uid and events kept, a warning when a gated event can't run, the list following the catalog** (Editor, about 155 lines) → [slices/PO2.md](slices/PO2.md)
- [ ] **PO3. The Placed object panel under the Objects panel's list; Edit events... opens the object's events** (Editor, about 115 lines) → [slices/PO3.md](slices/PO3.md)

### Phase 6: objects from the map
- [ ] **OM0. Moves only: the Tiles mode handlers into `MainWindow.TilePainting.cs`** (Editor, about 120 lines moved, no behavior change) → [slices/OM0.md](slices/OM0.md)
- [ ] **OM1. Ctrl + drag in Tiles mode selects map cells (`MapCellRectangle`), drawn dashed** (Editor, about 190 lines) → [slices/OM1.md](slices/OM1.md)
- [ ] **OM2. `MapSelectionCanvas`: the canvas a map selection makes, every layer on its level around the chosen one (level 0), and why it can't; `ObjectCanvas.OfCells`** (Editor, about 240 lines) → [slices/OM2.md](slices/OM2.md)
- [ ] **OM3. Objects > New object from map selection opens the composer on it, warning about layers that draw differently in an object; Save makes the object from the map's own tiles** (Editor, about 165 lines) → [slices/OM3.md](slices/OM3.md)
- [ ] **OM4. Replace the cells with the object: the composer's check box, `ReplaceCellsWithObjectEdit` erasing only the cells the object still holds, one map step after the catalog step** (Editor, about 210 lines) → [slices/OM4.md](slices/OM4.md)

## Decisions
- **497. Maps stay open in a session** (the developer's change; replaces the leave question): every map opened stays in memory with its unsaved changes and its undo history; switching maps and making a new map never ask. A new map is listed from its creation and stays open until it is saved or the editor closes (Don't save then drops it).
- **498. The map list marks a map with unsaved changes** with " *" after its text ("Forest (forest) *"); a new map is listed like the others, by display name and id.
- **499. Save (Ctrl+S) writes the tiles and objects when changed, then the shown map**; with no map shown it still writes the tiles and objects (C9).
- **500. Save all (File > Save all, Ctrl+Shift+S) writes the tiles and objects when changed, then every open map with changes, in id order**; the first map the game would refuse stops it with its reason in the error line, the maps written before it staying saved; the shown map stays shown.
- **501. Closing with unsaved changes asks once in a dark window: Save all, Don't save, Cancel** (Q1, Q2), listing the changed maps and "the tiles and objects"; a refused Save all keeps the editor open with the reason shown. Enter is Save all, Esc Cancel.
- **502. Usage checks read every open map:** undo refusals, Remove object, Remove sheet and the re-cut warning look at each open map's unsaved layout in place of its saved one; the purge's "Save first" holds while any open map has changes, and the purge rebuilds every open map from the written content.
- **503. Undo follows each open map:** Ctrl+Z undoes the newest step of the shown map or of the tiles and objects; another open map's steps wait until it is shown again; a new edit anywhere ends every redo (as today); switching forgets nothing. Redo of a tiles or objects step waits while another open map has a newer undone step (that map may use what the redo removes); Redo is then greyed (MS3 review, the coordinator's call: greyed rather than refused with a reason).
- **504. A filter box over the map list** (Q3): Ctrl+M focuses it; it matches the display name or the id, ignoring case; Enter opens the first map shown, Esc clears it; the open map stays selected whenever it is shown.
- **505. Back to previous map (Alt+Left)** (Q3, B2) switches between the two last shown maps; it is enabled only while the previous map is still listed.
- **506. A warp destination is a map and a cell picked on the map's picture** (Q5; RPG Maker's transfer dialog; content unchanged, no ADR): the picker lists every listed map, each open map drawn as its unsaved draft; it starts on the step's map and cell (a new step: no map); the cell must be inside the map.
- **507. A destination cell the player can't stand on, or that would start an event, is warned about, not refused** (Q4, C7): no tile, a blocked tile, inside an object's collision outline, or a cell with a walk-into event (a cell event or an object event run on touch). (WP1 review: neither starts on arrival; the warning says a cell's walk-into event starts on stepping off and back on, an object's on the first step.)
- **508. Go to keeps the dialog's edits (as OK), then shows the destination map at 100 % centered on the cell**, marked with a gold dashed square until the next click or another map shows; the map left stays open with its changes and undo (decisions 497, 503), and Alt+Left returns to it.
- **509. Selecting and nudging** (Q6): Ctrl + click selects the object drawn on top (on empty ground, nothing); Esc clears; with objects selected and the map focused, arrows move them one cell, Shift + arrow one art pixel (1/16 of a cell), each press one undo step; with nothing selected, arrows pan.
- **510. Several objects move as one step**, each with its exact from and to (an undo restores the exact positions); Shift + drag on a selected object moves the whole selection, on an unselected one that object alone.
- **511. Snapping** (Q7, C8): holding Alt while placing or moving lands the object's top-left on the nearest whole cell (halves go right and down), the ghost showing where; the Snap to grid check box in the Objects tool bar (off at the start: free placing stays the default) makes snapping the default and Alt then frees; G toggles the box in Objects mode; a snapped group lands the grabbed object on the grid, the others keeping their offsets from it.
- **512. Copy, cut and paste** (Q8, B1): the editor's own clipboard for the session, across maps; each copy gets the next uid of its object and event ids `{newUid}_{n}` keeping n when free (a taken id, or one of another shape, gets the next free one, checked against every event id on the map); every other field is kept (layer, gates, steps, warps); Ctrl+V pastes centered on the mouse, Edit > Paste on the middle of the view, snapped as placing is, and selects the pasted objects; Ctrl+X cuts as one step; Paste is greyed while a copied object's id is missing from the catalog.
- **513. The Placed object panel** (Q9): with exactly one object selected, its object (a swap keeps the uid, position, layer and events; a warning names an event waiting for an object id the placement can never become), X and Y typed with a dot, its layer, Apply as one step, and Edit events...; a placed object's uid never changes (saves remember it).
- **514. An object from map cells takes every layer with a tile in the rectangle** (Q10, Q12): the rectangle is selected with Ctrl + drag in Tiles mode; the chosen layer is level 0, the layers under it -1 and lower, those over it 1 and higher, a layer with no tile there taking no level; at most 7 layers under and 7 over (ADR 0040's range); its tiles are the map's own (no tile added).
- **515. Replacing the cells** (Q11, C4): the composer from the map shows a "Replace the cells on the map with the object" box, ticked; Save then erases a map cell only on the base layer and the layers over it (levels 0 and up: the ground under the base stays on the map, so no cell is left without a tile), and only where the saved object still holds that sheet cell on that layer's level (a cell erased in the composer stays on the map), and places the object at the rectangle's top-left on the chosen layer, as one map step after the catalog step; the object is then selected.
- **516. The composer from the map warns about layers that draw differently in an object** (C5): when a taken layer's z is at or above `MapLayers.AbovePlayerZ` (it drew over the player; in an object it draws by depth) or the chosen layer's z is below `MapLayers.PlayerZ` (the object would always draw under the player).

## Open design questions (developer decides)
None open.

## ADRs
None: no content or save format changes; the editor's in-memory clipboard and its open maps are not formats. An ADR would be needed for named warp destinations or a system clipboard format (both out of scope).
