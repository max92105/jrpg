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
- **Done, with deviations:** no reusable pool: each turn's `ActionPlayback` builds its numbers once (one per landed effect, text formatted once), and nothing is allocated per frame, which the pool was for. `ActionPlayback` gains its own clock (`Update`, `Elapsed`), advanced by the sequence even while a command is awaited, so numbers and falls finish playing; `FallOpacityOf(combatant)` (null when they didn't fall). Any combatant that falls fades, as legacy did, not only enemies. The white tint is a white silhouette (`UiTextures.WhiteSilhouette`) drawn over the sprite at 60 % over it, since there are no shaders. Numbers are drawn by a new `FloatingNumbersView` through `OutlinedFont.DrawCentered`; colors `UiTheme.FloatingHeal` and `FloatingMp`. The look is the record `FloatingNumberLook` (scale, rise, opacity). Added `FloatingNumber_At_EndOfLife_HasRisenFullyAndIsGone`. After review: `BattlersView.SpotOf` and `TopOf` give the stepped position, so numbers (and the target pointer) follow an actor who targets themselves; `BattleTurnSequence.OpacityOf` holds the alive/falling/gone rule for the battlers and the tags.
