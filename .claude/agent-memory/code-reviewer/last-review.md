---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 3b-2 (Canopies and roofs over the player)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): CanopyTextures bakes every canopy on first CanopyOf (device exists only at draw); TextureLibrary.Create owns baked textures; cellPixels passed from GameFlow (Rendering can't depend on Exploration); CanopyMask loops instead of Any.

- open (Suggestion): MapDrawList canopy key equals the body key when object Z >= AbovePlayerZ (5); List.Sort is unstable, so the canopy can sort before its body. Same pixels, so no visible change today; add a kind tiebreak or skip the canopy when CanopyZOf(z) == z
- open (Suggestion): CanopyTextures.CanopyOf takes a TextureLibrary on every call but only uses the first one; document it or bake from an explicit call in the scene's first Draw
- open (Suggestion): tick 3b-2 in the plan and record the deviations (CanopyTextures lazy bake, SheetPixels, TextureLibrary.Create, cellPixels parameter)
