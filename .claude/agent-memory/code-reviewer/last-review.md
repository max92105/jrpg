---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice AE1 (TileAnimationViewModel). Round 1.
Verdict: Fix the Criticals, then commit.

- open (Critical): ProblemOf `cell.Column + frames > columns` overflows (column 3, frames 2147483647 -> no problem);
  write `frames > columns - cell.Column`, add a row
- open (Critical): Close and IsOpen have no test; add Close_OpenPanel_IsNotOpen (IsOpen false, Cell null)
- open (Warning): panel stays open when its sheet leaves the catalog (Ctrl+Z of Set up, Remove sheet); Problem and
  PreviewCellAt then throw KeyNotFoundException; close on catalog change or record for AE2
- open (Suggestion): CanOpen's SheetChoices notify arm untested
- open (Follow-up): ArtReferenceCheck.LastFrameColumn overflows the same way for a hand-edited frameCount
- fixed (AI2): MinBy sheet pick now tries every sheet on the image for a matching cut; "1 tile" singular;
  TiledTileset.Columns removed
