---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B2b.md, B2b (battlers: BattlerArt, BattlerScaling, BattlersView, UiTextures.Battler)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1167 tests)

- open (Critical): UiTextures.ShrunkOnce has 5 parameters (max 4); fix by one `_shrunk` cache keyed (Folder, File, Size) replacing `_icons` + `_battlers`, so ShrunkOnce(textures, folder, file, size) is an instance method.
- open (Suggestion): BattlerScaling's 512 px boundary (`>`) is untested; a 512 px case pinning point-scale to 256 would fix the edge.
- open (Suggestion): ArtReferenceCheck party-member battle sprite line has no test of its own (only the enemy one).
