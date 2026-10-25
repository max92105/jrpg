# MP3b. Members and enemies lose MP

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Replacement mapping" (`Vitals`, `ClassStats`, `EnemyDefinition`). Decision 197; ADR 0022.

- **MP3b. Members and enemies lose MP** (Core + Data + content, about 260 lines, mostly deletions and test-site edits)
  - **Goal:** MP is gone from the model and content. `Vitals` is HP only, classes have no `maxMp`, and enemies neither. Record the solution's test count before and after.
  - **Changes:**
    - Core:
      - `Stats/Vitals(int hp, int maxHp)`: `Mp`, `MaxMp`, `TrySpendMp` and `RestoreMp` go, and `RaiseMaxima(int maxHpGain)` loses the MP gain;
      - `Party/ClassStats` loses `MaxMp` (RG1 adds `Speed`);
      - `Party/ClassGrowth` follows;
      - `PartyMember` (`Restore`'s MP comment from MP3a goes), `Battle/Combatant.FromEnemy` and `Battle/EnemyDefinition.MaxMp` follow.
    - Data: `ClassCatalogValidator` and `EnemyCatalogValidator` lose their MP checks.
    - Content: `classes.json` drops `maxMp` from `base` and `growth`; `enemies.json` drops `maxMp`, the training dummy included.
    - Tests:
      - `VitalsTests` loses its MP tests;
      - `ClassGrowthTests` and `PartyMemberTests` follow;
      - `PartyMemberBuilder`, `ClassBuilder` and `EnemyDefinitionBuilder` follow, as do `EffectResolverTests` (4 `new Vitals` sites), `TargetRulesTests`, `CombatantTests` and the validator tests.
  - **Tests:**
    - `ClassCatalogValidatorTests`: `Validate_MaxMpField_IsRejected` (an old-style file fails, naming the class)
    - `VitalsTests`: `RaiseMaxima_Alive_AddsToMaxAndCurrentHp` (the reshaped existing test)
  - **Verify by hand:** `git grep -n "Mp\b\|maxMp\|MaxMp" -- src content` finds nothing. A new game and a continued save both play.
