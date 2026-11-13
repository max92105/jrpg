---
name: database-window-copies
description: The per-catalog editor windows (quests, shops, enemies) are built by copying the Quests window; check for third copies
metadata:
  type: project
---

Each database window (ADR 0045: Quests, then Shops in SH2, Enemies later) is written by copying the Quests window's
types: the catalog's `Problem` loop ("X is listed twice" then "X #n: problem"), `*EditingViewModel` Start/Apply, and
CatalogUsage's map-name pipelines. Seen twice: SH1 (UsersOfShop, the third MapNamesWhere copy) and SH2
(ShopCatalogViewModel.Problem, the third copy of the duplicate-id loop).

**Why:** the growth rule makes a third copy a Warning; the enemies slices will add a fourth.

**How to apply:** on any enemy/database-window slice, grep `Take(index).Any` and `listed twice` in src/Jrpg.Editor
and the `*EditingViewModel` bodies; if a shared helper was extracted since, check the new window uses it.
