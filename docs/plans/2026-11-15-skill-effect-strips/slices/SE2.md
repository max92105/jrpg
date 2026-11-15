# SE2. The action plays its own strip and its swing

Game, about 240 lines (code about 70, tests about 170). Needs SE1. Decisions 706, 708; design.md "Key decisions",
"Replacement mapping". Not a view: the 1.6x rule for Game slices adding a view type doesn't apply.

## Goal
A skill or item that names a strip plays it: its lead during the wind-up on the targets it aims at, the rest on each
target damage landed on, at its record's pace and hit frame, and the swing its record names with the whoosh. An Attack,
and a skill or item without a strip, play exactly as today.

## New types
- `Game/Battle/ActionStrip` (internal static class): which strip an action plays, its skill's or item's own or its
  actor's attack strip (decision 678), and which swing goes with it.
  - `public static string FileOf(Combatant actor, BattleCommand command)`: a `SkillCommand` whose skill names a strip
    or an `ItemCommand` whose item names one gives that strip; anything else gives `BattlerArt.Of(actor)
    .AttackEffectFile`. Throws as `BattlerArt.Of` does for a combatant that is neither.
  - `public static string? SwingFileOf(Combatant actor, BattleCommand command, IReadOnlyDictionary<string,
    EffectStripDefinition> strips)`: the record's `SwingFile` of `FileOf`'s strip for an Attack or an action naming
    its own strip (decision 708); null for a skill or item on its actor's strip (decision 657's Q2).

## Changes
- `ActionWindUp` constructor: `ImpactFile = AimedTargets.Count > 0 ? ActionStrip.FileOf(actor, command) : null;`
  `SwingFile = ImpactFile is null ? null : ActionStrip.SwingFileOf(actor, command, strips);`. Summary and docs:
  "an Attack's swing" becomes "the swing of an Attack's or an action's own strip (decision 708)" in the class summary,
  `ImpactFile` ("the action's strip while it damages: its own or its actor's, decision 678"), `SwingFile`, `Sounds`
  ("an Attack's whoosh" -> "the swing's whoosh") and the `strips` param doc.
- `ActionPlayback`: `_attackEffectFile` becomes `_stripFile`, set to `ActionStrip.FileOf(started.Actor,
  started.Command)` on `ActionStarted`; the class summary's "damage as the attacker's effect strip" and `Strips`'
  doc say "the action's strip, its own or its attacker's (decision 678)". Net change under +5 lines (the file is 274).
- `SwingView` summary: "an Attack's swing" becomes "the swing of an Attack's or an action's own strip (decision 708)".
- Grep the Battle folder's comments for "attacker's strip", "attacker's effect strip", "an Attack's swing", "a party
  member's attack" (`ImpactEffectsView`'s summary: the mirroring now covers a party member's skill or item too) and
  "Only damage has an animation so far: the attacker's strip" (`ActionPlayback.cs`, the comment above `AddLanding`,
  near line 169), and `docs/map.md`'s Game Battle row; reword each that the change made wrong (code-quality.md,
  comments).
- `docs/map.md`, Game Battle row: add `ActionStrip` (the strip an action plays, its own or its actor's, decision 678;
  the wind-up and the playback read it) and reword `SwingView`'s and `ActionWindUp`'s swing to "an Attack's or an
  action's own strip".

## Tests
Fixtures: the existing `BattleScenarios.SarxianAgainstGoblinTrio` with its seed; skills from
`SoundingSkills.WithSound("") with { EffectFile = "Own.png" }`; items built in the test (a damaging consumable with
and without `EffectFile`); strip records built in the test (`Own.png` with `SwingFile = "OwnSwing.png"`, Sarxian's
attack strip with `SwingFile = "Swing.png"`, as `ActionWindUpEffectTests.SarxiansStripSwinging`).
- `ActionStripTests` (new; Glob found none):
  - `FileOf_Attack_IsTheActorsAttackStrip`
  - `FileOf_SkillNamingAStrip_IsTheSkillsStrip`
  - `FileOf_SkillWithoutAStrip_IsTheActorsAttackStrip`
  - `FileOf_ItemNamingAStrip_IsTheItemsStrip`
  - `FileOf_ItemWithoutAStrip_IsTheActorsAttackStrip`
  - `SwingFileOf_AttackOnASwingingStrip_IsItsSwing`
  - `SwingFileOf_SkillNamingASwingingStrip_IsItsSwing`
  - `SwingFileOf_SkillOnTheActorsSwingingStrip_IsNone` (decision 657 kept: the actor's strip swings, the skill doesn't)
  - `SwingFileOf_ItemNamingASwingingStrip_IsItsSwing`
  - `SwingFileOf_OwnStripUnlisted_IsNone`
- `ActionWindUpEffectTests` (166 lines, about +40):
  - `ImpactFile_SkillNamingAStrip_IsTheSkillsStrip`
  - `SwingFile_SkillNamingASwingingStrip_IsItsSwing`
  - `Sounds_SkillNamingASwingingStrip_IsItsSoundThenTheWhoosh` (the skill from `SoundingSkills.WithSound("Own.wav")
    with { EffectFile = "Own.png" }`; `[("Own.wav", 0), (Swing, SwingStart)]`)
  - The existing `SwingFile_Skill_IsNone` and `Sounds_SkillWithASoundAndNoSwing_IsItsSoundOnly` stay unchanged: they
    are decision 657's side.
- `ActionPlaybackStripTests` (39 lines, about +40):
  - `Start_SkillNamingAStrip_PlaysItOnTheTargetDamaged`
  - `Start_ItemNamingAStrip_PlaysItOnTheTargetDamaged`
  - `Start_SkillWithoutAStrip_PlaysTheAttackersStrip`
- Existing tests broken: none expected (an Attack's path is unchanged; `SwingFile_Skill_IsNone` keeps its meaning).

## Verify by hand
Each step edits `content/skills.json` or `content/items.json` locally (Debug reads `content/` at each start) and is
reverted at the end with `git checkout content/skills.json content/items.json`.
1. Add `"effectFile": "AttackEnemy.png",` after `fire_blast`'s `soundFile` line (an existing unlisted strip, so a
   different look from the golden impact). New game as **Omer**, walk until a random battle (`milestone_outside` or
   `primeval_growth_crossroad` have them), **Skills > Fire Blast** on a goblin: `AttackEnemy.png` plays on it, its
   number landing at once (hit frame 0), no swing. His **Attack** still plays the golden slash and impact.
2. On a later turn, **Skills > Fireball**: the golden impact on every goblin (his attack strip, decision 678's
   default), no swing, no whoosh.
3. Change `fire_blast`'s line to `"effectFile": "GoldenSlashImpact.png",`: Fire Blast now sweeps the golden slash in
   front of Omer with its whoosh after the skill's own sound, then the impact (decision 708).
4. Add `"effectFile": "AttackEnemy.png",` after `bomb`'s `price` line in `items.json` (SE3 gives it the explosion
   for good); **Items > Bomb** on a goblin: `AttackEnemy.png` plays on it.
5. Revert both files.
