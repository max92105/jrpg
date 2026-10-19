---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, G4-1 (Status page, text only; G4-2 bars split off)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 957/957 tests, format exit 2)

- open (Critical): format fails, ENDOFLINE on every line of new tests/Jrpg.Game.Tests/Menus/StatusPageTests.cs; run dotnet format
- open (Warning): StatusPage.SheetOf experience-to-next (table value - member.Experience) and "Max" at max level have no test
- open (Warning): UiTextures.DiamondPortrait duplicates DialogueBox's portrait bake/cache (lines ~125-135); developer lists it as follow-up but plan note doesn't record it
- open (Suggestion): plan test names differ from shipped (StatusPage_* vs Members_/Update_*, Data FindProblems_*); tick G4-1 and note deviations (diamond portrait, left-aligned values, no skills/icons, DialogueBox cache)
- open (Suggestion): MemberSheet.Experiences / StatusLabels.Experiences name the level/EXP/next trio poorly; e.g. Progress / ProgressLabels
- follow-up: DialogueBox should use UiTextures.DiamondPortrait
