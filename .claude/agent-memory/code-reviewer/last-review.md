---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice D1 (one `MapLayout.AllEvents`, `ContentJsonOptions.EnumName`,
`SheetCell.Of`), 2026-11-11. Round 1. Verdict: ready to commit.

- Verified: `MapLayout.AllEvents` (Core MapLayout.cs:27) is the same objects-then-cells concat as the three removed;
  every `EnumName` call gives `JsonNamingPolicy.CamelCase.ConvertName(value.ToString())`, as before.
- Verified: static init safe on both first-touch paths (Strict first; a converter first: same-thread recursive
  cctor, the converters' ctors don't read `_names`; `_enumNaming` set before `CreateStrict` runs).
- Suggestion (open): `_enumNaming` comment gives an uncertain reason (beforefieldinit converters need not init on
  `new`); the sure dependency is `new JsonStringEnumConverter(_enumNaming)` in CreateStrict; or make it an
  expression-bodied property so order stops mattering.
- Follow-up (open): Editor tests still spell `new SheetCell(tile.SheetId, tile.Column, tile.Row)`
  (MapDrawingTests:49, MainViewModelSaveTests:83, TileImagesAnimationTests:27).
- X1 items: SlotName warning fixed (deleted); "seven routed" suggestion fixed; X1 test-naming suggestion dropped
  with the commit; MapContent.With follow-up dropped (backlog line 209 records it).
