# Z. Demo walk-through against the captures and the index

Part of [the UI second pass plan](../README.md). Manual. After every other slice.

- **Z. Demo walk-through** (manual)
  - **Goal:** the developer plays the demo path once, captures each step with F12 (`after` subfolder) and compares it with S0's `before` capture, its screenshots in `docs/ui-reference/` and its section of `export-index.md`.
  - **Path and what to check:**
    1. Start: the title fades in (picture, serif name, entries); the selection is obvious, glides, the diamond bobs; the footer line.
    2. New game: the leader choice (serif name, the shared selection, the titled yes/no).
    3. The intro's stills, then the map: the dialogue box's frame; the interaction prompt; sizes readable.
    4. The innkeeper: the greeting, the fade to black, "the party rests".
    5. The menu, opened first thing after loading (BG1: the fade plays): tabs with glyphs slide in; the selection glides; Status (member column, level, class, speed, resources, skills, caps labels, large numbers); Items (column titles, the description strip, a potion's notice); Equipment ("HP"); Party, Save (the yes/no), Options, System; Esc fades the menu out.
    6. A forest encounter: freeze, flash, the glass breaks and flies; action names in the framed box; the command rows in the shared selection look; victory ("Victory!" serif) and, in a second fight, defeat.
    7. Coherence (decision 252): every selected row in every screen above looks the same.
  - **Record:** the findings with the slice each belongs to; new ones as follow-ups in the README (a bug goes before the retro).
  - Then the plan is `retro pending`.
- **Walk-through, part 1 (2026-10-26, by Claude, the machine free):** steps 1, 2, 3 (to the map prompt) and 5, captured with the game's F12 into `docs/captures/after/` (15 JPEGs, quality 90, named like the `before` set). Steps 4 (the inn) and 6 (an encounter, victory, defeat) are the developer's: walking blind from the save to the inn or the forest stalled.
  - **Matches the plan:** the title (serif name, the selection band and bobbing diamond, the footer line); the leader choice and its titled yes/no; the menu's tabs with glyphs and the glide; Status (member column, level beside the name, speed, resources, skills, spaced-caps labels, large values, hairlines); Items (spaced-caps column titles, the description strip, the dimmed unusable rows); the Equipment choice; Options and System in the shared selection look.
  - **Findings:**
    1. **Contrast on the menu pages (SZ1, M3b, M4):** the pages' text sits straight on the bright map art with no panel behind it: unusable items, section titles ("Experience", "Parameters"), the Equipment page's empty slots and the skills' descriptions are hard to read; the leader choice's class description too. The reference puts lists on dark translucent layers. Follow-up: a translucent panel behind the page area (like the sidebar's) or a stronger menu scrim; the developer decides which.
    2. **The item strip hides the next row (M4):** the description strip covers the row under the selected one (Hi-Potion disappears while Potion is selected), as planned, but it reads as a missing item. Follow-up: the developer decides: keep it, push the rows below down, or show the description in a fixed line.
    3. **Equipment choice preview crowding (M2b, decision 266):** with the item choice open, the sheet narrows and the attack preview ("25 ◆ 25") runs into the Defense column. Follow-up: preview values in their own column, or the parameters on two lines while the choice is open.
    4. **Leader choice labels (coherence, M3b):** "Level", "HP", "Attack", "Defense" there are small and mixed case, unlike the Status page's spaced caps with large values. Follow-up: the same row look.
    5. **Equipment page rows (M3b):** still the old look (backlog line from M3b): the same row look as Status when touched.
    6. **Capture tooling:** `play-game.ps1`'s screen copy is black for the borderless full-screen game (it only worked while another app's bar forced the desktop to compose); the game's own F12 works. Follow-up for the retro: the script presses F12 and collects the file from `docs/captures/`, waiting for the write before it closes the game.
- **Walk-through, part 2 (2026-10-26, by the developer):** steps 4 (the inn) and 6 (the encounter's freeze, flash and shatter, the action-name box, the command rows, victory and defeat) all good.
  - **Findings 1, 2 and 4 kept as they are** (the developer, "all is good"). Findings 3 and 5 wait in `docs/plans/backlog.md`; finding 6 was done in the retro (`play-game.ps1` saves the game's F12 capture).
