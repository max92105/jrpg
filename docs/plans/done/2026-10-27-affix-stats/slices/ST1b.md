# ST1b. The Affixes section: twenty-four stats

Part of [the affix stats plan](../README.md). Decisions 350, 368. After ST1a. Reference: `docs/ui-reference/menu-status.png`. Builds on ST1a's arithmetic and design.md's tunables (the text keys).

- **ST1b. The Affixes section: twenty-four stats** (Game + text, raw about 80 lines, about 130 at 1.6x)
  - **Goal:** the four affix stats join the Status page in the middle band's right half, under the Resistances, where Resources stood: an "Affixes" section of two columns of two rows (Lifesteal and Penetration, then Block and Drop luck, in `StatKinds.AffixOnly` order). A stat at 0 shows "0 %" (the page shows everything, decision 350).
  - **The arithmetic** (decision 368): the section starts 46 + 4 × 34 + 16 = 198 under the band's top, its rows 46 under that: 198 + 46 + 2 × 34 = 312, the band's height before ST1a. `ContentHeight` = 20 + 232 + 312 + 16 + 256 = 836 of 848. The right half is (1044 − 48) ÷ 2 = 498 wide; each column (498 − 12 − 24) ÷ 2 = 231, like the Resistances' columns, its cells 231 wide.
  - **New types:** none.
  - **Changes:**
    - `Menus/StatusSheet.Affixes` (`string[]`): each affix-only stat's value as `StatText` writes it, in `StatKinds.AffixOnly` order.
    - `Menus/StatusLabels.Affixes` (`string[]`): their labels in caps, each `StatNames.IdOf(kind)` except Armor penetration, which uses the short `MessageIds.StatusPenetration`.
    - `Menus/StatusParametersView`: the Affixes section under the Resistances (its `SectionTitle` with `MessageIds.StatusAffixes`); the Resistances' column drawing takes the rows per column (4 or 2) so both sections share it; `Height` is again the taller of Combat (284) and Resistances with Affixes under them (312). The summary names the section.
    - Text: `status_affixes` ("Affixes", placeholder) and `status_penetration` ("Penetration", placeholder) in ui.json; `MessageIds`.
    - `docs/map.md`'s Menus line: `StatusSheet` lists Affixes; `StatusParametersView` "Combat beside Resistances and Affixes".
  - **Tests:**
    - `StatusPageSheetTests: Open_LeaderWearingLifeSteal_ShowsItsPercent` (a rolled ring with a `life_steal` affix: the shown value is `StatText`'s and differs from before)
    - `StatusPageSheetTests: Open_NothingWorn_ShowsFourAffixStatsAtZero`
    - `StatusPageSheetTests: Open_Member_ListsAffixLabelsInStatKindOrder` (LIFESTEAL, PENETRATION, BLOCK, DROP LUCK)
    - `StatusPageSheetTests: ContentHeight_ProfileBandAndThreeSkills_FitsThePage` passes unchanged (836).
  - **Watch:** `StatusPageSheetTests.cs` 145 → about 185.
  - **Verify by hand:** F12 captures with a member wearing all four affixes (a Debug save) and one wearing none: the Affixes section lines up with the Resistances above it, "PENETRATION" and its value fit their cell, nothing overlaps Skills; the page still reads like the reference.
- **Done, with deviations:** `StatusParametersView` builds the Resistances' and Affixes' `SheetRowLook.Cell`s once in `Show` and draws both through one `DrawInTwoColumns(frame, section, cells, rowsPerColumn)`, so a resistance's tint is worked out when the member changes, not each frame. The zero test is `Open_NoAffixWorn_ShowsFourAffixStatsAtZero` (the leader wears plain starting gear, not nothing). `ContentHeight` is 847 of 848 (ST1a's honest Skills room). Captured the warlock: the Affixes section lines up under the Resistances, PENETRATION and its value fit their cell, the three skills still show. Not captured: a member wearing all four affixes (no save on this machine has one); a value up to "15 %" is one digit wider than the "0 %" shown. Tests 2801 → 2804. The review's suggestions taken: `ResistanceColumnGap` is `ColumnGap` (both sections use it) and `StatusSheetView`'s summary names the Affixes; left: drawing the two sections' titles inside `DrawInTwoColumns`, which would take it to five parameters.
