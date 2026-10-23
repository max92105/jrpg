---
name: content-quirks
description: Non-obvious facts of the repository's art (battlers, field sprites, icons, skill icons) that rendering code must handle
metadata:
  type: project
---

Checked 2026-10-20 against the files in content/art:
- Party `battlers/*.png` are 1024 px painted portraits (not sprites); the `Goblin.png` battler is ~256 px pixel art.
- Battle art is matched by exact name, case included (`ArtReferenceCheck`); Windows would hide a case mismatch.
- `player.png` rows are Down, Left, Right, Up (the `Facing` enum's order differs).
- Skills heal/recovery/heal_all have an empty `iconFile`.

**Why:** rendering must scale battlers by size, not assume one; art names must match the file's case exactly.

**How to apply:** check these before designing rendering or art lookups. See [[migration-roadmap]].
