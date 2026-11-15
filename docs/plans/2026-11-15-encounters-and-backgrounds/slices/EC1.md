# EC1. The editor writes `encounters.json`

Data; about 150 lines with tests. Needs BG1 (which laid the file out as the writer writes it). Decision 908. Design:
"Replacement mapping".

## Goal
`encounters.json` joins the catalog files the editor's writer checks and writes: sorted by id, an encounter without
its own background or music written without those fields, the loader's checks run first, and the repository's file
written back byte for byte. The Encounters page's problem line gets its public Data entry. No page yet.

## New types
None.

## Changes
- Data `Content/ContentCatalogLoader`: `EncountersFile` becomes `internal const`.
- Data `Content/AbsentWhenNullFilter`: `(typeof(EncounterDefinition), "backgroundFile")` and
  `(typeof(EncounterDefinition), "musicFile")`, with a comment naming decision 908.
- Data `Content/CatalogProblems` (recount first: design.md Risks; about +15): `OfEncounters(IReadOnlyList<EncounterDefinition>
  encounters, ContentCatalog content)` (the loader's message for the list against `content.Enemies`, null when it
  passes) and `CheckEncounters(...)`, as `OfShops`/`CheckShops`.
- Data `Content/ContentWriter.Catalogs`: `CheckCatalogs` calls `CheckEncounters`; `SerializedCatalogs` writes the file
  sorted by id (`SortedEncounters`).
- `docs/map.md`: Data's `CatalogProblems` line names `OfEncounters`; `docs/plans/2026-11-15-editor-push/design.md`
  section 2's "Hand-written JSON only" list is left as history (it records 2026-11-15).

## Tests
- `ContentWriterEncounterTests` (Data, new; globbed: no such file; a copied test-world content root, encounters built
  in the test):
  - `WriteMapCatalogsAndMaps_EncountersAddedOutOfOrder_WritesThemSortedById`
  - `WriteMapCatalogsAndMaps_EncounterWithoutBackgroundOrMusic_LeavesBothFieldsOut`
  - `WriteMapCatalogsAndMaps_EncounterWithBoth_WritesBoth`
  - `WriteMapCatalogsAndMaps_UnknownEnemy_IsRefusedAndNothingWritten`
  - `WriteMapCatalogsAndMaps_SameEnemyTwice_WritesBothSlotsInOrder` (slot order is screen order)
- `CatalogProblemsTests` (Data, 160 lines, about +20):
  - `OfEncounters_ValidList_IsNull`
  - `OfEncounters_UnknownEnemy_NamesIt`
- `ContentWriterRepositoryTests`: `CatalogFileBytes` adds `encounters.json`, so the identical-bytes test covers it.

## Verify by hand
1. Run the editor, change any map tile and undo it, **Save all**: `git status` shows `encounters.json` unchanged.
