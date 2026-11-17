---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU9 (CatalogDraft.ItemsRemovedByUndo, CatalogUsage.UsersOfItem +
GivesItem, CombinedHistory.RefusalOfItemUndo and RefusalOfEventNamedUndo, SaveWithCellEvent moved to EditorTestContent).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): third copy of the "event step names id X" pattern: UsersOfShop/UsersOfCinematic/UsersOfItem in
  CatalogUsage and RefusalOfShop/Cinematic/ItemUndo in CombinedHistory; proposed a shared step predicate query
  (MapsWithStep(catalog, openLayouts, Func<EventStep,bool>)) and one refusal loop.
- Checked, no finding: refusal order kept (quest, shop, cinematic, then item, before objects/sheets/tiles); other item
  references (shops, drop tables) only via later linear steps; NewGame starting items, party members, rules have no
  editor setter; MapEvent has no item condition; cinematics name no item; test-world has potion, not test_tonic; no test
  reads the old quest_check/shop_check/cinematic_check ids (shop file's own unsaved test still uses shop_check inline).
- Dropped (IU8 optional suggestions, not carried): SpriteImageFile summary, ChosenKey raise, MissingIconNote height.
