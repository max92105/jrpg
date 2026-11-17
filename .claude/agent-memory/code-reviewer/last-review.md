---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL9 (first part of four: `Party/ResourceRuleBoxes<TRule>`,
`WillScalingBoxes`, `BackfireBoxes`, `ResourceRuleTests`; CL9b-d follow). 2026-11-17. Round 1. Verdict: Ready to commit.
No Critical, no Warning. Abstract base judged acceptable (is-a, shared IsOn/Problem/ToRule/Changed, one level under
ObservableObject, four planned users). Blanks checked against `ResourceDefinitionValidator` (1..100, will >= 1).

- Suggestion: base's `Changed` relays every notice but `Problem` (a deny list) while siblings relay typed fields by
  `partial void OnXChanged` (an allow list); a subclass adding a computed notice would relay it — say so in the base's
  doc comment or relay by allow list — open (optional)
- Suggestion: constructors repeat `(rule ?? _blank)` per box; one local `shown` reads once — open (optional)
- (CL8's color-rule Suggestion: now in backlog.md line 362, dropped.)
