# SZ1. Sizing pass, menus

Part of [the UI second pass plan](../README.md). Decisions 251, 261. Design: [design.md](../design.md), "Tunables and placeholders" (sizing targets), "Risks" (sizing breaks layouts). After S0 and R1. Look at `docs/ui-reference/menu-tabs.webp`, `menu-status.webp`, `menu-inventory-description.webp`, `menu-equipment.webp` and S0's captures of every menu page first.

Developer: "look at the reference and go close to that; right now everything is too small, use more of the screen."

- **SZ1. Sizing pass, menus: text, numbers, rows, panels toward the reference** (Game, about 230 lines)
  - **Goal:** the game menu's frame and every page read at the reference's proportions on 1920 x 1080: larger body text and tab labels, taller rows and tabs, a larger header title and footer hint, larger numbers beside small labels in the sidebar, a wider sidebar.
  - **Steps:**
    1. Measure the reference shots at x1.6 against the captures (text cap heights, row pitch, panel widths) and write the chosen values in the Done note, against design.md's targets.
    2. Change the constants: `UiTheme.MenuTextPixels` and `MenuRowHeight`, `NavTabShape` (its height and shoulder follow the row), `MenuHeader`'s sizes and height, `GameMenuScene`'s footer and sidebar width, `MemberCardView` and `PartySidebarView` (names, HP numbers, labels), each page's own font and row constants (`StatusPage`, `InventoryPage`, `EquipmentSheetView`, `EquipmentChoiceView`, `PartyColumnsView`, `OptionsPage`, `SaveSlotListView`, `ItemTargetPickerView`).
    3. Fix what no longer fits: visible row counts, `PartySidebar.VisibleCards` (its summary's arithmetic), column widths. A page that can't hold its content at the target keeps the largest size that fits, noted in the Done note.
    4. Decision 261: the Equipment page's "Max HP" label becomes "HP" in `content/text/ui.json`; remove the backlog line.
  - Values change in place: files don't grow. If the step 3 fixes push a file near 300 (`InventoryPage` is already at 328), stop and note it for C5.
  - **Tests:** none new (sizes are look). Budget the existing tests that pin counts or places (`PartySidebarTests`, `InventoryPageTests` visible rows, `NavTabShapeTests`, `ItemTargetPickerViewTests`): update their expected values with a comment pointing to the new constant, about 30 lines.
  - **Verify by hand:** F12 every menu page again; compare before/after side by side with the reference: text clearly larger, nothing clipped or overlapping, the sidebar's cards readable, the footer hint centered, the Equipment page says "HP".
- **Done, with deviations:**
  - **Measured** on `menu-tabs.webp` and `menu-inventory-description.webp` at x1.6 against `docs/captures/before/menu-*.jpg`:
    - the reference's tabs are about 61 px tall on a 72 px pitch, with 29 px labels (ours: 56 / 70 / 20);
    - its list rows are 42 px with about 29 px text (ours: 44 / 18);
    - its header title is about 35 px (ours: 28), and its footer bar about 77 px (ours: 64).
  - **Chosen sizes:**
    - menu text 28, tabs 64 tall on a 74 px pitch, header 88 px with a 38 / 24 px title / info, footer 72;
    - page headings 30, body 26, labels and captions 18, list rows 52 px with 32 px icons;
    - member cards 132 px (28 px names, 24 px HP values beside 18 px labels, 96 px portraits), sidebar 520 px;
    - Status: a 240 px portrait and 46 px stat lines;
    - save rows 112 px; yes/no dialog text 30 / 24.
  - **Counts that changed:** the sidebar shows 5 cards (774 of the page's 848 px), and the save list 6 rows. The member card's label and bar now center on the larger value's line.
  - **Decision 261:** the Equipment page reuses `stat_hp` ("HP"); `stat_max_hp` and its backlog line are gone.
  - **Grew outside the menu:** the map's interaction prompt and the leader choice's hint use `UiTheme.MenuTextPixels`, so they grew to 28 too. SZ2 sets them.
  - **Tests:** none changed; every test reads the sizes through the constants (2095 before and after).
  - **Review:** the Status page's HP bar moved from 26 to 34 px under its line, so it clears the larger text.
