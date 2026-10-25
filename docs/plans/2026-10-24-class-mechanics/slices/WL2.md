# WL2. Instability can backfire

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (warlock row), "When things happen in a turn" (step 2), "Risks" (seeded tests). Decisions 194, 203; question 216.

- **WL2. Instability can backfire on the caster** (Core + Data + content + Game playback, about 230 lines)
  - **Goal:**
    - After a skill that changes a resource with a backfire rule, the caster rolls once.
      - The chance is 5 % per point of that resource after the cast.
      - A backfire costs the caster 10 % of max HP (at least 1), which can fell them.
    - The roll goes through the battle's `IRandom`, only when that resource is above 0, so other battles draw exactly as before.
    - The screen shows a "Backfire" cue and the HP lost on the caster, in its own beat after the impact.
  - **New types:**
    - Core `Resources/BackfireRule(int ChancePerPointPercent, int DamagePercentOfMaxHp)` (public sealed record): a resource's danger.
    - Core `Resources/BackfireFormula` (public static): `ChancePercent(int amount, BackfireRule rule)`, capped at 100; `Damage(int maxHp, BackfireRule rule)`, at least 1. The comment cites decision 194.
    - Core `Battle/Backfired(Combatant Caster, string ResourceId, int HpLost)` (BattleEvent).
  - **Changes:**
    - Core:
      - `ResourceDefinition.Backfire` (`BackfireRule?`);
      - `BattleState` keeps the `IRandom` its constructor already takes (no call site changes);
      - `UseSkill` ends with a private `RollBackfire(...)`: `NextBelow(100) < chance`, then `Backfired` and, on a fall, `CombatantDefeated` with the fall's triggers.
    - Data: `ResourceDefinitionValidator`: both percents are 1..100.
    - Content: the warlock's `instability` gets `"backfire": { "chancePerPointPercent": 5, "damagePercentOfMaxHp": 10 }`.
    - Game:
      - `PlaybackBeatKind.Backfire`;
      - `ActionPlayback` handles `Backfired` (a floating number and a cue on the caster);
      - `ui.json` and `MessageIds`: `battle_backfire`.
  - **Tests:**
    - `BackfireFormulaTests`:
      - `ChancePercent_FourPoints_Is20`
      - `ChancePercent_PastHundred_CapsAt100`
      - `Damage_SmallMaxHp_AtLeastOne`
    - `BattleBackfireTests` (new file, `FixedSequenceRandom` after the shuffle's draws):
      - `Execute_RollBelowChance_CasterLosesHpAndReportsIt`
      - `Execute_RollAtChance_NoBackfire`
      - `Execute_ZeroInstabilityAfterCast_DrawsNoRandom`
      - `Execute_BackfireFellsCaster_ReportsDefeat`
      - `Execute_SkillWithoutBackfireRule_DrawsNoRandom`
    - `ActionPlaybackTests`: `Start_Backfired_AddsBackfireBeatAfterImpact`
  - **Verify by hand:** as Sarxian against the dummy, at 7+ instability Chaos Lance now and then shows "Backfire" with an HP loss on him. At low instability it's rare.
