---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-game-code-health-cleanup, slice GT2 (CommandMenuTests split into CommandMenuSkillTests,
CommandMenuMoveTests over a static CommandMenuDriver, decision 556), 2026-11-12. Round 1. Verdict: ready to commit.
No Critical, no Warning.

- Verified by script: every removed line of CommandMenuTests, after the three mechanical rewrites, is in the new files,
  except the old helper signatures and the rewrapped class comment; Facts 12 + 8 + 3 = 23 as at HEAD.
- Suggestion (open): CommandMenuDriver.cs:7 class comment says "never by a count of presses" while its own Press(times)
  is a counted press; reword to describe both methods.
- Follow-up (open, not in backlog): fixed-count presses survive in moved/kept tests (OpenItems Down x2,
  RecoveryRow/HealAllRow) against .claude/rules/tests.md:16.
