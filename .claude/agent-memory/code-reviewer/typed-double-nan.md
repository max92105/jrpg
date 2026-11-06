---
name: typed-double-nan
description: editor fields parsed with double.TryParse accept "NaN"/"Infinity" whatever the NumberStyles; JSON save then throws
metadata:
  type: feedback
---

`double.TryParse(text, NumberStyles.AllowDecimalPoint..., Invariant)` still returns true for "NaN", "nan", "Infinity",
"-Infinity" (verified with fsi, .NET 10). `(int)Math.Floor(NaN)` is 0 (saturating cast), so a cell check like
`Layout.Contains(GridCell.Containing(p))` passes. System.Text.Json refuses to write NaN/Infinity (ArgumentException),
which the editor's save doesn't catch. Seen in PO1 (PlacedObjectEditing X/Y) and EventStepViewModel's seconds.

**Why:** a typed field that takes a symbol the writer can't serialize crashes the save with unsaved work.
**How to apply:** for every new typed double in Editor, check for `double.IsFinite` (or a `> 0` that also bounds above).
