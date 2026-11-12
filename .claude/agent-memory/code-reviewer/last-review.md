---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice FS4 (ObjectScreenArea.Of; MapView.DrawObject/DrawCanopy read it;
PromptPlacement.BoxFor(Rectangle, Point); InteractionPromptView takes the sprites).
2026-11-12. Round 1. Verdict: ready to commit (no Critical, no Warning).

- Suggestion (open): ObjectScreenAreaTests Of_ObjectBetweenCells_RoundsTheTopLeftOnce: 2.5*48-10 and 1.25*48 are whole, nothing rounds; use a fractional origin (e.g. 10.4) or rename.
- FS3 Suggestion MapView `var area = cells;`: fixed (one name, area).
- FS3 Suggestion FieldSpriteLayout leftovers: open, FS5 deletes the type.
- FS3 Suggestion AreaOver odd width: open (not this diff).
- Follow-up (open): ObjectDefinition.cs:31-32 "laid out like the player's" stale since ADR 0043 (still there).
