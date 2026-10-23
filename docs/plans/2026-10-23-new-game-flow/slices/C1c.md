# C1c. A level-up raises the stats by the class's growth

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Core changes". ADR 0017.

- **C1c. A level-up raises max HP, max MP, attack and defense by the class's growth** (Core + Game, about 150 lines; decision 164)
  - **Goal:**
    - When a member levels up, their base stats and maxima become their class's at the new level.
    - Current HP and MP rise by the max gained, so a level-up never leaves a member at a lower share of their HP.
    - The victory card and the Status page show the new numbers.
  - **Changes:**
    - Core:
      - `Stats/Vitals.RaiseMaxima(int maxHpGain, int maxMpGain)` adds the gains to both max and current values; a dead member stays at 0 HP.
      - `PartyMember`: `BaseStats` is computed from the level, or reset on level-up. On each level-up step, it raises the vitals by the difference between the class's stats at the new and old levels.
    - Game: nothing should need changing, since the victory card and Status page read the member. Check it on the hand check, and note any screen that cached a number.
  - **Tests:**
    - `VitalsTests`:
      - `RaiseMaxima_Alive_AddsToMaxAndCurrent`
      - `RaiseMaxima_Dead_StaysAtZeroHp`
    - `PartyMemberTests`:
      - `GainExperience_OneLevelUp_RaisesStatsByGrowth`
      - `GainExperience_TwoLevelUps_RaisesByTwoGrowths`
      - `GainExperience_NoLevelUp_StatsUnchanged`
  - **Verify by hand:** win the goblin fight with a level 1 leader. The victory card shows the level-up, and the Status page shows the higher max HP and attack.
