# MP2. No screen shows MP

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "What exists, checked" (MP everywhere), "Replacement mapping". Decision 197; ADR 0022.

- **MP2. No screen shows MP** (Game, about 230 lines)
  - **Goal:** MP disappears from every screen. The battle panel shows name and HP only until M3 puts the resources where MP was. The menus close up the space MP used.
  - **Changes (Game):**
    - Battle: `PartyStatusPanel` and `PartyStatusView` lose the MP row; `VitalsText` loses `Mp`/`MaxMp`.
    - Menus:
      - `StatusPage` shows HP, Attack and Defense; the first line is HP alone. Check the two-column layout still reads.
      - `SidebarCard` and `PartySidebar` lose MP.
      - `EquipmentSheet` and `EquipmentSheetView` lose the MP row; the stat preview never showed MP.
      - `ItemUseDialog` loses its MP mention.
    - Ui:
      - `MemberVitalBars` and `MemberCardView` keep the HP bar only;
      - `MessageIds.StatMp` and its `ui.json` line go.
    - NewGame: `LeaderProfile` lists HP, Attack and Defense at level 1.
    - Tests:
      - `VitalsTextTests`, `MemberVitalBarsTests` and `LeaderChoiceSceneTests` lose their MP cases;
      - the menu page tests that list stats follow.
  - **Tests:**
    - `StatusPageTests`: `ShownStats_Member_HasNoMp` (from the page's existing `ShownStats` accessor)
    - `LeaderProfileTests`: `Build_Character_ListsHpAttackDefense`
  - **Verify by hand:**
    - Battle: no MP row.
    - Status page, sidebar, equipment page and leader choice show no MP, and nothing is left with a gap or overlap.
