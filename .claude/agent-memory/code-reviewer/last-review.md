---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M4b (Speaker and portrait)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, format exit 0; full run 899/900 with ExplorationSceneTests flake, second full run and Game-only rerun 115/115 clean)

- open (Critical): DiamondCrop.Bake block side = side/size integer; a portrait smaller than 200 px (PortraitSize) gives a 0x0 block -> DivideByZeroException in Average during Draw; use per-pixel bounds [x*side/size, max((x+1)*side/size, start+1)) and add an upscale test
- open (Suggestion): record in M4b slice note: GradientStrip pulled from G1, new DiamondCrop type, extra tests
- open (Suggestion): name strip is a fixed 520 px; a long speaker name in spaced caps runs past its fade
- follow-up: content/art/portraits/fighter.png is a JPEG with a .png name; enemies.json names "goblin.png" but the file is "Goblin.png" (case only works on Windows)
