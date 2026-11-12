# Design: code health cleanup (the Game)

## Tunables and placeholders
None. No value changes; GB1 and GB2 change what is drawn, not a number.

## Design overview

### What exists (2026-11-11 counts)
Game files over 280 lines, and the Game test files over 300:

| File | Lines | Its backlog rule | This plan |
|---|---|---|---|
| `Ui/MessageIds.cs` | 767 | "split into partial files by feature" | GS1 → eight files, the largest about 210 |
| `Menus/PartyPage.cs` | 296 | the next growth moves the notice and the action choice out first | waits |
| `Exploration/EventPresenter.cs` | 294 | the next growth moves the rest's handling beside `RestPlayback` first | waits |
| `Battle/ActionPlayback.cs` | 292 | the next event kind brings a handler table | waits |
| `Battle/BattleScene.cs` | 290 | the next growth moves the outcome screens out first | waits |
| `Trade/ShopScene.cs` | 288 | the next growth moves the list's input out first | waits |
| `Battle/CommandPanelView.cs` | 281 | split before the next growth | waits |
| `Battle/CommandMenuTests.cs` (tests) | 393 | split by menu entry | GT2 → about 200, with two siblings |
| `Flow/GameFlowTests.cs` (tests) | 321 | split by scenario | GT1 → about 215, with a sibling |
| `Exploration/EventPresenterTests.cs` (tests) | 306 | split by behavior | GD1, GT3 → about 205, with a sibling |

`Battle/VictoryScreenView.cs` is 277 (the backlog says 263): GR1 corrects the count; its rule stands.

### Dead code survey
- **`TODO`, commented-out code, compatibility branches:** none in `src/Jrpg.Game` or `tests/Jrpg.Game.Tests` (the first plan's greps cover every project).
- **Interfaces:** `IScene` (every scene), `IMenuPage`/`IMemberPage` (two each), `ISaveFilesPage` (`SavePage`, `LoadPage`), `IBattleListChoice` (`BattleSkillChoice`, `BattleItemChoice`): none with one implementation.
- **Members with no caller:** GX1 lists them by the first plan's X1 method, over `src/Jrpg.Game` (`*.cs`; the Game has no XAML).
- **Duplicated test setup:** `new EventPresenter(exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), new PlayerSettings(), new VoiceChannel())` 20 times in 9 files (`Grep "new EventPresenter\("`), one of them in `StartMapScenarios`. GD1.

### Core changes
None.

### Structure
- `Ui/MessageIds` partial files (decision 552): `MessageIds.cs`, `MessageIds.Field.cs`, `MessageIds.Stats.cs`, `MessageIds.StatusPage.cs`, `MessageIds.Menus.cs`, `MessageIds.Battle.cs`, `MessageIds.Crafting.cs`, `MessageIds.Trade.cs`. `MessageIdsTests` reads the type by reflection, so it sees every part.
- `Menus/InventoryFilter.ShowsCounts(InventoryCategory)`: whether a category's rows carry a count (GB1).
- Test helpers: `tests/Jrpg.Game.Tests/Battle/CommandMenuDriver` (static: `Press` and `SelectEntry` on a given menu; GT2); `tests/Jrpg.Game.Tests/Exploration/EventPresenters` (static: `Over(exploration, settings = null, voice = null)`, the presenter tests build; GD1). The flow tests read the test world's catalog from the existing `StartMapScenarios.TestWorld` (GT1); `FlowStarts` doesn't change.
- New test classes: `GameFlowNewGameTests` (GT1), `CommandMenuSkillTests` and `CommandMenuMoveTests` (GT2), `EventPresenterStillsTests` (GT3).

### Folder dependencies
No new edge: `InventoryFilter` stays in `Menus`, the helpers are test-only in the folders whose types they build.

### Key decisions
- **`MessageIds` in partials, not per-feature classes**: 272 constants are read as `MessageIds.X` from every screen; a class per feature would change hundreds of call sites for a grouping the partial files already give.
- **Sibling test classes over a shared driver** (decision 556), as the Shop tests' `ShopDriver`.
- **GB2 moves one call, not the dialogs**: drawing the hint before the page is the smallest change that puts it under every page dialog's scrim; the header is already drawn before the page.

### Replacement mapping
| Today | After |
|---|---|
| one 767-line `MessageIds.cs` | eight partial files of the same class |
| `new EventPresenter(exploration, new EventLook(...), new PlayerSettings(), new VoiceChannel())` (20) | `EventPresenters.Over(exploration)` (with `settings:` or `voice:` where a test sets one) |
| `CommandMenuTests`' private `Press`, `SelectEntry` | `CommandMenuDriver` (`SelectSkill` stays private, in `CommandMenuSkillTests`) |
| `GameFlowTests._catalog` | `StartMapScenarios.TestWorld` |
| `EventPresenterTests._confirmPressed` in the moved stills tests | `InputFrames.Pressing(GameButton.Confirm)` |
| `File.Exists` in `UiArtFilesTests` | the folder's exact names |
| `OptionsPage.DrawRow`'s inline pips (25 lines) | `DrawVolumePips` |
| the four text speed tests' one Down press | `PageOn(OptionsPage.TextSpeedRow)` |

## Backlog survey
Every `docs/plans/backlog.md` line about `src/Jrpg.Game` or `tests/Jrpg.Game.Tests`, checked on 2026-11-11.

### Already done: GR1 deletes these lines
- Escape menu third pass (F1 review): "Status: the Skills section starts under the longer of the gear and resource rows, so the stats plan's nine slots will push it off the page; that plan places it." The stats plan did (decision 368 put the class resources beside the skills, `StatusResourcesView`); GR1's hand check confirms it on the Status page before deleting the line.

### Done by this plan
| Backlog line | Slice |
|---|---|
| The Inventory's Weapons and Armor tabs show the QTY column title (loot plan, P1a) | GB1 |
| A dialog a menu page draws leaves the footer hint bright (escape menu third pass, PD1) | GB2 |
| `UiArtFilesTests` checks with `File.Exists` (music plan, P1) | GB3 |
| `Ui/MessageIds.cs` is 767 lines | GS1 |
| `MessageIds`' comment line over 120 columns (new-game flow) | GS1 rewraps all five (lines 15, 350, 440, 443, 592); the backlog line keeps the Core files |
| `GameFlowTests` (two lines: 369, then 321) | GT1: the new-game flow's line keeps Core's `PartyStateTests` (322, which the sibling plan keeps), the composer plan's line loses `GameFlowTests` only |
| `tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` is at 393 lines | GT2 |
| Tests build an `EventPresenter` by hand 20 times in 9 files | GD1 |
| `EventPresenterTests` (three lines: 304, 306, 306) | GT3: deletes the class mechanics line, rewrites the quests retro's line to keep Data's `ContentWriterTests`, then deletes the composer plan's line (by then naming only `EventPresenterTests`) |
| `OptionsPageTests` (lines 53-88) reach their row with one Down press | GR1 |
| `Menus/OptionsPage.DrawRow` is 25 lines since the volume pips | GR1 |
| `ScreenFade` skips its black moment on a long update (two lines) | closed into one by decision 557 (GR1) |

### Stays, with its reason
- **Files at the limit, each with its own rule** (they split before their next growth): `PartyPage` 296, `EventPresenter` 294, `ActionPlayback` 292 (and its `Read` at the complexity limit), `BattleScene` 290, `ShopScene` 288, `CommandPanelView` 281, `VictoryScreenView` 277 (count corrected).
- **Shared drawers waiting for a third copy:** the yes/no question in three screens (a fourth), the tip strip, the scroll cues, the titled dialog frame, the rarity color, row text centering (ten one-liners), `SectionTitle`'s gap, labels uppercased in three places, `ActionPlayback`'s numbers-per-beat count, `ObjectSpriteBaker.Over`, `VictoryScreenSoundTests`' builders, the "move down until" helper (`ShopDriver.MoveDownUntil` exists; the Shop tests switch when next touched), `ResourceGaugeView.LabelSpacing`, the page list's margins and the pointed tabs (each view reads `PageListLayout` and `PointedTabsView` when next touched), `ForgeView`/`ShopView`'s frame, `ForgeList`/`InventoryList` onto `RowSelection`, `StatusSkillsView.DescriptionLayout` into `Ui/` (two users), `StatusParametersView.Draw`'s inline section titles.
- **The battle's look**, which follows its own plan (decision 272): enemy attack strips drawn unmirrored (a design question for the developer), the skill hint that doesn't wrap, `MenuPointer` at the cursor's row in the battle commands, the defeat menu and the leader choice, the placeholder marks waiting on icon art.
- **Parameters over four** in `ShopListView.Draw`, `ForgeListView.Draw`, `ShopDetailView`'s and `ShopListView`'s draw helpers: `verify.ps1` makes the next slice that changes each fix it.
- **Test seams that need a production change:** `SystemPageTests` and `BattleSceneTests` reaching rows by fixed presses (the pages expose no selection), `InventoryPageSoundTests` leaning on the potion, `BattleTurnSequenceTests`' `EnemyTactics` setup (five copies, a Core-facing helper).
- **Later plans and the developer's calls:** "Load the latest save" written twice (the autosave plan), `GameMenu.png`'s size (the developer's art), `TextureLibrary` keeping stills (real art), `UiTheme.ColorFromHex("")` and the test fixtures, the inventory hint's `{button}`, `ForgeScene.Craft`'s re-derived rule (a Core return value), the save page's red line (decision 284), a debug `--start`, the music's joins and streaming (ADR 0035), the battle plan's banner band (rewritten: the map banner, MB2, drew a different fading band, so a third banner shares, not the second), `OverlayFade`'s note, the enemy plan's `BossFightSimulation` copy (a third simulation).
- **Out-of-scope lists of earlier plans:** pointers, kept.

## Risks
- **GS1's size.** About 770 lines move in one slice. Every line is a constant with its summary; `git diff --color-moved` shows them unchanged. A constant left out fails the build at its call site, a doubled one fails it at the declaration; the guard is the `public const string` count, 272 summed over the eight files, recorded in the Done note with each file's first and last constant.
- **GD1's optional parameters.** `EventPresenters.Over` must build exactly what each site built: the settings (`TextSpeed = TextSpeed.Instant` at three sites, a theory's speed at one) and a shared `VoiceChannel` where a test reads it afterward (`StartIntro`, `EventPresenterSoundTests`). A site that passes its own voice keeps passing it.
- **GT2's fixture.** The skill and item tests read the same Zanyr battle and bag; the sibling classes each build it in their constructor (five lines) rather than share an instance, so tests stay independent.
- **GB1 and GB2 are looks.** The developer checks them by hand; no agent capture.
- **Save format, determinism, content:** untouched.
