---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 3a (Map tiles on screen)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): art check in Program.FirstScene not GameFlow; IScene.Draw(SpriteBatch, TextureLibrary); TextureLibrary keyed by (folder, file); TileAtlas pure, built in GameFlow; ExplorationScene reads Location once (following in 3b); layers split at Z <= PlayerZ, DrawOrderKey merge in 3b; player.png 384x288 uses rows 0-3.

- open (Warning): Release/published build: no ArtRoot metadata -> {content}/art, which doesn't exist until slice 10, so the published game stops on the error screen; record in slice note/CLAUDE.md or publish Assets art to content/art
- open (Suggestion): ExplorationScene.DrawPlayer uses MapView.CellPixels (screen size) as the sprite sheet's frame size (art fact); name a separate 48 px frame constant
- open (Suggestion): PngSizeTests.Read_RepositoryTilesheet_ReturnsItsSize reads player.png, not a tilesheet; rename Read_RepositoryFieldSprite_ReturnsItsSize (plan's name)
- open (Follow-up): ArtReferenceCheck doesn't check the field sprite is >= 4 rows x 48 px
