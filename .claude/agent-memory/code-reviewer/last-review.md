---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice R3 (the art check's lines that do two things; the backlog closed), 2026-11-12.
Round 1. Verdict: fix the Critical, then commit.

- Critical (open): backlog.md:205 — the merged "Files at the limit" line replaced the unrelated `verify.ps1 -Fix`
  generator-hooks line (lost) instead of the map fixes' line, which still sits at :207 as a duplicate. Restore the
  -Fix line, delete :207. Done note's "the two MainWindow.xaml lines one" is false until then.
- Suggestion (open): the items' icons line (ArtReferenceCheck:56) is the only check left with an inline Select.
- Verified equal: all four split messages byte-identical; effect strip cases same (only a header >= 2^31, read
  negative by PngSize, now reports where it didn't); lazy locals enumerated in the same call order; 4 signatures
  remain over 130 columns; no comment over 120.
- R2's Warning and Suggestion: not carried (R2 committed).
