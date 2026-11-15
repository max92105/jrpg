---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-cinematic-focus-point, slice FP1 (StillFocus record, CinematicStill.Focus and ZoomOrigin, validator
FindOptionalFieldProblem/FindFocusProblem, AbsentWhenNullFilter focus, ADR 0046 amendment). 2026-11-15. Round 1.
Verdict: Ready to commit.

- EW6 Suggestion (open-editor-windows.ps1 Test-StepListKeys one Alt+Down): dropped, other plan, never acted on.
- Suggestion, open: ContentCatalogLoaderCinematicTests.Load_FocusMissingYPercent_ThrowsNamingTheFile asserts only the
  "cinematics.json" prefix, so any error in the file passes it; assert the focus path or "yPercent" too.
- Noted, not raised: CinematicStillRow.From/ToStill drop Focus until FP3 (planned there; no content has a focus).
