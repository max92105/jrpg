# BG4. The startup art check on backgrounds, and a placeholder animated background

Data, content, a script; about 200 lines with tests. Needs BG1, BG3. Decisions 895, 897, 910. Design: "Animated
backgrounds (ADR 0059)", "Tunables and placeholders".

## Goal
The game's startup refuses an animated background whose image is missing, wider or taller than 4096 px, or too small
for its record's grid, and any background a map or an encounter names that is over 4096 px a side. The editor's image
import refuses a background over 4096 px a side. The repository holds one animated background, `ForestPulse.png`, made
by a script and listed in `animated-backgrounds.json`, so BG5 has content to play without waiting on art.

## New types
None (`ArtReferenceCheck.Backgrounds.cs` exists since BG1).

## Changes
- Data `Art/ArtReferenceCheck.Backgrounds.cs`: `AddBackgroundProblems` also checks
  - every record of `catalog.AnimatedBackgrounds`, in file order: present by exact name (named by
    "animated-backgrounds.json '<file>'"); at most `TextureLimit.MaxSide` a side; holds its grid:
    `public static string? GridProblemOf(AnimatedBackgroundDefinition record, (int Width, int Height) size)`, by counts
    in `long` (columns `= width / frameWidth`, at least 1; rows needed `= ceil(frameCount / columns)` at most `height /
    frameHeight`), the message naming the grid it needs ("4 frames of 960 x 540 px need 2 x 2; the image holds 1 x
    2"). Public because the Battle backgrounds page's problem line reads the same rule (BG7; Data's validators are
    internal, so the editor's live problem goes through one public Data entry);
  - every background a map or an encounter names (BG1's list): at most `TextureLimit.MaxSide` a side, the message
    "a 5000 x 2000 px image is larger than the game loads (4096 px a side)".
- Data `Content/ContentWriter.ArtImports`: `RefuseTooWide` becomes `RefuseTooLarge`: battlers and effects keep their
  width check; backgrounds are checked on both sides (decision 895); the summary of `ImportArtImage` says so. If the
  import's message and the art check's would be copies, both read one `TextureLimit.ProblemOf(size)`.
- Script `art-src/backgrounds/New-PlaceholderAnimatedBackground.ps1` (PowerShell + System.Drawing, as
  `art-src/digits/New-DigitStrips.ps1`): reads `content/art/backgrounds/Forest.png`, crops it to 16:9 about its
  center, scales it to 960 x 540 (high-quality bicubic: it is painted art), and lays four frames by hand in a 2 x 2
  grid (1920 x 1080; the editor's writer would lay four such frames in one row, so this sheet exercises the game's
  column rule, decision 897) at 100 %, 90 %, 80 % and 90 % brightness, to `content/art/backgrounds/ForestPulse.png`. It
  refuses to overwrite without `-Force`. `art-src/README.md` gains its line.
- `content/art/backgrounds/ForestPulse.png`: written by the script.
- `content/animated-backgrounds.json`: one record, the writer's layout: `"file": "ForestPulse.png"`,
  `"frameWidth": 960`, `"frameHeight": 540`, `"animation"` with `"frameCount": 4` and `"frameMilliseconds": 200`.
- `content/art/art-sources.json`: `backgrounds/ForestPulse.png` under `generated`, after `backgrounds/Forest.png`, in
  the file's layout; `generated` is already credited in `CREDITS.md`. Its source stays a placeholder (decision 910).
- No map names `ForestPulse.png` (BG5's hand check names it locally and reverts).

## Tests
- `ArtReferenceCheckBackgroundTests` (Data, created by BG1; PNGs written in the test's folder with the existing PNG
  fixture helper the art tests use):
  - `FindProblems_RecordImageMissing_NamesTheRecord`
  - `FindProblems_RecordImageHoldsItsGrid_NoProblem` (4 frames in a 2 x 2 grid, exactly)
  - `FindProblems_RecordImageOneRowShort_NamesTheGrid`
  - `FindProblems_RecordImageNarrowerThanAFrame_IsAProblem`
  - `FindProblems_RecordWithNearMaxFrameCount_IsAProblemWithoutOverflow` (`int.MaxValue` frames)
  - `GridProblemOf_ImageExactlyHoldsTheGrid_IsNull` and `GridProblemOf_ImageOneColumnShortOnASingleRow_NamesTheGrid`
  - `FindProblems_MapBackgroundOver4096High_IsAProblem` and `FindProblems_MapBackgroundExactly4096_NoProblem`
- `ContentWriterArtImportTests` (Data, 154 lines):
  - `ImportArtImage_BackgroundTallerThan4096_IsRefusedAndNotCopied`
  - `ImportArtImage_Background4096Square_IsCopied`
- The repository guards cover the content: the startup art check over the repository (`ForestPulse.png` holds its
  grid), `ArtSourcesTests` (the new file is listed), `ContentWriterRepositoryTests` (the record reads as written).

## Verify by hand
1. Run the script, then `dotnet run --project src/Jrpg.Game`: the game starts (the art check passes).
2. Open `content/art/backgrounds/ForestPulse.png` (agents may: it is made from the `generated` forest, not Mana Seed):
   four 960 x 540 frames in two rows of two, darkening then brightening.
3. Change the record's `frameHeight` to 600 locally: the start names `ForestPulse.png` and the grid it needs. Revert.
