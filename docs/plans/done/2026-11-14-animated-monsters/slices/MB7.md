# MB7. The battle loops each animated enemy's frames

Game, about 230 lines (about 110 of them tests). Needs MB1; its hand check needs the gnolls (MB6). Decisions 634, 635,
636, 637; risk "Fallen silhouette".

## Goal
In battle, every living animated enemy loops its strip's frames in place at the strip's own size (the import scaled
it, decision 637), all the time, each one frame later than the enemy before it; it keeps looping through its own turn,
in place (enemies don't step since the battle turn presentation plan's TN1, its decision 643; decision 636); a fallen one
holds the frame it fell on while it whitens and fades. A battler without a loop draws exactly as today (decision 100).
The blink and its spill copies (that plan's FL1, `BattlerWhiteness`) whiten only the shown frame, as the fallen's
silhouette does.

## New types
- `Game/Battle/BattlerFrames` (internal sealed): which frame each combatant's sprite shows: its loop's frame at the
  battle's time, one frame later per place, held once fallen. Built from the battle's combatants and their
  `BattlerArt`; `Advance(TimeSpan delta)` (moves the battle's time; notes the frame of each combatant seen fallen for
  the first time); `int FrameOf(int index)` (0 for a still battler). No allocation after construction.

## Changes
- `Game/Battle/BattlerArt`: `TileAnimation? IdleAnimation` (the enemy's; null for party members, decision 641); `Of`
  copies it.
- `Game/Battle/BattlerScaling`: `static BattlerScaling AtOwnSize(Point frameSize)` (drawn at its own size, not shrunk:
  an animated strip is already at its battle height); its summary names decision 637 beside decision 100.
- `Game/Battle/BattlersView` (about 185 after the battle turn presentation plan's phase 1 → about 210): a
  `BattlerFrames`; `Update` advances it beside the slide; for an
  animated battler, `DrawnArea` and `DrawBattler` use the frame's size (image width / frame count by height) through
  `AtOwnSize` and draw with the frame's source rectangle (the white silhouette, and the blink's spill copies, with the
  same rectangle); a still
  battler keeps `BattlerScaling.Of` and the whole image. The summary names the loop.
- `docs/map.md`: the Game `Battle` row names `BattlerFrames`.

## Tests
- `BattlerFramesTests` (new, `tests/Jrpg.Game.Tests/Battle/`), combatants from a battle built on the test world's
  catalog `with` an animated enemy built in the test (4 frames of 100 ms), its `BattlerArt`s from `BattlerArt.Of`:
  - `FrameOf_StillBattler_IsZero` (a party member).
  - `FrameOf_AnimatedAtStart_IsZero`; `FrameOf_AnimatedAfterOneFrameTime_IsOne`.
  - `FrameOf_AfterTheLastFrame_LoopsToZero`.
  - `FrameOf_SecondAnimatedEnemy_IsOneFrameAhead` (two of a kind, decision 635).
  - `FrameOf_FallenEnemy_HoldsItsFrame` (fallen at frame 2, three frame times later still 2).
  - `Advance_Negative_Throws` (Core's `FrameAt` rule).
- `BattlerArtTests` (new, or the existing file if any): `Of_EnemyWithLoop_CarriesIt` (6 frames: not a default);
  `Of_PartyMember_HasNoLoop`.
- `BattlerScalingTests` (existing or new): `AtOwnSize_FrameOver512_IsNotShrunk` (the painted rule doesn't apply);
  `Of_StillPixelArt_IsStillHalved` (today's path unchanged).

## Verify by hand
1. After MB6 the gnolls exist. Put them in a battle locally: in `content/encounters.json`, add `gnoll_warrior` and
   `gnoll_archer` to the forest's goblin encounter (or the encounter the forest map rolls), save; run the game, walk into
   the forest until a battle starts. Revert afterwards (`git checkout content/encounters.json`) unless you keep it
   (decision 640).
2. The gnolls loop in place, two of a kind one frame apart; the goblins draw as before, same size.
3. On a gnoll's turn it keeps looping in place while it blinks twice and acts; each blink, and its soft spill, whitens
   only the frame shown.
4. Defeat a gnoll: it whitens and fades on one frame, not looping, only that frame whitened.
5. F12 capture: the gnolls at 128 px high beside the goblins, the warlord taller; pixels crisp where the factor was
   whole.

- **Done, with deviations:** an enemy's place counts the enemies before it, so the first animated enemy starts at
  frame 0. `BattlerArt.IdleAnimation` is an init property (the record's constructor keeps four parameters).
  `FrameOf_LivingBesideTheFallen_KeepsLooping` added; `Of_StillPixelArt_IsStillHalved` wasn't written, as
  `Of_256PxPixelArt_PointScalesByHalf` already pins that path. `BattlersView.cs` 191 → 216.
  Review: a Critical fixed: a revived enemy (an enemy's tactics can revive an ally) stayed on its held frame; `Advance`
  clears the hold while the combatant lives, `FrameOf_RevivedEnemy_LoopsAgain` added. `_places` renamed `_enemiesBefore`.
  Left: building the frame rectangle from the drawn size, which is the frame's own size by `AtOwnSize`.
