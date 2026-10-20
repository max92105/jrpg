# B4a. Command menu and target cursor

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B4a. Command menu and target cursor** (Game, about 230 lines)
  - **Goal:** the pure command choice for a party turn:
    - Attack, Skills, Items, Defend, Run, with Skills and Items greyed until B6;
    - Attack opens a target cursor on `TargetChoiceForAttack`;
    - Defend and Run act at once.
  - **New types (Game):**
    - `Battle/CommandEntry`: Attack, Skills, Items, Defend, Run.
    - `Battle/CommandMenu` (pure): the party actor's command choice, from the list down to a target. `Start(BattleState battle)` begins a turn on Attack; `Update` returns a `BattleCommand` or null.
    - `Battle/BattleTargetCursor` (pure): the targets picked from a `TargetChoice<Combatant>`.
      - It starts on the last living enemy, or on the actor when they are a candidate (decision 56).
      - `MovePrevious`, `MoveNext` (wrapping).
      - `Selected`: everyone for a "takes all" choice.
  - **Tests:**
    - Menu: `CommandMenu_ConfirmAttack_OpensTargetsOnLivingEnemies`, `CommandMenu_ConfirmTarget_ProducesAttackOnIt`, `CommandMenu_CancelOnTargets_ReturnsToCommands`, `CommandMenu_ConfirmDefend_ProducesDefend`, `CommandMenu_MoveDown_SkipsGreyedSkillsAndItems`.
    - Cursor: `BattleTargetCursor_Start_SingleEnemy_SelectsLastLivingEnemy`, `BattleTargetCursor_MoveNext_OnLastCandidate_WrapsToFirst`, `BattleTargetCursor_TakesAll_SelectsEveryCandidate`.
- **Done, with deviations:** test names drop the class prefix. Targets move back with Up or Left and forward with Down or Right. `BattleTargetCursor` exposes `Candidates` for the pointer (B4c). The repository battle the tests build moved to a shared `Battle/BattleScenarios` test helper (third copy), used by the turn sequence and turn order strip tests too. Review added tests for starting on the actor, stepping back past allies to the last enemy, the backward wrap and Run producing a flee.
