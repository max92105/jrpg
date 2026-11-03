# E3b. The purge: a sheet and everything drawn from it, and the writer for it

Builds on E3a, decision 442 and design.md, "Key decisions" 6. E3c is its first user in the editor; this slice is the pure part and the Data writer, each tested on its own.

## Goal
Given a sheet, work out the catalog and the maps without it and without everything drawn from it (its tiles, their cells on every map, the objects made of any of its tiles and their placements), with the counts a question would show; or the reason it can't (an event swaps to an object it would remove). And write such a result in one go: the catalogs and every changed map, all or none.

## New types
- `Editor/Catalogs/SheetPurge`: the catalog and the maps without a sheet and everything drawn from it, or the reason it can't. Pure: catalog in, `SheetPurgeResult` out.
- `Editor/Catalogs/SheetPurgeResult`: the purged catalog, the ids of the maps it changed, and the counts (cells, objects, placements, placements with events).

## Changes
- `Data/Content/ContentWriter.WriteMapCatalogsAndMaps(ContentCatalog catalog, IReadOnlyCollection<string> mapIds)`: the catalogs and the named maps' files in one all-or-none `WriteAll`, after the same checks as `WriteMapCatalogs` (every layout against the new catalogs) and `WriteMap` (the new game's start).

## Tests (new `Catalogs/SheetPurgeTests`, `Data.Tests/Content/ContentWriterMapsTests`)
- `SheetPurgeTests: Purge_SheetUsedOnTwoMaps_ErasesItsCellsOnBoth`.
- `SheetPurgeTests: Purge_ObjectMadeOfItsTiles_RemovesTheObjectAndItsPlacements`.
- `SheetPurgeTests: Purge_ObjectMixingSheets_IsRemovedToo` (any tile of the sheet takes the object).
- `SheetPurgeTests: Purge_EventSwapsToARemovedObject_RefusesNamingTheMapAndObject`.
- `SheetPurgeTests: Purge_OtherSheetsCellsAndObjects_AreUntouched`.
- `SheetPurgeTests: Purge_CountsCellsObjectsAndPlacementsWithEvents`.
- `SheetPurgeTests: Purge_MapsWithoutItsTiles_AreNotListedAsChanged`.
- `ContentWriterMapsTests: WriteMapCatalogsAndMaps_ValidPurge_WritesCatalogsAndMaps`.
- `ContentWriterMapsTests: WriteMapCatalogsAndMaps_InvalidMap_WritesNothing`.

Size: about 85 lines of code (Data about 25), 9 tests (about 200).

## Verify by hand
None: no screen yet (E3c's hand check covers it).

- **Done, with deviations:** the refusal also covers an event gated on an object that would go (`onlyWhenObjectId`, which the save refuses as it does a swap); one more test for it. `SheetPurge` has `RefusalOf` and `Purge` (which throws when refused) rather than one result with an optional reason. `ContentWriter.WriteMapCatalogs` and the new `WriteMapCatalogsAndMaps` share one private `CheckedCatalogWrites`; the named maps' settings are checked and written too (unchanged by a purge, so the same bytes).
