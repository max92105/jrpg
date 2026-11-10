---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice RN1 (MapIdRename + ContentWriter.RenameMap), 2026-11-10.
Round 1. Verdict: ready to commit.

- Open (Warning): MapIdRename.cs:24 backstop compares the new id case-sensitively; files are case-insensitive on
  Windows, so "forest" -> "Forest" (or "woods" -> "Forest" with forest saved) overwrites forest's files and the
  RenameMap deletes (ContentWriter.cs:111-112) then delete the just-written files. Fix: OrdinalIgnoreCase check
  + test Apply_NewIdTakenWithOtherCase_Throws.
- Open (Suggestion): MapIdRename.cs:59 `renamedSteps` counts events, used only as a flag; a captured bool
  `isAnyWarpRenamed` says it.
- Open (Suggestion): ContentWriterRenameTests.cs:100 NewIdTaken asserts forest's bytes; the file a wrong write would
  clobber is start's; assert start's layout bytes too.
- ML0 items all fixed (map.md Shell row, `_openMapId`, MapSwitching summary plain text).
- start.json residue: the developer's hand test, excluded by the caller.
