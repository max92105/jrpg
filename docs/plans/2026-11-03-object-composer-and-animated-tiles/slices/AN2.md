# AN2. The game draws animated tiles

Builds on design.md, "Key decisions" 6 and 7, and the README's picks 10 and 11.

## Goal
On the map, an animated tile shows the frame its animation calls for, on layers and inside objects, every copy in step. The time is the map view's: it starts at zero on entering a map and moves on with each update of the map scene, events and encounters included; it stands while another screen is on top (the menu, the Forge, a shop), since the stack updates only the top scene. Canopies keep the first frame. No allocation per frame.

## New types
None.

## Changes
- `Game/Rendering/TileAtlas`: keeps per tile its sheet file, an array of source rectangles (one for a still tile, one per frame for an animated one) and its `TileAnimation?`; `TileImage ImageAt(int tileId, TimeSpan elapsed)` (the frame `FrameAt` gives); `ImageOf(int tileId)` stays, the first frame, for `ObjectSpriteBaker` (summary says so).
- `Game/Exploration/MapView`: `TimeSpan AnimationTime` (internal read), `void Advance(TimeSpan delta)`; `DrawTile` reads `atlas.ImageAt(tileId, AnimationTime)`. The class summary says the view holds the time its animated tiles show.
- `Game/Exploration/ExplorationScene.Update`: `_view.Advance(delta)` first, so the time moves on whatever the update then does; `internal TimeSpan TileAnimationTime => _view.AnimationTime` for the tests (189 lines, about +4).
- `Game/Rendering/ObjectSpriteBaker` summary: canopies are baked from each tile's first frame.

## Tests
- `Game.Tests/Rendering/TileAtlasTests`:
  - `ImageAt_AnimatedTileAtTheFrameTime_IsTheSecondFramesCell`.
  - `ImageAt_StillTile_IsItsCellAtAnyTime`.
  - `ImageOf_AnimatedTile_IsTheFirstFrame`.
- `Game.Tests/Exploration/ExplorationSceneTests`:
  - `Update_Walking_AdvancesTheTileAnimationTimeByTheDelta`.
  - `Update_WhileAMessageShows_StillAdvancesTheTileAnimationTime` (on the test world's start map, through `StartMapScenarios`, at the 1/60 s step).
  - `Update_AfterAWarp_RestartsTheTileAnimationTime`.

Size: about 45 lines of code, 6 tests (about 170). No view type is added, so the Game's 1.6x doesn't apply; the scene tests reuse `StartMapScenarios`.

## Verify by hand
- Locally (revert after): in `content/tiles.json`, give a painted water or grass tile an `animation` with its sheet's frames side by side (`frameMilliseconds` 150), and paint it into an object too. Run the game: the cells on the map and in the object cycle in step; walking, talking (the water keeps moving under a message) and opening the menu (it stands, then carries on). Set a frame past the sheet's edge: the game refuses to start, naming the tile and frame. Revert the file.
