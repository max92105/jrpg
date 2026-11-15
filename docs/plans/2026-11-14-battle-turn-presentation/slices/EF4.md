# EF4. The impact strip's hit frame lands on the impact

Game, about 200 lines (about 110 of them tests). Needs EF3. Decisions 656, 657 (Q2); design "Key decisions" (the wind-up
decides from the command; strip records, drawing in the Game).

## Goal
An impact strip shows its hit frame at the impact: it starts its lead (hit frame x frame time) before it, during the
wind-up, on every target a damaging action aims at (an Attack's target; a skill's or item's targets when its effect is
damage, Q2); from the impact on it goes on only on the targets damage landed on, so a miss cuts it at its hit frame. A
lead longer than the wind-up loses its first frames; the hit still lands on the impact. With today's strips (unlisted:
hit frame 0) nothing changes on screen; EF7's golden impact shows it.

## Changes
- `ActionWindUp`: `IReadOnlyList<Combatant> AimedTargets` (empty for an action that doesn't damage); `string?
  ImpactFile` (the actor's `BattlerArt.AttackEffectFile` when it damages); `bool IsMirrored` (a party actor); `TimeSpan
  SinceImpact` (`Elapsed - Duration`, negative until the impact). "Damages" reads the command: an `AttackCommand`; a
  `SkillCommand` whose `Skill.Effect.Kind` is `EffectKind.Damage`; an `ItemCommand` whose `Item.Effect?.Kind` is.
- `Game/Battle/EffectStrip`: `int HitFrame`; `int? FrameAtImpactTime(TimeSpan sinceImpact)` (`FrameAt(HitLead +
  sinceImpact)`, none before the strip's start).
- `Game/Battle/ImpactEffectsView` (about 95 → 125): while the sequence is `WindingUp` and the wind-up has an impact
  file, draws each aimed target's frame at `FrameAtImpactTime(windUp.SinceImpact)`; the playback's strips draw at
  `FrameAtImpactTime(playback.Elapsed)`. One private draw for both. The summary says when a strip starts.

## Tests
- `ActionWindUpEffectTests` (new sibling of `ActionWindUpTests`):
  - `Of_Attack_AimsAtItsTarget`; `Of_DamagingSkill_AimsAtItsTargets` (a skill built in the test `with` a damage
    effect, two targets); `Of_HealingSkill_AimsAtNoOne`; `Of_Defend_AimsAtNoOne`.
  - `ImpactFile_Attack_IsTheAttackersStrip` (read from the actor's record); `IsMirrored_PartyActor_IsTrue`;
    `IsMirrored_EnemyActor_IsFalse`.
  - `SinceImpact_HalfwayThroughTheWindUp_IsNegative`; `SinceImpact_AtTheEnd_IsZero`.
- `EffectStripTests`: `FrameAtImpactTime_BeforeTheLead_IsNone` (hit frame 2 at 80 ms, 0.17 s before: none);
  `FrameAtImpactTime_AtTheLeadsStart_IsTheFirstFrame` (0.16 s before: 0); `FrameAtImpactTime_AtTheImpact_IsTheHitFrame`;
  `FrameAtImpactTime_LeadLongerThanTheWindUp_SkipsItsFirstFrames` (hit frame 10 at 80 ms, 0.7 s before: frame 1);
  `FrameAtImpactTime_HitFrameZero_StartsAtTheImpact` (today's strips: none just before, 0 at the impact).

## Verify by hand
1. With today's strips nothing changes (their hit frame is 0).
2. Put `[{ "file": "AttackPlayer.png", "frameMilliseconds": 80, "hitFrame": 2 }]` in `content/effect-strips.json`:
   Sarxian's strip now starts on the goblin a little before its HP drops, its third frame on the hit. Make the goblin
   miss (a few tries): the strip starts, then stops when "Miss" shows. Revert the file.
3. A healing skill: no strip on its target before the heal.
