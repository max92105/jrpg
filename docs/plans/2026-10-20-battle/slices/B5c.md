# B5c. Floating numbers and fading enemies

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B5c. Floating numbers and fading enemies** (Game, about 220 lines)
  - **Goal:** on the impact beat, each landed effect pops its amount over the target:
    - white for damage;
    - green for healing and revive;
    - blue for MP.
    A fallen enemy shows a white tint, then fades out with its tag on the defeat beat.
  - **New types (Game):** `Battle/FloatingNumber` (pure): one number's scale, rise and opacity over time. `ActionPlayback` holds a fixed pool sized to the combatants.
  - **Tests:** `FloatingNumber_At_Start_IsPopScale`, `FloatingNumber_At_PastHalfLife_IsFading`, `ActionPlayback_Start_AttackLanded_AddsDamageNumberOnTarget`, `ActionPlayback_Update_FallenEnemy_FadesAfterTint`.
  - **Verify by hand:**
    - [ ] Numbers rise and fade like legacy.
    - [ ] Fireball shows three numbers.
