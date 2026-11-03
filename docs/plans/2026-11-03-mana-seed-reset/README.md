# Mana Seed reset: tests off the old maps, art sources, sheets managed in the editor, the maps rebuilt
Status: approved

Drafted on 2026-11-03 from the developer's request: every system is in, the next work is content and art; the developer bought Mana Seed packs (Seliel the Shaper, 16 px SNES-style art; `Forest Autumn` in hand, more to come) and wants to start the maps over with them. The old tilesheets were deleted from the working tree, and the editor stopped opening because five of them were still named by `content/tilesheets.json`; they were restored so it opens again. Builds on ADR 0009 (art location), ADR 0011 (content is the source of truth), ADR 0014 (editing shared catalogs) and ADR 0036 (the audio sources manifest).

- Numbers: decisions 438-450, ADR 0037 and ADR 0038.
- Approved on 2026-11-03 with every recommendation taken (decisions 443-448), the devil's advocate's findings applied and one new requirement (decision 442: the editor opens and saves whatever art is missing, marks what it can't draw, and manages sheets): S0 restores the deleted sheets until T2b stages them; T2c keeps the identical-bytes, forest-resistance and rest tests on `content`; R1 shrinks to the cell pins, since map ids and uids stay (decision 447); the frozen `new-game.json`'s real reason and cost are stated; AS1's test reads the source tree; E2 left for the backlog (decision 450); E1 split in two (E1a, E1b); the re-skin slice O1 added (decision 449); the tile id high-water mark in E3a; the editor's missing-art slices X1 and X2 and the purge E3b and E3c added (E3 split three ways by size); the manifest's `original` field dropped; a content rule keeps agents off Mana Seed images.

## Goal
1. **The old maps can change without breaking tests.** About 125 test files stand on the repository's start map cells (the forest warp at (0, 14), the shopkeeper at (27, 14)), its tiles, objects and tilesheet images. The mechanism tests move onto a frozen copy of today's maps (the test world, ADR 0037); the repository tests that pin a cell are restated without it.
2. **Every art file says where it came from.** An art sources manifest and a credits file, with a placeholder flag for art to replace before release, checked by a Data test (ADR 0038).
3. **The editor fits Mana Seed and manages the sheets** (decision 442): it opens and saves whatever art is missing, draws what it can't show as a marked cell with a warning instead of a lockout; sheets are guessed at 16 px; an object can be re-skinned keeping its id and events; an unused object or sheet can be removed, and a sheet whose image is missing can be removed with everything drawn from it.
4. **The developer rebuilds the maps** in the editor with Mana Seed sheets, on the same map ids, and removes the old objects and sheets once nothing uses them; the last slice deletes the old images.
5. **Core is untouched**: no rule, state, save format or event kind changes.

## Order, and why
0. **S0 (the developer)**: the pack leaves the repository, and the deleted sheets come back until T2b, so T1 and T2a end with every test passing.
1. **Phase 1: tests off the old maps (ADR 0037, T1, T2a, T2b, T2c).** Nothing can touch the start or forest map while a hundred tests pin their cells. T1 freezes today's maps; each T2 slice moves one test project's map tests onto them, a mechanical change guarded by unchanged test counts. T2b stages the deleted sheets.
2. **Phase 2: the cell pins restated (R1).** Map ids and uids stay (decision 447), so only the tests pinning a cell need restating: they pass on today's maps and guard the rebuild.
3. **Phase 3: art sources (ADR 0038, AS1).** The manifest lands with an entry for every art file already there, so the first imported Mana Seed sheet gets its line from day one.
4. **Phase 4: the editor (M1, X1, X2, E1a, E1b, E3a, E3b, E3c, O1).** M1 first, since the first import uses its guess and ids. X1 and X2 next, so the editor survives C3's deletions and any later one (decision 442). Then the removals and the re-skin, all before the rebuild needs them.
5. **Phase 5: the rebuild (C1, C2, C3; the developer's content work).** Import, cut and build objects; re-skin the chest and doors; rebuild each map; remove what nothing uses. R1's tests and the editor's warnings guard every save.
6. **Phase 6: the old art leaves (L1).** The old images go once no sheet names them, so the game's startup art check passes at the end of every slice and step.

## Guiding rules
- **The game's startup art check passes at the end of every slice and step** (`ArtReferenceCheck`, `FindProblems_RepositoryContent_FindsNone`). The editor tolerates missing art (decision 442); the game doesn't, and that is where the strict check stays.
- **Repository tests pin ids, never cells.** Map ids and placed objects' uids survive the rebuild (decision 447: NPCs are moved with `MoveObjectEdit`, never deleted and recreated); cells don't. Totals stay banned (`.claude/rules/tests.md`). Tests of a mechanism (walking through a door, a forest's encounter rate) run on the test world.
- **The test world is frozen.** It changes only by hand for a test's need, never through the editor; `tests/TestWorld/README.md` says so.
- **Removal is an undo step, refused while in use, naming the first user** (decision 117's wording), the open map counting as it is on screen in place of its saved file. No cascade, with one exception: a sheet whose image is missing (E3b, E3c), asked first and written at once.
- **Agents never open a Mana Seed image** (or a capture showing one): the license refuses AI-related use; the developer accepted the risk (decision 438), and this is the free mitigation (`.claude/rules/content.md`, AS1).
- **Slice sizes:** under about 300 lines, Editor slices about 200 (tests count about as many lines as the code); a mechanical test switch counts its changed lines. No changed file crosses 300 lines; `Shell/MainViewModel` (502) doesn't grow over the plan: X1 moves out about 25 lines (the backlog's image-size cache and import status) before X2, E3a, E3c and O1 add about 11; `Tiles/TileBrowserViewModel` (310) loses its sheet listing in X2 before it gains anything.

## Out of scope (follow-ups, in `docs/plans/backlog.md`, "From the Mana Seed reset plan")
- Removing a sheet's unused tiles while the sheet stays (the former E2, decision 450).
- An autotile or Wang brush for the tree walls (128 x 128 autotile sheets) and the Wang ground sheet: painted cell by cell, or built as objects, until then.
- Animated tiles (water sparkles, waterfall, the tall grass effect).
- Mana Seed characters and monsters: field sprites are 48 px frames in a fixed grid (`Data/Art/FieldSpriteLayout`), battlers are their own files; later plans. Until then the player and NPCs keep their placeholder sprites, at a scale that won't match 3x tiles.
- The lighting layer (decision 445).
- Removing a map in the editor; deleting an image from the editor (Explorer or `git rm` does it; the editor then marks what it drew, decision 442).
- Import writing the manifest entry (decision 448); a release check refusing placeholder art; repairing a saved position that a rebuilt map no longer holds (decision 186 repairs only a map that left).
- Undoing E3c's purge: it is a file operation like Import, asked first.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Step 0: before any slice
- [x] **S0. The developer** moves `content/art/tilesheets/Forest Autumn/` out of the repository (to `C:\git_perso\art-packs\mana-seed\Forest Autumn\` or anywhere outside it, copied to both PCs), and runs `git restore content/art/tilesheets/` so the 23 deleted sheets are back until T2b stages their deletion. Nothing to commit.

### Phase 1: tests off the old maps
- [x] ADR 0037: tests run on a frozen test world. Write it from the entry under "ADRs" below (decision 446).
- [x] **T1. The test world: today's maps, tiles, sheets, objects, new game and tilesheet images frozen in `tests/TestWorld/`, overlaid on the live content in each test output** (props + Data test, about 70 lines plus the copied files) → [slices/T1.md](slices/T1.md)
- [x] **T2a. Game tests on the test world** (tests only, mechanical, about 70 lines) → [slices/T2a.md](slices/T2a.md)
- [x] **T2b. Editor tests on the test world; the deleted tilesheets staged** (tests only, mechanical, about 100 lines) → [slices/T2b.md](slices/T2b.md)
- [x] **T2c. Data tests and the encounter-rate simulation on the test world; the cell-pinned new game tests move there** (tests only, about 150 lines) → [slices/T2c.md](slices/T2c.md)

### Phase 2: the cell pins restated
- [x] **R1. The intros end with a warp to the start map, which has a door into an interior and an exit to a map with random battles, with no cell named** (Data tests, about 70 lines) → [slices/R1.md](slices/R1.md)

### Phase 3: art sources
- [x] ADR 0038: art sources manifest. Write it from the entry under "ADRs" below (decision 440).
- [x] **AS1. `content/art/art-sources.json` and `content/art/CREDITS.md` with every current art file; the Data test, reading the source tree, that keeps files, entries, sources, licenses and credits in step; the content rules** (content + Data test, about 180 lines plus the manifest) → [slices/AS1.md](slices/AS1.md)

### Phase 4: the editor
- [x] **M1. Mana Seed cuts: the Set up guess tries 16 px first, ids from pack file names lose doubled underscores, non-square tiles are refused** (Editor + Data, about 140 lines) → [slices/M1.md](slices/M1.md)
- [x] **X1. The editor opens with missing art: a warning in the status band, tiles and NPCs it can't draw marked on the map, in objects and canopies; `MainViewModel` makes room first** (Editor, about 240 lines with a 25-line move) → [slices/X1.md](slices/X1.md)
- [x] **X2. Sheets with a missing image: listed as "image missing", re-imported under their file name, named by the cursor status; the catalogs save with art missing** (Editor + Data, about 230 lines) → [slices/X2.md](slices/X2.md)
- [x] **E1a. A swap step naming an object counts as a use** (Editor, about 90 lines) → [slices/E1a.md](slices/E1a.md)
- [ ] **E1b. Remove an object definition nothing uses** (Editor, about 160 lines) → [slices/E1b.md](slices/E1b.md)
- [ ] **E3a. Remove a sheet with its tiles when none is used; tile ids never reused in a session** (Editor, about 200 lines) → [slices/E3a.md](slices/E3a.md)
- [ ] **E3b. The purge: a sheet and everything drawn from it, worked out, and the writer for catalogs with maps** (Editor + Data, about 200 lines) → [slices/E3b.md](slices/E3b.md)
- [ ] **E3c. Remove a sheet whose image is missing, asked first with the counts, written at once** (Editor, about 175 lines) → [slices/E3c.md](slices/E3c.md)
- [ ] **O1. Re-skin an object: its cells replaced from the sheet, its id, events, outline and zones kept** (Editor, about 170 lines) → [slices/O1.md](slices/O1.md)

### Phase 5: the rebuild (the developer's content work)
Q6 and Q8 were answered on 2026-11-03 (decisions 451, 452).
- [ ] **C1.** Import the Forest Autumn sheets the maps use (cut at 16 px), add each one's manifest line (AS1's test names a missing one), build the trees and props as objects with their outlines and above-player zones; re-skin `chest`, `chest_opened`, `door_close` and `door_open` (O1) so their events stay.
- [ ] **C2.** Rebuild each map in the editor on its id: erase the old layers (Erase rectangle per layer), remove the old decor objects (rectangle select, Delete), resize, paint, place the new objects; **move the NPCs, chest and doors with their events, never delete and recreate them** (their uids are what R1's and the quest tests read, decision 447); the intro's tiles are erased or repainted. R1's tests pass after each saved map; the hand check is walking every map in the game.
- [ ] **C3.** Remove, in the editor, the old objects (E1b), then each old sheet with its tiles (E3a), once nothing uses them; save. Delete the development saves made before the rebuild (design.md, "Risks").

### Phase 6: the old art leaves
- [ ] **L1. The old tilesheet images no sheet names are deleted, with their manifest entries** (content, about 30 lines) → [slices/L1.md](slices/L1.md)

## Decisions
- **438. Mana Seed is the art family** for tiles, props, characters and monsters from now on. The developer bought the packs knowing the license: one product per purchase, no redistribution, no credit needed, and a "No GenAI" clause whose risk the developer accepts. Agents never open a Mana Seed image (AS1's content rule).
- **439. Purchased packs stay outside the repository.** No redistribution, and no readme, `.tmx` or `.tsx` bloat. Only the sheets a map uses are imported, through the editor's Import, into `content/art/tilesheets/`.
- **440. An art sources manifest**, like the audio one: every art file gets a source, author and license; a credits file; a test that every file under `content/art/` has an entry and every entry a file; a placeholder flag ("replace before release") so generated or borrowed placeholders are tracked. Its shape is ADR 0038.
- **441. The old maps are rebuilt by the developer in the editor**; the systems, items, skills, enemies, quests, classes and shops stay. The rebuilt world keeps: an intro map with one arrival event per leader ending in a warp to the start map; the new game starting on the intro; a start map with every character to recruit, a door into an interior, an exit to a map with random battles, the general store, the Forge, a priced inn and the `placeholder_errand` quest's giver and innkeeper.
- **442. The editor opens and saves whatever art is missing, and the developer manages sheets from it** ("I want the map to load even if I deleted a sheet: mark the stuff that can't be displayed. I don't want to be stuck in the future."). A tile whose sheet image is missing (or whose cell lies outside a smaller image), and an NPC whose field sprite is missing, draw as a marked cell (magenta with a dark hatch) on the map, in objects and canopies; the cursor status names the sheet; the sheet box lists the sheet as "image missing"; the art problems show as a warning in the status band, not a lockout, and maps, tiles and objects stay editable. A sheet with a missing image can be re-imported under its file name, or removed with everything drawn from it (E3b, E3c). **Saving:** the writer still validates the content itself (ids, references, layouts, cells against the catalogs) and refuses what the loader would; a missing image is reported, not blocking. This is an exception to `.claude/rules/editor.md`, "Validate on save", for art only: art is the one kind of content that can disappear outside the editor (a file deleted in Explorer), and refusing to save would leave no way to repair it from the editor. The game's startup check keeps refusing and naming the file: the one place the strict art check stays. X2 adds the exception to `editor.md`.
- **443. Removal is lasting and "unused only"** (question Q1, option a): an object or a sheet leaves when nothing uses it, refused otherwise, as an undo step; no cascade over saved maps, except decision 442's missing-image sheet (E3b, E3c).
- **444. The map cell stays 48 px and 16 px tiles are drawn at 3x** (question Q2).
- **445. The lighting layer is its own later plan** (question Q3), once a map exists to light.
- **446. Tests run on a frozen test world** (question Q4, option a; ADR 0037), with the repository tests pinning ids, never cells. Re-pinning tests to each rebuilt map would only cost about 15 coordinates in `StartMapScenarios` per map, but every map sits half-erased during C2, so the tests would break for the whole rebuild; the frozen world also outlives the next rebuild.
- **447. Map ids stay** (`intro`, `start`, `interior_1`, `forest`, `to_aslas_1`; question Q5): display names change if wanted. NPCs, the chest and the doors are moved with their events, never deleted and recreated, so their uids survive and the repository tests may read them.
- **448. Manifest lines are added by hand** (question Q7); AS1's test names a file without one. Import writing the entry is a backlog line.
- **449. An object can be re-skinned** (O1): its cells replaced from a sheet rectangle, keeping its id, category, outline and zones, so every placement and event naming it (the chest's give and swap, the door's swap and warp) survives the art change.
- **450. Removing a sheet's unused tiles while the sheet stays** (the draft's E2) goes to the backlog: E3a removes a whole sheet once unused, which the cleanup needs.

Planner's picks (not numbered; design.md records them): the test world freezes `new-game.json` with the maps (design.md, "Key decisions" 1); a swap step naming an object counts as a use, closing the undo refusal's gap too; cuts are 16 px and every bigger Mana Seed prop is an object built from cells; the guess drops 48 px first; E3c's purge is not undoable (a file operation like Import, asked first, refused while anything is unsaved; making it undoable would need saved maps written from the catalog draft, about one more slice); the interior is told from the wild by the door being an object's interact event (an edge exit is a cell's touch event).

- **451. The repository is private and stays so** (Q6): the imported Mana Seed sheets are committed under `content/art/tilesheets/`.
- **452. Every art file in the repository today is an AI-generated placeholder** (Q8), except the Lato and Cormorant Garamond fonts (OFL) and the three Rogue Adventure sheets (ELV Games, bought); the seven old tilesheets whose origin nobody recorded go under an unrecorded placeholder source until L1 deletes them.

## Open design questions (developer decides)
None open.

## ADRs
- **ADR 0037: tests run on a frozen test world** (written before T1 from this entry; decision 446):
  - **Context:** repository content changes in the editor every day, yet about 125 test files pin the start map's cells, its tiles, objects and tilesheet images; rebuilding the maps would break them all, and every map sits half-erased during the rebuild.
  - **Decision:** `tests/TestWorld/` holds a frozen copy of the maps (settings and layouts), `tiles.json`, `tilesheets.json`, `objects.json`, `new-game.json` and the tilesheet images they name; `tests/TestWorld/TestWorld.props`, imported by the Data, Editor, Game and Simulations test projects, copies the live `content/` without those files, then the frozen ones, into `test-world/` in the output. A test that reads a map, a tile, a sheet, an object or a tilesheet image, or starts a session, loads `test-world`; a test of what the repository holds loads `content`, pinning ids, never cells.
  - **Options:** re-pinning tests against each rebuilt map (about 15 coordinates per map in `StartMapScenarios`, broken for the whole rebuild); a world built in code (the start map's 20-odd events and the intro's nine cutscenes rewritten in C#); the frozen copy.
  - **Consequences:** the frozen maps name live quests, shops, items, members, encounters, music, stills and voices, so renaming one of those fails the test world's load test by file; a change to the map or new game format updates the frozen files by hand. The frozen `new-game.json` holds the start location on the frozen intro's arrival cell, and also freezes the starting items, gold and leader choices the Game tests count on: a change to those in `content/` no longer reaches the mechanism tests, only `RepositoryNewGameTests`, which stays on `content`. Mechanism tests no longer follow the repository's maps; R1's and the repository tests are what tie the real maps to the rules.
- **ADR 0038: art sources manifest** (written before AS1 from this entry; decision 440):
  - **Context:** decision 440; audio has a per-file manifest (ADR 0036) because each sound has its own freesound page; art comes from a few sources (a pack, a font family, generated placeholders) with many files each.
  - **Decision:** `content/art/art-sources.json` holds `sources` (id, name, author, url, license as an SPDX id or a `LicenseRef-`, an optional `licenseFile` path under `content/art/`, `isPlaceholder`) and `files` (a path under `content/art/` and a `sourceId`); `content/art/CREDITS.md` is written by hand, one section per source (name linked to its page, author, license) and a "Placeholders (replace before release)" section; the game never reads either, so no Data loader; `Data.Tests/Art/ArtSourcesTests` reads the repository's folder (stamped into the test assembly as the apps stamp `ContentRoot`) and keeps files, entries, sources, licenses and credits in step. Licenses: CC0, CC-BY 3.0 or 4.0, OFL 1.1 for fonts, a bought pack's `LicenseRef-<publisher>`; never NC or SA (decision 427's rule, extended to art); `LicenseRef-Unrecorded` and `LicenseRef-Generated` only on placeholder sources.
  - **Options:** a flat per-file list as ADR 0036 (every Mana Seed sheet repeats author, url and license); sources and files (chosen); a sidecar file per art folder; fields on `tilesheets.json` (sheets only, and the game would read license data it never uses); credits generated by a script (worth it once there are many sources).
  - **Consequences:** an imported sheet fails the test until its line is added (decision 448); a placeholder source lists what must be replaced before release; a bought pack's license text stays outside the repository (it isn't redistributable either), named by the source's url; the test reads the source tree, so a deleted file never haunts it from a stale test output.
