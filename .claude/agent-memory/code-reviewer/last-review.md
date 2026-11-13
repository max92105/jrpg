---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice SH1 (shops save from the editor: CatalogProblems.OfShops/CheckShops,
ContentWriter writes shops.json, CatalogDraft.Database SetShops/ShopsRemovedByUndo, CatalogUsage.UsersOfShop,
CombinedHistory.RefusalOfShopUndo). 2026-11-13. Round 1. Verdict: ready to commit.

- MV3 carried: its two Suggestions not carried (committed slice, optional); its Follow-up (deleting a placed NPC a
  moveNpc step names only caught on save) still open, not in backlog.
- Warning (open): ContentWriterShopTests.cs rewritten over an existing file; the two WriteMaps openShop tests (accepted /
  unknown shop refused through the writer's references, ContentWriter.cs:240) deleted, not in the Done note.
- Warning (open): ContentWriterShopTests.cs WriteMapCatalogs_NewShop_WritesItSorted asserts the repository's whole shop
  id list ["armory","general_store"] (G2); assert armory precedes general_store / the ids are ordinal-sorted.
- Warning (open): CatalogUsage.cs:86-94 UsersOfShop is the third copy of the LayoutsOf.Where.Select(Id).Order.Select
  ("map {id}") pipeline (UsersOfObject, UsersOfQuest); extract MapNamesWhere(catalog, openLayouts, predicate).
- Suggestion (open): CatalogProblemsTests.cs WithGeneralStore helper sits between [Fact]s; move it to the class end.
- Suggestion (open): no test of UsersOfShop counting an unsaved open layout (refusal test saves first).
