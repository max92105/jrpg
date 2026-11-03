---
name: int-overflow-bounds
description: bound checks written as start + count > limit overflow on a typed int (no CheckForOverflowUnderflow); ask for count > limit - start
metadata:
  type: feedback
---

Bound checks like `column + frameCount > columns` wrap negative for a typed or hand-edited int near int.MaxValue
(the build has no CheckForOverflowUnderflow), so the check passes and the value reaches Enumerable.Range or drawing.
Seen in AE1's TileAnimationViewModel.ProblemOf and AN1's ArtReferenceCheck.LastFrameColumn.

**Why:** the editor must not write content the game rejects or crashes on; int.TryParse accepts up to int.MaxValue.
**How to apply:** for each new editor Problem or Data check adding a parsed count to a position, ask for
`count > limit - start` and a theory row with int.MaxValue.
