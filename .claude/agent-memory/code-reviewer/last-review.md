---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice CH (every grant has a landing chance, decisions 798/801/802/819).
2026-11-15. Round 1. Verdict: Ready to commit.

- Warning, open: EffectLanding.cs:138 comment above `Grant` still says "A stun rolls ...; the other statuses land"
  (every grant rolls now).
- Checked: no assertion weakened beyond the slice (loader test unpinned by the slice; stun amount 100 -> 0 follows the
  move; refresh test kept on Exposed). Self-taunt test discriminates: FixedSequenceRandom(0) has one value for the
  shuffle and throws on a second draw, so cutting a taunt by resistance would fail it.
- Not raised (plan decision 799): SkillEmpowerment raises only Power, so a dominant force no longer raises a stun's
  chance; no status-only stun skill in content.
