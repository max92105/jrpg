# ST1. The Status page shows twenty-four stats

Part of [the affix stats plan](../README.md). Decision 350 (the Status page shows everything), decision 316 (its layout). After A4. Reference: `docs/ui-reference/menu-status.png`.

- **ST1. The Status page shows twenty-four stats** (Game + text, raw 160, about 250 lines at 1.6x: a layout pass)
  - **Goal:** the four affix stats join the Status page beside the twenty: Lifesteal, Armor penetration and Block with the Combat column, Drop luck where the page's arithmetic places it; a stat at 0 shows 0 (the page shows everything). The stats plan's S1a proved the page for twenty numbers in 1366 × 848; this slice redoes that arithmetic for twenty-four before anything is drawn, and picks a layout that fits: a fourth band, a taller Combat column with 32 px rows, or the Resources section moving beside Skills.
  - **Changes:**
    - `Menus/StatusSheet`: the four values, formatted once.
    - `Menus/StatusParametersView` (or a new band view, if the arithmetic says the middle band can't take four more rows): the rows in `SheetRowLook`.
    - The written arithmetic in this file's Done note: every band's height, row count and gaps, summed against 848 px.
  - **Tests:**
    - `StatusSheetTests: Of_LifeStealWorn_ShowsItsPercent`
    - `StatusSheetTests: Of_NothingWorn_ShowsFourAffixStatsAtZero`
    - `StatusPageSheetTests: Open_MemberWithDropLuck_ShowsDropLuck`
  - **Verify by hand:** before and after F12 captures of the Status page with a member wearing all four affixes; nothing overlaps; the page still reads like the reference.
