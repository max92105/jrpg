# UI second pass: design

Part of [the UI second pass plan](README.md).

### Tunables and placeholders

Every value is a placeholder. Screen values are named constants in the type that uses them (colors in `UiTheme`).

**Sizing targets** (SZ1, SZ2; decision 251). Measured on the reference screenshots scaled to 1920 px wide (the menu shots are 1200 px: x1.6). The slices measure our S0 captures against them and adjust; the rule is "close to the reference, using more of the screen", not these exact numbers.

| What | Today | Target | Where |
|---|---|---|---|
| Menu body text, tab labels, list rows' text (`UiTheme.MenuTextPixels`) | 20 | 28 | `UiTheme` |
| Menu row and tab height (`UiTheme.MenuRowHeight`, `NavTabShape.Height`) | 56 | 64 | `UiTheme`, `NavTabShape` |
| Header title / header info (`MenuHeader`) | 28 / 18 | 38 / 24 | `Menus/MenuHeader` |
| Footer hint | 20 | 26 | `GameMenuScene`, outcome screens |
| Header / footer bar height | 80 / 64 | 88 / 72 | `MenuHeader`, `GameMenuScene` |
| Small spaced-caps labels (`HP`, `LV`) | 14 | 16 | each view |
| Large numbers (sidebar HP, status values) | 18-22 | 30-34 | `MemberCardView`, `StatusSheetView` |
| Party sidebar width | 460 | 520 (fewer visible cards if needed) | `GameMenuScene`, `PartySidebar.VisibleCards` |
| Dialogue text / speaker | 32 / 26 | 36 / 28 | `UiTheme` (dialogue part) |
| Battle names / HP numbers / labels | as today | +25 % (reference: names 30, numbers 34, labels 18) | `Battle/PartyStatusView`, `BattleVitalRow` |
| Command rows' text | as today | 28 | `Battle/CommandPanelView` |
| Title entries | 48 with glow | 34, plain, in the selection look | `Title/TitleScene` (T3) |

**Other tunables**

| Name | Where | Placeholder | Meaning | Slice |
|---|---|---|---|---|
| Capture key | `JrpgGame` (Debug only) | F12; files `capture-<yyyyMMdd-HHmmss>-<n>.png` in the repository's `docs/captures/` (Debug builds) | Today's screens for specs and hand checks | S0 |
| Title font | `content/art/fonts/titles/` | Cormorant Garamond SemiBold (decision 250) | | T1 |
| Title name size | `Title/TitleScene` | 120 px | | T1 |
| Selection look | `UiTextures.DrawSelectedRow` + `MenuPointer` | today's `UiTheme.SelectedRow`, `HighlightLine`, 14 px diamond | Decision 252 | P1a |
| `TitleEntrance` | `Title/TitleEntrance` | picture 0-0.6 s, name 0.3-0.8 s, entries 0.8-1.2 s | | T3 |
| Title footer | `content/text/ui.json` `title_footer` | "Prototype build" | | T3 |
| Untitled window | `Ui/FramedWindowLayout` | padding 32 x 24; outer hairline 1 px; inner hairline 4 px in, corners stopping 6 px short | `Common/Texture/UiTX_Common_Dialog_Frame.png`'s idea | W1a |
| Titled window | `Ui/FramedWindowLayout` | title 28 px; separator 1 px fading at both ends, a 10 px diamond in its middle, 14 px under the title | `DialogLine_CeltCenter.png`'s idea | W1b |
| Window colors | `UiTheme` | body `#C70C0D11`, hairlines `#66FFFFFF`, separator `#99FFFFFF` | | W1a, W1b |
| Action name box | `Battle/BattleBannerView` | 28 px text, at least 300 px wide, 150 px from the top | | W1a |
| Yes/no dialog | `Ui/ConfirmPromptView` | 720 px wide; buttons 260 x 50 stacked, 10 px apart, in the selection look | | W1b |
| Dialogue box | `Dialogue/DialogueBox` | square corners; body `#B3000000`; the window's two hairlines | | W2 |
| Rest fade | `Exploration/EventPresenter` | out 0.5 s, black 1.0 s, in 0.5 s | | X1 |
| `MenuMotion` | `Menus/MenuMotion` | each tab 0.2 s, 0.03 s after the one above, from 32 px left; frame fade 0.15 s; closing fade 0.12 s | | MO1 |
| `SelectionGlide` | `Ui/SelectionGlide` | 0.08 s, `Easing.CubicOut` | | MO2a |
| Pointer bob | `Ui/MenuPointer` | 4 px left and back over 0.9 s (`PointerBob`, moved to `Ui`) | | MO2a |
| Status sections | `Menus/StatusSheetView` | profile (LV, class, speed); resources (name, max, lasting amount); skills (name, cost line) | Decision 260 | M3a |
| Status labels | `Menus/StatusSheetView` | labels 16 px spaced caps (letter spacing 2), values 32 px, 1 px hairline per row | | M3b |
| Inventory columns | `Menus/InventoryListView` | "ITEM", "QTY" in spaced caps; the description in a `UiTheme.CommandHintFill` strip under the selected row | | M4 |
| Member column | `Menus/MemberColumnView` | 80 px wide, 56 px diamond portraits 72 px apart, the shown one in the selection look | Decision 255 | M2b |
| Tab glyphs | `Ui/TabGlyphShape` | 12 x 12 masks drawn at 2x (24 px), white | Decision 254 | M1 |
| `EncounterFlash` | `Flow/EncounterFlash` | white, up to 85 % in 0.04 s, down in 0.08 s | | B1a |
| `ShatterLayout` | `Flow/ShatterLayout` | about 24 triangles around an impact point at the screen's center; seed = a placeholder constant plus the session's encounter count | Decision 258 | B1b |
| Shard flight | `Flow/ShatterLayout`, `Flow/EncounterShatter` | each shard 300-900 px/s outward, spinning up to 1.5 turns/s, fading over the 0.5 s fade-out | | B1c |

## Design overview

### What exists

From the code (`docs/map.md`, Game section). S0's captures become the record of today's look.
- **Already close to the reference:** the menu's arrow tabs with the blue glow, its header and footer bars, the selected row gradient, section titles, the battle's party panel, the turn order diamonds, enemy tags, the victory reward bars and cards, the leader choice.
- **Plainer, or incoherent:**
  - everything is small for 1920 x 1080 (decision 251);
  - one sans font; the game's name and "Victory!" aren't serif;
  - selected states differ: `DrawSelectedRow` in most lists, but the save list draws `UiTheme.Highlight`, the defeat menu a lit reward bar, the command rows their own lit row, the yes/no buttons `SelectionFill`, the title a glow; the defeat menu and command rows draw their own pointer diamonds (backlog);
  - the title: large glowing entries, hard to tell which is selected, no entrance;
  - the yes/no prompt is a full-width band; the action name a band; the dialogue box a rounded panel with a thick white border;
  - resting is a message only;
  - the menu fades in (and the first time, not even that: BG1) and closes at once; selections jump; page lists can't move over time because `IMenuPage.Update` has no time step;
  - the Status page shows HP, attack, defense, experience and equipment only; no class resources, speed or skills;
  - the inventory's description sits at a fixed place under the list;
  - an encounter is a plain fade.
- **No Core, Data or save change.** The Status page reads `PartyMember.Skills`, `Speed`, `Class.Resources` and `LastingAmounts`, which exist.

### Reference material

The export (decision 231) is at `C:\Users\max92\Downloads\umodel_win32\Export\Game\UserInterface\`; use the `Texture` folders (1x). Walking it whole times out: list one subfolder at a time. R2's index names 2-4 files per screen from these subfolders:

| Our screen | Export subfolders |
|---|---|
| Title | `Title/Texture` (`UiTX_Title_SelectMenu_Focus.png`, `_OutFocus.png`) |
| Shared pieces (windows, rows, bars) | `Common/Texture` (`UiTX_Common_Dialog_Frame.png`, `UiTX_Common_DialogLine_CeltCenter.png`, `UiTX_Common_ListFocus_Base.png`, `UiTX_Common_Footer_Base.png`) |
| Dialogue box | `Narration`, `Event`, `Balloon` |
| Battle party panel, turn order | `Battle/CharacterStatus/Texture`, `Battle/ActionOrder/Texture` |
| Command panel, floating numbers, action name | `Battle/Command/Texture`, `Battle/Effect/Texture` |
| Victory, defeat | `Battle/Result/Texture`, `GameOver` |
| Game menu, Status, Equipment | `MainMenu/Main`, `MainMenu/Status`, `MainMenu/Equip` |
| Save and load | `SaveLoad` |

### Screenshot names

Decision 235, in `docs/ui-reference/` (decision 263). `Octopath I` marks the four shots from the first game (H'aanit, Primrose).

| Today (`docs/ui_ux_exemples/`) | New name | What it shows |
|---|---|---|
| `Main_menu_exemple.jpg` | `title-screen.jpg` | Title: serif logo over art, small centered entries, footer line |
| `esc_menu_exemple.webp` | `menu-tabs.webp` | Game menu: arrow tabs with icons, sub-entries, pointer |
| `esc_menu_status.webp` | `menu-status.webp` | Status: member column, profile, sections with hairlines, character art |
| `esc_menu_equipment.webp` | `menu-equipment.webp` | Equipment: slots, choice list, stat preview, parameters |
| `esc_menu_inventory_and_item_seletion.webp` | `menu-inventory-target.webp` | Items: using an item on a member, the sidebar's stat preview |
| `esc_menu_inventry_2.webp` | `menu-inventory-description.webp` | Items: category rail, the description under the selected row, sidebar vitals |
| `esc_menu_recovery.webp` | `menu-field-skills.webp` | Field skills: member column, cost column, sidebar with HP/SP |
| `esc_menu_skill_selection_unlock.webp` | `menu-skill-learned-dialog.webp` | A titled framed dialog with separator and OK over a list |
| `esc_menu_map.webp` | `menu-world-map.webp` | World map (no screen of ours yet) |
| `merchant_menu.webp` | `shop-sell.webp` | Shop: price column, party stat preview (no screen of ours yet) |
| `Generic_interactive_menu.webp` | `dialog-confirm.webp` | Yes/no dialog: framed window, title, separator, stacked buttons |
| `Victory_Menu.jpg` | `battle-victory.jpg` | Victory: serif title, reward rows, member cards |
| `combat_player_info_exemple.webp` | `battle-party-panel.webp` | Party panel close-up: name, pips, HP/SP, status marks |
| `combat_turn_order_exemple.webp` | `battle-turn-order.webp` | Turn order strip |
| `Combat_selection_menu_exemple.webp` | `battle-command-menu-1.webp` | Command list, description tip, enemy tags |
| `Combat_selection_menu_exemple_2.webp` | `battle-command-menu-2.webp` | The same, in a forest |
| `combat_selection_menu_exemple_3.webp` | `battle-command-menu-3.webp` | Octopath I: command list, boosted attack |
| `combat_selection_menu_exemple_4.webp` | `battle-command-menu-4.webp` | Octopath I: full command list |
| `Combat_select_faint_exemple.webp` | `battle-target-fallen-ally.webp` | Aiming at a fallen ally |
| `combat_targeting_ennemies_exemple.webp` | `battle-target-enemy.webp` | Octopath I: aiming at an enemy |
| `combat_damage_ui_ux_exemple.webp` | `battle-damage-numbers.webp` | Octopath I: damage numbers, the action name box |

Names of the folder outside `docs/plans/done/` (R1 updates each): CLAUDE.md, `.claude/rules/game.md`, `src/Jrpg.Game/Ui/DiamondCrop.cs`, `src/Jrpg.Game/Dialogue/DialogueBox.cs`, `src/Jrpg.Game/Menus/OptionsPage.cs`, `src/Jrpg.Game/Battle/VictoryScreenView.cs` (which also names `Victory_Menu.jpg`), the architect's memory. Done plans keep the old names.

### Structure and folder dependencies

Every new edge checked: none goes from `Ui` to a feature folder.

| Folder | Uses | New types here |
|---|---|---|
| `Ui` | `Rendering` | `FramedWindowLayout`, `FramedWindowView`, `SelectionGlide`, `PointerBob` (moved from `Battle`), `TabGlyphShape` |
| `Rendering` | (Data) | `FontLibrary.TitleFontAt` (a member) |
| (root) | everything | the capture key in `JrpgGame` (S0); the frame capture for the shatter (B1a) |
| `Hosting` | | `CaptureFiles` |
| `Title` | `Ui`, `Rendering`, `Saving`, `Scenes`, `Input` | `TitleEntrance` |
| `Exploration` | `Dialogue`, `Ui`, `Scenes`, `Audio` | `StillRun` |
| `Menus` | `Ui`, `Rendering`, `Saving`, `Scenes`, `Input` | `MenuMotion`, `StatusSheet`, `StatusSheetView`, `InventoryRow`, `InventoryListView`, `ItemDetailLine`, `MemberColumn`, `MemberColumnView` |
| `Battle` | `Ui`, `Rendering`, `Scenes` | (none; `PointerBob` leaves for `Ui`; `SkillCostText` may leave for `Ui`, M3a) |
| `Flow` | `Scenes`, `Ui`, every scene folder | `EncounterFlash`, `ShatterLayout`, `Shard`, `EncounterShatter`, `ShatterView` |

### New types

| Type | Folder | Responsibility (one line) | Slice |
|---|---|---|---|
| `CaptureFiles` | Hosting | The next capture file's path, never overwriting one. | S0 |
| `TitleEntrance` | Title | How visible the title's picture, name and entries are at a time since the screen opened. | T3 |
| `FramedWindowLayout` | Ui | Where a framed window's frame, body and (titled) title and separator sit around a body of a given size. | W1a, W1b |
| `FramedWindowView` | Ui | Draws a framed window from its layout. | W1a, W1b |
| `StillRun` | Exploration | One run of cutscene stills: the black lead-in, each still's time and skip, the black lead-out. | C3 |
| `MenuMotion` | Menus | The menu's opening and closing over time: each tab's slide and opacity, the frame's opacity, closing done. | MO1 |
| `SelectionGlide` | Ui | Where a list's highlight is while it glides from the row it left to the selected one. | MO2a |
| `InventoryRow` | Menus | One item line as the inventory shows it. | C5 |
| `InventoryListView` | Menus | Draws the inventory's visible rows, its selection and its empty line. | C5 |
| `ItemDetailLine` | Menus | The strip under the selected item: the last use's notice until the player moves on, otherwise the item's description. | M4 |
| `StatusSheet` | Menus | What the Status page shows of one member, built when the shown member changes. | C4 |
| `StatusSheetView` | Menus | Draws a `StatusSheet`. | C4 |
| `MemberColumn` | Menus | The members a member page goes through, in order, and which one is shown. | M2a |
| `MemberColumnView` | Menus | Draws the member column: small diamond portraits, the shown one in the selection look. | M2b |
| `TabGlyphShape` | Ui | The placeholder tab icons' pixels, one mask per glyph, baked once by `UiTextures`. | M1 |
| `TabGlyph` | Ui | The closed list of placeholder tab icons (enum). | M1 |
| `EncounterFlash` | Flow | The white flash's opacity at a time since an encounter was asked for. | B1a |
| `Shard` | Flow | One shard of the broken frame: its triangle in the frame and its flight. | B1b |
| `ShatterLayout` | Flow | The frame broken into shards around an impact point, from a seed. | B1b |
| `EncounterShatter` | Flow | How far the shards have flown at a time since the break. | B1c |
| `ShatterView` | Flow | Draws the shards as textured triangles from the frozen frame. | B1b |

### Key decisions (implementation, no ADR)

- **One selection look** (decision 252): `UiTextures.DrawSelectedRow` plus `MenuPointer` for every list; P1a moves the save list, the defeat menu and the command rows onto it, T3 the title, W1b the yes/no buttons.
- **Shapes are baked, not loaded** (decision 233): tab glyphs are masks in code like `NavTabShape`; the framed window is hairlines drawn with the pixel texture.
- **Timing types are pure** and take the elapsed time (`TitleEntrance`, `MenuMotion`, `SelectionGlide`, `EncounterFlash`, `EncounterShatter`).
- **Pages get time through `IMenuPage.Advance(TimeSpan)`** (TS), called by `GameMenuScene` before `Update`; `Update(InputFrame)` keeps its signature, so the 170-odd test calls don't change.
- **The menu's closing waits for its fade**: `GameMenuScene` returns `MenuClosed` once, when `MenuMotion` says the closing is done. Input works during the slide-in; none is read while closing.
- **The capture is the host's** (ADR 0008: the update path never touches the device). S0's key and B1a's frozen frame are read in `JrpgGame.Draw`, after the virtual screen is drawn: the flow asks (`WantsFrozenFrame`), the host copies the render target into a second one it owns and hands it to the flow.
- **Shards are triangles** drawn with `GraphicsDevice.DrawUserIndexedPrimitives` and a `BasicEffect` textured with the frozen frame (MonoGame itself, no package), point-sampled. `SpriteBatch` can only draw rectangles, which wouldn't read as glass.
- **The shatter's randomness is the Game's**, seeded (`new Random(seed)` inside `ShatterLayout`, which is Game code), never the battle's `IRandom`, so the shatter can't change a battle's outcome.
- **The rest fade lives in `EventPresenter`** beside the warp fade, after C3 has made room.

### Extension points

- `FramedWindowView`: the shop, a save-done notice, a skill-learned notice, the map name banner's box.
- `SelectionGlide` and `IMenuPage.Advance`: any later list.
- `ShatterLayout`: a boss transition with another impact point.
- `FontLibrary` roles: a third face is a third subfolder (ADR 0025).

### Replacement mapping

| New | Replaces |
|---|---|
| `MenuPointer` and `DrawSelectedRow` everywhere | The pointers `DefeatScreenView` and `CommandPanelView` draw by hand; `SaveSlotListView`'s `UiTheme.Highlight`; the defeat menu's lit reward bar; the command rows' own lit row; `ConfirmPromptView`'s `SelectionFill` (W1b); the title's glow (T3) |
| `FramedWindowView` | `BattleBannerView`'s band; `ConfirmPromptView`'s full-width band; `DialogueBox`'s rounded `PanelView` (`UiTheme.MessagePanel` goes) |
| `MenuMotion` | `GameMenuScene._openFade` and `_sinceOpened` |
| `SelectionGlide` | The jump of the selection on the tabs, the title and the page lists |
| `PointerBob` in `Ui` | `Battle/PointerBob` (the target pointer reads the moved type) |
| `StillRun` | `EventPresenter`'s `_still`, `_leadInLeft`, `_leadOutLeft` and their methods |
| `StatusSheet`, `StatusSheetView` | `StatusPage`'s `MemberSheet`, `StatusLabels`, `PageFrame` and `Draw*` methods |
| `InventoryRow`, `InventoryListView` | `InventoryPage`'s `ItemRow`, `InventoryLabels`, `DrawList`, `DrawRow` |
| `ItemDetailLine` | `InventoryPage`'s `_notice` and the description drawing |
| `EncounterFlash`, `EncounterShatter`, `ShatterView` | The plain fade into battle (the fade itself stays, under the shards) |
| `FooterBar.TextTop` (exists) | The centering in `GameMenuScene.DrawFooterHint`, `VictoryScreenView.Draw`, `DefeatScreenView.DrawFooter` |
| `Easing.ShareOf`, `Easing.CubicOut` (exist) | The clamped time shares in `ActionPlayback`, `FloatingNumber` (twice), `LevelUpPop`, `VictoryEntrance`, `DefeatEntrance`, `ResourceGaugeView.FlashShare`, `StatusMarksView.Share`; the cubic ease-outs in `LevelUpPop` and `VictoryEntrance` |

### Clean-up selection

Each `docs/plans/backlog.md` line, checked against the code on 2026-10-25. A line this plan covers is removed by its slice; the others stay, with the reason.

| Backlog line | Here? | Why |
|---|---|---|
| `UiTheme.cs` (291) to partial files | **C1** | W1a and later add colors. |
| `MessageIds.cs` (397) partial files | Stays | Already over 300, so the check doesn't fire; this plan adds a few constants. |
| Row pointer diamonds drawn by hand | **P1a** | The pointer unification. |
| `EventPresenter.cs` (285) | **C3** | X1 grows it. |
| `InventoryPage` split (328) | **C5** (rows, list view), **M4** (the notice line) | MO2b and M4 change the list. |
| `StatusPage` split (289), `ClassOf` | **C4** | M3a and M3b change the page. |
| Equipment "Max HP" vs "HP" | **SZ1** | Decision 261. |
| Banner band drawn twice | **W1a** changes it | Only `BattleScene.DrawEscaped` keeps the band; W1a rewrites the line for the map banner plan. |
| Footer hint centering written 3 times; easing copies | **C2** (last) | Nothing after it adds footers or timing. |
| An open yes/no question held 3 times | Stays | W1b changes the view, not how screens hold the prompt. |
| `ExplorationState` (296), `BattleState` (296), `BattleStateTests` (361) | Stays | No Core change. |
| Test files over 300 (`CommandMenuTests`, `GameFlowTests`, `InventoryPageTests`, `PartyStateTests`, `PartyMemberTests`, `EventPresenterTests`) | Stays | New tests go in new classes (`EventPresenterRestTests`, `GameFlowEncounterTests`, `MenuMotionTests`). |
| `CommandPanelView.cs` (282) | Watched | P1a should shrink it; SZ2 changes values only. Split its target pointer out first if a slice would cross 300. |
| `TurnKept` event, `ColorFromHex("")` fixtures, placeholder icons for Move and the hourglass, `ResourceDefinition` start, `TextureLibrary` keeping textures, the New NPC dialog check, editor lines, `Directory.Build.props`, item target picker, unmirrored enemy strips, load-latest copies, `TileImages.Forget`, `ScreenFade` long frame, editor save checks, comment lines over 120 columns | Stays | Not touched by this plan; each keeps its reason. BG1 may touch `ScreenFade`'s long-frame line if the stall is that cause: then it fixes it. |

## Risks

- **Sizing breaks layouts.** Larger text overflows rows, columns and the sidebar; tests that pin counts (`PartySidebar.VisibleCards`, visible rows) change. SZ1 and SZ2 check every page against captures and budget those tests; a page that can't fit its content at the target size keeps the largest size that fits, noted in the slice.
- **The first-open stall's cause is unknown** until BG1 measures it. Likely candidates: the menu picture's first decode, `DiamondPortrait` cutting 1024 px portraits on first draw, FontStashSharp rasterizing new sizes, a long first frame making the fixed step catch up past the fade. BG1 plans for any of them.
- **Look without the architect's eyes.** S0's captures and the developer's hand checks are the acceptance.
- **Estimates.** View slices ran 1.6x; every estimate here includes it. MO1, M3a, M1, B1b are closest to 250 and say where to split.
- **The 300-line check.** `CommandPanelView` (282), `BattleScene` (254), `VictoryScreenView` (250), `PartyPage` (256), `GameMenuScene` (196, grows in MO1, TS, MO2a, M1, M2b): each slice checks before growing one; `GameMenuScene` may need its tab drawing moved to a `MenuTabsView` before M1 (M1 says so).
- **Menu closing timing.** MO1 delays `MenuClosed`; `GameMenuSceneTests` that expect it at once are budgeted.
- **Shatter rendering.** Primitives with a `BasicEffect` are a new drawing path: the batch must end before and restart after, at the virtual screen's scale. A 1920 x 1080 render target copy is about 8 MB of video memory, held only during the transition.
- **Flow tests.** The flash and the shatter delay the battle's start; headless flow tests step through them and stay seed-independent.
- **Determinism and saves.** No Core change; no save change.
- **Reference art leaking in.** Decision 264's rule line; `git status` in R1 and R2 shows no image added outside `docs/ui-reference/`'s existing screenshots.
