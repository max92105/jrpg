---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice R1 (tests only: Builders/TestStatuses fixture, 17 Core test files
converted, 1355 tests before and after). 2026-11-15. Round 1. Verdict: Ready to commit.

- Suggestion (open, optional): the event-match predicates (`events.OfType<StatusApplied>()` + tuple ==, TurnSkipped
  FindIndex) are repeated 6-7 times; a fixture reader would absorb SW's Cause->Status rename in one place.
- Follow-up (MS): EffectScalingTests.ScaledFor_GrantStatus_FollowsTheStatusPercentNotTheDamageOne compares Effect
  whole; once Statuses is a list, record equality compares it by reference.
- Checked: every converted event tuple covers all the record's fields (StatusApplied 3, StatusTicked 3, StatusEnded 2,
  StatusResisted 2, TurnSkipped 2); Combatant is a class without Equals, so tuple == matches record equality;
  Assert.Single+tuple equals the old one-element list compare; Assert.Collection keeps the length check; no
  StatusKind/new StatusGrant( outside the fixture.
- SR1 suggestions (optional) not carried: different slice, optional.
