---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, G3 (Menu header: location and play time)
Round: 1 (verdict: Ready to commit; build 0 warnings, format exit 0; full test run hit the known flaky Game host crash, Game-only rerun 168/168)

- open (Suggestion): tick G3 in the plan and record deviations (no header icons, test names, extra PlayTimeText tests, seeded session in GameMenuSceneTests, menu_title message)
- open (Suggestion): header title rule (menu_title on tabs, tab label inside a page) has no test; expose the title or pin it if a page adds its own title later
- open (Suggestion): MenuHeader.Draw `_measuredLocation is not null` guard only covers Draw-before-Update; measure once in the constructor (call Update) and drop the guard
- follow-up: FontLibraryTests non-font input still crashes the Game test host intermittently (guard header size before AddFont)
