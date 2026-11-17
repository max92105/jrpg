---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG8 (first part, split; BG8b holds the page, FramePreview Stretch,
menu wiring). Backgrounds/BattleBackgroundEditingViewModel, CatalogDraft.SetAnimatedBackgrounds,
BattleBackgroundCatalogViewModel.RefreshAfterStillImport.
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): no test imports a PNG over a missing-image record's file (Gone.png): pin that its record drops
  and the row moves ahead of the missing rows.
- Follow-up (open): BG9.md still plans a mutating BattleBackgroundViewModel.TakeAnimation; BG8 chose a fresh row
  built by the list, so BG9's TakeImportedAnimation should build `From(file, record, size)` and share TakeStillImage's
  replace-and-insert step.
- BG7c's two suggestions: fixed (Assert.NotNull added; ArtImageRowProblem `chosen` doc names the Character sprites case).
