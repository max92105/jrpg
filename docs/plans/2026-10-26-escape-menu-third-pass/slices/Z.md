# Z. Menu walk-through against the reference and the 2026-10-26 captures

Part of [the escape menu third pass plan](../README.md). Manual. After every other slice.

- **Z. Menu walk-through** (manual)
  - **Goal:** the developer walks the menu once more, captures each screen with F12 into `docs/captures/<date>-menu-third-pass/` under the 2026-10-26 names, and compares each with `docs/captures/2026-10-26-menu/` and its reference shot.
  - **Captures to redo** (the 2026-10-26 set, plus the new screens):
    - `menu-tabs` (no left edge, the new selection; `menu-tabs.webp`);
    - `menu-inventory` (the folded tabs, rail, list column, map gap, sidebar; `menu-inventory-description.webp`);
    - `menu-inventory-use` → now `menu-inventory-aim` (the pointer on the sidebar, lit cards, HP previews; `menu-inventory-target.webp`), and new `menu-inventory-aim-again` (after a use, copies left);
    - `menu-party`, `menu-party-action`, and new `menu-party-notice` (the OK dialog; `menu-skill-learned-dialog.webp`);
    - `menu-equipment`, `menu-equipment-choice` (two columns, no narrowing; `menu-equipment.webp`);
    - `menu-status` (an 1100 px column, the map on its right, the member column strip; `menu-status.webp`);
    - new `menu-options` (missing from the 2026-10-26 set);
    - `menu-system`, `menu-save` (the yes/no prompt; `dialog-confirm.webp`);
    - new `menu-tabs-folding` (a capture mid-fold);
    - the selection elsewhere (decision 270): new `title`, `leader-choice`, `battle-command`, `battle-defeat`.
  - **Check:**
    - the nine findings of 2026-10-26 are answered;
    - one selection look on every screen above (decisions 252, 270), readable on the battle's command rows and the title;
    - one column look on every page (decision 269); the side column's darkness against the page column and the map (`SideColumn`, PC1 and PC2); whether the flat fills want a gradient;
    - the tabs fold and unfold smoothly, with the page moving as one (decision 286);
    - the placeholders in design.md's tunables, kept or changed.
  - **Record:** findings with the slice each belongs to; new ones as follow-ups in the README (a bug goes before the retro).
  - Then the plan is `retro pending`.
