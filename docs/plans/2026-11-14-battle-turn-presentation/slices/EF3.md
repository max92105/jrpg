# EF3. The impact strip at its own pace and size, centered on the target, mirrored for the party

Game, about 190 lines (about 100 of them tests). Needs EF1. Decisions 654, 655; replacement mapping rows EF3.

## Goal
The attacker's impact strip (`AttackEffectFile`) plays at its record's pace (80 ms a frame when unlisted), drawn at its
frame's own size (decision 57's fixed 96 px goes), centered on the target's drawn sprite, and mirrored when a party
member attacks (effect art faces right, an enemy attacking the party). The placeholder cues ("Miss", status names) sit on
the target's drawn center too. It still starts at the impact; EF4 moves its start to its hit frame's lead.

## Changes
- `Game/Battle/EffectStrip` (record struct): `EffectStrip(int FrameCount, TimeSpan FrameTime)`; `static EffectStrip
  Of(Point imageSize, EffectStripDefinition strip)`; `FrameAt` reads its own frame time. Its summary names decision 654.
- `Game/Battle/EffectOnTarget`: `bool IsMirrored` (the attacker is a party member).
- `Game/Battle/ActionPlayback` (about 272 → 277): the attacker's side kept from `ActionStarted`; `AddLanding` sets
  `IsMirrored`.
- `Game/Battle/BattlersView`: `Rectangle AreaOf(TextureLibrary textures, Combatant combatant)` (the drawn sprite's area,
  stepped and slid), beside `TopOf` (which `CommandPanelView` still reads).
- `Game/Battle/ImpactEffectsView` (83 → about 95): constructor gains `IReadOnlyDictionary<string, EffectStripDefinition>
  strips` (`session.Catalog.EffectStrips`, passed by `BattleScene`, one line); a strip's frame is `image.Height` square,
  drawn at that size centered on `AreaOf`'s center, `SpriteEffects.FlipHorizontally` when mirrored; cues centered on
  `AreaOf`'s center. `EffectSize` and `BelowTargetTop` go; the summary drops "96 px, 30 px below the target's top". Grep
  "96 px" and "30 px" in `Game/Battle/` comments and `docs/map.md`.

## Tests
- `EffectStripTests` (28): its three tests call `EffectStrip.Of(size, EffectStripDefinition.Of(file, empty))` and keep
  their expectations (80 ms); new `FrameAt_ListedPace_UsesItsFrameTime` (a 17 ms record: 0.04 s is frame 2) and
  `FrameAt_ListedPace_EndsAfterItsFrames` (4 frames of 17 ms: 0.068 s is none).
- `ActionPlaybackStripTests` (new sibling: `ActionPlaybackTests` is at about 250 after TN3):
  `Start_PartyMemberAttack_MirrorsTheStrip`; `Start_EnemyAttack_KeepsTheStripAsDrawn`.
- `ActionPlaybackTests.Start_AttackLanded_PlaysAttackersStripOnTarget` pins `"AttackPlayer.png"`, the repository's
  current value: re-pinned to `new EffectOnTarget(_goblin, _sarxian.Member!.Definition.AttackEffectFile, IsMirrored:
  true)`, so EF7's content change passes.

## Verify by hand
1. Attack a goblin: today's strip plays over the goblin's body at the strip's own size (bigger or smaller than before,
   whatever its frames are), mirrored compared with before for a party attack; F12 a capture.
2. A goblin's hit on a party member: the strip as drawn, centered on the member.
3. A miss: "Miss" over the target's middle.
