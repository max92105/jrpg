---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11a (Writing maps with validation)
Round: 1 (verdict: Fix the Criticals; caller said this is the single review round)

- open (Critical): ContentWriter.WriteMap skips NewGameValidator's startLocation check; shrinking start to 10x45 saves, then ContentCatalogLoader.Load fails "new-game.json: startLocation.position (10, 3) must be inside map 'start'"
- open (Suggestion): a failed second WriteAside leaves "{layout}.saving" behind in content/ (shows as untracked); delete the aside files on failure
- open (Suggestion): ContentReferencesOf duplicates the MapContentReferences construction in ContentCatalogLoader; share one factory
- open (Suggestion): WritesIdenticalBytes compares ReadAllText (hides BOM); compare File.ReadAllBytes per file; tick slice 11a in the plan with deviations
- follow-up: after WriteMap the editor's ContentCatalog is stale; a second save validates against the old map (11d must reload or WriteMap return the updated catalog)
