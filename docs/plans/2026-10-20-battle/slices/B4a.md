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
