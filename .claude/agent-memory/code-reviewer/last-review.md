---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-effect-strips, slice SE3 (FireExplosion.png content for Fireball and the bomb; last slice).
2026-11-15. Round 1. Verdict: ready to commit. SE2's items rechecked in the tree: BattlePacing, EffectOnTarget and
ActionWindUp comments fixed; SE2's suggestions dropped (SE2 committed).

- Suggestion (open): Done note calls frame 4 "the widest burst"; measured, it has the most flame (16k opaque px vs
  14k on frame 5) but frames 5-7 span wider boxes; "fullest" fits.
- Suggestion (open): ContentCatalogLoaderEffectFileTests item test now leans on bomb_all staying strip-less.
- Follow-up (open): docs/map.md:71 ActionWindUp still "and an Attack's swing (decision 657) included"; docs/map.md:225
  "Projectiles and skills' own effects" still says skills' and items' own strips are unbuilt (SE1-SE3 built them).
