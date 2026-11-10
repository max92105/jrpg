---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice DO2 (canopies keep their object's layer, decisions 521, 522),
2026-11-10. Round 1. Verdict: ready to commit.

- Open (Warning): DrawOrderKey.cs:53-58 the From tiebreak also reorders a BODY on Z 5 (AbovePlayerZ, the editor's
  "Above player" preset) against canopies from lower layers: before by base, now always after them. Consistent with
  ADR 0041's wording but design.md says "bodies are unchanged"; record in the Done note + one Fill test, or confirm.
- Open (Suggestion): one concept, three words: `Layers.From`, local `byOrigin`, test name/comment "owner"; pick one.
- DO1 items: all fixed (static `_byKey` Comparison, Fill/FillWithPlayer param docs, `SpriteKeyOf_FartherUpTheScreen`,
  `Drawables` gone).
