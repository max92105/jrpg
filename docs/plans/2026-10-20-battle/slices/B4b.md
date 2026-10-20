# B4b. Fighting with Attack, Defend and Run

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B4b. Fighting with Attack, Defend and Run** (Game, about 170 lines)
  - **Goal:**
    - On a party turn, a plain list beside the actor shows the menu, with a marker over each selected target.
    - Battles can be won and lost.
    - Until the end screens: after a victory, the scene calls `FinishBattle` and returns `BattleFinished`; after a defeat, it returns `TitleRequested`. Each comes once.
  - **New types (Game):** `Battle/CommandListView`: the plain list and target markers.
  - **Changes:** `BattleScene` feeds `CommandMenu`'s commands to the sequence. The B1b flow test now presses Up once to reach Run (requested change).
  - **Tests:** `BattleScene_Update_AttackUntilOver_RequestsBattleFinished`, on a test catalog with one weak enemy so the outcome doesn't depend on balance (devil's advocate B-2). Also `BattleScene_Update_DefendOnPartyTurn_PassesTheTurn`.
  - **Verify by hand:**
    - [ ] Equip the Iron Sword, then beat the goblins with Attack.
    - [ ] Defend passes the turn.
    - [ ] Cancel while aiming returns to the list.
    - [ ] Losing (a near-dead leader set in a save) goes to the title.
