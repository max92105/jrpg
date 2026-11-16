---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice SW (a grant names a status record; StatusKind deleted).
2026-11-15. Round 1. Verdict: Ready to commit.

- Fixed (from CH): EffectLanding comment above `Grant` now says every grant rolls.
- Warning, open: StatusDefinition.cs `Strengthened` doc says "rounded down as every percent in battle is", but
  StatFormula.Scaled truncates toward zero (GrantScalingTests pins -15 at 150 % = -22; ADR 0026 floors to -23).
- Suggestion, open: EffectValidator "listed twice" compares Status.Id, so guard_1 + guard_2 in one effect pass and
  the second refreshes the first (decision 798 only bans the same status; FamilyKey would catch it).
- Suggestion, open: no loader test for an unknown status id in a grant ("no status 'x'") through the loader wiring.
- Checked: 13 grants equal design 3.2 and statuses.json amounts; Empowered == Scaled(PermilleOf(100+b)) for ints;
  StrongerPart Zip arg order right; no assertion weakened (only the IsHarmful theory removed, slice-sanctioned).
