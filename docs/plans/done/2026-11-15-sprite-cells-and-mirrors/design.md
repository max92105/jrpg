# Character sprite cells and mirrors: design

## Tunables and placeholders

No content value is added. The editor's look values are named constants in the type that draws them:

| Name | Where | Value | Meaning |
|---|---|---|---|
| `PickNumberPixels` | `Editor/Sprites/SequenceMarks` | 12 (screen px) | The size of a picked cell's order number, as the row numbers are (`SpriteSheetMarks`) |
| pick colors | `Editor/Sprites/SequenceMarks` | the row numbers' gold for the numbers, a 2 screen px gold outline per picked frame | How the picks read over any sheet |

The repository's records stay as they are; no placeholder record is written.

## Design overview

### What exists
- **Core `Sprites/`** (ADR 0043): `FieldSpriteSheet` (frame size, scale, feet, `Walk`; `StandingCell(Facing)`,
  `WalkCellAt(Facing, TimeSpan)`, `FrameLeft`, `FrameTop`, `AreaOver`, `ColumnsNeeded` (int: the frame count),
  `RowsNeeded` (long: the highest row + 1)), `FacingCycle` (`Rows`, `Frames`), `FacingRows` (four required ints,
  `RowOf`), `SpriteCell` (record struct), `SpriteArea`. `Maps/TileAnimation` (`FrameCount`, `FrameMilliseconds`,
  `FrameAt`).
- **Data**: `Sprites/FieldSpriteCatalogValidator` (rows from 0, `AnimationProblems.Find` on `walk.frames`),
  `Content/CatalogProblems.OfFieldSprites` (the editor's problem line), `Art/ArtReferenceCheck.HoldsFrames` (start
  check and import, by `ColumnsNeeded` and `RowsNeeded`), the `AbsentWhen*Filter`s, `ContentJsonOptions` (strict:
  required members, unmapped members and doubled properties refused, camelCase enums; its `Modifiers` list).
- **Game**: `Exploration/MapView.DrawNpc` (`WalkCellAt` walking, `StandingCell` standing), `ExplorationScene.DrawPlayer`
  (`WalkCellAt`), `FieldSpriteFrame.SourceOf(sheet, cell)`.
- **Editor**: `Tiles/TileImages.FieldSpriteFrameOf(file, facing)` (the standing frame cropped, cached by
  `(sheet, facing)`), `Sprites/WalkPreview` (four facings walking, redrawn when a cell changes), `Sprites/
  FacingCycleSetup` (four row ints, frames, frame time; `CopyFrom`, `ProblemWithin`, `ToCycle`), `Sprites/
  FieldSpriteSetupViewModel` (the setup, `Problem`, `ToSheet`, `PreviewCellAt`), `Sprites/FieldSpriteCatalogViewModel`
  (`ToSheets` compares records), `Sprites/SpriteSheetMarks` (grid and row numbers), `Sprites/FieldSpriteWindow`
  (the window: list, image in a `ZoomedScroller`, the setup panel in `Grid.Column="3"` (330 px wide, not scrolling),
  preview on `RenderTick` at `TileAnimationTime`). `Objects/NewNpcWindow` shares `WalkPreview`.

### Core changes
- `Sprites/FacingSequence` (new, SC1; `MirrorOf` in SC3): one facing's frames read off the sheet in place of its row:
  its cells in order, or the mirror of another facing. Content equality.
- `Sprites/FacingSequences` (new, SC1): the facings whose sequence overrides their row, each optional; `Of(Facing)`.
- `Sprites/FacingCycle.Sequences` (new, SC1): `FacingSequences?`, null by default. `FacingRows` is unchanged.
- `Sprites/SpriteFrame` (new, SC3): a frame to draw: its cell, and whether it is drawn mirrored.
- `Sprites/FieldSpriteSheet`: SC1 reads a facing's sequence in `StandingCell` and `WalkCellAt`, and counts only what
  each facing reads in `ColumnsNeeded` (now `long`) and `RowsNeeded`; SC3 replaces both reads with `StandingFrame` and
  `WalkFrameAt`.
- `Maps/TileAnimation.LoopFrameAt(TimeSpan elapsed, TimeSpan frameTime, int frameCount)` (new static, SC1): the one
  loop formula, which `FrameAt` and a sequence's frame both call.

### Structure and folder dependencies

| From | To | Why |
|---|---|---|
| Core `Sprites` | Core `Geometry` | `Facing` (exists) |
| Core `Sprites` | Core `Maps` | `TileAnimation` (exists) |
| Data `Sprites` | Core `Sprites` | the validator (exists) |
| Data `Content` | Core `Sprites` | the filters and `RequiredStructMembers` name `FacingCycle`, `FacingSequences`, `FacingSequence`, `SpriteCell` (as `AbsentWhenZeroFilter` names `FieldSpriteSheet`) |
| Game `Exploration` | Core `Sprites` | `SpriteFrame` (exists for `SpriteCell`) |
| Editor `Sprites`, `Tiles` | Core `Sprites` | exists |

No new edge between Core folders; `Sprites` stays a leaf over `Geometry` and `Maps`.

### The record (ADR 0054)

```json
{
  "file": "npc merchant A v00.png",
  "frameWidth": 32,
  "frameHeight": 32,
  "scale": 3,
  "walk": {
    "rows": {
      "down": 0,
      "up": 2,
      "left": 1,
      "right": 2
    },
    "frames": {
      "frameCount": 4,
      "frameMilliseconds": 150
    },
    "sequences": {
      "up": {
        "cells": [
          { "column": 0, "row": 2 },
          { "column": 1, "row": 2 }
        ]
      },
      "right": {
        "mirrorOf": "left"
      }
    }
  }
}
```

(The writer puts each cell's two members on their own lines; the example is shortened.) Up and Right keep rows 2 in
`rows`, unread while their sequences override them. This is an illustration, not a content change: decision 775.

The reading, for a facing at a walked time (all in `FieldSpriteSheet`):
1. The facing's sequence, if `Walk.Sequences?.Of(facing)` has one; a mirror reads the mirrored facing's frame (its
   sequence or its row) and sets `IsMirrored`.
2. Else its row, frames `0 .. frameCount - 1` from column 0.
3. The frame index is `TileAnimation.LoopFrameAt(walked, Walk.Frames.FrameDuration, count)`, count being the row's
   `frameCount` or the sequence's cell count; the standing frame is index 0.

What a record needs of its image (`ColumnsNeeded`, `RowsNeeded`): the rows of the facings with no sequence, the
`frameCount` only while one of them exists, and every cell of every sequence. A mirror adds nothing: it reads what
its facing reads.

### Key decisions (ADR 0054's options)
- **A. A sequence overriding the row (chosen):** `walk.rows` stays four required ints in every record; an optional
  `sequences` object gives a facing cells or a mirror, which override its row. Every record loads unchanged, nothing
  nullable ripples through Core, Data or the editor, and the record grows additively, so the work splits into small
  slices; the row form stays the short way for well-made sheets (`player.png`, most bought sheets). Cost: a row (and
  `frameCount` when every facing has a sequence) is kept but unread (decision 767), as the window's Row choice keeps
  it to restore.
- **B. One form for every facing, every record converted (rejected):** each facing `{ "cells": [...] }` or
  `{ "mirrorOf" }`, nothing else. One reader and one form, but in the writer's layout each cell takes 4 lines:
  `player.png`'s record grows to about 140 lines and `field-sprites.json` to about 400; the frozen test world's file
  must change; and the swap is one atomic slice across Core, Data, the Editor and the fixtures (about 12 test sites),
  with the editor unable to open a record its row boxes don't express until the picker lands.
- **C. A start column and count per facing:** covers a run starting mid-row, not "0, 1, 2, 1", a run crossing rows or
  a mirror: misses the request.
- **D. Repaint sheets into the fixed layout:** ADR 0043 refused it; the developer asked for the opposite.

### Extension points
- Roadmap row 7's "Make strip from sheet cells" picks cells in order on a sheet too: SW2's click-to-cell rule
  (`FieldSpriteSetupViewModel.PickCellAt`) and SW3's `SequenceMarks` are where it starts; row 7 moves them to a shared
  place when it has its second caller, not before.
- The shared preview control (row 7) replaces `WalkPreview`'s timing and adds play, pause, step, per-facing frame
  counters and the playing cell outlined on the image (row 7's backlog line gains the last two).
- `RequiredStructMembers` lists `SpriteCell`'s members; `ObjectTile`'s and `GridCell`'s can join it (backlog).

### Replacement mapping
| Old | New |
|---|---|
| `FieldSpriteSheet.StandingCell(Facing)` → `SpriteCell` | `StandingFrame(Facing)` → `SpriteFrame` (SC3) |
| `FieldSpriteSheet.WalkCellAt(Facing, TimeSpan)` → `SpriteCell` | `WalkFrameAt(Facing, TimeSpan)` → `SpriteFrame` (SC3) |
| `FieldSpriteSheet.ColumnsNeeded` (int, the frame count) | `long`, the farthest column a facing reads + 1 (SC1) |
| `FieldSpriteSheet.RowsNeeded` (every row) | the rows a facing reads and every sequence cell's (SC1) |
| `TileAnimation.FrameAt` body | calls `LoopFrameAt` (SC1) |
| `FacingCycleSetup.DownRow` .. `RightRow` | `FacingCycleSetup.Down` .. `Right`, each a `FacingSetup` (SW1) |
| `FieldSpriteSetupViewModel.PreviewCellAt` → `SpriteCell?` | `PreviewFrameAt` → `SpriteFrame?` (SC3) |
| `MapView`, `ExplorationScene`, `WalkPreview`, `TileImages` drawing a cell | drawing a `SpriteFrame`, flipped when mirrored (SC3) |

## Risks
- **Record equality**: `FieldSpriteSheet` equality reaches `FacingSequence`; without content equality over its cell
  list, an unchanged record would count as changed (`FieldSpriteCatalogViewModel.ToSheets`) and `TileImages` would
  crop a new frame on every draw (its cache key). SC1 tests equality and hash by content, down to two sheets built
  apart.
- **The struct trap**: `SpriteCell` is a positional record struct; the serializer builds structs through the
  parameterless constructor, so without SC2's `RequiredStructMembers` `{ "column": 2 }` would load as row 0. The same
  gap exists today for `ObjectTile` and `GridCell` (a missing `x` reads 0): a backlog line, not this plan.
- **Between the slices nothing has a sequence**: after SC1 the strict loader maps a `sequences` member without SC2's
  checks, and until SW1 the editor's setup rebuilds the walk from its row boxes, so OK in the Character sprites window
  would drop a sequence. No record has one, the repository's or the test world's, and none can be made in the editor
  before SW3; the developer sets the merchant up only after SW3 (decision 775).
- **Unread fields** (decision 767): a row under a sequence and `frameCount` with every facing on a sequence are kept
  as typed; the window hides the first and greys the second (SW3).
- **Per-frame cost**: the game reads a frame per NPC per draw; the read is a null check, a switch and a division, no
  allocation. The editor redraws the pick marks on a pick or a zoom, never per frame.
- **Determinism**: none at stake; the walk's time comes from the shells as today.
- **Save format**: untouched (decision 777).
- **Hand checks on Mana Seed art**: every visual check is the developer's (README, "Guiding rules").
- **XAML**: SW1 and SW3 change `FieldSpriteWindow.xaml`; each runs `.claude/scripts/open-editor-windows.ps1` (the
  window is already in its default list; no new window), reading only its `ERROR` lines.
- **The setup panel's height**: four facing lines with their boxes and buttons make the 330 px panel taller than the
  window's smallest height; SW3 wraps it in a vertical `ScrollViewer` and wraps the cells text.
