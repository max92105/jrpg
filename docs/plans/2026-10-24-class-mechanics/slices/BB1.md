# BB1. The barbarian: ferocity from hits; Taunt draws attacks

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (barbarian rows, Taunt), "Risks" (seeded tests). Decisions 190, 199; questions 208, 209.

- **BB1. The barbarian: ferocity from hits taken; Taunt draws the enemies' attacks** (Core + Data + content + Game text, about 260 lines)
  - **Goal:**
    - Jazog has Ferocity (max 10, starts at 0) and gains 2 for each hit that costs him HP (question 208).
    - Taunt, a free skill and part of his kit (decision 199), makes every enemy attack go to him for 2 of his turns. It's self-granted, so the fresh mark gives it its full two turns.
    - Rampage (6 ferocity) is a crushing blow.
    - Without a taunt, enemies choose exactly as before, with the same random calls.
  - **Changes:**
    - Core:
      - `Resources/ResourceGains.PerHitTaken`;
      - `Statuses/StatusKind.Taunt`;
      - `Battle/ResourceTriggers.OnHitTaken(Combatant target, int hpLost, List<BattleEvent> events)`, called by `ApplyToTargets` after a damage hit that took HP;
      - `Battle/EnemyTactics.ChooseCommand`: the living members with Taunt if any, else all living members; then one `NextBelow` over that list. With no taunter, the list and the call are today's.
    - Data: `ResourceDefinitionValidator` accepts the gain.
    - Content:
      - `classes.json`: the barbarian gets `ferocity` and `skillIds` `taunt`, `rampage`;
      - `skills.json`: the two skills (`taunt`: `grantStatus`, `self`, Taunt for 2 turns).
    - Game: `ui.json` and `MessageIds`: `status_taunt` ("Taunt").
  - **Tests:**
    - `ResourceTriggersTests`:
      - `OnHitTaken_MemberWithPerHitGain_GainsAndReports`
      - `OnHitTaken_NoHpLost_GainsNothing`
      - `OnHitTaken_Enemy_GainsNothing`
    - `EnemyTacticsTests`:
      - `ChooseCommand_OneMemberTaunting_AttacksThem`
      - `ChooseCommand_TwoTaunting_PicksAmongThemOnly`
      - `ChooseCommand_NoTaunt_SameTargetAsBeforeForSequence` (pins today's choice)
      - `ChooseCommand_TaunterFallen_AttacksAnyLivingMember`
    - `BattleStateResourceTests`: `Execute_EnemyHitsBarbarian_GainsFerocity`
    - `BattleStatusTests`: `Execute_SelfTaunt_LastsTwoOwnTurnsAfterCast`
  - **Verify by hand:**
    - As Jazog: ferocity +2 per goblin hit.
    - With a recruit: after Taunt, every goblin attacks Jazog for two of his turns.
    - Rampage greys until 6.
