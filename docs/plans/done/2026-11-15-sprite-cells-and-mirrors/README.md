# Character sprite cells and mirrors (editor push, roadmap row 5)
Status: done

Drafted on 2026-11-15 from the editor push's roadmap row 5 and design 3.5 (need 1), while the developer was away
(see "Defaults to confirm"). His words: "for NPC you assume row and column will always be the same
left/right/top/bottom animation. I need it more flexible: I need to be able to choose a sequence from the sheet that
will construct an animation"; and "when I work with animation I always want a preview and be able to tweak them:
frame rate etc." The evidence in content: `field-sprites.json`'s `npc merchant A v00.png` lists row 2 for both Up and
Right, since the record can't say "Right is Left mirrored" or "frames 0, 1, 2, 1".

Approved on 2026-11-15 (the developer delegated approval) with the devil's advocate's review applied: a sequence
overrides its facing's row, which stays in every record (B1, C1); six steps instead of nine (C2); a contract modifier
instead of a cell converter (C3); one picking facing (C5); the frames check only while a facing reads its row (C6);
the setup panel scrolls (C7); the window's own visibility converter (C8); window slices named SW (C9); mark placement
(N1); two more tests (N2); hand checks on a Mana Seed sheet are the developer's (N3).

- Numbers: decisions 761-778, ADR 0054 (a new ADR amending 0043, decision 776). Nothing else is numbered here.
- Projects: Core (`Sprites/`, one formula in `Maps/TileAnimation`), Data (`Sprites/`, `Content/` filters and one
  modifier), Game (`Exploration/`, the flip only), Editor (`Sprites/`, `Tiles/TileImages`, one F1 line). No save
  change, no content change.

## Goal
1. **A facing reads any cells of its sheet, in any order**: a field sprite's facing shows its row (today's form), or a
   sequence of cells picked anywhere on the sheet that overrides the row, repeats allowed (0, 1, 2, 1), or the mirror
   of another facing. Every record keeps its four rows; old records load as they are.
2. **The game and the editor draw it**: the map, the player, NPCs on the editor's map and every walk preview show the
   sequences, a mirrored facing flipped left to right.
3. **The Character sprites window builds it**: each facing has a "Shows" box (Row, Cells, Mirror of); with Cells, a
   click on the image adds the cell under it to the facing's sequence, numbered on the image in order; the four
   facings walk in the preview at the typed frame time, with a pace line beside the box.

## Order, and why
1. **ADR 0054 first**: the record's shape had real alternatives (design.md, "Key decisions").
2. **Phase 1, the record (SC1-SC3)**: SC1 is Core alone (the cells and their arithmetic: no caller changes, since a
   sequence only overrides what `StandingCell` and `WalkCellAt` already return), SC2 Data's checks and writing, SC3
   the mirror end to end (the record, its check and the flip in both shells).
3. **Phase 2, the window (SW1-SW3)**: the setup's rules as typed (SW1), then the picking rules and the members the
   boxes bind to (SW2), then the window itself (SW3: the Shows boxes, picking on the image, the marks, the pace line).
   The editor guideline splits a panel into its rules and its window.

## Guiding rules
- **Old records load unchanged** (decision 761): `content/field-sprites.json` and `tests/TestWorld/field-sprites.json`
  stay byte for byte; the writer leaves `sequences` out while null.
- **A sequence overrides its row** (decisions 762, 767): `walk.rows` keeps its four required ints in every record; a
  facing with a sequence ignores its row as a record with no row facing ignores `frames.frameCount`. Everything that
  counts frames (`ColumnsNeeded`, `RowsNeeded`, the window's `ProblemWithin`) counts only what a facing reads.
- **One rule, Core's**: which cell and whether mirrored, for a facing at a time, is `FieldSpriteSheet`'s; the game's
  `MapView` and `ExplorationScene`, the editor's `TileImages` and `WalkPreview` only draw it.
- **No allocation per frame**: reading a facing's sequence and its frame allocates nothing (`SpriteFrame` is a
  struct; the sequences are the record's).
- **Records compare by content** (decision 769): the window's changed-record check (`ToSheets`) and `TileImages`'
  cache key compare `FieldSpriteSheet`s, so a sequence's cell list compares by its cells, as a drop table does
  (decision 604).
- **Agents never open a Mana Seed image, nor a capture showing one** (decision 438, `.claude/rules/content.md`): the
  maps, the main window's panels and the Character sprites window all show Mana Seed sheets, so every visual hand
  check here is the developer's, and each slice says so. The hand checks use `npc_placeholder.png` (the villager on
  the milestone map) where they can, and mark any step on `npc merchant A v00.png`. The implementer runs the tests,
  the hand checks' non-visual parts (a refused start's message in the console) and
  `.claude/scripts/open-editor-windows.ps1` (it opens NPCs > Character sprites... and screenshots it), reading only its
  `ERROR` lines, never its screenshots.
- **Sizes**: Core and Data slices about 150-260 lines with tests; Editor slices about 220-260 (tests at about 10
  lines each), above the editor's 200 because the review merged them into three; SW2 and SC3 each name a split point,
  decided at the slice's start, never midway. SW3 is mostly draw and wiring.
- **Files near 300** (checked 2026-11-15): none touched is above 250. `Sprites/FieldSpriteWindow.xaml.cs` 189 (+about
  25 in SW3), `Sprites/FieldSpriteWindow.xaml` 146 (+about 70), `Sprites/FieldSpriteSetupViewModel.cs` 150 (+about
  35), `Tiles/TileImages.cs` 182 (+8), `Exploration/ExplorationScene.cs` 213 (+2), `Exploration/MapView.cs` 127 (+3),
  `Data/Art/ArtReferenceCheck.cs` 231 (-1: a cast the wider `ColumnsNeeded` makes redundant).
- **Test files, globbed on 2026-11-15**: `Editor.Tests/Sprites/FieldSpriteSetupViewModelTests` (202) and
  `FieldSpriteCatalogViewModelTests` (242) are not extended: each slice names a new sibling. None of the files the
  slices call new exists (`FacingSequenceTests`, `FieldSpriteSheetSequenceTests`, `FieldSpriteSheetMirrorTests`,
  `RequiredStructMembersTests`, `FacingSetupTests`, `FacingCycleSetupTests`, `FieldSpriteCellPickingTests`,
  `FieldSpritePaceTests`; in `src/`: `FacingSequence`, `FacingSequences`, `SpriteFrame`, `RequiredStructMembers`,
  `FacingSource`, `FacingSetup`, `FacingSourceChoice`, `SequenceMarks`, `SameItemConverter`; the editor has no
  multi-value converter yet). Extended: `TileAnimationTests` (33), `FieldSpriteCatalogValidatorTests` (87),
  `ContentCatalogLoaderFieldSpriteTests` (93), `ContentWriterFieldSpriteTests` (66), `ArtReferenceCheckFieldSpriteTests`
  (129), `FieldSpriteFrameTests` (42), `TileImagesSpriteTests` (101), `FieldSpriteCopySetupTests` (107),
  `ControlsHelpWindow.xaml` (114).

## Out of scope
- Play and pause, stepping frame by frame, per-facing frame counters ("2/4") and the playing cell outlined on the
  image: the shared preview control of roadmap row 7 (decision 701), whose backlog line gains them; this window
  switches to it when next touched.
- A frame time per facing or per frame (decision 765; the backlog's per-frame times, decision 632).
- Sequences for anything but field sprites: tiles, enemy idle loops and effect strips keep ADR 0039's run (decision
  680; "Make strip from sheet cells" is row 7's).
- Inserting or reordering picked cells (decision 772).
- Converting `player.png`'s or `npc_placeholder.png`'s record: they read rows and stay as they are.
- Marking `ObjectTile`'s and `GridCell`'s members required too: they have the same gap a cell had (a missing `x` reads
  0); they can join `RequiredStructMembers`' list later (a backlog line, design.md "Risks").

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: the record
- [x] ADR 0054, a field sprite facing's sequence of cells or mirror, overriding its row (amends ADR 0043). Write it
  from decisions 761-770 and 776, with design.md's "Key decisions" as its options (A, the override on today's form,
  chosen; B, one form for every facing with every record converted, rejected with its numbers; C, a start column per
  facing; D, repainting sheets). ADR 0043 gets an "Amended by 0054" line; `docs/decisions/README.md` a row.
- [x] **SC1. A facing reads a sequence of cells: the record and its arithmetic** (Core, about 220 lines) →
  [slices/SC1.md](slices/SC1.md)
- [x] **SC2. Sequences checked on load and written back** (Data, about 150 lines) → [slices/SC2.md](slices/SC2.md)
- [x] **SC3. A facing mirrors another: the record, the check and both shells' flip** (Core, Data, Game, Editor, about
  260 lines) → [slices/SC3.md](slices/SC3.md)

### Phase 2: the Character sprites window
- [x] **SW1. Each facing's setup as typed: row, cells or mirror** (Editor, about 240 lines) →
  [slices/SW1.md](slices/SW1.md)
- [x] **SW2. Picking cells and the boxes' members: the rules** (Editor, about 260 lines; split point SW2a/SW2b named)
  → [slices/SW2.md](slices/SW2.md)
- [x] **SW3. The window: Shows boxes, picking on the image, the marks and the pace line** (Editor, about 220 lines) →
  [slices/SW3.md](slices/SW3.md)

## Decisions
- **761.** A field sprite facing shows one of three: its row (`walk.rows`, `walk.frames.frameCount` frames from column
  0, today's form), a sequence of cells picked on its sheet in order, or the mirror of another facing. A sequence
  overrides the row; a record without sequences reads as today.
- **762.** The shape (ADR 0054): `walk.rows` keeps its four required members in every record; `walk.sequences` (left
  out while null) holds `down`, `up`, `left`, `right`, each optional, each `{ "cells": [...] }` or
  `{ "mirrorOf": "<facing>" }`. A facing with a sequence ignores its row.
- **763.** A sequence holds at least one cell (a single cell stands still while walking); cells may repeat; a cell's
  column and row count from 0. Whether every cell lies inside the image stays the art check's, over what each facing
  reads (`ColumnsNeeded` and `RowsNeeded`, in `long`).
- **764.** A facing's standing frame is its first (extends decision 564): column 0 of its row, its sequence's first
  cell, or the mirrored facing's first, mirrored.
- **765.** One frame time for the whole walk, `walk.frames.frameMilliseconds`; each facing loops over its own frame
  count at that pace. *(Default taken while the developer was away; his to overturn.)*
- **766.** A mirror shows the named facing's frame at the same time, flipped left to right in the same area (centered
  and feet line unchanged). Any facing may mirror any other, on a row or a sequence; a mirror of itself or of another
  mirror is refused (one hop, so no loop).
- **767.** What nothing reads stays required and kept as typed: `walk.frames.frameCount` when every facing has a
  sequence, a facing's row when it has one. The price of loading old records unchanged with one form (ADR 0054);
  Data still checks both (rows from 0, frames from 2).
- **768.** A cell is written `{ "column": c, "row": r }`, both members required through a contract modifier
  (`Content/RequiredStructMembers`): the serializer builds a struct through its parameterless constructor, so a
  missing row would otherwise read as 0.
- **769.** `FacingSequence` compares by content (its cells in order, its mirror), so equal records are equal for the
  window's changed-record check and the editor's frame cache.
- **770.** Core gives a facing's frame as `SpriteFrame` (its cell, whether mirrored): `StandingFrame` and
  `WalkFrameAt` replace `StandingCell` and `WalkCellAt`. The game flips with `SpriteEffects.FlipHorizontally`, the
  editor with a horizontal flip of the cropped frame.
- **771.** In the Character sprites window each facing has a "Shows" box: Row, Cells, Mirror of. Switching from Row to
  Cells with no cell yet starts the sequence with that row's run (column 0 for the frame count), so a sheet is edited
  from where it was. The row stays typed under a sequence and is saved with the record (it's the record's row), so
  switching back to Row restores it. *(The seeding is a default taken while the developer was away; his to
  overturn.)*
- **772.** Picking: one facing picks at a time (`FacingCycleSetup.PickingFacing`); switching a facing to Cells makes it
  the picking one, its line's Pick toggle chooses it, leaving Cells clears it. Each click on the image adds the cell
  under it at the end of the picking facing's sequence; Remove last and Clear undo picks; there is no insert or
  reorder (a sequence is rebuilt from Clear). A click outside whole frames adds nothing. *(Default taken while the
  developer was away; his to overturn.)*
- **773.** The image numbers the picking facing's cells in pick order at each frame's bottom-right corner (a repeated
  cell shows each number, "2, 4") and numbers the columns from 1, centered on each column's top edge, as decision 568
  numbers the rows on its left.
- **774.** The preview walks the four facings at the typed frame time (the box is the record's own pace, so a change
  shows at once), a mirror drawn flipped; beside the box a pace line reads "150 ms a frame, 6.7 frames a second".
  Frame counters and the playing cell on the image are row 7's (Out of scope).
- **775.** The plan changes no content: setting the merchant's Right (a mirror of Left, or a sequence) is the
  developer's, in the window, after SW3; agents never open the sheet (decision 438).
- **776.** ADR 0054 is a new ADR amending ADR 0043, not a dated amendment in it: the record's shape had real
  alternatives.
- **777.** Nothing is saved of a sprite's frames: no save format change. The frozen test world's file is unchanged
  (its two records read rows).
- **778.** The window's own problem line adds, before Data's message: a Cells facing with no cell ("Up has no cells:
  pick them on the image."), a cell past the cut ("Up's cell 3 (5, 2) is outside the 4 x 4 frames."); a row past the
  rows and the frames past the columns only for what a facing reads (the frames check only while some facing reads
  its row); the mirror's rules are Data's message.

## Defaults to confirm
Each was decided by the plan while the developer was away; each is his to overturn.
- **765.** One frame time for the whole walk, not one per facing.
- **771.** Switching a facing from Row to Cells starts its sequence with the row's run.
- **772.** One picking facing; picks are appended, with Remove last and Clear; no insert or reorder.

## Open design questions (developer decides)
- None beyond the defaults above.

## ADRs needed
- **0054, a field sprite facing's sequence of cells or mirror, overriding its row** (amends ADR 0043): written from
  decisions 761-770 and 776 before SC1.
