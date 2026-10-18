# 0011. Content after the converter

Status: accepted
Date: 2026-10-18

## Context
ADR 0005 made `Assets/` and the legacy MapEditor the source of truth for maps until a new editor exists, with `tools/Jrpg.ContentConverter` rewriting `content/generated/` from them. Part 3a ends that exception: slice 10 runs the converter one last time and deletes it, and the new editor (11a-11j) writes maps itself through Data's `ContentWriter`. Today a map is two files composed at load: the generated layout `content/generated/maps/{id}.json` (tile layers, placed objects, events) and the hand-written settings `content/maps/{id}.json` (display name, encounter table). Catalogs sit in `content/generated/` (`tiles.json`, `tilesheets.json`, `objects.json`); art is still in `Assets/` (ADR 0009 names its final home, `content/art/<kind>/`).

Two choices are open: how maps are stored once the editor owns them, and whether the cutover moves the art out of `Assets/` or copies it. The legacy demo reads `Assets/` directly, and part 3b's slices compare the new game against it at the same spot.

## Options
### Maps
#### A. Two files per map, both editor-written: `content/maps/{id}.json` (settings) and `content/map-layouts/{id}.json` (layout)
- Pros: the cutover is pure moves, and `MapFolderReader` keeps composing as today; settings stay small and readable, so encounter tuning is easy to review in a diff; a layout's large row grids don't bury setting changes.
- Cons: two files to keep in step (the loader already fails when one is missing); the editor writes two files per save.

#### B. One file per map
- Pros: one file per map, simplest to reason about; no orphan risk.
- Cons: the cutover must merge files and change the loader and its tests; every setting tweak diffs inside a file that's mostly row grids.

### Art
#### C. Move it (`git mv Assets/... content/art/...`)
- Pros: one copy of each image; `Assets/` holds only what's left for part 3c.
- Cons: the legacy demo can't run any more, so part 3b loses its side-by-side comparison.

#### D. Copy it, leaving `Assets/` untouched until part 3c removes it with the legacy code
- Pros: the legacy demo keeps running for comparison; git stores identical files once, so the repository doesn't grow.
- Cons: two copies on disk until 3c; an art edit in `Assets/` no longer reaches the game (it's frozen anyway after the cutover).

## Decision
**A** for maps and **D** for art (developer, 2026-10-18).

Rules that hold either way:
- `content/` is the only source of truth; ADR 0005 is superseded. The converter, its tests and `content/generated/` are deleted; catalogs move to the content root (`tiles.json`, `tilesheets.json`, `objects.json`).
- The editor writes content only through `ContentWriter`, which runs the loader's validators first, with the same JSON options the loader reads.
- Ids are never renumbered: map ids, object uids and event ids are save keys (plan key decision 8).
- New content paths get CRLF through `.gitattributes`, as generated content had, so editor saves diff cleanly.

## Consequences
- Easier: one place to change content, one tool (the new editor) to change maps.
- Harder: between slice 10 and 11e no tool paints maps (decision 72 accepts it as implementation time).
- With D, part 3c deletes `Assets/` together with the legacy code; until then nothing may read it except the legacy demo.
