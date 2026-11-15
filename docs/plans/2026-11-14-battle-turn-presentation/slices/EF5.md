# EF5. The swing its impact strip names sweeps in front of the attacker

Game, about 120 lines raw (about 60 of them tests), about 190 at the 1.6x (a new view). Needs EF4. Decisions 655, 657
(C2, Q2); tunables "Swing start", "Swing reach".

## Goal
When an attacker attacks (the Attack command) and its impact strip's record names a swing, the swing strip starts 0.47 s
into the wind-up, at its own record's pace and its own size, its frame centered 60 px ahead of the attacker's drawn
center toward the other side, mirrored for a party member. It keeps playing past the impact until its last frame (the
clip's slash tail over the hit). An impact strip without a swing, or a skill, item, Defend, Move or Run, swings nothing.

## New types
- `Game/Battle/SwingView` (internal sealed): draws an attack's swing in front of its attacker. Constructor
  `(BattleTurnSequence sequence, BattlersView battlers, IReadOnlyDictionary<string, EffectStripDefinition> strips)`;
  `Draw(SpriteBatch, TextureLibrary)` reads `sequence.WindUp`: no swing file, or a frame of none, draws nothing.

## Changes
- `ActionWindUp.Of(Combatant actor, BattleCommand command, UiMessages messages, IReadOnlyDictionary<string,
  EffectStripDefinition> strips)`; `string? SwingFile` (for an `AttackCommand`, the actor's impact strip's record's
  `SwingFile` through `EffectStripDefinition.Of`); `TimeSpan SinceSwing` (`Elapsed - BattlePacing.SwingStart`).
- `BattleTurnSequence`: constructor gains the strips (`session.Catalog.EffectStrips` from `BattleScene`), its fourth
  parameter, passed to each wind-up; the five constructions in `BattleTurnSequenceTests` (and its sibling) pass the
  test catalog's (empty) strips.
- `Game/Battle/BattlePacing.SwingStart` (0.47 s, "into the wind-up: the clip's slash").
- `BattleScene` (+3): the `SwingView` field, its construction, its draw after the impact effects.
- `docs/map.md`: the Game `Battle` row names `ActionWindUp`, `ActionSounds`, `ActorFlash`, `BattlerWhiteness` and
  `SwingView`.

## Tests
- `ActionWindUpEffectTests` (EF4's file), strips built in the test: `SwingFile_AttackWithASwingingStrip_IsItsSwing`;
  `SwingFile_AttackWithAnUnlistedStrip_IsNone`; `SwingFile_Skill_IsNone` (a damaging skill whose strip swings);
  `SinceSwing_AtTheSwingStart_IsZero`.
- `BattlerArtTests` (31): `Of_PartyMember_UsesMemberSprite` pins the repository's `"AttackPlayer.png"`: re-pinned to
  read the member definition's own `AttackEffectFile`, so EF7 passes.
- The view's placement and mirroring are drawn: hand check and capture.

## Verify by hand
1. Locally, list `AttackEnemy.png` with `"swingFile": "AttackPlayer.png"` in `content/effect-strips.json`. On a goblin's
   turn, after the blinks, the swing plays in front of the goblin toward the party, then the hit lands while it
   finishes.
2. Give Sarxian's strip a swing the same way (`AttackPlayer.png` with `"swingFile": "AttackEnemy.png"`): his attack plays
   it in front of him toward the goblins, mirrored.
3. A skill by either plays no swing. Revert the file.

- **Done:** as planned. The tests share `BattleScenarios.NoEffectStrips` for the strips no record lists; the five
  `ActionWindUp.Of` callers and six sequence constructions in the tests pass it. `SwingView` keeps each swing image's
  pace once, as `ImpactEffectsView` does.
  Review: no Critical or Warning; `Reach` private; `EnemyTagViewTests` builds its sequence through `SequenceOver`; the
  map's `ActionWindUp` entry names the swing; the two views' shared strip drawing went to the backlog.
