# IU9. An undo removing an item a map gives is refused

Editor, about 120 lines (about 70 of them tests). Needs IU8. Design: decisions 877, 883.

## Goal
An item added in the Items page can be given by a map's event (the event window's give-item step offers the draft's
items). Undoing that Items session would leave the map naming a missing item, which the save refuses. As for shops
(decision 581), the undo is refused with "Can't undo: item test_tonic is given on map start." while a saved map's event,
or the open map's unsaved one, names it. Catalogs naming it (a shop, a drop table) undo in order first, so they need no
refusal.

## Changes
- `Editor/Catalogs/CatalogDraft.Database`: `ItemsRemovedByUndo()` through `KeysRemovedByUndo(catalog => catalog.Items)`,
  its summary as the shops' one.
- `Editor/Catalogs/CatalogUsage`: `UsersOfItem(ContentCatalog content, string itemId, open layouts)`: the maps whose
  events have a give-item step naming it, the open maps' layouts read as they are now (as `UsersOfShop`).
- `Editor/Shell/CombinedHistory`: `RefusalOfItemUndo()` beside `RefusalOfShopUndo` ("A saved item is never removed by
  its page (decision 877), so only an added one can be undone away."), called where the others are; 221 lines, about
  236 after.

## Tests
- `Editor.Tests/Catalogs/CatalogUsageItemTests` (new; layouts built in the test with a give-item step):
  `UsersOfItem_SavedMapGivesIt_NamesTheMap`, `UsersOfItem_OpenMapsUnsavedStepGivesIt_NamesTheMap`,
  `UsersOfItem_StepGivesAnotherItem_IsEmpty`.
- `Editor.Tests/Shell/CombinedHistoryItemTests` (new; `EditorTestContent` as `CombinedHistoryShopTests`, the item built
  in the test, a `SaveWithCellEvent` helper like that file's):
  - `Undo_RemovesAnItemAnEventGives_RefusedNamingTheMap`.
  - `Undo_RemovesAnItemNobodyGives_Undoes`.
  - `Undo_RemovesAnItemAnUnsavedEventGives_RefusedNamingTheMap`.

## Verify by hand
- Run the editor on the start map: Database > Items..., Add consumable, id `test_tonic`, name "Test Tonic", OK. Events
  tab: click an empty cell, add an event with a Give item step giving `test_tonic`, OK. Ctrl+S. In the map list, open
  another map, press Ctrl+Z: the status line says "Can't undo: item test_tonic is given on map start." and Test Tonic
  is still in Database > Items. Go back to the start map, Ctrl+Z (the event goes), then Ctrl+Z again (Test Tonic
  goes). Revert the content (`git checkout content`).
- **Done, with deviations:** the four event-named refusals moved into `CombinedHistory.RefusalOfEventNamedUndo`, as
  one more `??` put `RefusalOfCatalogUndo` at complexity 11 (CA1502); the shop, cinematic and item copies extracted
  (third copy): `CatalogUsage.MapsWithStep` and `CombinedHistory.RefusalNamingFirstUser`, and the four tests'
  `SaveWithCellEvent` to `EditorTestContent`; `docs/map.md` names the item refusal. The hand check is the developer's.
