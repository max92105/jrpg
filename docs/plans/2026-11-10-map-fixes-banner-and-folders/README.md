# Map fixes, area banner, map ids and folders
Status: approved

Drafted on 2026-11-10 from the developer's next batch of map and editor work (lightly cleaned up by the coordinator):

> 1. "I had a bug when resizing a map: I could not save because events were blocking the save, because one sent the player outside the map. I changed the event, but I could not save anything, even after moving my event to the right place."
> 2. "I need to be able to edit stuff under an object: for example an object on a layer under another object; if another object on a layer above covers it, I can't reach it. Same for a ground event."
> 3. "I want to be able to set the zone level, and when you enter a map, I want the name and (level) displayed top right, then disappearing."
> 4. "I want to be able to edit the id of a map, because I renamed some maps and the id is still the old id, and I hate it."
> 5. "I want to be able to create folders for maps, to organise them from the editor."
> 6. "I tried to put a tree over or under a house by putting it on a different ground layer (z value) and it did not do what I expected: on a higher z level it was still under the house. On the same level it works really well."

Approved on 2026-11-10 with the devil's advocate's changes ("proceed with changes": DO2's diagnosis on the real layers and its flip list, SV1's catalogs written alone on a refused map, SV2's dry run, UO1's canopy-aware hits, RN2's Back after a rename, RN3 greyed while unsaved, MF3 keeping Rename map..., MF1's joined problem decided) and the developer's answers: layers win among objects with no depth offset (DO3 and DO4 went to the backlog), the banner shown on every entry to a map whose new `showsBanner` setting says so, folders managed from the tree, every other question as recommended.

Builds on ADR 0006 (map events), ADR 0011 (two files per map, content the only source of truth), ADR 0012 (editor architecture; its "Watch" line on the draw order living in two places), ADR 0014 (one undo order, the catalogs saved with Ctrl+S), ADR 0022 (saves before the first release), ADR 0037 (the frozen test world), ADR 0040 (object tile levels), the map editing flows plan (`done/2026-11-05-map-editing-flows`, decisions 497-516: open maps, Save all, the Placed object panel, Ctrl + click selection) and the object composer plan (`done/2026-11-03-object-composer-and-animated-tiles`, decisions 455-471, 517).

- Numbers: decisions 518-535 and ADRs 0041-0042.
- Core: one draw order builder (`Maps/MapDrawOrder`), the canopy's sort rule, `MapSettings.AreaLevel`, `ShowsBanner` and `Folder`, the folder path rule (`Maps/MapFolderPath`). No save format change (design.md, "Saves").

## Goal
1. **A resized map saves.** Save all writes every changed map and the tiles and objects in one go, each map checked against the others as they are now, so a warp fixed on one map no longer waits for another map that can't save without it (design.md, "Diagnosis: the save deadlock").
2. **Layers draw as they say.** An object on a higher layer draws over an object on a lower one, canopies included (design.md, "Diagnosis: canopies flatten the layers"). The layer is the whole model for objects among themselves; leaves stay over the player and walls under, as today.
3. **What is covered can be reached.** Ctrl + click again on the same spot selects the next object under it, in the order things are drawn; Shift + drag grabs the selected one even under another; **Dim and lock other layers** makes the faint layers ignore clicks; in Events mode Ctrl + click opens the ground event under any object.
4. **A map has an area level, and the game names the map on entry.** The map settings get an optional area level and a "Show banner on entry" box (on by default); entering such a map shows its name and level at the top right, fading out after a few seconds.
5. **A map id can be renamed.** Map > Rename map... renames the two files and every reference (warps on every map, the new game's start) at once.
6. **Maps sit in folders, managed from the tree.** The map list becomes a tree; New folder, Move to folder, a drag onto a folder and Rename folder from its context menus; a map's folder is a field of its settings (also typed in Map settings).

## Order, and why
1. **Phase 1: saving (SV1, SV2).** The developer is blocked on saving now: SV1 first of the whole plan, reproducing the deadlock in a test before fixing it. Run order: SV1, then phase 2 (DO1, ADR 0041, DO2), then SV2, then phases 3 to 6 in order.
2. **Phase 2: draw order (DO1, ADR 0041, DO2).** A near-bug the developer hits while painting. DO1 moves the two copies of the draw order (game and editor) into one Core builder with no behavior change, so DO2's fix is one rule in one place.
3. **Phase 3: reaching covered things (UO1-UO3).** After DO2, so the "next object under" follows the one draw order, canopies included, and the lock treats a canopy with its object. The cycle first (it reaches everything on any layer), then the lock, then cell events under objects.
4. **Phase 4: area level and banner (AL1, AL2, MB1-MB3).** The two settings first, so the banner has something to show and something to obey; then the presenter, the plain draw, the look.
5. **Phase 5: map ids (ML0, ADR 0042, RN1-RN3).** ML0 moves the map list out of `MainWindow.xaml` (298 lines) before anything adds to it; the writer first, then the editor's rules, then the window.
6. **Phase 6: folders (MF1-MF6).** Last: it builds on ML0's panel and on RN2's "written at once" path for Rename folder. The field first (a folder can be typed in Map settings at once), then the tree's rules and view, then the folder commands' rules, their dialog and menus, and the rename.

Phases 1, 2, 4 and 5 don't depend on each other, except RN1 and SV2 reusing SV1's shared map checks in `ContentWriter`; phase 3 needs DO1 and DO2; phase 6 needs SV1 (`WriteMaps`), ML0, RN2 and RN3 (the list's Rename map... entry MF3 carries over). Between DO1 and DO2 nothing changes on screen. Between MB1 and MB2 the banner exists only in tests.

## Guiding rules
- **Bug slices start with the failing test** (SV1, DO2): the test is written and seen failing before the fix, and the slice's Done note says so.
- **One rule, read by both shells** (ADR 0001, ADR 0012): the draw order and the canopy rule live in Core (`MapDrawOrder`, `DrawOrderKey`); the game, the editor's drawing and its hit test read them.
- **Undo covers every edit** (the developer's rule), except the two cross-map rewrites that the sheet purge set the pattern for (decision 442): a map id rename and a folder rename are written at once, refused (greyed) while anything is unsaved, and every map's undo history starts afresh after them (decisions 530, 533).
- **A modifier gesture gets a visible control and an F1 line** (`.claude/rules/editor.md`): the cycle has the Placed object panel's **Next under** button, the lock is the renamed **Dim and lock other layers** box, Ctrl + click in Events mode is named on the Events tab's text; each slice adds its line to `Shell/ControlsHelpWindow.xaml`.
- **Every field copied into settings is copied back**: `MapSettingsViewModel.ToSettings` and `AreSame` carry `AreaLevel` (AL1), `ShowsBanner` (AL2) and `Folder` (MF1), each with a test whose fixture differs from the default, or applying the settings dialog would wipe them.
- **Headless rules, thin windows**: each new window or panel is its rules with tests first, then the view with a hand check.
- **Agents never open a Mana Seed image** (decision 438): tests use the test world (`tests/TestWorld`, ADR 0037) copied, and objects built in code; the draw order's hand checks are the developer's (DO2's throwaway test reads layouts, never images).
- **Slice sizes:** Editor slices about 200 lines, tests counted as about as many lines as the code; Game slices that add a view type are estimated at 1.6x their raw count (MB2, MB3 are split for that).
- **Files that must not grow** (verify refuses a changed file crossing 300 lines):
  - `Shell/MainWindow.xaml` (298): ML0 moves the map list out (about 13 lines out, 2 in); RN3 adds one menu line; UO2 only relabels; nothing else touches it.
  - `Shell/MainViewModel.cs` (340) and `Shell/MainWindow.xaml.cs` (425) never grow: new members go in partial files named per slice (`MainViewModel.MapRenaming.cs` in RN2, `MainViewModel.MapFolders.cs` in MF4); `MainViewModel.cs` changes one constructor line in UO2 and one guard line in AL1.
  - `Catalogs/CatalogDraft.cs` (298): not touched.
  - `Objects/ObjectPlacingViewModel.cs` (260): UO1 puts the cycle rule in `Maps/Editing/PlacedObjectHits` and adds about 15 lines; UO2 about 6 (ends about 281).
  - Test files past 250 lines take no new test: `MainViewModelOpenMapsTests` (286), `MainViewModelTests` (292), `ContentWriterTests` (265), `ObjectShapesViewModelTests` (347), `ObjectComposingTests` (261); new tests go in sibling files named in each slice.

## Out of scope (follow-ups in `docs/plans/backlog.md`)
- A per-placement depth offset and its panel box (the former DO3, DO4; backlog, "From the map fixes, banner and folders plan").
- A save migration for renamed map ids (a rename table in content): needed only from the first release (ADR 0042).
- Deleting a map from the editor; closing one open map.
- Leaves of a tree behind a house showing over the house's walls (design.md, "Risks": a canopy draws over every body under the canopy layer).
- The banner's own sound; a world map; a banner on a same-map warp.
- Folders for anything but maps (objects, quests).
- An area level range ("Lv 5-8") or the area level driving encounters.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: saving
- [x] **SV1. The save deadlock reproduced, then fixed: Save and Save all write in one go, all or none (the tiles and objects alone when a map is refused); Save all checks each map against the open maps as they are now (`ContentWriter.WriteMaps`, `OpenMaps.ContentWith`)** (Data + Editor, about 250 lines, 25 of them re-pinned tests) → [slices/SV1.md](slices/SV1.md)
- [x] **SV2. A refused Ctrl+S that Save all would pass says so: a dry run of the map checks on the open maps (`ContentWriter.CheckMaps`)** (Data + Editor, about 110 lines) → [slices/SV2.md](slices/SV2.md)

### Phase 2: draw order
- [x] **DO1. Moves only: one draw order in Core (`MapDrawOrder`, `MapDrawItem`), read by the game's `MapDrawList`, the editor's `MapDrawing` and `PlacedObjectHits`** (Core + Game + Editor, about 150 lines changed, 140 lines of tests moved) → [slices/DO1.md](slices/DO1.md)
- [x] ADR 0041: one draw order and the canopy's layer. Write it from the entry under "ADRs" below (decisions 520-522).
- [x] **DO2. Canopies keep their object's layer: `DrawOrderKey.ForCanopy`; the editor dims a canopy with its object; a throwaway listing of the repository's flipped pairs** (Core + Editor, about 150 lines, the throwaway test deleted) → [slices/DO2.md](slices/DO2.md)

### Phase 3: reaching covered things
- [x] **UO1. Ctrl + click again selects the next object under the point, ranked as drawn (a roof's zone counts as the roof); Next under in the Placed object panel; Shift + drag grabs the selected object first** (Core + Editor, about 215 lines) → [slices/UO1.md](slices/UO1.md)
- [x] **UO2. Dim and lock other layers: clicks reach only the chosen layer's objects, in Objects and Events modes** (Editor, about 150 lines) → [slices/UO2.md](slices/UO2.md)
- [x] **UO3. Events mode: Ctrl + click opens the cell's event under any object, Ctrl + right click removes it** (Editor, about 120 lines) → [slices/UO3.md](slices/UO3.md)

### Phase 4: area level and banner
- [x] **AL1. `MapSettings.AreaLevel`: `areaLevel` in map settings, validated, left out while null; the Map settings window's Area level box; `MapSettingsViewModel.Problem` joining the dialog's problems** (Core + Data + Editor, about 185 lines) → [slices/AL1.md](slices/AL1.md)
- [x] **AL2. `MapSettings.ShowsBanner`: `showsBanner` in map settings, true by default and left out while true; the Map settings window's "Show banner on entry" box** (Core + Data + Editor, about 150 lines; a sibling of AL1, which would pass 250 with it) → [slices/AL2.md](slices/AL2.md)
- [x] **MB1. `MapBanner`: which map's name and level show and how opaque, from entering a map (another one than the last, whose settings show the banner) through its wait, fade in, hold and fade out** (Game, about 185 lines) → [slices/MB1.md](slices/MB1.md)
- [x] **MB2. The banner drawn plain at the top right, waiting while an event runs: `MapBannerView`, `FieldOverlays` in `ExplorationScene`, the level line from `ui.json`** (Game, about 150 raw, 240 at 1.6x) → [slices/MB2.md](slices/MB2.md)
- [x] **MB3. The banner's look: serif name, spaced-caps level label and large number, a dark band fading to the left, a short slide** (Game, about 140 raw, 225 at 1.6x) → [slices/MB3.md](slices/MB3.md)

### Phase 5: map ids
- [x] **ML0. Moves only: the map list and its filter box into `Shell/MapListPanel`, their handlers with them** (Editor, about 110 lines moved, no behavior change) → [slices/ML0.md](slices/ML0.md)
- [x] ADR 0042: renaming map ids before the first release. Write it from the entry under "ADRs" below (decisions 530, 531).
- [x] **RN1. `MapIdRename` and `ContentWriter.RenameMap`: the map's two files under the new id, every warp and the new game's start following, checked, written, the old files deleted** (Data, about 260 lines) → [slices/RN1.md](slices/RN1.md)
- [ ] **RN2. `MapRenamingViewModel` and `MainViewModel.MapRenaming.cs`: the new id typed (prefilled from the display name), its problems, refused while anything is unsaved, written, the editor carrying on under the new id with Back still working** (Editor, about 230 lines) → [slices/RN2.md](slices/RN2.md)
- [ ] **RN3. The Rename map window; Map > Rename map... and the map list's context menu, greyed with the reason while anything is unsaved** (Editor, about 125 lines) → [slices/RN3.md](slices/RN3.md)

### Phase 6: folders
- [ ] **MF1. `MapSettings.Folder` and `MapFolderPath`: `folder` in map settings, validated, left out while null; the Map settings window's Folder box** (Core + Data + Editor, about 175 lines) → [slices/MF1.md](slices/MF1.md)
- [ ] **MF2. `MapTree`: the map list as folders and maps, the filter narrowing it, the session's empty folders** (Editor, about 200 lines) → [slices/MF2.md](slices/MF2.md)
- [ ] **MF3. The map list panel shows the tree: folders open and close, a map row opens the map, the shown map selected, Rename map... kept on map rows** (Editor, about 170 lines) → [slices/MF3.md](slices/MF3.md)
- [ ] **MF4. New folder and Move to folder, the rules: `MapFolderChoiceViewModel`, `MainViewModel.MapFolders.cs`; a move is a settings step on the moved map, shown** (Editor, about 200 lines) → [slices/MF4.md](slices/MF4.md)
- [ ] **MF5. The folder dialog (`MapFolderWindow`), the map list's folder context menus and dragging a map onto a folder** (Editor, about 160 lines) → [slices/MF5.md](slices/MF5.md)
- [ ] **MF6. Rename folder: every map in it written at once, refused while anything is unsaved** (Editor, about 170 lines) → [slices/MF6.md](slices/MF6.md)

## Decisions
The developer's answers of 2026-11-10 (Q1-Q15 below; Q3, Q8, Q9, Q12 chosen explicitly, the others as recommended).

- **518. Save and Save all write in one go, all or none** (Q1): Save writes the changed tiles and objects and the shown map together, checked together (the other maps as saved, since they stay as they are on disk); Save all writes the changed tiles and objects and every open map with changes together, every map checked with every open map as it is now in place of its saved file. When that write is refused and the tiles and objects changed, they are written alone as Ctrl+S always did (when they pass on their own) and the error line names the refused file and says no map was written. Replaces decision 500's "the maps written before it staying saved".
- **519. A refused Ctrl+S that Save all would pass says so** (Q2): after a refusal, the map checks run as a dry run with every open map as it is now; when they pass, the error line adds "Save all (Ctrl+Shift+S) writes map 'start' with it, which makes this valid." naming the other open maps with changes. Ctrl+S never writes a map other than the shown one.
- **520. One draw order, built in Core** (`Maps/MapDrawOrder`): the game's map, the editor's map and the editor's hit test read the same list and keys (ADR 0012's "Watch" closed).
- **521. Layers win among objects, canopies included** (Q3 a): canopies all draw on the canopy layer (`MapLayers.CanopyZOf`), and there an object from a higher layer draws after one from a lower layer; on the same layer the lower base still draws later. A tree on -3 draws its leaves over the roof of a house on -4. No depth offset: the layer is the model (the developer's houses and trees sit on -3 to -5, where the player is always in front).
- **522. The editor dims and locks a canopy with its object's layer**, not with the canopy layer: the object is shown and picked whole on its own layer.
- **523. Repeated Ctrl + click selects the next object under the point** (Q5): the objects covering the point in the order they are drawn, topmost first (an object whose above-player zone holds the point ranks by its canopy, else by its body); a Ctrl + click while the one selected object is among them selects the next one down, wrapping to the top; the Placed object panel says "bench_2 selected: 2 of 3 objects here" and its **Next under** button does the same at the last Ctrl + click point.
- **524. Shift + drag grabs a selected object under the point first**, even when another object covers it; with none selected there, the top one as today.
- **525. Dim other layers becomes Dim and lock other layers** (Q6): while ticked, the objects on other layers are faint and ignore clicks (select, rectangle select, Shift + drag, right click, Delete under the mouse, Shift + double-click, Events mode clicks). The box keeps its place and its menu entry.
- **526. Events mode reaches the ground under objects** (Q7): Ctrl + click opens the clicked cell's event dialog whatever object covers it (a new event when it has none); Ctrl + right click removes the cell's event. A plain click keeps opening the object's events.
- **527. A map's area level** (Q8 a): `areaLevel` in the map's settings, one whole number from 1, optional (a town has none); the party level the area is made for, shown to the player and nothing else.
- **528. A map's settings say whether it shows the banner** (Q9): `showsBanner`, true by default and left out of the file while true; the Map settings window's "Show banner on entry" box.
- **529. The map banner** (Q9): on every entry to a map other than the one left (a new game's first map, a warp to another map, a loaded save) whose settings show the banner, once no event runs, the map's display name and its area level ("Lv 5") show at the top right, fading in, holding and fading out (tunables in design.md); a map with no area level shows its name alone; a same-map warp, a battle's end and a closed menu show nothing.
- **530. A map id is renamed at once** (Q10): Map > Rename map... (and the map list's context menu) asks for the new id, prefilled from the display name by the new map rule (`Maps/Editing/MapIds`); greyed while anything is unsaved, its tooltip saying why; written at once with every reference (each warp step on every map, the new game's start); the editor carries on with the map shown under its new id, Back to previous map still working, and every map's undo history starting afresh, as after the sheet purge (decision 442). It reverses question 70 ("the id can't change").
- **531. A save on a renamed map is repaired as for a removed map** (Q11): the party starts at the new game's start and the map's done events and object swaps are forgotten, each noted (`SaveGameRepair`, `WorldStateRepair`); no save format change, no migration until the first release (ADR 0042 amends ADR 0011's "map ids are never renumbered").
- **532. A map's folder is a field of its settings** (Q12 a): `folder`, a path of names joined by "/" ("Act 1/Village"), optional (no folder: the top of the list); only the editor reads it, so no ADR. The tree's context menus and drag are the main way to change it; the Map settings window's Folder box is a second way.
- **533. Moving a map is a settings step; renaming a folder is written at once** (Q13): Move to folder and a drag show the moved map and record one settings step on it (Ctrl+Z, saved by Ctrl+S); Rename folder changes every map in it and is written at once, refused while anything is unsaved, as a map id rename.
- **534. A new folder lives for the session until a map moves in** (Q14): New folder adds an empty folder to the tree; it is kept on disk only through the maps in it.
- **535. The filter narrows the tree** (Q15): while the filter box holds text, the tree shows only the maps matching it, inside their folders, every folder open; Enter opens the first one shown.

## Open design questions (developer decides)
None open. Answered on 2026-11-10, for the references in the slices: Q1 one write, all or none; Q2 a hint toward Save all; Q3 layers win, canopies included; Q5 Ctrl + click again cycles; Q6 the lock tied to Dim other layers; Q7 Ctrl + click in Events mode; Q8 one whole number, display only; Q9 every entry, per-map `showsBanner`; Q10 written at once; Q11 repaired, no migration; Q12 a settings field, the tree kept; Q13 moves undoable, folder renames written at once; Q14 empty folders for the session; Q15 the filter narrows the tree. (Q4, the depth offset, was dropped with DO3 and DO4.)

## ADRs
- **ADR 0041: one draw order and the canopy's layer** (decisions 520-522):
  - **Context:** objects draw by layer, then tiles before sprites, then base (`DrawOrderKey`); an object's above-player zones draw on the canopy layer (`MapLayers.CanopyZOf`, at least 5) sorted by base only, so a tree on -3 drew its leaves under the roof of a house on -4 whenever the house's bottom edge was lower on screen; the game (`MapDrawList`) and the editor (`MapDrawing.PartsInDrawOrder`) each built the list (ADR 0012's "Watch").
  - **Decision:** one builder in Core (`MapDrawOrder`) read by the game, the editor's drawing and its hit test; on any layer, sprites order by the layer they come from (a canopy: its object's), then base, then placement order.
  - **Options:** today's rule; layers win for bodies only; a per-placement depth offset (backlog); a sort line per object definition; the chosen.
  - **Consequences:** the layer is the whole model among objects; a canopy still draws over every body below the canopy layer and the player, so leaves of a tree behind a house show over its walls where they overlap, as before; saves and content formats untouched.
- **ADR 0042: renaming map ids before the first release** (decisions 530, 531), amending ADR 0011's "map ids are never renumbered":
  - **Context:** map ids name the two files, every warp, the new game's start and the save's location, done events and object swaps; the developer renamed display names and wants the ids to follow.
  - **Decision:** until the first release the editor renames a map id at once with every content reference, refused while anything is unsaved; saves naming the old id are repaired as for a removed map; from the first release a rename needs a rename table the repair follows (a later ADR).
  - **Options:** never rename (ADR 0011 as written); rename with a rename table now; rename pending until Ctrl+S; the chosen.
