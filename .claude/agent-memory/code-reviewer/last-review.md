---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF2 (MapTree, MapTreeNode, MapEntry.Folder, MapNavigation
ShownTree / AddSessionFolder / NoteExpanded), 2026-11-10. Round 1. Verdict: ready to commit.

- Open (Warning): MapTree.cs:52 the `!MapFolderPath.IsWithin(path, parent)` clause has no test; deleting it lists
  "Act 2/Keep" (or "Act 10/Keep") as "Keep" under "Act 1". Add a test with sibling folders "Act 1" and "Act 10/Keep".
- Open (Warning): the filter predicate (`Text.Contains(filter, OrdinalIgnoreCase)`) now lives twice, MapTree.cs:21 and
  MapNavigation.cs:114; one rule (e.g. `MapEntry.Matches(filter)`) for both.
- Open (Suggestion): MapTreeNode's ctor admits a node with both or neither of FolderPath/Map; two named factories.
- Open (Suggestion): a test for a session folder nested under a map's folder showing once.
- Follow-up: a session folder stays after a map moves in and out again (shown empty); confirm vs decision 534 in the
  move slice.
- MF1 item: MapFolderPath Rule message now names the backslash (fixed).
- Checked fine: FolderRightUnder for "Act 10" beside "Act 1", shorter path, top level; session/map folder dedupe.
- start.json residue: the developer's hand test, excluded by the caller.
