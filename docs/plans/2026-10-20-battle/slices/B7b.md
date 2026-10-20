# B7b. Victory look

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B7b. Victory look** (Game, about 260 lines)
  - **Goal:** the legacy `VictoryScreen` layout, leaning toward `Victory_Menu.jpg`:
    - a left-dark shade over the battle (dimmed, no blur; decision 52), with the HUD hidden;
    - "Victory!" sliding in, with a hairline;
    - one pointed reward bar per reward row (only "EXP 120" now; gold and drops add rows later);
    - two columns of member cards with pointed name tabs, "Lv", "EXP n / m" and orange bars;
    - the "Level Up!" pop;
    - the footer's "Next" hint once done.
  - **New types (Game):**
    - `Battle/VictoryScreenView`: draws the screen; its reward rows are a list built from `VictoryRewards`.
    - `Battle/PointedBarShape` (pure masks: legacy `Shape.RewardBar`, `Shape.CardHeader`).
  - **Tests:** `PointedBarShape_RewardBar_PointedEnd_IsClearAtCorners`, `PointedBarShape_CardHeader_BothEndsPointed`, `VictoryScreenView_RewardRows_ExperienceOnly_IsOneRow`.
  - **Verify by hand:**
    - [ ] Compare with `Victory_Menu.jpg`: order, timing, Level Up pop.
