# IL3. Red and green fringes on the impact's bright parts

Game, about 180 lines (about 90 of them tests). Needs IL2 and ADR 0049. Decision 661 (Q6, the devil's advocate's C1);
tunable "Fringe".

## Goal
On the impact where damage landed, the bright parts split like a lens: while the fringe lasts, the impact strip's frame
and the white silhouettes (the wash's, a fallen's) are drawn as three additive single-channel copies, red shifted left,
green shifted right, blue in place, about 6 virtual px apart at the impact and back to one picture by about 0.167 s (the
clip's frames #77-#82). They are drawn before the numbers, so the number, the plaque and the HUD stay clean. No host,
flow, render-target or shader change.

## New types
- `Game/Battle/ColorFringe` (internal static): how far the bright parts' red and green split at a time since the impact.
  `static float ShiftAt(TimeSpan sinceImpact)` (virtual pixels: 6 at the impact, easing out to 0 at 0.167 s, 0 after).
- `Game/Battle/DrawnSprite` (internal readonly record struct): a texture's part as drawn: `Texture2D Texture`,
  `Rectangle Destination`, `Rectangle? Source`, `SpriteEffects Effects`.
- `Game/Battle/ChannelSplit` (internal static): draws a texture as three additive single-channel copies, red and green
  shifted apart. `static void Draw(SpriteBatch batch, DrawnSprite sprite, float shift, float strength)` (inside a light
  batch; the three tints add back to the texture's colors where nothing is shifted).

## Changes
- `Game/Battle/ImpactEffectsView` (about 125 → 140): while `ColorFringe.ShiftAt(playback.Elapsed)` is above 0, the
  playback's strip frames draw through `ChannelSplit` in a light batch instead of once.
- `Game/Battle/BattlersView` (+15): while the shift is above 0, a battler's white silhouette draws through `ChannelSplit`
  in a light batch (one switch per washed battler, only during the fringe).
- The summaries of both views say when they split.

## Tests
- `ColorFringeTests` (new): `ShiftAt_TheImpact_IsTheWholeShift` (6); `ShiftAt_HalfTheTime_IsBetween`;
  `ShiftAt_JustBeforeItsEnd_IsAboveZero`; `ShiftAt_ItsEnd_IsZero` (0.167 s).
- `ChannelSplit` and the views draw: hand check and capture (the tints adding back to the frame are MonoGame's additive
  blend, not ours to test).

## Verify by hand
1. Hit a goblin: on the impact the burst's spikes and the washed sprites show red edges on one side and green on the
   other for a split second, then they are clean; the number, the plaque, the panels and the background never fringe.
2. A heal and a miss: no fringe.
3. F12 on the impact frame and compare with the clip's frames #77-#80 (the fringe is in the capture).
