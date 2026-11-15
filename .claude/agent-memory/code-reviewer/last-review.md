---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-14-battle-turn-presentation, slice IL3 (ColorFringe, DrawnSprite, ChannelSplit write-masked copies in
ImpactEffectsView and BattlersView). 2026-11-15. Round 1. Verdict: ready to commit.

- Warning: the same `batch.Draw(sprite.Texture, dest, sprite.Source, tint, 0, Vector2.Zero, sprite.Effects, 0)` is in
  ImpactEffectsView.DrawOnce, BattlersView.WhiteLayer.Draw and ChannelSplit.DrawCopy; move it onto DrawnSprite
  (e.g. `Draw(batch, Point offset, Color tint)`). open
- Suggestion: `StripSprite` -> `StripFrameAt` (names the time it takes). open
- Suggestion: split only while `MathF.Round(shift) >= 1`: below 0.5 px (last ~0.07 s) the four batch flushes per
  sprite draw the same picture. open
- IL2 items: test renamed IsNineSixteenths fixed; halo now before `_enemyTags` fixed; LitImpactNow fixed; `_heldFor`
  comment fixed; ADR HUD-order follow-up fixed (tags draw after DrawOverTheEffects).
