# 0037. Tests run on a frozen test world

Status: accepted
Date: 2026-11-03

## Context
Repository content changes in the editor every day, and the Mana Seed reset (plan 2026-11-03-mana-seed-reset) rebuilds
every map. Yet about 125 test files pin the start map's cells (the forest warp, the shopkeeper, the door, the chest), its
tiles, objects and tilesheet images, mostly through the shared `StartMapScenarios`, `EditorTestContent` and
`CopiedContentRoot`. Rebuilding a map would break them all, and every map sits half-erased during the rebuild.

## Options
### A. Re-pin the tests against each rebuilt map
- Pros: no new files; the tests follow the real maps.
- Cons: about 15 coordinates per map in `StartMapScenarios` re-pinned at every move, and broken for the whole rebuild,
  while the maps are half-erased.

### B. A test world built in code
- Pros: no frozen JSON to maintain; each test states what it needs.
- Cons: the start map's 20-odd events and the intro's nine cutscenes rewritten in C#, with every line the tests assert:
  several slices before the rebuild can start.

### C. A frozen copy of today's map side (chosen)
- Pros: byte-for-byte copies; the mechanism tests keep running unchanged on content they already pass on; the live
  systems (items, skills, enemies, quests, shops) stay live under them.
- Cons: the frozen files name live content by id and rot when it is renamed; a format change updates them by hand.

## Decision
`tests/TestWorld/` holds a frozen copy of the maps (settings and layouts), `tiles.json`, `tilesheets.json`,
`objects.json`, `new-game.json` and the tilesheet images they name. `tests/TestWorld/TestWorld.props`, imported by the
Data, Editor, Game and Simulations test projects, copies the live `content/` without those files, then the frozen ones,
into `test-world/` in the output. A test that reads a map, a tile, a sheet, an object or a tilesheet image, or starts a
session, loads `test-world`; a test of what the repository holds loads `content`, pinning ids, never cells.

## Consequences
The frozen maps name live quests, shops, items, members, encounters, music, stills and voices, so renaming one of those
fails the test world's load test by file name; a change to the map or new game format updates the frozen files by
hand. The frozen `new-game.json` holds the start location on the frozen intro's arrival cell, and also freezes the
starting items, gold and leader choices the Game tests count on: a change to those in `content/` no longer reaches the
mechanism tests, only `RepositoryNewGameTests`, which stays on `content`. Mechanism tests no longer follow the
repository's maps; the repository tests (R1) are what tie the real maps to the rules.
