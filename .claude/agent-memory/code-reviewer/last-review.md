---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-enemy-skills-and-bosses, slice T3 (heal aims at the most wounded ally its condition found; Field Dressing). Round 1.
Verdict: Ready to commit.

- Open (suggestion): RepositoryEncounterTests pins the Chief's cleave but not field_dressing
- Open (suggestion): docs/map.md's EnemyTargeting entry omits the heal's rule (decision 401)
- Open (suggestion): T3 Done note says "as planned" but ForSkill takes the TacticCondition, not the EnemyAction
- Fixed (from T2): enemy-side half of an ally is pinned by the AllyHpBelowOnlyAMemberHurt and OnlyAMemberFallen tests
- Fixed (from T2): TacticConditions.MeetingIt renamed CombatantsMeeting
