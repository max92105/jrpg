---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice R1 (Shell: the folder rename's write extracted; the composer's
ruled-out states thrown), 2026-11-12. Round 1. Verdict: ready to commit.

- Suggestion (open): `StartObjectFromMap` summary "so no shown map is a state this reaches" reads both ways; say
  "so it is reached with no map shown, and returns null then".
- Suggestion (open): `ErasedOn` merged its two `Where`s into one `&&`; the slice only asked to name the canvas cell.
- Verified: TryRenameFolder effects order kept (MovedMaps pure, so computing it after `SaveError = null` is
  harmless); KeptTile/IsKept/Moved build the same values; the `!` reads NRE before `map.Do` (catalog step already
  applied, as before); all three TryWriteMovedMaps branches tested (empty session folder, two maps, write refused).
- D2: no findings, nothing carried.
