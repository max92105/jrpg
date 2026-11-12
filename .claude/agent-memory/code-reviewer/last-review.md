---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice FS3 (SpriteCell, SpriteArea, FieldSpriteSheet StandingCell/
WalkCellAt/FrameLeft/FrameTop/AreaOver; WalkAnimation.Walked, FieldSpriteFrame.SourceOf(sheet, cell), MapArt
PlayerSprite/FieldSprites, MapView takes MapArt, ExplorationScene.DrawPlayer).
2026-11-12. Round 1. Verdict: ready to commit (no Critical, no Warning).

- Suggestion (open): MapView.cs:75 `var area = cells;` leftover alias; use one name.
- Suggestion (open): FieldSpriteLayout.WalkFrames and Rows now unused, summary "The game draws from it" stale (FS5 deletes the type).
- Suggestion (open): AreaOver odd width: truncation toward zero puts the extra pixel right when narrower, left when wider.
- Follow-up: ObjectDefinition.cs:31-32 "laid out like the player's" stale since ADR 0043.
- FS2 Critical (RowsNeeded wrap) and Warning (FieldSpriteLayout.Holds): fixed in 258885f0.
