---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice NG2 first part (gold, start, sprite; lists split to NG2b). 2026-11-17.
Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: TypedDecimal.Text prints exponent form below 1e-4 / from 1e15 ("1E-05"), which Parse refuses
  (no AllowExponent): add AllowExponent to Style or pin Parse(Text(x)) == x.
- Suggestion, open: the culture tests only bite on a comma-locale machine (this one is fr-CA); on en-US they pass
  without InvariantCulture. Optional: a test formatting/parsing under a scoped fr-CA culture.
- Follow-up, open: Cinematics/CinematicStillRow.cs:192 is a third invariant double reader (own style: exponent,
  no sign); consider TypedDecimal with a style when it is next touched.
- NG1 items: CheckItems summary fixed (now names the whole new game); RenameMap and NewGameReferences dropped
  (backlog.md lists both); NG1 suggestions not carried.
