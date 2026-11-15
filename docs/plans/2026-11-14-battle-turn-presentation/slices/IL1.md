# IL1. The impact's warm flash and the white wash on nearby battlers

Game, about 140 lines raw (about 70 of them tests), about 220 at the 1.6x (a new view). Needs ADR 0049 (and FL1's
`BattlerWhiteness`). Decision 660 (Q5, the devil's advocate's C3, C7); tunables "Scene flash", "Wash".

## Goal
When damage lands, the battlefield flashes warm (an additive orange-white over the background and the battlers, under
the HUD and the numbers) at 45 %, and every battler within 400 px of a hit target is washed white (0.6 at the target,
falling off with distance); both hold near full for about 0.1 s and are gone by 0.2 s (the clip's frames #77-#82). A
heal, a miss or a status alone lights nothing. One warm color for every impact.

## New types
- `Game/Battle/ImpactLight` (internal static): how the impact's light looks at a time since it. `static ImpactLightLook
  At(TimeSpan sinceImpact)`; `static float WashAt(float distance, float wash)` (the wash at a distance from the hit
  target, 0 past 400 px).
- `Game/Battle/ImpactLightLook` (internal readonly record struct): the impact's light at one moment: `SceneFlash`,
  `Wash` (IL2 adds `FloorGlow`, `Halo`), each 0 to 1.
- `Game/Battle/ImpactLightView` (internal sealed): draws the impact's light. Constructor `(BattleTurnSequence sequence,
  BattlersView battlers, UiTextures ui)`; this slice: `DrawOverTheBattlers(SpriteBatch, TextureLibrary)` (the warm
  flash; nothing unless the playback's strips are non-empty and the flash is above 0). IL2 adds `DrawUnderTheBattlers`.

## Changes
- `Game/Rendering/VirtualScreen.BeginLightBatch(SpriteBatch batch)`: additive, linear sampling (soft light, never pixel
  art), beside `BeginBatch`; its summary says the caller ends it and restarts `BeginBatch` right after.
- `Game/Ui/UiTheme.Battle`: `ImpactWarmth` (the warm orange-white).
- `Game/Battle/BattlerWhiteness.Of` gains the wash: `Of(Combatant combatant, ActionWindUp? windUp, float wash)`, the
  strongest of the fallen's, the blink's and the wash winning.
- `Game/Battle/BattlersView` (+5): passes each battler's wash (`ImpactLight.WashAt` its distance to the nearest strip
  target of `_sequence.Playback`, at `Playback.Elapsed`).
- `BattleScene` (+3): the view's field, construction and `DrawOverTheBattlers` after the impact effects and the swing,
  before the numbers.

## Tests
- `ImpactLightTests` (new): `At_TheImpact_FlashesAtItsHeight` (0.45); `At_PointOneSeconds_StillNearFull`;
  `At_PointTwoSeconds_IsDark` (0.2 s: 0; and 0.19 s above 0, the other side); `At_TheImpact_WashesAtItsHeight` (0.6);
  `WashAt_TheTarget_IsTheWholeWash`; `WashAt_HalfTheReach_IsHalf`; `WashAt_PastTheReach_IsNone` (400 px: 0).
- `BattlerWhitenessTests` (FL1's): `Of_WashedBystander_IsTheWash`; `Of_BlinkingActorWashed_IsTheStronger`;
  `Of_FallenWashed_IsTheFallensWhite` (0.6 against a 0.3 wash).
- The view's drawing: hand check.

## Verify by hand
1. Hit a goblin: on the impact the whole battlefield flashes warm for a tenth of a second and fades, the goblins nearest
   the hit flash white with it, the farthest barely; the turn order, plaque and panels aren't tinted; the number stays
   white over it.
2. A heal or a miss: no flash.
3. F12 on the impact frame (a few tries) and compare with the clip's frames #77-#80.

- **Done, with deviations:** `ImpactLightView` takes only the sequence for now (IL2 adds the battlers and the textures
  when the floor glow and the halo need them). `VirtualScreen.BeginLightBatch` adds premultiplied colors as they are
  (One, One): MonoGame's `BlendState.Additive` multiplies by alpha again and would square the fade. The flash holds at
  its height to 0.1 s, then falls linearly to 0 at 0.2 s; `ImpactWarmth` is `#FFC890` (placeholder). No captures in the
  unattended run: the hand check carries the look. `ImpactLight` 45 lines, the view 37, `BattlersView` 245; tests 73.
- **Review:** the flash draws right after the battlers, before the enemy tags, so the always-shown tags stay clean (the
  slice put it after the swing, over them); the wash reads the battler's drawn center `DrawBattler` already has. ADR
  0049's last consequence names the One/One light batch.
