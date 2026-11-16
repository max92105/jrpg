---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice MS (Effect.Statuses list, decision 809). 2026-11-15. Round 1.
Verdict: Ready to commit. No findings.

- Checked: LandOn's else-if keeps the old guard (standing, landed) with the loop inside; SkillEmpowerment's GrantStatus
  branch with an empty list is unreachable (validator refuses it); `"statuses": null` refused by
  RespectNullableAnnotations, null elements by FindStatusesProblem; no `"status"` key left in content/; the miss test
  discriminates (goblin 40 HP survives a 10-power hit, so a landed hit would grant).
- R1 Follow-up (EffectScalingTests whole-Effect compare) answered by Effect.Equals comparing the list in order: fixed.
