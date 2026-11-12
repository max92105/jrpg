# NB3. The placed facing in the editor

Editor, about 190 lines (100 of them tests). Builds on NB2 and FS5. Decision 572.

## Goal
Select a placed NPC: the Placed object panel shows a **Facing** box (Down, Left, Right, Up), applied with the panel's other fields as one undo step; the map draws the NPC facing that way; copy and paste keep it; a tile object shows no box.

## New types
None.

## Changes
- `Editor/Objects/PlacedObjectEditing` (197): `Facing` (the chosen NPC's, `Facing.Down` when absent), `HasFacing` (the object has a field sprite), `FacingChoices` (the four, as a fixed list bound by `SelectedValue`, the free-levels plan's rule); `Apply` writes `null` for Down so an untouched NPC's layout stays byte for byte; switching to a tile object (object box) drops the facing, since Data refuses it there. Its applied record copies the facing back (the map fixes plan's rule on fields an editor rebuilds with `new()`): grep `new PlacedObject` in `PlacedObjectEditing` and `PlacedObjectCopies`.
- `Editor/Objects/PlacedObjectPanel.xaml`: the Facing row, visible with `HasFacing`.
- `Editor/Maps/Editing/PlacedObjectCopies`: copies keep `Facing`.
- `Editor/Tiles/TileImages.FieldSpriteFrameOf(string file, Facing facing)`: the standing frame of a facing (cached by record and facing); `Editor/Maps/MapDrawing` passes `placed.Facing ?? Facing.Down`; the Objects panel's preview and the ghost stay down.

## Tests
- `PlacedObjectEditingFacingTests` (new sibling; `PlacedObjectEditingTests` is at 214): `Facing_PlacedNpcFacingLeft_ShowsLeft`; `HasFacing_TileObject_IsFalse`; `Apply_FacingChanged_IsOneMapStep`; `Apply_FacingDown_WritesNoFacing`; `Apply_ObjectSwappedToTileObject_DropsFacing`; `Apply_PositionOnly_KeepsFacing` (the copy-back guard).
- `PlacedObjectCopiesTests`: `Copy_NpcFacingUp_KeepsIt`.
- `TileImagesTests`: `FieldSpriteFrameOf_FacingLeft_CropsItsRow`.

## Verify by hand
1. Select a placed NPC in Objects mode: the panel shows Facing: Down. Choose Left, Apply: it faces left on the map. Ctrl+Z, Ctrl+Y.
2. Select a chest: no Facing row.
3. Ctrl+C, Ctrl+V the NPC: the copy faces left too.
4. Ctrl+S, start the game: it stands facing left and turns back to left after talking.
5. Revert with `git checkout -- content` if this was only a check.
