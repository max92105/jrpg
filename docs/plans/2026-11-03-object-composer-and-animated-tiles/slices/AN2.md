# AN2. The game draws animated tiles

Builds on design.md, "Key decisions" 6 and 7, and decisions 464 and 465.

## Goal
On the map, an animated tile shows the frame its animation calls for, on any layer and inside objects, every copy in step. The time is the map view's: it starts at zero on entering a map and moves on with each update of the map scene, events and encounters included; it stands while another screen is on top (the menu, the Forge, a shop), since the stack updates only the top scene. Canopies keep the first frame. No allocation per frame.

## New types
None.

## Changes
- `Game/Rendering/TileAtlas`: keeps per tile its sheet file, an array of source rectangles (one for a still tile, one per frame for an animated one, the cells to the right of its own) and its `TileAnimation?`; `TileImage ImageAt(int tileId, TimeSpan elapsed)` (the frame `FrameAt` gives); `ImageOf(int tileId)` stays, the first frame, for `ObjectSpriteBaker` (summary says so).
- `Game/Exploration/MapView`: `TimeSpan AnimationTime` (internal read), `void Advance(TimeSpan delta)`; `DrawTile` reads `atlas.ImageAt(tileId, AnimationTime)`. The class summary says the view holds the time its animated tiles show.
- `Game/Exploration/ExplorationScene.Update`: `_view.Advance(delta)` first, so the time moves on whatever the update then does; `internal TimeSpan TileAnimationTime => _view.AnimationTime` for the tests (189 lines, about +4).
- `Game/Rendering/ObjectSpriteBaker` summary: canopies are baked from each tile's first frame.

## Tests
- `Game.Tests/Rendering/TileAtlasTests`:
  - `ImageAt_AnimatedTileAtTheFrameTime_IsTheNextCellToTheRight`.
  - `ImageAt_StillTile_IsItsCellAtAnyTime`.
  - `ImageOf_AnimatedTile_IsTheFirstFrame`.
- `Game.Tests/Exploration/ExplorationSceneTests`:
  - `Update_Walking_AdvancesTheTileAnimationTimeByTheDelta`.
  - `Update_WhileAMessageShows_StillAdvancesTheTileAnimationTime` (on the test world's start map, through `StartMapScenarios`, at the 1/60 s step).
  - `Update_AfterAWarp_RestartsTheTileAnimationTime`.

Size: about 45 lines of code, 6 tests (about 170). No view type is added, so the Game's 1.6x doesn't apply; the scene tests reuse `StartMapScenarios`.

## Verify by hand
- Locally (revert after): set up the waterfall B sheet in the editor if it isn't yet, paint one of its row-4 tiles on an overlay layer over a cliff, save; in `content/tiles.json`, give that tile `"animation": { "frameCount": 4, "frameMilliseconds": 140 }`. Run the game: every painted copy cycles in step; it keeps moving under a message, stands under the menu and carries on after. Set `frameCount` to run past the sheet's right edge: the game refuses to start, naming the tile. Take the `animation` back out (AI2's import sets it properly); the painted tile and map stay.
