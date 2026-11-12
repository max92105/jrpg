# EC1. Encounters save from the editor

**Later: not in the approved plan** (README, "Later": a "New encounter from these enemies" button is the lighter first step); a draft kept, its decision numbers never given.

Data + Editor, about 200 lines (110 of them tests). Builds on SH1 and EN1.

## Goal
`encounters.json` round-trips through the writer (relaid once, `musicFile` left out while null); Ctrl+S writes it checked against the enemies, and every map's settings are checked against the encounters written (a map's random battles name them); the draft replaces the encounters as one undo step; an undo that would remove an encounter a map's random battles name is refused, naming the map.

## New types
None.

## Changes
- `Data/Content/AbsentWhenNullFilter`: a row for `EncounterDefinition.musicFile` if the hand file leaves it out (check the file; decision 420 made it optional).
- `Data/Content/ContentWriter.CheckedCatalogWrites`: validates (`EncounterCatalogValidator` against the enemy ids) and writes `encounters.json`; then every map's settings through `MapSettingsValidator` against the encounters written (the same call `CheckSettingsAndNewGame` makes for the maps it writes).
- `Data/Content/CatalogProblems.OfEncounters(IReadOnlyList<EncounterDefinition>, ContentCatalog)`: the validator, then the maps' settings.
- `content/encounters.json`: relaid by the writer (its commit holds only that).
- `Editor/Catalogs/CatalogDraft.Database.cs`: `SetEncounters` (equal by content: `EnemyIds` compared element by element), `EncountersRemovedByUndo()`.
- `Editor/Catalogs/CatalogUsage.UsersOfEncounter(ContentCatalog, string encounterId, OpenMaps)`: the maps whose settings roll it, the open maps' unsaved settings included (decision 502's rule).
- `Editor/Shell/CombinedHistory`: `RefusalOfEncounterUndo()`, its own method called once from `RefusalOfCatalogUndo`: "Can't undo: encounter {id} is rolled on {map}."

## Tests
- `ContentWriterEncounterTests` (new sibling): `WriteMapCatalogs_RepositoryEncounters_LoadsTheSameAfterRelay`; `WriteMapCatalogs_EncounterNamingUnknownEnemy_WritesNothing`; `WriteMapCatalogs_EncounterRemovedWhileAMapRollsIt_WritesNothing`.
- `ContentWriterRepositoryTests`: the identical-bytes list gains `encounters.json`.
- `CatalogProblemsTests`: `OfEncounters_RepositoryEncounters_IsNull`; `OfEncounters_EmptyEnemies_NamesEncounter`.
- `CatalogDraftEncounterTests` (new): `SetEncounters_NewEncounter_IsOneUndoStep`; `SetEncounters_EqualByContent_RecordsNothing`.
- `CombinedHistoryEncounterTests` (new): `Undo_EncounterRolledByAMapsSettings_IsRefusedNamingMap`; `Undo_EncounterNoMapRolls_Undoes`.

## Verify by hand
None: no window changes encounters yet (EC3).
