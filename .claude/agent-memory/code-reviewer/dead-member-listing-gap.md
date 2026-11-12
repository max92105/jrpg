---
name: dead-member-listing-gap
description: X1/GX1 dead-code listings grep bare names; a name shared with another member hides test-only or dead members; type-scoped recheck recipe
metadata:
  type: reference
---

The plans' dead-code method (X1, GX1) greps each `public`/`internal` member as `\bName\b` over `src/`, own file
dropped. A name another type also declares (SelectionPosition, Notice, Category, Entries, Selected, Height...) gets
counted as called through the other type's readers, so its test-only seams or dead copies never reach the listing.
Seen GX1 (2026-11-12): 25+ test seams missed, none dead.

Recheck recipe (bash, ~6 min over the Game): build `file name` pairs from the slice's declaration regex, keep names
declared in 2+ files minus interface members (IMenuPage/IMemberPage/ISaveFilesPage/IBattleListChoice/IScene names)
and overrides; for each, look for `\bName\b` only in files that also name the declaring type (basename of the file);
hand-check the hits flagged (partial files like `UiTheme.Loot.cs` and property-chain access `x.Audio.Effects.Clear`
are false positives). For MessageIds/UiArtFiles/UiSoundFiles, `comm` the declared constants against
`MessageIds\.X` occurrences in non-MessageIds files: one pass, seconds. Precompute lists; per-name greps over the
tree time out at 120 s.
