# B4c. Command panel and target pointer look

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B4c. Command panel and target pointer look** (Game, about 260 lines)
  - **Goal:** the list becomes the legacy `CommandPanel`, leaning toward `Combat9.webp`:
    - a pointed header tab in the selection gradient, with the actor's name;
    - a thin double frame;
    - rows with command icons;
    - the selected row in the selection gradient with a bright line and a pointer;
    - a one-line hint under the panel (legacy wording in `ui.json`).
    Placement: the panel sits left of the actor, clamped on screen. Targets get a bobbing pointer, and the turn order dims everyone not targeted.
  - **New types (Game):**
    - `Battle/CommandPanelPlacement` (pure): where the panel goes for an actor's spot.
    - `Battle/CommandHeaderShape` (pure mask, through `CanopyMask.Build`).
    - `Battle/PointerBob` (pure): `static float OffsetAt(TimeSpan elapsed)`.
    - `Battle/CommandPanelView`: draws the panel.
  - **Tests:**
    - Placement: `CommandPanelPlacement_Place_BottomRowActor_ClampsAboveMaxHeight`, `CommandPanelPlacement_Place_TopRowActor_StaysBelowHud`.
    - Shape and bob: `CommandHeaderShape_Pixels_Tips_AreClear`, `PointerBob_OffsetAt_HalfPeriod_IsFullBob`.
  - **Verify by hand:**
    - [ ] Compare with `Combat9.webp` and legacy `CommandPanel.xaml`.
    - [ ] If `TargetArrow.png` isn't transparent, the pointer is drawn as a shape (decision 56).
