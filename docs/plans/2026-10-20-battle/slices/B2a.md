# B2a. Backdrop, formation and background check

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B2a. Backdrop, formation and background check** (Data + Game, about 200 lines)
  - **Goal:**
    - The background has the legacy vignette and top shade.
    - Each combatant's spot is marked in the legacy formation: enemies on the left in encounter slots, the party on the right, rows stepping up and sideways. The marks are placeholders until B2b.
  - **New types (Game):**
    - `Battle/BattleLayout` (pure): where each battler stands. `Rectangle EnemySpot(int slot)`, `Rectangle PartySpot(int index, int count)`; the leader is on the top row.
    - `Ui/Vignette` (pure): a radial vignette's alpha per pixel.
    - `UiTextures.BattleVignette`, `TopShade`.
  - **Data:** `ArtReferenceCheck` covers encounter backgrounds, matching file names exactly (ordinal, case included). That shared helper serves every battle art check after it.
  - **Tests:**
    - Layout: `BattleLayout_EnemySpot_SecondSlot_IsOneRowUpAndOneShiftRight`, `BattleLayout_PartySpot_Leader_IsTopRowFurthestLeft`, `BattleLayout_PartySpot_LastMember_IsBottomRow`.
    - Vignette: `Vignette_Alphas_Center_IsClear`, `Vignette_Alphas_Corner_IsDarkest`.
    - Data: `ArtReferenceCheck_FindProblems_MissingEncounterBackground_NamesTheEncounter`, `ArtReferenceCheck_FindProblems_BackgroundNameDiffersInCase_NamesTheFile`.
  - **Verify by hand:**
    - [ ] Corners darkened like `Combat3.webp`.
