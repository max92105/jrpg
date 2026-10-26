---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/B1c.md, slice B1c (shards fly apart: Shard flight, EncounterShatter, ShatterView.PlaceShards, GameFlow black under shards)
Round: 1 (verdict: Ready to commit)

- W1 EncounterShatter.Length 0.5 s copies GameFlow._fadeOut; pass the fade-out length into EncounterShatter's constructor; open
- W2 ShatterView.PlaceShards recomputes Normalize(center - _impact), which is Shard.Direction; use shard.Direction * CrackWidth, drop _impact field and BuildVertices' impact parameter; open
- S1 Shard's empty <param name="A/B/C"></param> tags; drop or describe "a corner"; open
- S2 Speed 300-900 and spin +-1.5 bounds untested; optional InRange test over the shards; open
