---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG2 (Map settings' Battle background box).
2026-11-16. Round 1. Verdict: Ready to commit (no Critical).

- Warning (open): src/Jrpg.Editor/Shell/MainViewModel.MapSettings.cs:27 — the edited summary line still says "Each is
  one undo step"; ToSettings goes out as one SetMapSettingsEdit (resize apart), so one Ctrl+Z reverts name and
  background together. Fix: "then the other fields together as one undo step"; same for the partial's class summary
  (line 10-11, "each one undo step").
- Dropped (in backlog.md:207): ArtFolders.ImagesIn throws when art/backgrounds is missing.
- Dropped from BG1: Suggestion on ArtReferenceCheck.Backgrounds.cs `!` after Where (a Suggestion, not carried).
- Checked, no finding: the box sits outside the random-battles panel (a town may name one, decision 909); the
  MapSettingsViewModelTests fixture (random battles, no background) now has CanApply false, and no test there asserts
  Problem null or CanApply; test-world maps with random battles name a background (BG1), so shell apply tests hold.
