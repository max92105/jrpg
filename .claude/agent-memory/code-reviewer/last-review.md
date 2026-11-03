---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO4a (ObjectComposing). Round 1.
Verdict: Ready to commit.

- open (Warning): StartEdit throws ArgumentOutOfRangeException (ObjectCanvas.Of) for an object over 32 cells either
  way (Replace cells / New object from selection accept any rectangle) while CanEdit says true; cap CanEdit + test
- open (Warning): TryApply on an unchanged edit chooses `definition`, a fresh record not in the browser's Objects
  (SetObject no-op); choose _catalog.Current.Objects[definition.Id] + Assert.Same test
- open (Suggestion): StartNew's column clip (Math.Min on Columns) untested; only rows are pinned
- fixed (carried from CO3): OnPartChanged arms tested, edit-own-id test, Title tested
- fixed (carried from CO1/CO3 follow-up): StartNew clips a rectangle over 32 to its top-left 32
