---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice X1 (dead code listing; `ContentWriter.WriteMap` deleted, 20 test calls
moved onto `WriteMaps(MapContent.With(...))`), 2026-11-11. Round 1. Verdict: ready to commit.

- Verified: `WriteMaps(With(catalog, map), [id])` runs the same validators in the same order as the deleted
  `WriteMap` (layouts with the map in place, its settings with the same encounter ids, new game over the replaced
  maps), writes the same two files; the 20 rewrites change only the call (plus hoisted locals).
- Verified: an independent perl scan of public/internal methods in Data and Editor found none without an outside
  caller (matches the note).
- Warning (open): X1 Done note keeps `UnreadableSaveException.SlotName` saying "each has a reader inside it" and
  "for the game's message"; no src reader exists (only FileSaveStoreFormatTests:52), so the slice's fate rules call
  for delete or a backlog line, or an honest "kept: public exception API" deviation.
- Suggestion (open): Done note says "seven routed event fields" but lists eight (8+8+8+1 = 25).
- Suggestion (open): ContentWriterQuestTests hoisted local named `gated` twice and `edited` once; blank lines vary.
- Follow-up (open): ArtReferenceCheckTests and AudioReferenceCheckTests (Data.Tests) still spell out the
  map-replace expression `MapContent.With` now holds.
