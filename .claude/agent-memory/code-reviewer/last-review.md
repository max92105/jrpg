---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL9d (the Rules group on the Classes page's Resources tab; decision
1017: per-box id choices `StrainBoxes.LowersStartOfChoices` / `DominanceBoxes.OverChoices` via `ResourceIdChoices.Of`).
2026-11-17. Round 2. Verdict: Ready to commit.

- Critical (editable id combos blanking a rule's id on a class switch or a rename): fixed. Re-probed (scratchpad
  cl9dprobe, cl9dtyping): repros keep every id, keystroke typing and backspace keep text and caret, 0 trace lines.
- Suggestion (tautological EffectChoices test): fixed, now pins the three values.
