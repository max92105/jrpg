# LK1. The selection retuned; the tabs open at the screen edge

Part of [the escape menu third pass plan](../README.md). Decisions 252, 270, 271, 272. Design: [design.md](../design.md), "Tunables and placeholders" (selection and tabs), "Key decisions" (tab-only colors, the fixed glow, the off-screen edge). First slice. Look at `docs/ui-reference/menu-tabs.webp` and `menu-inventory-description.webp` (the selected row) first, and `docs/captures/2026-10-26-menu/menu-tabs.jpg`, `menu-system.jpg`.

- **LK1. The selection retuned (navy to black, a fixed glow by the pointer); tab-only colors; the tabs 1 px off-screen** (Game, about 110 lines)
  - **Goal:** every selected row and the selected tab read as the reference's: a deep navy band fading to black with a soft white glow by the pointer (near the point on a tab), faint hairlines. The tabs show no vertical line where they meet the screen's left edge. The battle's command header and turn order look as before.
  - **New types:** none.
  - **Changes (Game):**
    - `Ui/UiTheme`: `SelectedRow` retuned (navy to black); new `SelectionGlow` and `SelectionGlowWidth` (160 px), new `SelectionHairline`. New tab-only `SelectedTab` and `SelectedTabStroke`; `SelectionLine` renamed `SelectedTabLine` and softened. `Selection` and `SelectionStroke` don't change.
    - `Ui/UiTextures.DrawSelectedRow`: the band, then the glow as a fixed-width piece at the row's left (`SelectionGlowWidth`, or the row's width if narrower), then the two `SelectionHairline` lines. `HighlightLine` stays for the inventory's tip strip, the command panel's box line and `BattleScene`'s line.
    - `Ui/UiTextures.SelectedNavTab` and `SelectionLine`: baked from `SelectedTab`, `SelectedTabStroke`, `SelectedTabLine`. `CommandHeader` keeps reading `Selection` and `SelectionStroke`, so it needs no pin.
    - The two other menu readers of `SelectionStroke`:
      - `Menus/CategoryRailView`: its selected tab is a selected entry, so it draws `DrawSelectedRow` (the shared look, decision 252) instead of its own `SelectedRow` gradient with a `SelectionStroke` outline;
      - `Menus/PartyColumnsView`: the moving member's 2 px outline keeps `SelectionStroke`; it marks the member being moved, not the selection.
    - `Battle/TurnOrderView` reads `SelectionStroke` (the current turn's diamond and the extra-turn mark): it must not change, and doesn't, since `SelectionStroke` is untouched.
    - `Menus/MenuTabsView`: `TabLeft` subtracts a named `OffScreenEdge` = 1, so each tab's outlined first column sits off-screen; the glow, line, icons and labels follow it. `NavTabShape`, `OutlinedShape` and their tests don't change.
    - `.claude/rules/game.md` "UI look": the selected tab reads "deep navy fading to black, a soft white glow near the pointer" (decision 270).
  - **Tests:** none new (colors and draw positions; captures). Existing tests pass unchanged.
  - **Verify by hand:** F12 before and after on the tabs, Inventory (rail and list), System, the title screen, the leader choice, a yes/no prompt, a battle's command rows and the defeat menu.
    - Every selected row is the navy band with the glow by the pointer; the glow is the same size on a yes/no button and on a long row.
    - The tabs touch the screen's left edge with no line; their top and bottom hairlines remain.
    - The battle's command header and the turn order's current diamond look as before.
    - **Pass/fail:** on the battle's command rows (over the battle background) and the title screen (over its art), the selected entry is told from the others at a glance in a capture viewed at half size. If not, raise `SelectionHairline`'s peak from `#80` to `#B3` and recapture; if still not, raise `SelectionGlow`'s peak from `#40` to `#66`. Note the values kept in the Done note.
- **Done, with deviations:** as planned, with the placeholder values; the readability fallback wasn't needed (the title's Continue, the inventory row and the Yes button are told apart at a glance in captures). `SelectionLine` became `SelectedTabLine` on both `UiTheme` and `UiTextures`; the rail's inset constants are named for the band (`BandInsetX`/`Y`), not a glow. `Selection` and `SelectionStroke` keep their colors; their summaries now name their users (command header, turn order, Party's moving outline). The battle's command rows weren't captured (no encounter in the scripted walk): they stay on the developer's hand check. The review's two comment suggestions were taken. No new test; 2194 in the solution.
