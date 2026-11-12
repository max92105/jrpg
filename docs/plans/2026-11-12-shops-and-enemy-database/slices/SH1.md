# SH1. Shops save from the editor

Data + Editor, about 220 lines (120 of them tests). Design: "Data changes", key decisions 1 and 2. Decisions 581, 591. Builds on plan 1's CW1 and SI1; ADR 0045 written first.

## Goal
The draft can replace the shops as one undo step, Ctrl+S writes `shops.json` (relaid once) checked against the items, and an undo that would remove a shop a map's `openShop` step names is refused, naming the map. `CatalogProblems.OfShops` gives Data's message for an edited list.

## New types
- `Editor/Catalogs/CatalogDraft.Database.cs` (partial): `SetShops(IReadOnlyList<ShopDefinition>)` (one step, nothing when equal by content; `ShopDefinition` already compares its items by content) and `ShopsRemovedByUndo()`.
## Changes
- `Data/Content/CatalogProblems.OfShops(IReadOnlyList<ShopDefinition> shops, ContentCatalog content)`.
- `Data/Content/ContentWriter.CheckedCatalogWrites`: validates (`ShopCatalogValidator` against `content.Items`) and writes `shops.json` sorted by id; the file's constant `ShopsFile` becomes internal; summaries name the shops.
- `content/shops.json`: relaid by the writer (the array one item per line); its commit holds only that.
- `Editor/Catalogs/CatalogUsage.UsersOfShop(ContentCatalog, string shopId, IReadOnlyCollection<MapLayout> openLayouts)`: the maps whose events open it.
- `Editor/Shell/CombinedHistory`: `RefusalOfShopUndo()` (its own method, called once from `RefusalOfCatalogUndo`, which is near its complexity limit): "Can't undo: shop {id} is opened on {map}."

## Tests
- `ContentWriterShopTests` (new sibling): `WriteMapCatalogs_RepositoryShops_LoadsTheSameAfterRelay` (load, write, load, compare); `WriteMapCatalogs_ShopWithUnpricedItem_WritesNothing`; `WriteMapCatalogs_NewShop_WritesItSorted`.
- `ContentWriterRepositoryTests`: the identical-bytes test's file list gains `shops.json` (after the relay).
- `CatalogProblemsTests`: `OfShops_RepositoryShops_IsNull`; `OfShops_EmptyName_NamesShopAndField`; `OfShops_UnknownItem_NamesIt`.
- `CatalogDraftShopTests` (new): `SetShops_NewShop_IsOneUndoStep`; `SetShops_SameShops_RecordsNothing`; `ShopsRemovedByUndo_AfterAdding_ListsIt`.
- `CombinedHistoryShopTests` (new, as `CombinedHistoryQuestTests`): `Undo_ShopAddedThenNamedByEvent_IsRefusedNamingMap`; `Undo_ShopNobodyOpens_Undoes`.

## Verify by hand
None: no window changes shops yet (SH3). Ctrl+S in the editor after any edit rewrites `shops.json` in the relaid layout only once (the relay commit), then leaves it unchanged.
