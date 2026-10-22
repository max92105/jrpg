# B3b. Enemy tags and letters

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B3b. Enemy tags and letters** (Game, about 160 lines; decision 99)
  - **Goal:** under each living enemy, its name and red HP bar. Enemies sharing a name get letters in formation order ("Goblin A", "Goblin B", "Goblin C"); a single one has none.
  - **New types (Game):**
    - `Battle/EnemyLabels` (pure): each enemy's shown name, built once per battle.
    - `Battle/EnemyTagView`: draws each living enemy's name and HP bar.
  - **Tests:** `EnemyLabels_Of_ThreeGoblins_LettersInFormationOrder`, `EnemyLabels_Of_UniqueNames_NoLetters`, `EnemyLabels_Of_MixedEncounter_LettersOnlyRepeatedNames`.
  - **Verify by hand:**
    - [ ] Tags sit under each goblin, and a bar shrinks as it is hit (after B4b).
- **Done, with deviations:** "Goblin A" is written through the new message `battle_enemy_letter` ("{name} {letter}"), so `EnemyLabels.Of` takes the names and `UiMessages`. Tags use `OutlinedFont` and the battle bar track from B3a.
