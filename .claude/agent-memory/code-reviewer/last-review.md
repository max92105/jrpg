---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 1d (map content)
Round: 1 (verdict: Fix the Criticals, then commit)

- open (Critical): untested new throws - ObjectConversion.IdsByLegacyId duplicate legacy id, MapConversion placed-object Meta, MapConversion null object entry (only null tile tested), MapLayoutValidator empty uid
- open (Warning): two legacy map files giving the same id (Interior1.json + interior_1.json) -> second silently overwrites the first in generated/maps; no error
- open (Warning): MatchesCommittedGeneratedContent compares bytes; writer pins CRLF, index stores LF, so a checkout with LF working files (autocrlf=input, WSL/Linux) fails falsely - pin eol=crlf for content/generated in .gitattributes
- open (Suggestion): split combined messages ("null entry or an empty uid", "tile or object list holds a null entry")
- open (Suggestion): IdsByLegacyId temporal coupling ("call after Convert") and recomputes LegacyIds.FromName
- follow-up: MapFolderReader <-> ContentCatalogLoader mutual class reference (GeneratedFolder const)
