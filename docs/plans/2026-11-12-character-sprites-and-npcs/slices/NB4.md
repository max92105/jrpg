# NB4. NPCs idle

**Later: not in the approved plan** (README, "Later"). A draft kept for when idle loops come back; before then it needs a per-NPC time offset from the placed uid (a fixed hash), or every NPC on a street bobs in step. Its decision numbers were never given.

Core + Data + Game, about 220 lines raw (120 of them tests). Builds on NB2.

## Goal
A sheet may name an idle loop (a row per facing, a frame count and a time); an NPC drawn from it loops that facing's idle frames at the map's time while it stands, also while it faces the player during its event. A sheet without one stands still, as today. The player never idles.

## New types
None.

## Changes
- `Core/Sprites/FieldSpriteSheet`: `Idle` (`FacingCycle?`, init); `IdleCellAt(Facing, TimeSpan elapsed)` (the idle loop's frame, or `StandingCell` when the sheet has none); `ColumnsNeeded` and `RowsNeeded` take the idle loop into account.
- `Data/Sprites/FieldSpriteCatalogValidator`: the idle loop's rows and frames by the walk's rules.
- `Data/Content/AbsentWhenNullFilter`: a row for `FieldSpriteSheet.idle`.
- `Data/Art/ArtReferenceCheck`: unchanged code; FS2's counts now include the idle rows (a test pins it).
- `Game/Exploration/MapView.DrawObject`: an NPC draws `IdleCellAt(npcFacingOf(placed), AnimationTime)`.

## Tests
- `FieldSpriteSheetTests`: `IdleCellAt_NoIdleLoop_IsStandingCell`; `IdleCellAt_TwoFrameLoop_AlternatesOnItsRow` (theory over times); `RowsNeeded_IdleRowsBelowWalk_CountsThem`; `ColumnsNeeded_LongerIdleLoop_CountsIt`.
- `FieldSpriteCatalogValidatorTests`: `InvalidSheets` rows: idle row negative, idle frame count 1, idle frame time 0.
- `ArtReferenceCheckFieldSpriteTests`: `FindProblems_IdleRowsPastImage_NamesFile`.
- `ContentWriterFieldSpriteTests`: `WriteMapCatalogs_SheetWithoutIdle_WritesNoIdleField`.

## Verify by hand
1. In `content/field-sprites.json`, give the bought sheet's record an `idle` with the rows of its two-frame poses (one per facing, as the sheet holds them; repeat a row where a facing has none) and a time of 500 ms. Start the game: the NPC loops its idle frames while standing; talk to it from the left: it idles facing the player.
2. The placeholder NPC (no idle) stands still as before.
3. Revert with `git checkout -- content` if this was only a check.
