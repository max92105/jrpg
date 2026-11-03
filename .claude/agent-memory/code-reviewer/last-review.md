---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO1 (ObjectCanvas; TileIdsOf takes any cells). Round 1.
Verdict: Ready to commit.

- open (Suggestion): ObjectCanvas.Of(SheetCellRectangle) and Of(ObjectDefinition) don't name the
  ArgumentOutOfRangeException a size over MaxCells throws, as Empty and Resize do
- open (Follow-up, CO5): a Shift+drag rectangle over 32 columns or rows makes ObjectCanvas.Of throw when New object
  from selection opens the composer; clamp, refuse, or say so in CO5
- fixed (carried from O1): CanReskin checks the marked cells' sheet (ObjectReskinning.cs:34)
- fixed (carried from O1): ObjectReskin shares ObjectFromCells.TilesOf; one CatalogDraft.TileIdsOf call
- fixed (carried from O1): Reskin_PlacedObjectWithEvents asserts NotEmpty events and compares them
