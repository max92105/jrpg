# F1d. The Forge's look

Part of [the forge plan](../README.md). Decisions 252 (one selection look), 269 (a page's dark column), 335 (section bands), 370 (it fades itself in). After F1c. Was the look half of the draft's F1. References: `docs/ui-reference/shop-sell.webp` (an NPC-opened screen: header, list with a value column, the scene dimmed behind), `menu-equipment.webp` (a list beside a detail column), `menu-status.png` (sections under dark bands, values beside spaced-caps labels). Design: [design.md](../design.md), "Tunables" (the fade), "Replacement mapping".

- **F1d. The Forge's look: bars, columns, bands and its fade** (Game, raw 150, about 240 lines at 1.6x: adds two view types)
  - **Goal:** the Forge in the game's look: the map dimmed behind it, a header bar with "Forge" at top left, the list in a page column (decision 269) with column titles, the detail column under `SectionTitle` bands ("Affixes", "Materials"), the materials as `SheetRowLook` rows, a footer with its hint. It fades in when it opens and out when it closes, as the menu does (decision 370), with no fade to black.
  - **New types:**
    - `Game/Crafting/ForgeListView`: draws the list column: page column, column titles, rows (icon, name in rarity color), the gliding selection, the empty line.
    - `Game/Crafting/ForgeDetailView`: draws the detail column: the piece's name and rarity, the Affixes band and lines, the Materials band and rows.
  - **Changes:**
    - `Crafting/ForgeView`: the dim layer, header and footer, and the two views at their places (written below); the opacity from the scene.
    - `Crafting/ForgeScene`: the fade in (0.15 s) and out (0.12 s), with `Ui/Easing` (the menu's times, `Menus/MenuMotion`; placeholders); Cancel starts the fade out, and `ForgeClosed` comes once it has run, once.
    - Text `forge_hint` (the footer's line); `MessageIds.ForgeHint`.
  - **Pixel arithmetic** (1920 x 1080, the menu's frame: header 88, body from 128, footer 72 from 1008, 32 above it, so the body ends at 976: 848 px):
    - List column, x 40 to 1000: column titles 36 + 12 visible rows x 52 (the Inventory's row) = 660 ≤ 848; more pieces scroll (`ListCursor`'s window).
    - Detail column, x 1040 to 1880: name row 56; Affixes band 46 (36 + 10, as the Equipment sheet) + 3 rows x 40 = 166; gap 24; Materials band 46 + 2 rows x 40 = 126; gap 24; F2a's Actions band 46 + 3 rows x 48 = 190; F2a's reason or F2b's notice line 40. Total 626 ≤ 848: 222 px spare, room for five more materials at 40 each before anything moves.
  - **Tests:**
    - `ForgeSceneTests: Update_Cancel_RequestsCloseOnceTheFadeHasRun` (replaces `Update_Cancel_RequestsCloseOnce`: Cancel, then updates through 0.12 s; one `ForgeClosed`)
    - Existing: `GameFlowForgeTests: Update_CancelInTheForge_ContinuesTheEvent` steps through the closing fade (its updates, not its assertion, change).
    - Draw-only: before and after captures (F12) instead of invented tests.
  - **Watch:** the look must be the menu's (one selection look, the same bands and rows); ask before departing from both the reference and the menu.
  - **Verify by hand:** at the forge, capture with F12 and compare with the references: the map dimmed behind; the header, columns, bands and footer; the Forge fades in and out over the map, never through black; a long list scrolls with the highlight.
