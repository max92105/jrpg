# The test world

The map side of the content as it was on 2026-11-03, frozen for the tests (ADR 0037): the five maps (`maps/`,
`map-layouts/`), `tiles.json`, `tilesheets.json`, `objects.json`, `new-game.json` and the tilesheet images they name,
plus `Grass3.png`, an image no sheet is set up on, which the editor tests open.

`TestWorld.props`, imported by every test project but Core's, lays the live `content/` without those files, then this
folder, into `test-world/` in the test output. A test that walks a map, reads a tile, a sheet, an object or a tilesheet
image, or starts a session loads `test-world`; a test of what the repository holds loads `content` and pins ids, never
cells.

Rules:

- The editor never writes here. A change is a hand edit for a test's need, with the test that needs it.
- The frozen maps name live quests, shops, items, members, encounters, music, stills and voices. Renaming one of those
  in `content/` fails `TestWorldTests` by file name; fix the frozen file by hand.
- A change to the map or new game format updates these files by hand.
- The frozen `new-game.json` holds the start location on the frozen intro's arrival cell, and also the starting items,
  gold and leader choices the Game tests count on. A change to those in `content/` reaches only the repository tests.
