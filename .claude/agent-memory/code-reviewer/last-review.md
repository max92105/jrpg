---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-enemy-skills-and-bosses, slice T2 (conditions on enemy actions). Round 1.
Verdict: Ready to commit.

- Open (warning): no test pins the enemy-side half of an ally (Side == enemy.Side in TacticConditions.AlliesOf); a hurt or fallen party member beside a healthy ally
- Open (suggestion): TacticConditions.MeetingIt reads vaguely; CombatantsMeeting (T3 is its next caller)
- Fixed (from C1): RepositoryEncounterTests pins the Chief's last slot with EnemyIds[^1]
- Fixed (follow-up from T1): EnemyTacticsTests builds its tactics through the Tactics helper
