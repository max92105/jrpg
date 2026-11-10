# 0042. Renaming map ids before the first release

Status: accepted
Date: 2026-11-10

## Context
A map id names its two files (ADR 0011), every warp step that leads to the map, the new game's start and, in a save,
where the party stands and which of the map's events are done and objects swapped (ADR 0022). ADR 0011 says map ids
are never renamed, and the editor's New map fixes the id once typed (question 70). The developer renamed several maps'
display names while laying them out and wants the ids to follow, so the map list's "Name (id)" stops contradicting
itself. Plan `2026-11-10-map-fixes-banner-and-folders`, decisions 530 and 531.

## Options
### A. Never rename (ADR 0011 as written)
- Pros: nothing to write, saves always find their map.
- Cons: the ids drift from the names for good; the developer hates it.

### B. Rename pending until Ctrl+S, undoable until then
- Pros: one more undoable edit.
- Cons: a rename spans every open map's journal and a file move at save time; the sheet purge (decision 442) already
  set the pattern for a change across maps and files, written at once.

### C. Rename with a rename table the saves follow, now
- Pros: a save made before the rename keeps its place.
- Cons: a content file and a repair step for the developer's own test saves, the only saves that exist before the
  first release.

### D. Rename at once with every content reference, saves repaired as for a removed map (chosen)
- Pros: one checked write through Data, all or none; the editor carries on from the written content as after the
  purge; the repair in place (`SaveGameRepair`, `WorldStateRepair`) already handles a map that is gone.
- Cons: a save made on the renamed map restarts the party at the new game's start and forgets that map's done events
  and swaps, each noted; the undo history of every open map clears.

## Decision
Until the first release, Map > Rename map... renames a map id at once (decision 530): Data rewrites the map's two
files under the new id, every map whose warps lead to it and the new game's start when it starts there, in one write
after the loader's checks, then deletes the old files; the editor refuses the rename while anything is unsaved, and
afterwards shows the map under its new id with every undo history starting afresh. A save naming the old id is
repaired as for a removed map (decision 531): no save format change, no migration. From the first release on, a rename
needs a rename table the repair follows, a later ADR.

## Consequences
- ADR 0011's "map ids are never renamed" holds only from the first release on.
- Repository tests that pin a map id break when that map is renamed in `content/`; the commit that renames re-pins them.
- `content/new-game.json` is written by the writer for the first time when the start map is renamed, so the file is
  laid out as the writer lays it out.
- A crash between the writes and the deletes leaves both maps on disk, both valid; the developer deletes the old pair.
