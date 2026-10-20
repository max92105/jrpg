# B3c. Turn order bar

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B3c. Turn order bar** (Core + Data + content + Game, about 280 lines)
  - **Goal:**
    - At the top left, the acting combatant in a 140 px diamond with a bright second frame.
    - Then the next eight of the cycle in small touching diamonds on a line, labeled "NEXT". Enemies' diamonds have a red lower half.
    - It refreshes at each turn start.
  - **Core:**
    - `TurnOrder.Upcoming(int count)`: the next `count` living combatants after the current one, walking the cycle, wrapping, skipping the fallen.
    - `BattleState.UpcomingActors(int count)`: the turns after the current one, as the order stands now (a fall changes it).
  - **New types (Game):**
    - `Battle/TurnOrderStrip` (pure): who the bar shows and who is lit. `Refresh(BattleState battle)`, `Shown`, `Highlight(IReadOnlyCollection<Combatant> targets)` (empty lights everyone), `IsLit(Combatant combatant)`.
    - `Battle/TurnOrderView`: draws the bar.
  - **Data and content:** `ArtReferenceCheck` covers enemy portraits (exact names), which catches `goblin.png`. The content fix changes `goblin`'s `portraitFile` to `Goblin.png`.
  - **Tests:**
    - Core: `BattleState_UpcomingActors_FourCombatants_ListsCycleAfterCurrent`, `BattleState_UpcomingActors_DefeatedCombatant_IsSkipped`, `BattleState_UpcomingActors_CountPastCycle_Wraps`.
    - Strip: `TurnOrderStrip_Refresh_ListsCurrentThenEightNext`, `TurnOrderStrip_Highlight_DimsEveryoneElse`, `TurnOrderStrip_HighlightNobody_LightsEveryone`.
    - Data: `ArtReferenceCheck_FindProblems_EnemyPortraitNameDiffersInCase_NamesTheEnemy`.
  - **Verify by hand:**
    - [ ] Compare with `Combat5.webp`.
    - [ ] The order advances each turn.
