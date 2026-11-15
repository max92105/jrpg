# TN4. An action's own sound at its wind-up's start, its hit at the impact

Game, about 200 lines (about 120 of them tests, 6 moved). Needs TN3. Decision 647 (Q8, the devil's advocate's C6);
decision 423 (sounds at their beat).

## Goal
A skill's own sound, the item-use sound and the Defend sound play when the action's wind-up starts, with the plaque; the
hit, critical, heal, miss, block, resisted and status sounds still play at the impact, now 0.7 s later. One type says
which sound an action makes at each of the two moments, read by the wind-up and by `BattleSounds`, so when the developer
adds per-action opening and impact sound files to the content, each fills its moment there and nowhere else.

## New types
- `Game/Battle/ActionSounds` (internal static): which sound an action makes at its wind-up's start and which at its
  impact. `static string? OpeningOf(BattleCommand command)` (a skill's `SoundFile` when it has one, the item-use sound
  for an item, the Defend sound; none for Attack, Move and Run, whose sounds come from their events);
  `static string? ImpactOf(BattleEvent battleEvent, IReadOnlyList<BattleEvent> events)` (moved from
  `BattleSounds.ImpactFileOf`). Its summary says where a per-action file will go.
- `Game/Battle/ISoundTimeline` (internal interface): sounds due at times along their own clock. `IReadOnlyList<TimedSound>
  Sounds`; `TimeSpan Elapsed`. Two implementations: `ActionPlayback` (from the impact) and `ActionWindUp` (from the
  plaque).

## Changes
- `ActionWindUp`: implements `ISoundTimeline`; `Sounds` is the opening sound at 0 when there is one, built in `Of`.
- `ActionPlayback`: implements `ISoundTimeline` (its members already match).
- `BattleSounds` (88 → about 70): reads `ActionSounds.ImpactOf`; `ActionStarted` makes no sound here any more (the
  wind-up plays it), so its switch keeps `ActionStarted` among the silent events. Its summary says so.
- `PlaybackSoundQueue.Follow(ISoundTimeline? timeline, AskedSounds sounds)`; its summary: "a turn's sounds, the
  wind-up's or the playback's".
- `BattleScene` (+2): a second queue following `_sequence.WindUp` beside the playback's.

## Tests
- `ActionSoundsTests` (new): `OpeningOf_SkillWithASound_IsItsSound`; `OpeningOf_SkillWithoutASound_IsNone`;
  `OpeningOf_Item_IsTheItemUseSound`; `OpeningOf_Defend_IsTheDefendSound`; `OpeningOf_Attack_IsNone`. These replace
  `BattleSoundsTests`' `Of_SkillUsed_IsItsOwnSoundAtTheStart`, `Of_SkillWithoutASound_IsSilent`,
  `Of_ItemUsed_IsTheItemSound`, `Of_Defended_IsTheDefendSound` and `Of_Attacked_IsSilentAsItStarts` (moved, said in the
  Done note); `Of_SkillThatHits_IsItsOwnSoundThenTheHit` becomes `Of_SkillThatHits_IsTheHitOnly` (the opening is the
  wind-up's now). `BattleSoundsTests`' impact tests pass unchanged through `ImpactOf`.
- `ActionWindUpSoundTests` (new): `Sounds_Skill_IsItsSoundAtTheStart`; `Sounds_Attack_IsNothing`.
- `PlaybackSoundQueueTests` (90): `Follow_WindUp_AsksItsOpeningAtOnce` (a wind-up through the interface).
- `BattleSceneSoundTests` (166): `Update_ItemConfirmed_AsksTheItemSoundAtOnce` (the item chosen through
  `CommandMenuDriver`, the sound in the confirming update's `Sounds`); `Update_AttackConfirmed_AsksTheHitOnlyAtTheImpact`
  (a sure hit: no hit sound before the wind-up's 0.7 s, the hit within the update that ends it).

## Verify by hand
1. Use a potion in battle: its sound plays as "Potion" pops in, before the heal number.
2. A skill with a sound (a goblin chief's, or a member's): its sound with the plaque, the hit 0.7 s later.
3. A plain attack: silent until the hit lands with the number.
4. Defend: its sound at once.

- **Done, with deviations:** `ActionWindUpSoundTests`' two tests went into `ActionWindUpTests` (same fixture; 86 → 100
  lines). The five opening tests moved from `BattleSoundsTests` to `ActionSoundsTests` as planned; the test skill with a
  chosen sound, now used by both and by `ActionWindUpTests`, moved to a shared `SoundingSkills` builder.
  `BattleSounds.cs` 88 → 73. The scene's item test counts two Down presses to Items, as `BattleSceneTests` does for Defend:
  the scene doesn't expose its menu for `CommandMenuDriver`. Review: no Critical; the `PlaybackSoundQueue` rename to
  match `ISoundTimeline` left as taste.
