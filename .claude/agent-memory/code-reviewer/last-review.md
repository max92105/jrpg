---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice DO1 (one draw order in Core, decision 520, moves only), 2026-11-10.
Round 1. Verdict: ready to commit.

- Open (Warning): MapDrawOrder.cs:17/25/41 `List.Sort(IComparer)` allocates a 64-byte Comparison delegate per call
  (probed with fsi), so FillWithPlayer allocates every update against its summary; fix: static readonly
  `Comparison<MapDrawItem>` + `into.Sort(_byKey)`. Pre-existing in the game's MapDrawList, carried by the move.
- Open (Suggestion): MapDrawOrder.cs:32-34 empty `<param>` tags (CS1573 filler); copy Fill's wording.
- Open (Suggestion): MapDrawOrderTests.cs:147 `SpriteKeyOf_LowerBase_SortsBefore` with variable `lower` sorting after;
  rename variables (`nearer`/`farther`) or the test (`..._SmallerBaseY_SortsBefore`).
- Open (Suggestion): MapDrawList.Drawables named after the deleted Drawable type; `Items` (scene local already `items`).
- SV2 items: not re-checked (different slice; the SV2 Warning was addressed in the SV2 commit message).
