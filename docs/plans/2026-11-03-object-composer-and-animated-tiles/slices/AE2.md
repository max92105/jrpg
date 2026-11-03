# AE2. Apply and Remove animation as one undo step

Builds on design.md, "Key decisions" 5, and the README's picks 9 and 13.

## Goal
The Animate panel's catalog step: Apply makes the chosen cell a tile when it isn't one yet (walkable, outside the history, as painting does, decision 113), then sets its animation as one undo step; Remove animation makes an animated tile still again, also one step; Ctrl+Z covers both and the catalogs save them (AN1's writer). `CatalogDraft` (294 lines) first moves its two removals into a partial file, unchanged, so it can grow.

## New types
- `Editor/Catalogs/CatalogDraft.Removals.cs`: the partial file holding `RemoveTilesheet` and `RemoveObject`, moved as they are (about 30 lines; `CatalogDraftRemovalTests` pass unchanged).

## Changes
- `Editor/Catalogs/CatalogDraft`: `public void SetTileAnimation(int tileId, TileAnimation? animation)` (one step through `WithTile` both ways; nothing when the tile already has an animation equal by content: the record compares `Frames` by reference, so the frames are compared in order, as `IsSameObject` does).
- `Editor/Tiles/TileAnimationViewModel`: `[RelayCommand] Apply` (enabled without a problem; closes the panel) and `[RelayCommand] RemoveAnimation` (enabled on an animated tile); the class summary takes "applied or removed as one undo step".

## Tests
- `Catalogs/CatalogDraftAnimationTests` (new):
  - `SetTileAnimation_NewAnimation_IsOneUndoStep` (Undo brings the still tile back).
  - `SetTileAnimation_SameFramesAndTime_RecordsNothing`.
  - `SetTileAnimation_Null_MakesTheTileStill`.
- `Tiles/TileAnimationApplyTests` (new, beside `TileAnimationViewModelTests` to keep each under 300):
  - `Apply_CellNotATileYet_MakesTheTileWithTheAnimation`.
  - `Apply_AnimatedTile_ReplacesItsFramesAndTime`.
  - `Apply_WithAProblem_CannotExecute`.
  - `RemoveAnimation_AnimatedTile_LeavesItStill`.
  - `RemoveAnimation_StillTile_CannotExecute`.

Size: about 30 lines moved, 35 lines of code, 8 tests (about 160).

## Verify by hand
Nothing to see yet (AE3).
