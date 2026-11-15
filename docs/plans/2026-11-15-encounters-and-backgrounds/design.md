# Encounters and battle backgrounds: design

The README holds the order, the decisions and the defaults to confirm. This file holds the tunables, the design and
the risks; each slice points to the section it builds on.

## Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Screen divisors | `Editor/Battle/GifImportOptionsViewModel` (BG9's background kind) | 1080, 540, 360, 270, 216, 180 | The heights offered below the GIF's own: the 1080 px screen's whole divisors, so a 16:9 frame scales up by a whole factor (decision 898). |
| Bytes per pixel | `Editor/Battle/GifImportOptionsViewModel` | 4 | What the video memory line multiplies the sheet's pixels by (an uncompressed RGBA texture). |
| Preview area | `Editor/Backgrounds/BattleBackgroundPage` | 480 x 270 (a `double` each, read by `x:Static`) | The 16:9 area `FramePreview` fills, cover-fit. |
| Background box's first entry | `Editor/Maps/Settings/MapSettingsViewModel.NoBattleBackground` | "(none)" | No battle background (decision 909). |
| Encounter background's first entry | `Editor/Encounters/EncounterViewModel.MapsBackground` | "(the map's)" | The encounter has none of its own (decision 906). |
| Encounter music's first entry | `Editor/Encounters/EncounterViewModel.BattleTheme` | "(battle theme)" | The encounter plays the battle theme (decision 420). |
| Placeholder animated background | `content/art/backgrounds/ForestPulse.png`, made by `art-src/backgrounds/New-PlaceholderAnimatedBackground.ps1` | `Forest.png` cover-cropped to 960 x 540, 4 frames in a hand-laid 2 x 2 grid (1920 x 1080), brightness 100 %, 90 %, 80 %, 90 %, 200 ms a frame | The hand checks' animated background until the developer imports his own (decision 910). |

## Design overview

### What exists (2026-11-15), and what lands before this row
- `Core/Battle/EncounterDefinition`: `Id`, `EnemyIds` (slot order is screen order), a required `BackgroundFile`, an
  optional `MusicFile` (decision 420). `content/encounters.json` is hand-written (inline enemy arrays); every one of its
  four encounters names `Forest.png`.
- `Core/Maps/MapSettings.RandomEncounters` (`RandomEncounterTable`). Battles start only from it:
  `ExplorationState.TryStartEncounter` sets `PendingEncounter`; no event step starts a battle.
- `Game/Battle/BattleScene` reads `PendingEncounter.BackgroundFile` and draws it with `Ui/CoverFit.DrawOverScreen`, then
  the vignette and the top shade. `Rendering/TextureLibrary` decodes on first `Get` and caches forever; `JrpgGame`
  decodes the 4096 px menu background at startup because its 200 ms decode once broke the menu's fade.
- `Data/Art/ArtReferenceCheck` checks every encounter's background by exact name. `ContentWriter.ImportArtImage`
  copies a PNG into `backgrounds/` with no check beyond being a PNG.
- Editor: Map > Settings... (`Maps/Settings/MapSettingsViewModel`, rebuilt with `new()` in `ToSettings`, compared field
  by field in `AreSame`); `GifImport/` (read, compose, scale, write a one-row strip; `GifConversion`), `Catalogs/
  ArtImageImport` (PNGs and, for battlers and effects, GIFs).
- **Lands before this row:** the mode bar plan's stage 1 (`Workspaces/`: `IEditedCatalog`, `ICatalogEditing<T>`, the
  page registry, `Workspaces/CatalogDialog` hosting a page, `Shell/DatabaseMenu.xaml`; its WS12 recipe); row 7's AN1
  (`Animation/FramePreview`, `FramePlayback`, `PreviewFrames.OfStrip`), AN2 (`GifImport/ComposedGif`, `PreviewFrames.
  OfPixels`, `GifImportOptions.FrameMilliseconds`, `Battle/GifImportWindow.AskFor` with a playing preview and a pace
  box) and AN6 (`PreviewFrames.OfCells`). Each slice reads them as they landed and names any difference in its Done
  note.

### Which background a battle uses (phase 1)
`EncounterBackground.On(encounter, settings)` = `encounter.BackgroundFile ?? settings.BattleBackgroundFile`. The map
validator requires `battleBackgroundFile` while `randomEncounters` is set, so every random battle has one; Core still
returns null when neither names one (a code-built test catalog), and `BattleScene` throws then, as it does today without
a pending encounter. The rule stays in one map file: an Encounters page edit never invalidates a map (decision 892).

### Animated backgrounds (ADR 0059)
A sheet holds its frames row by row: frame `i` sits at column `i % columns`, row `i / columns`, where
`columns = imageWidth / frameWidth`. The editor writes `columns = min(frameCount, 4096 / frameWidth)`, so the sheet is
`columns x frameWidth` wide and `ceil(frameCount / columns) x frameHeight` high; the game and the art check derive the
columns from the image's width, so a hand-laid sheet of any column count works (the placeholder is one).

Costs for 16:9 frames, RGBA in video memory; decode time estimated from the one measured point in the code (the 4096 x
4096 menu background, "over 200 ms", about 12 ms a megapixel):

| Shape | Most frames within 4096 px | 24 frames: sheet | Video memory | Decode |
|---|---|---|---|---|
| Grid, 180 px frames (320 x 180) | 264 (12 x 22) | 3840 x 360 | 5.5 MB | about 17 ms |
| Grid, 270 px frames (480 x 270) | 120 (8 x 15) | 3840 x 810 | 12.4 MB | about 40 ms |
| Grid, 360 px frames (640 x 360) | 66 (6 x 11) | 3840 x 1440 | 22.1 MB | about 70 ms |
| Grid, 540 px frames (960 x 540) | 28 (4 x 7) | 3840 x 3240 | 49.8 MB | about 150 ms |
| Grid, full size (1920 x 1080) | 6 (2 x 3) | can't hold 24 | 49.8 MB at 6 | about 150 ms |
| Folder of full-size frames | no limit | 24 files | 199 MB | about 600 ms |
| Strip, 270 px frames | 8 | can't hold 24 | 12.4 MB at 8 | about 40 ms |

So **the ceiling**: a 24-frame loop is at most 960 x 540 a frame (shown at 2x); full-size frames fit 6 to a sheet.
Option A keeps one texture and one decode per background; pixel-art GIFs (typically 180 to 360 px tall) import at their
own height, untouched. Option B's only gain is full resolution for painted art, at four to sixteen times the memory and
a visible decode.

### Core changes
- `Maps/MapSettings.BattleBackgroundFile` (`string?`), declared just before `RandomEncounters` so a written map file
  reads it beside its battles (BG1).
- `Battle/EncounterDefinition.BackgroundFile` becomes `string?`, not `required` (BG1).
- New `Exploration/EncounterBackground` (BG1): which background a battle on a map is fought on. Static: `On(encounter,
  settings)`; BG5 adds `PossibleOn(settings, encounters)`, every file the map's battles can show.
- `Exploration/ExplorationState.PendingBattleBackground` (BG1).
- New `Battle/AnimatedBackgroundDefinition` (BG3): one animated battle background's frame size and pace by its file
  name; `FrameAt(elapsed)` through its `TileAnimation`, `FrameOrigin(frame, imageWidth)`.
- `Content/ContentCatalog.AnimatedBackgrounds` (BG3): by file, exact case, empty by default (as `EffectStrips`).

### Structure and folder dependencies
| From | To | Why | Exists? |
|---|---|---|---|
| Core `Exploration` | Core `Battle`, `Maps` | `EncounterBackground` reads an encounter and a map's settings | yes (`ExplorationState` already) |
| Core `Battle` | Core `Maps` | `AnimatedBackgroundDefinition.Animation` is a `TileAnimation` | yes (`EnemyDefinition.IdleAnimation`) |
| Data `Battle` | Data `Maps` | `AnimatedBackgroundCatalogValidator` reuses `Maps/AnimationProblems` | yes (the enemy validator) |
| Game `Battle` | Core `Battle`, `Exploration` | `BattleBackdrop` reads the record; the scene reads the pending background | yes |
| Game `Exploration` | Game `Rendering`, Core `Exploration` | the scene asks `MapArt` for the map's backgrounds, releases and decodes through `TextureLibrary` | yes |
| Editor `Backgrounds` (new) | Editor `Animation`, `Catalogs`, `Workspaces` | `FramePreview`, `ArtImageRows`, the page recipe | new folder |
| Editor `Encounters` (new) | Editor `Catalogs`, `Events`, `Objects`, `Workspaces` | `RecordListProblem`, `ListOrder`, `ObjectIds`, the page recipe | new folder |
| Editor `Shell` | Editor `Backgrounds`, `Encounters` | the page registrations and the menu | as every Database page |
| Editor `GifImport` | Data `Art` | `FrameGrid` reads `TextureLimit.MaxSide` | new edge to another project; `GifImport` stays a leaf among Editor folders |
| Editor `Battle` (the GIF dialog) | Editor `GifImport` | the background kind's sheet line reads `FrameGrid` | yes (`GifImportWindow` reads `GifImport`) |

No Editor folder points back at `Shell`; `Backgrounds` and `Encounters` don't read each other (the Encounters page's
background box lists file names from a lookup).

### New types (one line each)
- Core `Exploration/EncounterBackground`: which background a battle on a map is fought on.
- Core `Battle/AnimatedBackgroundDefinition`: one animated battle background's frame size and pace, by its file name.
- Data `Battle/AnimatedBackgroundCatalogValidator`: the rules for `animated-backgrounds.json`.
- Data `Art/ArtReferenceCheck.Backgrounds` (partial): the startup check of every background maps, encounters and records
  name.
- Game `Battle/BattleBackdrop`: the part of the background image shown at the battle's time.
- Game `Exploration/BackgroundRelease`: which of the last map's backgrounds to release on entering another.
- Editor `GifImport/FrameGrid`: frames written row by row into one sheet within the texture limit.
- Editor `GifImport/PngWriter`: BGRA pixels written as a PNG file (moved out of `FrameStrip`, its second user being
  `FrameGrid`).
- Editor `Catalogs/ArtImageRows`: an art folder's images in the given order with their records' files, then the records
  whose image is missing (extracted at its third copy); `Catalogs/ArtImageRow`: one of them, its file and whether its
  image is missing.
- Editor `Backgrounds/BattleBackgroundViewModel`: one background image as the page shows it.
- Editor `Backgrounds/BattleBackgroundCatalogViewModel`: the Battle backgrounds page's state.
- Editor `Backgrounds/BattleBackgroundEditingViewModel`: `Start`/`Apply` of that page.
- Editor `Backgrounds/BattleBackgroundPage`: the page.
- Editor `Encounters/EnemySlotRow`: one place in an encounter's line of enemies.
- Editor `Encounters/EnemyChoice`: one enemy the add box offers, by name.
- Editor `Encounters/EncounterChoices`: what the Encounters page's boxes offer, read once when it starts.
- Editor `Encounters/EncounterViewModel`: one encounter as edited.
- Editor `Encounters/EncounterCatalogViewModel`: the Encounters page's state.
- Editor `Encounters/EncounterEditingViewModel`: `Start`/`Apply` of that page.
- Editor `Encounters/EncounterPage`: the page.

### Replacement mapping
| Today | After |
|---|---|
| `BattleScene` reading `PendingEncounter.BackgroundFile` | `ExplorationState.PendingBattleBackground` (BG1), then `BattleBackdrop` drawing its frame (BG5) |
| `CoverFit.DrawOverScreen` for the battle's picture | `CoverFit.SourceIn` over the frame's cell, offset to it (BG5); the title and menu keep `DrawOverScreen` |
| The encounter-background lines of `ArtReferenceCheck.FindProblems` | `ArtReferenceCheck.Backgrounds.AddBackgroundProblems` (BG1) |
| The image list built in `EffectStripCatalogViewModel` and `FieldSpriteCatalogViewModel` | `Catalogs/ArtImageRows` (BG7) |
| `FrameScaler.Averaged` (private) | `FrameScaler.ScaleAveraged`, public, the background's shrink (BG6) |
| `CombinedHistory.RefusalOfShopUndo` and `RefusalOfCinematicUndo` | one shared refusal over removed ids (EC4) |
| Hand-written `encounters.json` | written by `ContentWriter` (EC1) |

### Extension points
- A battle an event step starts (not planned) reads `MapSettings.BattleBackgroundFile` through `EncounterBackground.On`.
- An animated title or menu background would be a record in the same file read by `Title`/`Menus` (not planned).
- Per-frame times (backlog, decision 632) would become a list on the record's `animation`.

## Risks
- **`Data/Content/CatalogProblems.cs`** grows by a pair of methods per catalog the push makes editable (rows 6-10); this
  row adds two pairs (about +30). Recount before EC1 and BG3.
- **A decode stall**: a background is decoded on the map's entry (decision 900), under the warp's black; a long decode
  there makes the fixed step catch up during the map's fade-in (the menu's precedent). A 270 px sheet is about 40 ms; a
  full 4096 sheet about 200 ms. BG5's hand check watches the fade-in.
- **A release while drawn**: a background is released only on entering another map, never during a battle (a battle
  never warps), and never the title's or the menu's (decision 900); a map naming `GameMenu.png` as its battle
  background keeps it.
- **Painted art at a non-divisor height**: a GIF imported at its own height (decision 898) that isn't a divisor of 1080
  is scaled by a fraction when the game covers the screen, so point sampling gives uneven pixels; the dialog's divisor
  heights avoid it. A frame narrower than 16:9 is scaled by its width, not a whole factor, for the same reason.
- **Content format**: maps gain an optional field; `encounters.json`'s `backgroundFile` becomes optional; a new
  `animated-backgrounds.json`. No save change: a battle is never saved (`GameSession` refuses a save while one is
  pending).
- **Cross-plan**: row 7's AN1, AN2 and AN6 and the mode bar plan's stage 1 must have landed (README); BG8, BG9 and EC5
  read their types as they landed. Row 8 may change `Shell/CombinedHistory` or `CatalogDraft.Database`; EC3 and EC4
  read the files as they are then. `Battle/GifImportOptionsViewModel` (57 lines, AN2 grows it) gains the background
  kind in BG9: recount first, and give the kind its own partial file if it would pass 250.
- **Determinism**: the background's frame comes from the update's delta; nothing random.
