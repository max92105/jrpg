# CE2. An undo that would remove a cinematic a map's event plays is refused

Editor, about 150 lines (about 90 of them tests). Needs CE1 and CM3. Design: key decision 8.

## Goal
Adding a cinematic in the window, then giving a map's event a step that plays it, then undoing on another map (whose
own history doesn't hold the event's step) is refused with "Can't undo: cinematic ragnir_intro is played on map
intro.", as for shops (decision 581). The refusal reads every open map as it is now.

## Changes
- `Catalogs/CatalogUsage`: `UsersOfCinematic(ContentCatalog catalog, string cinematicId, IReadOnlyCollection<MapLayout>
  openLayouts)` ("map intro" style names, sorted; summary as `UsersOfShop`'s, "so an undo that would remove it can be
  refused"), through `MapNamesWhere` and a private `PlaysCinematic(MapEvent, string)`.
- `Catalogs/CatalogDraft.cs`: the four string-keyed `...RemovedByUndo` one-liners (objects, sheets, quests) and
  `CatalogDraft.Database.cs`' `ShopsRemovedByUndo` fold onto one private `KeysRemovedByUndo<T>(Func<ContentCatalog,
  IReadOnlyDictionary<string, T>> records)` (the fifth copy would be the cinematics': extract at this one, the
  code-quality rule's third copy long passed); their public signatures stay. `CinematicsRemovedByUndo()` joins them in
  `CatalogDraft.Database.cs`.
- `Shell/CombinedHistory`: `RefusalOfCinematicUndo()` beside `RefusalOfShopUndo()`, chained where the shop's is.

## Tests
- `CatalogUsageTests` (157): `UsersOfCinematic_EventPlaysIt_NamesTheMap`; `UsersOfCinematic_OpenMapUnsavedStep_NamesIt`
  (an open layout with the step, the saved one without); `UsersOfCinematic_NoEventPlaysIt_IsEmpty`.
- `CatalogDraftCinematicTests` (CE1's): `CinematicsRemovedByUndo_AfterAdding_NamesTheAdded`.
- `CatalogDraftRemovalTests` or the existing removal tests: unchanged and passing (the fold's guard).
- `CombinedHistoryCinematicTests` (new; Glob found `CombinedHistoryQuestTests`, `...ShopTests`, `...ObjectTests`):
  - `Undo_AddedCinematicPlayedOnAnotherMap_IsRefused` (message pinned).
  - `Undo_AddedCinematicNoEventPlays_Undoes`.

## Verify by hand
After CE6 (no window yet): noted there.

- **Done, with deviations:** the usage tests are a sibling, `CatalogUsageCinematicTests`; the draft tests add `CinematicsRemovedByUndo_AfterEditingOnly_NamesNone` (the filter's other side). `SetCinematics`' summary drops CE1's "(CE2)" (CE1 review).
