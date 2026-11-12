# NB2. Each placed NPC's standing facing

Core + Data + Game, about 170 lines (100 of them tests). Design: "Core changes", key decision 4. Decision 572. Builds on NB1.

## Goal
A placed NPC may stand facing any way: the map layout's placed object carries an optional `facing` (down when absent), the game draws it, and an NPC turns back to it after its event. Content can set it by hand in this slice; the editor's box is NB3.

## New types
None.

## Changes
- `Core/Maps/PlacedObject`: `Facing` (`Facing?`, init, null meaning down); its summary says only an NPC carries one.
- `Core/Exploration/NpcFacing.Of`: the fallback is `placed.Facing ?? Facing.Down`.
- `Data/Content/AbsentWhenNullFilter`: a row for `PlacedObject.facing` (NB2: only a turned NPC has one), so every layout writes back byte for byte.
- `Data/Maps/MapLayoutValidator` (135): a placed object with a `facing` whose object has no field sprite is refused, naming the map, the uid and the object ("map-layouts/start.json: object 'chest_1' (chest) has a facing, but only an NPC turns."); its check goes in the placed-object loop's own helper, keeping the loop under the complexity limit.
- `tests/TestWorld`: unchanged (the field is optional).

## Tests
- `NpcFacingTests`: `Of_NoEventRunning_FacesItsPlacedFacing`; `Of_OwnEventEnded_TurnsBackToItsPlacedFacing`.
- `MapLayoutValidatorTests` (114): `Validate_NpcWithFacing_Passes`; `Validate_TileObjectWithFacing_ThrowsNamingUid`.
- `ContentWriterRepositoryTests`: the existing identical-bytes tests cover the layouts (no layout has a facing yet).
- `ContentCatalogLoaderFieldSpriteTests`: `Load_LayoutWithNpcFacingLeft_ReadsIt`.

## Verify by hand
1. In a map layout under `content/map-layouts/`, add `"facing": "left"` to a placed NPC by hand; start the game: it stands facing left; talk to it from below: it faces up, then left again after.
2. Add `"facing": "up"` to a chest: the game's start refuses the content, naming the map and the chest. Revert with `git checkout -- content`.
