# CE1. Cinematics save from the editor

Core + Data + Editor, about 200 lines (about 120 of them tests). Needs CE0. Design: "Content format", key decision 8
(the refusal comes in CE2).

## Goal
The draft can replace the cinematics as one undo step; a save writes `cinematics.json` sorted by id, the stills in their
order, optional fields left out, checked as the loader checks it; `CatalogProblems.OfCinematics` gives Data's message
for an edited list.

## Changes
- Core `Cinematics/CinematicDefinition`: `Equals(CinematicDefinition?)` comparing the id, the track and the stills by
  content (`SequenceEqual`; `CinematicStill` and `StillZoom` are records of values), `GetHashCode` from the id and the
  still count; the comment `ShopDefinition` carries ("a record compares a list by reference; two cinematics read from
  the same content are one cinematic"). First user: `SetCinematics`.
- Data `Content/CatalogProblems`: `public static string? OfCinematics(IReadOnlyList<CinematicDefinition> cinematics)`
  (summary: a cinematic's own rule broken, by id and still; null when the loader would take them) and `internal static
  void CheckCinematics(...)` (`CinematicCatalogValidator.Validate(CinematicsFile, cinematics)`).
- Data `Content/ContentWriter.Catalogs.cs`: `CheckCatalogs` calls `CheckCinematics`; `SerializedCatalogs` gains
  `cinematics.json`; `SortedCinematics` by id, ordinal. `WriteMapCatalogsAndMaps`' summary names the cinematics.
- Data `Content/CatalogFileStamps`: nothing if it stamps every file `SerializedCatalogs` lists (check; else the file's
  name joins its list).
- Editor `Catalogs/CatalogDraft.Database.cs`: `SetCinematics(IReadOnlyList<CinematicDefinition>)` in `SetShops`' shape
  (`HasSameRecords` with record equality; one step; nothing when equal by content). Summary: "Replaces the cinematics as
  one undo step (ADR 0045); an undo removing one a map's event plays is refused (CE2)." The partial's summary names the
  cinematics.
- `docs/map.md`: the `Catalogs` row names `SetCinematics`; the Data `Content` row names `OfCinematics`.

## Tests
- Core `CinematicDefinitionTests` (new, `tests/Jrpg.Core.Tests/Cinematics/`):
  - `Equals_SameStillsInNewLists_IsTrue` (hash codes equal too).
  - `Equals_StillZoomDiffers_IsFalse`.
  - `Equals_StillsInOtherOrder_IsFalse`.
- `ContentWriterCinematicTests` (CM3's file), on the test world:
  - `WriteMapCatalogs_NewCinematics_WritesThemSortedWithStillsInOrder` (`zeta` then `alpha` given; `alpha` first in the
    file, its stills `b.png` before `a.png`).
  - `WriteMapCatalogs_StillWithoutOptionalFields_LeavesThemOut` (no `caption`, `voiceFile`, `zoom` keys; no `musicFile`).
  - `WriteMapCatalogs_StillWithEveryField_WritesThem` (the other side of the filter).
  - `WriteMapCatalogs_StillSecondsZero_WritesNothing` (message pinned; the file's bytes unchanged).
- `ContentWriterRepositoryTests`: `CatalogFileBytes` gains `cinematics.json` (proves CM1's `[]` is the writer's layout).
- `CatalogProblemsTests`: `OfCinematics_Valid_IsNull`; `OfCinematics_ZoomPast150_NamesCinematicAndStill`;
  `OfCinematics_ZoomAt150_IsNull`.
- `CatalogDraftCinematicTests` (new):
  - `SetCinematics_AddedCinematic_IsOneUndoStep`.
  - `SetCinematics_Undo_RestoresTheCinematics` (its own test).
  - `SetCinematics_EqualByContent_RecordsNothing` (stills copied into new lists).

## Verify by hand
None: no window edits cinematics yet (CE6).

- **Done:** as planned (`CatalogFileStamps` needed nothing: it stamps what `SerializedCatalogs` lists). One test more than listed: `CinematicDefinitionTests.Equals_TrackDiffers_IsFalse`.
