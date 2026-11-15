# FL1. The actor brightens twice with a soft spill instead of moving

Game, about 250 lines (about 160 of them tests). Needs TN2. Decisions 648, 649 (the devil's advocate's C5, C7);
tunables "First blink", "Second blink", "Blink strength", "Blink spill".

## Goal
The developer's favorite moment in the clip: on a monster's turn it **flashes slightly instead of moving**. During its
wind-up the acting battler, enemy or party member, brightens twice: for 33 ms at 0.10 s, then for 100 ms at 0.20 s, the
last 30 ms fading. Each blink lays white at 0.6 over the sprite, the fallen's strength, so its colors and lines still
show, and spills softly past its outline. Nothing before, between or after: not a lasting aura. One pure rule says how
white a battler draws (the fallen's or the blink's; IL1 adds the impact's wash).

This runs before the animated-monsters plan, so it whitens whole images; MB7 (amended) gives the blink and its spill the
shown frame's rectangle, as the fallen's.

## New types
- `Game/Battle/ActorFlash` (internal static): the acting battler's double blink. `static float StrengthAt(TimeSpan
  sinceWindUpStart)` (0 to 1: 1 during a blink, fading over the second blink's last 30 ms). Its constants are the
  tunables, the clip as their source.
- `Game/Battle/BattlerWhiteness` (internal static): how white a battler draws. `static float Of(Combatant combatant,
  ActionWindUp? windUp)`: the fallen's 0.6 for a fallen battler; else 0.6 x the blink's strength for the wind-up's actor;
  else 0. IL1 adds the wash as a third input (the strongest wins).

## Changes
- `ActionWindUp.BlinkOf(Combatant combatant)`: `ActorFlash.StrengthAt(Elapsed)` for its actor, 0 for anyone else.
- `Game/Battle/BattlersView` (about 160 → 185): the white silhouette's strength is `BattlerWhiteness.Of(...)`; while the
  actor blinks, the same silhouette is first drawn 8 times around the sprite (6 px out in the eight directions) at 0.12
  x the blink, the spill; the fallen keep no spill. `FallenWhiteness` moves into `BattlerWhiteness`. The summary names
  the blink and the spill.

## Tests
- `ActorFlashTests` (new): `StrengthAt_BeforeTheFirstBlink_IsZero` (0.09 s); `StrengthAt_FirstBlink_IsFull` (0.10 s);
  `StrengthAt_FirstBlinkOver_IsZero` (0.134 s); `StrengthAt_SecondBlink_IsFull` (0.20 s and 0.26 s, theory rows);
  `StrengthAt_SecondBlinksEnd_IsFading` (0.285 s: between 0 and 1); `StrengthAt_AfterTheBlinks_IsZero` (0.30 s).
- `BattlerWhitenessTests` (new), combatants from `BattleScenarios.SarxianAgainstGoblinTrio`, a fallen one from a
  battle on `BattleScenarios.WithOneGoblin(..., goblin => goblin with { MaxHp = 1, Dodge = 0 })` after Sarxian's attack:
  `Of_FallenBattler_IsTheFallensWhite` (0.6, a blinking wind-up present too); `Of_ActorMidBlink_IsTheBlinksWhite` (0.6); `Of_Bystander_IsNone`; `Of_NoWindUp_IsNone`.
- `ActionWindUpFlashTests` (new sibling of `ActionWindUpTests`): `BlinkOf_ActorAtTheFirstBlink_IsFull` (updated to
  0.10 s at `OneUpdate` steps); `BlinkOf_Target_IsZero` (same time); `BlinkOf_ActorAfterTheWindUp_IsZero`.
- The spill's drawing: hand check and capture.

## Verify by hand
1. A goblin's turn: after "Attack" pops in, the goblin brightens once, briefly, then once more a little longer, its
   features still visible, a soft white haze just past its outline each time; it never moves and never stays bright.
2. A party member's attack: the same double brightening after the command, the painted figure's detail kept.
3. A goblin felled by the hit still whitens and fades as before, with no haze.
4. F12 at the second blink and compare with the clip's glow frames.

- **Done, with deviations:** `ActionWindUp.BlinkOf` counts only while the action winds up, since the sequence keeps
  the last wind-up's clock running until the next action: without it a defender would blink 0.1 s after Defend
  (`BlinkOf_DefenderAtTheFirstBlinksTime_IsZero` added). `BattlerWhitenessTests`' fallen goblin takes damage directly
  instead of a battle. The spill and the white share a private `WhiteLayer` in `BattlersView` (163 → 191 lines). Review: no Critical or
  Warning; the spill's guard moved into a `DrawSpill` method.
