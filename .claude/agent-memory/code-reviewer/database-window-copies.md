---
name: database-window-copies
description: The per-catalog editor windows (quests, shops, enemies) and the art imports are built by copying older types; check for third copies
metadata:
  type: project
---

Each database window (ADR 0045: Quests, then Shops in SH2, Enemies later) is written by copying the Quests window's
types: the catalog's `Problem` loop ("X is listed twice" then "X #n: problem"), `*EditingViewModel` Start/Apply, and
CatalogUsage's map-name pipelines. Seen twice: SH1 (UsersOfShop, the third MapNamesWhere copy) and SH2
(ShopCatalogViewModel.Problem, the third copy of the duplicate-id loop). The image-import loop too: Tiles/SheetImport,
Sprites/FieldSpriteImport, then Battle/BattleArtImport in EN6 (third copy; backlog line records the fold-up).

**Why:** the growth rule makes a third copy a Warning; each new window or art box adds another.

**How to apply:** on any enemy/database-window slice, grep `Take(index).Any` and `listed twice` in src/Jrpg.Editor
and the `*EditingViewModel` bodies; on an art-import slice grep `HasArtImage(` and `SheetImportResult(`; if a shared
type was extracted since, check the new code uses it.
