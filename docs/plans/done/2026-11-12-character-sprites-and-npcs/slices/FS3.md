# FS3. The game draws the player and NPCs from their records

Core + Game, about 250 lines raw (120 of them tests, 40 of them re-pinned). Design: "Core changes" (the drawn area), key decision 2, "Replacement mapping". Builds on FS1.

## Goal
The player walks with its record's frames and time, and an NPC stands on its record's standing frame, both scaled by the record and standing on their cell; with today's two records the screen is unchanged. A hand-written record for the bought sheet shows the bought NPC on a map.

## New types
- `Core/Sprites/SpriteCell` (public readonly record struct): a frame's column and row on a sheet.
- `Core/Sprites/SpriteArea` (public readonly record struct): where a sprite is drawn relative to its cell's top-left, in screen pixels.

## Changes
- `Core/Sprites/FieldSpriteSheet`: `StandingCell(Facing)` (column 0 of the facing's walk row, decision 564); `WalkCellAt(Facing, TimeSpan walked)` (the walk's `FrameAt`); `FrameLeft(SpriteCell)`, `FrameTop(SpriteCell)`; `AreaOver(int cellPixels)` (design, "Core changes").
- `Game/Exploration/WalkAnimation`: holds `Walked` (the time walked since the player last stood, reset by `Stand`), no frame count or time of its own; `RowOf` and `_frameDuration` go (`FacingRows.RowOf`, the record).
- `Game/Exploration/FieldSpriteFrame.SourceOf(FieldSpriteSheet sheet, SpriteCell cell)`: the frame's rectangle from the record.
- `Game/Rendering/MapArt`: `PlayerSprite` (the new game's record) replaces `PlayerSpriteFile`; `FieldSprites` (the catalog's records).
- `Game/Exploration/MapView`: its constructor takes the `MapArt` instead of the `TileAtlas` (still three parameters); an NPC draws `StandingCell(Facing.Down)` (decisions 127, 128 until NB1) at its cell's top-left plus `AreaOver(CellPixels)`; the on-screen test uses that area.
- `Game/Exploration/ExplorationScene`: `EnterCurrentMap` builds the view with `_art` (the one call site besides `MapViewTests`); `DrawPlayer` places the cell centered on the position, then `AreaOver`, the frame `WalkCellAt(PlayerFacing, Walked)`.

## Tests
- `FieldSpriteSheetTests`: `AreaOver_PlayerSheet_FillsTheCell` (48 x 48, scale 1: (0, 0, 48, 48), the player's square today); `AreaOver_ScaledTallFrame_StandsOnCellBottom` (32 x 32 at 3: (-24, -48, 96, 96)); `AreaOver_FeetPixels_LowersTheSprite`; `WalkCellAt_PlayerSheet_MatchesTodaysFrames` (theory: walked 0, 99, 100, 799, 800 ms give columns 0, 0, 1, 7, 0, the row of each facing as today); `StandingCell_EachFacing_IsColumnZeroOfItsRow`.
- `WalkAnimationTests` (re-pinned): `Advance_Walked_AddsTheDelta`; `Stand_AfterWalking_ResetsWalked`; the old frame-count tests become the sheet's `WalkCellAt` test above.
- `FieldSpriteFrameTests` (re-pinned): `SourceOf_PlayerSheetCell_MatchesTodaysRectangle` (theory over the four rows and columns 0 and 7: the same rectangles `SourceOf(facing, frame)` gave); `SourceOf_NonSquareFrames_UsesWidthAndHeight`.
- `MapViewTests`: `ScreenArea_PlacedNpc_UsesTheSheetsArea` if the view exposes it for tests, else none (the draw is checked by hand).

## Verify by hand
1. `dotnet run --project src/Jrpg.Game`, start a new game: the player stands, walks in all four directions and stops exactly as before (same frames, same speed of the legs).
2. Walk to the placeholder NPC on the start map: it stands facing down as before.
3. Bought sheet, by hand (until phase 2 makes it a few clicks): copy the PNG into `content/art/field-sprites/`, add its line to `content/art/art-sources.json` and `CREDITS.md` (ADR 0038), add a record to `content/field-sprites.json` with its frame size, `"scale": 3` and its four rows, and set one NPC object's `fieldSpriteFile` in `content/objects.json` to it. Start the game: the NPC stands on its cell, feet on the cell's bottom, rising above it, pixels crisp. An NPC just under a roof: note whether the head draws under the canopy (design, "Risks"). Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** both sprites are drawn from their record's own `File`, so `MapArt` keeps no file name beside the record; `MapView` has no test seam for the drawn area (the draw is the hand check), as the slice allowed.
