# SZ2. Sizing pass, title, dialogue, battle and outcomes

Part of [the UI second pass plan](../README.md). Decision 251. Design: [design.md](../design.md), "Tunables and placeholders" (sizing targets). After SZ1. Look at `docs/ui-reference/battle-command-menu-1.webp`, `battle-command-menu-4.webp`, `battle-party-panel.webp`, `battle-victory.jpg`, `title-screen.jpg` and S0's captures of the same screens first.

- **SZ2. Sizing pass, title, dialogue, battle and outcomes** (Game, about 200 lines)
  - **Goal:** the same proportions outside the menu: the dialogue box's text and speaker, the interaction prompt, the yes/no prompt, the battle's party panel (names, HP numbers, labels, pips), the command panel's rows and hint, enemy tags, the turn order's diamonds, the banner, floating numbers, the victory and defeat screens, the leader choice. The title's entries wait for T3, which redraws them.
  - **Steps:** as SZ1: measure, change the constants in place (`UiTheme`'s dialogue and prompt sizes, `Battle/PartyStatusView`, `BattleVitalRow`, `ResourceGaugeView`, `StatusMarksView`, `CommandPanelView`, `CommandPanelPlacement`, `EnemyTagView`, `TurnDiamondView`, `BattleBannerView`, `FloatingNumbersView`, `VictoryScreenView`, `DefeatScreenView`, `NewGame/LeaderProfileView`, `ConfirmPromptView`, `Exploration/InteractionPromptView`), then fix what no longer fits (the party panel's stacking for 4 members, the command panel's placement beside the actor, the victory cards' widths).
  - `CommandPanelView` is at 282 lines: values only; if a fix adds lines, move the target pointer drawing out first and say so.
  - **Tests:** none new. Budget the tests that pin places (`PromptPlacementTests`, battle layout tests such as `BattleLayout` spots, `PointedBarShapeTests` if the reward bar grows), about 25 lines.
  - **Verify by hand:** F12 a dialogue, a battle (commands, a skill list, aiming, numbers), victory, defeat, the leader choice; compare with the reference and the before captures: larger, nothing clipped, the party panel still fits 4 members with their statuses.
- **Done, with deviations:**
  - **Measured** at x1.6 on `battle-command-menu-1.webp` against `docs/captures/before/battle-*.jpg`:
    - the reference's party names are about 24 px, its HP numbers 32-34 and its labels 11-18 (ours: 22 / 30 / 14);
    - its command rows are 38 px with 27 px text (ours: 46 / 20).
  - **Chosen sizes:**
    - party panel: names 28, HP 36 / 22, labels 16, 76 px portraits, 300 px rows;
    - status marks 26; resource bar 150 wide with 11 px markers;
    - commands: 52 px rows with 26 px text, a 24 px header and a 20 px hint, the panel 400 x 470 at most;
    - enemy tags: 190 wide with 20 px names;
    - turn order: 76 px next-turn diamonds, a 16 px label and an 860 px line;
    - banner 28 px, at least 640 wide; floating numbers 48;
    - dialogue: 36 px text and a 28 px speaker, in a 1320 x 260 box;
    - prompt 24, now its own size rather than the menu's;
    - leader choice: an 88 / 72 px header and footer with a 38 px heading; its profile's class, body and stat labels at 18 / 26 / 18.
  - **Fits:** 4 party panels end about 474 px down, at x 1494 and beyond, clear of the battlers and of the command panel, which sits left of its actor.
  - **Victory and defeat unchanged:** they already match or exceed the reference (titles 96, reward numbers 40, names 26).
  - **Also unchanged:** the yes/no prompt (already 30 / 24 from SZ1), the turn diamonds and the vital row, which follow their callers' sizes; the interaction prompt follows `PromptTextPixels`, its padding now 12 x 5.
  - **Review:** stacked floating numbers 36 px apart (were 30), keeping their spacing against the larger text.
  - **Tests:** one changed, `CommandPanelPlacementTests` (the panel's new width and height); 2095 before and after.
  - **Seen in the before captures, not fixed here:** a long skill description runs past its hint box over the actor's portrait (`battle-skill.jpg`); noted in the backlog.
