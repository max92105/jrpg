---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 1c (object content)
Round: 2 (verdict: Ready to commit)

- fixed (Critical): ObjectConversion.RejectUnsupportedFields also rejected an object with no tiles - check moved into TileGroupSize
- fixed (Warning): null entry in a legacy object's Tiles list -> NRE - now a ConversionException naming the object, tested
- fixed (Warning): IsNpc/Meta shared one message, Meta untested - split messages, both tested
- fixed (Suggestion): empty `<param name="name">` in LegacyIds.FromName
- fixed (Suggestion): PointStringConverter moved to tool root (slice note records it)
- fixed (Suggestion): Load_RepositoryContent objects test now pins count, tree size, polygon count and a point
- follow-up: committed content/generated vs fresh conversion (still 1d's end-to-end test)
