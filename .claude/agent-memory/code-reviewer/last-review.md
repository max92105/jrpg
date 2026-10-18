---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 3b (event content loading)
Round: 1 (verdict: Fix the Criticals, then commit)

Accepted choices (don't re-raise): swap-in-cell-event checked in the cell path (no flag arg); only null *entries* checked (strict reading rejects null lists); converter needs content/items.json (tests copy it); MapFolderReader.Read takes 4 params instead of MapReferences; MapReferences carries whole layouts, not just sizes.

- open (Critical): MapFolderReader.ValidateMap reads the settings file and builds the MapDefinition; name hides I/O + construction; rename e.g. ReadValidatedMap / CompleteMap
- open (Suggestion): converter ReadItemIds with no items.json -> uncaught FileNotFoundException stack trace (Program.cs only catches ConversionException/ContentValidationException)
- open (Suggestion): ContentFileWriterTests round-trip never asserts Speaker survives (only Lines); also combined "steps or dialogue lines" null message
- open (Suggestion): slice note: test names differ from plan (FindProblem_..._Names... vs MapEventValidator_Validate_..._Throws...), MapReferences holds layouts not sizes; tick 3b
- follow-up: two cell events on the same cell are not rejected (which one fires in 3d?)
- follow-up: ContentFileReader's NotSupportedException catch also relabels programmer errors (Core type STJ can't build) as content errors
