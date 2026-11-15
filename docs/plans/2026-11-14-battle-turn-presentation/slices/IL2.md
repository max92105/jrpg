# IL2. The floor glow around the target and its lingering halo

Game, about 200 lines (about 100 of them tests). Needs IL1. Decision 660 (the devil's advocate's C3, C7); tunables
"Floor glow", "Target halo".

## Goal
On the impact the floor around each hit target lights orange-white: a soft 600 x 200 px ellipse at its feet, under the
battlers, 80 % for about 0.1 s and gone by 0.2 s, no longer than the flash. Then a warm halo, a soft 280 px disc on the
target's drawn center over the effects, rises as the flash fades (0 at 0.1 s, 50 % at 0.2 s), holds to 0.43 s and is
gone by 0.5 s, around the impact strip's crescent glow (the clip's frames #83-#90). The floor glow is drawn by
`ImpactLightView` from `BattleScene`, between the backdrop and the battlers, never inside `BattlersView`.

## New types
- `Game/Ui/RadialGlowShape` (internal static): a soft disc's pixels. `static Color[] Pixels(int size)`: white, its alpha
  falling from the center to 0 at the edge, smoothly (`(1 - d^2)^2`), baked once at 128 px and stretched to either
  shape.

## Changes
- `ImpactLightLook`: `FloorGlow`, `Halo`; `ImpactLight.At` fills them.
- `Game/Ui/UiTextures` (about 259 after the cinematics plan → 267): `RadialGlow(TextureLibrary textures)`.
- `Game/Ui/UiTheme.Battle`: `ImpactFloor`, `ImpactHalo` (the glow's warm tints).
- `Game/Battle/ImpactLightView`: `DrawUnderTheBattlers` (each strip target's floor ellipse at its feet, in a light batch
  while the floor glow is above 0); the halo on each strip target's drawn center (`BattlersView.AreaOf`) in
  `DrawOverTheBattlers`, in the same light batch as the flash.
- `BattleScene` (+1): `DrawUnderTheBattlers` after the backdrop, before `_battlers.Draw`.

## Tests
- `RadialGlowShapeTests` (new): `Pixels_Center_IsOpaque`; `Pixels_Edge_IsClear`; `Pixels_HalfwayOut_IsBetween`;
  `Pixels_IsSymmetric` (a pixel and its mirror across the center).
- `ImpactLightTests`: `At_TheImpact_LightsTheFloor` (0.8); `At_PointTwoSeconds_FloorIsDark` (0.2 s: 0; 0.19 s above
  0); `At_TheImpact_HasNoHaloYet`; `At_PointTwoSeconds_HaloAtItsHeight` (0.5); `At_PointFourThreeSeconds_HaloStillHeld`;
  `At_HalfASecond_HaloIsGone`.

## Verify by hand
1. Hit a goblin: the ground under it lights up warm in a wide soft oval, under the goblins' feet (they stand on it),
   gone with the flash; then a warm glow lingers around the burst's crescent for a moment and fades before the sparks.
2. A party member hit by a goblin: the same at the member's feet on the right.
3. An attack on all goblins (a skill that takes all): each target's floor lights.
4. F12 at the lingering halo and compare with the clip's frames #84-#90.
