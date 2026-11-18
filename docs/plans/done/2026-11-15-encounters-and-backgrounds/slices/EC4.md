# EC4. An undo can't remove an encounter a map's battles list

Editor; about 130 lines with tests. Needs EC3. Decision 907. Design: "Replacement mapping".

## Goal
After the Encounters page applied an added encounter and a map's random battles were set to roll it (Map >
Settings..., a step in that map's own history), Ctrl+Z on the page's step is refused with "Can't undo: encounter <id> is rolled on
<map>.", as an added shop an event opens. The three refusals over removed ids share one helper (the third copy).

## New types
None.

## Changes
- `Catalogs/CatalogDraft.Database`: `EncountersRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Encounters)`,
  its summary as the shops'.
- `Catalogs/CatalogUsage` (149 lines, about +12): `UsersOfEncounter(ContentCatalog content, string encounterId)`: the
  maps, in id order, whose random battles list it; `content` is the draft with the open maps' unsaved settings
  (`OpenMaps.ContentWith`, decision 502). Its summary line joins the class's list.
- `Shell/CombinedHistory` (221 lines, about +5 net): `RefusalOfShopUndo` and `RefusalOfCinematicUndo` become calls of
  one `RefusalOfRemoved(IReadOnlyList<string> removedIds, Func<string, IReadOnlyList<string>> usersOf, Func<string,
  string, string> refusal)` (the first id with a user, the refusal worded by its caller), and `RefusalOfEncounterUndo`
  is the third call, reading `_openMaps.ContentWith(_catalog.Current)` once; it joins the refusals the undo asks.
  The existing shop and cinematic refusal tests pass unchanged: that is the guard of the extraction.
- `docs/map.md`: the `Catalogs` row's list of refused undos names "an encounter a map's random battles roll".

## Tests
- `CombinedHistoryEncounterTests` (Editor, new; globbed: the folder has `CombinedHistoryShopTests` and
  `CombinedHistoryCinematicTests`, built the same way):
  - `Undo_AddedEncounterRolledOnAnOpenMap_IsRefusedNamingTheMap` (the map's settings set after the page's step, unsaved)
  - `Undo_AddedEncounterNoMapRolls_RemovesIt`
  - `Undo_AddedEncounterRolledOnlyOnAnUnopenedSavedMap_IsRefused` (the map in the draft's saved maps, not open)
- `CatalogUsageEncounterTests` (Editor, new): `UsersOfEncounter_TwoMapsRollIt_ListsBothInIdOrder` (maps set up in the
  reverse order), `UsersOfEncounter_MapWithoutBattles_NotListed`.

## Verify by hand
None here: no page adds an encounter yet. EC5's hand check (step 5) covers the refusal by hand.
- **Note from BG1:** `CombinedHistory` already holds quest, shop, cinematic and item refusals (`RefusalOfEventNamedUndo`,
  IU9 added the item one): restate the shared-helper step against those before writing the encounter refusal.
- **Note from EC3:** EC3 was split: `CatalogDraft.SetEncounters` and `EncounterEditingViewModel` land in EC3b, which
  EC4 needs as well.
- **Done, with deviations:** no extraction: `CombinedHistory.RefusalNamingFirstUser` (kind, use, removed ids, users
  of), which the shop, cinematic and item refusals already share, is decision 907's one helper, and
  `RefusalOfEncounterUndo` is its fourth call, checked after the event-named refusals; `RefusalOfShopUndo`,
  `RefusalOfCinematicUndo` and their tests unchanged. The unopened-map test takes goblin_chief (rolled on the test
  world's to_aslas_1) out and back as two `SetEncounters` steps, since no page adds an encounter a saved map rolls;
  `CatalogUsageEncounterTests` adds `UsersOfEncounter_BattlesRollAnother_NotListed`. `CombinedHistory` 260 lines,
  `CatalogUsage` 180, `CatalogDraft.Database` 214; tests 70 and 52.
