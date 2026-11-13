---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice SH2 (Shops window's headless rules: Editor/Trade ShopViewModel,
PricedItemChoice, ShopCatalogViewModel, ShopEditingViewModel). 2026-11-13. Round 1. Verdict: ready to commit.

- SH1 carried: its three Warnings fixed (WriteMaps openShop tests back, ordinal-order assert, MapNamesWhere
  extracted); its Suggestions not carried (committed slice); MV3's moveNpc Follow-up now in backlog, dropped.
- Warning (open): ShopCatalogViewModel.cs:57-69 copies QuestCatalogViewModel.Problem's "listed twice / own problem by
  id or #position" loop (third copy with QuestViewModel.FirstStageProblem; Enemies window will be the fourth);
  extract a shared list-problem function.
- Suggestion (open): two test names claim what they don't assert (RemoveShop_NewShop_RemovesItAndSelectsAnother,
  AddItem_AfterAdding_GreysUntilAnotherIsPicked).
- Suggestion (open): OfferedItems has no ThenBy(id) for same-named items.
- Suggestion (open): RemoveShop's unsubscription untested.
- Follow-up: Events/ListOrder summary says "the event dialog's lists", now used by Quests and Trade too.
