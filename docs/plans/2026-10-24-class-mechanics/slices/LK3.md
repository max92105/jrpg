# LK3. Formation look

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (`SwapSlide`), "Structure" (Game). Look at `docs/ui_ux_exemples/combat_turn_order_exemple.webp` first: the turn strip's diamonds and the battlers' line.

- **LK3. Formation look: battlers slide when they swap; the aura's reach and the readied turn are shown** (Game, about 200 lines)
  - **Goal:**
    - Two battlers trading places slide to their new spots over `SwapSlide`, easing, instead of jumping. Their panels slide too.
    - While an aura is being aimed, the covered allies' spots get a soft blue floor glow, so the range reads before Confirm.
    - A readied extra turn's diamond in the turn-order strip has a small hourglass corner mark, so it reads as an extra turn.
  - **New types:** Game `Battle/FormationSlide` (internal sealed): the swap's progress for two combatants, from `PlacesSwapped`, giving each one's interpolated spot.
  - **Changes (Game):**
    - `ActionPlayback` starts a `FormationSlide` on `PlacesSwapped`; `BattlersView` and `PartyStatusView` read it.
    - `CommandPanelView` (or `BattlersView`) draws the floor glow for an `AlliesInRange` choice.
    - `TurnOrderView` marks extra turns. `TurnOrderStrip` takes an `IsExtra` flag per entry, so Core needs `UpcomingTurns` with that flag: a small read addition to `BattleState`, tested in Core.
  - **Tests:**
    - `FormationSlideTests`:
      - `SpotOf_Halfway_IsBetweenSpots`
      - `SpotOf_AfterSlide_IsNewSpot`
    - `BattleReadyTests`: `UpcomingTurns_Readied_FlaggedExtra`
    - `TurnOrderStripTests`: `Refresh_ExtraTurn_EntryMarkedExtra`
  - **Verify by hand:** a Move slides both battlers; aiming Aura glows the covered spots; a readied turn's diamond shows its mark.
