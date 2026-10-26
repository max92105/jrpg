# M3b. Status page look

Part of [the UI second pass plan](../README.md). Decisions 252, 261. Design: [design.md](../design.md), "Tunables and placeholders" (status labels). After M3a. Look at `docs/ui-reference/menu-status.webp`, `menu-field-skills.webp` (the sidebar's "HP 2649/2649" and "LV 35": small labels, large numbers) and the index's "Status" section first.

- **M3b. Status page look: spaced-caps labels beside large numbers, hairline rows** (Game, about 170 lines)
  - **Goal:**
    - Each parameter row reads like the reference: the label small, in spaced caps and secondary color (`HP`, `ATTACK`, `DEFENSE`, `SPEED`, `LV`, `NEXT`), the value large and right-aligned in its column, a 1 px hairline under the row; skills and resources take the same rows.
    - "LV" plus a large number beside the name; the experience bar keeps its place.
  - **Changes (Game):**
    - `Menus/StatusSheet`: caps labels built with the sheet (`ToUpperInvariant` once), value widths measured then (`MeasuredText`), so drawing allocates nothing.
    - `Menus/StatusSheetView`: label and value fonts (SZ1's sizes), letter spacing 2 for labels (`DrawText(..., characterSpacing:)`), the hairline in `UiTheme.Stroke`.
  - **Tests** (`StatusPageTests`): `Open_FirstMember_LabelsAreInCaps` (through `ShownLabels`, like `ShownStats`).
  - **Verify by hand:** compare with `menu-status.webp`: small caps labels, large numbers on their right edge, hairlines between rows, nothing overlapping for the longest class, skill and item names; numbers change when equipment changes.
