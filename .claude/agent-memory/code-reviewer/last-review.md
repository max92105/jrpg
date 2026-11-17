---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL8 (the Classes page's Resources tab: `Party/ClassResourcesView`,
`Party/HexColorBrushConverter`). 2026-11-17. Round 1. Verdict: Ready to commit.
No Critical, no Warning. Runtime probe (scratchpad cl8probe, real page + theme + repository content): no binding or
resource trace line; the page's NumberBox reaches all 11 number boxes through DynamicResource; greying, swatch and
Add/Remove-all behave.

- Suggestion: the converter's "#" + six hex digits rule repeats Data's private
  `ResourceDefinitionValidator.FindColorProblem`; one shared rule would keep the swatch and the line in step — open (optional)
- (CL7d's Follow-up on the move greying copy: now in backlog.md line 359, dropped.)
