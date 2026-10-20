# B3a. Party status

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B3a. Party status** (Game, about 240 lines)
  - **Goal:** at the top right, each active member in the legacy `CharacterPanel` layout:
    - name, large HP over max, MP;
    - thin battle bars and a diamond portrait;
    - the acting member's panel slides left, and a fallen member's is dimmed with HP in red.
    Numbers are formatted only when they change.
  - **New types (Game):**
    - `Battle/VitalsText` (pure): one combatant's HP and MP as text, reformatted only when a value changed.
    - `Battle/PartyStatusView`: draws the party panels.
  - **Changes:**
    - `UiTheme` gains `BattleHpBar`, `BattleMpBar`, `EnemyHpBar`, `BattleBarTrack`.
    - `StatBar` takes its track color.
    - Text on the background gets the legacy outline.
  - **Tests:** `VitalsText_Refresh_SameValues_KeepsSameStrings` (same instances), `VitalsText_Refresh_HpChanged_ReformatsHp`, `VitalsText_Refresh_FallenMember_HpIsZero`. The existing `StatBarTests` pass unchanged.
  - **Verify by hand:**
    - [ ] Compare with `Combat6.webp`.
    - [ ] A hurt member (set in a save) shows the same numbers as the Status page.
