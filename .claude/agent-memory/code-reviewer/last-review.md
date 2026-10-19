---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, G4-2 (Status bars)
Round: 1 (verdict: Ready to commit; build 0 warnings, 962/962 tests, format exit 0)

- open (Warning): StatusPage.ShowMember XP fill (experience / ExperienceToReachNext, full at max level) untested; dropping the IsMaxLevel guard throws on a max-level member. Extract pure static like ToNextLevelText + 2 tests
- open (Suggestion): StatBar.Draw MathF.Round makes a living member with small HP/max show an empty bar; use ceiling when Fraction > 0
- open (Suggestion): _vitalBars[0]/[1] tied to stat order by comment only; name two fields or derive from sheet
- open (Suggestion): tick G4-2 in plan with deviations (square corners, 6 px HP/MP, HP/MP bars not in legacy StatusView)
- follow-up: DialogueBox name strip bakes its own gradient (DialogueBox.cs:110); could use UiTextures.Gradient
