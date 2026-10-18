---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 3c (event conversion; Back maps dropped by developer)
Round: 1 (verdict: Fix the Criticals, then commit)

Accepted choices (don't re-raise): Back maps dropped (no BackMapConversion, back_* settings, Back tests); Dialogue triggers converted though no live map has one (ADR 0005 editing); Battle skipped+reported, Item trigger and old "event" field are errors; event ids keep legacy index when an earlier one is skipped; two converted triggers on one cell = error; empty prompt -> "Check"; MapConversionContext in root namespace; test names ConvertTileTriggers_/ConvertObjectEvents_ instead of plan's EventConversion_Convert_ (developer to note).

- open (Critical): dotnet format --verify-no-changes fails (exit 2, ENDOFLINE): EventConversion.cs and EventConversionTests.cs are LF-only; run dotnet format Jrpg.slnx
- open (Warning): GridCell.Center (computed) is written into generated content for every cell event and warp cell; ignored on read, stale if hand-edited; skip read-only props in ContentJsonOptions modifier
- open (Suggestion): EventConversion.NewObjectId is a noun; ToNewObjectId / FindNewObjectId
- open (Suggestion): LegacyMapEventJson.Activation binds legacy JSON to Core EventActivation member names, unlike the Legacy*Type enums for actions/triggers
- open (Suggestion): object-action Portrait mapping untested (only trigger PortraitPath is)
- follow-up: two id-0 tiles with triggers on the same cell/z would report "triggers on two layers" (wrong wording)
