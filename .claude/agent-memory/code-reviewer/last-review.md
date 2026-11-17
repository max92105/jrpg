---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG3 (animated background record: Core definition, loader,
validator, writer, empty content file).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Fixed (from BG2): MainViewModel.MapSettings.cs summaries now say "together as one undo step".
- Suggestion (open): FrameOrigin's row term `frame / columns * FrameHeight` can overflow for a huge FrameHeight the
  validator lets through; harmless once BG4's art check bounds rows by the image height. Not carried as a finding.
- Checked, no finding: OfAnimatedBackgrounds has no src caller yet but BG7 uses it (slice planned); the validator's
  null-animation branch is reachable from an editor record (comment states why); no `new ContentCatalog {}` site drops
  the new property (only `with`); content file bytes `[]\r\n` match the writer; repository bytes test includes it.
