# Cleanup backlog

Code-health work waiting for the next cleanup plan; `/retro` appends to it. One line per item, with where it came from. A cleanup plan (`/plan-feature`) starts from this list and removes the lines it covers.

The code health plan (`done/2026-10-23-code-health`) did the lines it covered; the ones its review moved wait under "When next touched" below.

## When next touched

These waited at the 2026-10-23 code health review. Do each one with the next change to its file, or when a plan needs it. Each says why it can wait.

- **A fixed class resource that leaves out `startsAt` loads and starts at 0.** `ResourceDefinition`'s `Max` and `StartsAt` default to 0 so a speed-scaled resource can leave them out (RG1), and 0 is a valid start, so the validator can't tell the two apart. Make both nullable when a content edit trips on it. (class-mechanics RG1 review)
- **`TextureLibrary` keeps every texture it loads for good**, cutscene stills included. That is fine with the 4 KB placeholder stills, but full-screen real stills (about 8 MB of video memory each, 3 per intro) would stay loaded after the intro. Drop a still once its event ends when real art arrives. (new-game-flow K2a)
- **An open yes/no question is held and drawn the same way in 3 screens** (`SavePage`, `SystemPage`, `NewGame/LeaderChoiceScene`: a `ConfirmPrompt?` field, a `ConfirmPromptView`, drawn while open, cleared when answered). Each copy is a few lines, and what Yes does differs per screen. Pull it into a small shared `Ui` type when a fourth screen needs it. (new-game-flow S1b review)
- **A dialog a menu page draws leaves the footer hint bright**: `GameMenuScene.Draw` draws the footer after the page, so the scrim of the Party notice and of the Save and System prompts dims everything but the hint. Draw page dialogs after the footer, or the footer under the page, when the walk-through or the next menu change gets to it. (escape-menu-third-pass PD1 review)
- `MainViewModel` (502 lines after the music plan's E1): the sheet image-size cache (`SheetImageSizes`) and the import status text (`SheetImportResult.Summary`) could leave it.
- **`Directory.Build.props`:** merge `src/` and `tests/` into a root file. Decision 144 keeps two files for now; the developer didn't choose to merge. When it happens, use the standard form:
  - the gates go in a root `Directory.Build.props`;
  - `tests/Directory.Build.props` keeps its two test-only settings (`IsPackable=false`, `using Xunit`) and imports the root file through `$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))`;
  - no path condition.

  The file is protected, so ask the developer first.
- **Why Core refused an equipment change:** `SlotChoice.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. `PartyEquipment.ItemWithoutRoom` and `UnequipAll` would own both. It can wait because it is correct today, and nothing planned changes equipment.
- **`Ui/MessageIds.cs` is 767 lines** of constants. Split it into partial files by feature, so no call site changes. It can wait because a table of constants doesn't get harder to change as it grows.
- **`MainViewModelTests`, part 2 (449 lines after E1a):** move the new content tests (`CreateObject_*`, `CreateNpc_*`, `CanStartNewObject_*`, `TryCreateMap_*`) and the map settings tests (`TryApplySettings_*`) into their own classes, on E1a's `EditorTestContent`.
- **`MainWindow.xaml.cs` (547 lines):** move the viewport (pan, zoom, fit) and the tile painting handlers into partial files, as `.EventEditing` and `.ObjectEditing` already are.
- **`TileBrowserViewModelTests` (410 lines):** move the Cut panel tests (Set up, Cut, Apply, Cancel, with `OpenGrass3Cut` and `Ask`) into a `TileBrowserCutTests` class.
- **`TileBrowserViewModel` (about 300 lines):** move the sheet hit-testing (`CellAt`, `NearestCellTo`, `GridPositionOf`, F1's grid check) into a `Tiles/SheetGrid`.
- **`ObjectShapesViewModelTests` (347 lines):** move the zone tests (E10) and the point-editing tests (E11a) into their own classes.
- **The New NPC dialog** doesn't check that the chosen sprite holds the walk frames. The startup and save check (plan 2026-10-23, F5a) refuses such a sprite, and `NewNpcViewModel.SpriteProblem` could say so in the dialog, through a size lookup.

## From the battle plan (2026-10-20)

- **Enemy attack strips are drawn unmirrored over the party**; no flip rule is decided (B5d). A design question for the developer.
- **"Load the latest save" and the save-list overlay are written twice** (`TitleScene`, `DefeatScreen`/`DefeatScreenView`). Share them in the **autosave plan**, which is likely to add the third copy.
- **The banner band** is drawn only by `BattleScene.DrawEscaped` now (the action name moved into a framed window in W1a). Share it with the **map name banner plan** when that plan draws the second one.
- `TileImages.Forget` keeps cached field-sprite frames; drop them too once field sprites can be imported (no import yet).

## From the demo retirement plan (2026-10-23)

- Editor: with a button focused (Fit, Add layer), Space + drag pans, but releasing Space still clicks the button (WPF clicks a focused button on Space up; Space is swallowed only while the map has focus). Older than F2, easier to meet since F2 (review of F2).

## From the new-game flow plan (2026-10-24)

- The editor's map save (`ContentWriter.WriteMap`) doesn't check that the files a map's events name exist: still images (`ArtReferenceCheck`) and voice files (`AudioReferenceCheck`). The game's startup rejects both, so an editor save can write content the game won't start with (`editor.md`, "Validate on save").
- Test files over 300 lines: `GameFlowTests` (369), `PartyStateTests` (322). Split by behavior when next touched.
- Comment lines over 120 columns from before the rule was enforced: `ExplorationState` (5), `GameSession` (8), `ArtReferenceCheck` (2), `MessageIds` (1). Rewrap when next touched.
- `ScreenFade` skips its black moment when one update is longer than its fade-out plus hold (about 0.33 s), so a warp never moves the player. The fixed 1/60 s step hides it in play; tests must step at 1/60 s. Make `Update` stop on Holding if a long frame can ever reach it.
- `BattleState.cs` is at 283 lines after the enemy plan's B2 (296 at DR2): split the per-command bodies before the next command path lands (DR2 review).
- `tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` is at 393 lines: split it by menu entry (MO1 review).
- The tip strip (dark `UiTheme.CommandHintFill` fill, highlight hairline, padded text) is drawn twice: `Battle/CommandPanelView.DrawHint` and `Menus/InventoryListView.DrawStrip`. Move it into one drawer in `Ui` when a third appears (M4 review).
- The gap under a section title's band, `SectionTitle.Height + 10`, is written three times (`EquipmentSheetView.SectionHeight`, `StatusParametersView.BelowBand`, `StatusSheetView.AttributesBelowBand`; `StatusSkillsView`'s is 4 since decision 373): give `SectionTitle` a `ContentTop` (M3b review; the band's height itself moved there in the escape menu third pass, F1).
- The scroll cues above and below a list are drawn twice: `PartySidebarView.DrawScrollCues` and `MemberColumnView.DrawScrollMarks`. Move them into one `Ui` drawer if a third list needs them (M2b review).

## From the class mechanics plan (2026-10-25)

- `tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (304): split by behavior when next touched.
- Core can't say whether a turn was kept: the screen keeps the actor forward both after a quick skill and when the same member gets two turns in a row (a readied turn right before their own; LK2b review). A `TurnKept` event from `TurnFlow.KeepTurn` would tell them apart.
- `UiTheme.ColorFromHex("")` throws, and the Core test fixtures build resources without a color; a Game test that reaches them through a battle panel would fail (LK1a review). Give the test builders a color, or make the validator's rule a Core default.
- Placeholder marks waiting on icon art: Move borrows the Run icon (FM2), and the extra turn's hourglass is a bright diamond (LK3).

## From the UI second pass plan (2026-10-25)

- `content/art/backgrounds/GameMenu.png` is 4096 x 2838 (23 MB, about 46 MB of video memory) for a 1920 x 1080 screen: a 1920 x 1330 copy would decode about five times faster (BG1). The developer's art; ask first.
- The battle's skill hint doesn't wrap: a long description runs past its box over the actor's portrait (`docs/captures/before/battle-skill.jpg`; SZ2).
- **Centering a line of text in a row is written about 10 times**, as `row.Y + ((RowHeight - font.LineHeight) / 2f)`: `EquipmentChoiceView`, `EquipmentSheetView` (twice), `InventoryListView`, `OptionsPage`, `SystemPage`, `PartyActionChoiceView` (integer division), `ConfirmPromptView` and others. A small `Ui` helper could hold it, as `FooterBar.TextTop` does for footers. It can wait because each copy is one line. (C2 review)
- `src/Jrpg.Game/Battle/CommandPanelView.cs` is at 281 lines: split it before the next change that grows it (retro on the other machine).
- No debug way to start on a given map and cell: Z's automated walk (on the other machine) couldn't reach the inn or the forest, so those hand checks fell back to the developer. A Debug-only `--start <map> <x> <y>` beside `--content` would let `play-game.ps1` reach any screen.

## From the escape menu third pass plan (2026-10-26)

- The save page's failure line (`SavePage`) is still a red line while the Party page's refusals open the OK dialog (`Ui/NoticeDialog`, decision 268). Kept as it is by decision 284; move it to `NoticeDialog` if the developer asks for it. (Equipment's notice left with the room rule, loot plan G1b-2.)

## From the escape menu third pass plan (2026-10-27)

- `Menus/PartyPage.cs` (296 after the music plan's S4) is at the limit: the next change that grows it splits the notice and the action choice into their own presenter first. (`InventoryPage` split its list into `InventoryList` in the loot plan, C1.)
- A titled dialog's frame (the scrim, a `FramedWindowLayout.Titled` window, the centered title, measured when the dialog changes) is written twice: `ConfirmPromptView` and `NoticeDialogView`. Share it when a third dialog comes, such as Equipment's or the save page's notice (decision 284).
- The inventory hint names the Info button as "X (Y on a controller)" in `ui.json`, a copy of `InputBindings`; give the hint a `{button}` from `ButtonLabels.Of(GameButton.Info, device)` as `cinematic_skip` does, once menu pages know the device in use (F1 review).
- Status: the Skills section starts under the longer of the gear and resource rows, so the stats plan's nine slots will push it off the page; that plan places it (F1 review).
- Lists outside the menu still draw `MenuPointer` at the cursor's row, against the glide rule (`.claude/rules/game.md`, decision 336): the battle's command list (`CommandPanelView`), the defeat menu (`DefeatScreenView`) and the leader choice (`LeaderChoiceScene`). Glide them when next touched; the battle's look follows later anyway (decision 272).

## From the stats and gear slots plan (2026-10-27)

- `Battle/ActionPlayback.Read` is at the complexity limit (the H1 and I1 slices each had to fold cases to pass CA1502): the next `BattleEvent` kind needs a handler table, not another case.
- `SaveGameRepair.RepairMember` sums worn bonuses from item ids beside `WornEquipment.Bonuses` (G1b): a public `WornEquipment` built from the catalog would let the repair use the one sum.
- `ClassCatalogValidator.FindUnreachableRequirementProblem` checks a skill's requirement against the resource's definition max, not its max under Will (`raiseMax`); conservative today, wrong once a skill needs Faith above 5 (W1b review).
- Labels are uppercased in three places (`StatusLabels.Caps`, `EquipmentLabels.From`, `EquipmentChangesView.Measure`): `SheetRowLook.DrawLabel` could cap once, if measuring stays out of `Draw` (E2 review).
- A one-stat `StatBonuses` is built inline about 20 times as `new(new Dictionary<StatKind, int> { [stat] = value })` (`GearPiece.BonusOf`, `GearPieceTests.Bonus`, `MemberSheetTests.Bonus`, `PartyMemberGearTests` and others): a `StatBonuses.Of(stat, value)` factory would replace them (loot plan, G1a review).
- Equipment's `maxStack` is no longer read since gear became pieces (loot plan, G1b-1): drop it from equipment entries in `items.json` and the validator's rule for them.
- `WholeChance = 100` is declared in three Data validators (`EnemyCatalogValidator`, `StatusGrantValidator`, `DropTableCatalogValidator`): a percent check in `RangeProblems` would hold it once (loot plan, D2 review).
- `.claude/scripts/reflow-comments.pl` (run by `verify.ps1 -Fix`) sometimes turns the first line of a rewrapped `///` block into `//` or `/// //` (loot plan: S1a's `SaveGameFixtures`, P1b's `VictoryDropRow`, both blocks holding a character outside ASCII or an edited first line): the build catches it as bad XML or a doubled summary, but the script should keep the prefix.
- "A rarity's color unless Common, else a fallback" is written twice (`InventoryListView.TextColorOf`, `EquipmentChoiceView.Show`): a third copy moves it into `UiTheme.Loot` as `RarityColorOr(rarity, fallback)` (loot plan, P1b review).

## From the loot plan (2026-10-29)

- The Inventory page's Weapons and Armor tabs still show the QTY column title, though each of their rows is a piece without a count (P1a).
- `Battle/VictoryScreenView.cs` is at 263 lines since the Items step (D3): the next change that grows it moves the cards' drawing into a `VictoryCardsView` first.
- `tests/Jrpg.Core.Tests/Session/GameSessionSaveTests.cs` is at 270 lines: the next save test goes in a new file split by concern (gear, members, world).

## From the affix stats plan (2026-10-29)

- `Battle/ActionPlayback` counts a target's numbers already shown this beat twice (`_numbers.Count(… ShowsAt == TimeSpan.Zero)`, for a hit and for stolen life, A1): a third copy extracts a `StackOf(target)`.
- `Menus/StatusParametersView.Draw` draws the Resistances' and Affixes' section titles inline while `DrawCombat` draws its own (ST1b review): a `DrawSection(frame, title, section, cells)` would keep `Draw` at one level; it also has a doubled blank line above `Place`.

## From the forge plan (2026-10-29)

- The "lists 'x' more than once" check is written in six Data validators (`NewGameValidator`, `ClassCatalogValidator`, `ResourceDefinitionValidator`, `SkillCatalogValidator`, `MapEventValidator`, `CraftingRulesValidator`), and `NewGame`'s `startingItems` repeats the crafting lists' entry checks (null, known item, 1..maxStack): one helper beside `RangeProblems` (M4 review).
- `Ui/OverlayFade` (the Forge's and the Shop's fade, gold plan L1) is the one frame fade for screens over the map; `MenuMotion` (the tabs' slide too) and `TitleEntrance` (from black) stay separate because they do more than this fade; fold them in only if a third screen needs the extra.
- `ForgeScene.Craft` re-derives Core's "the first equal piece is the one replaced" rule to put the highlight on the changed row (F2c): `Inventory.TryReplace` (or the Forge's `Try` methods) could return the replaced index instead.

## From the quests plan (2026-10-29, at approval)

- **A page list's margins (28, 20) and row height (52) are private constants in about eight views**: `Crafting/ForgeListView`, `Menus/InventoryColumnView` and `InventoryListView`, `EquipmentChoiceView`, `EquipmentSheetView`, `PartyColumnsView`, `StatusSheetView`, `Saving/SaveFilesPanel` (and the gold plan's `Trade/ShopListView`). Once the quests plan's L1 has made `Ui/PageListLayout`, each reads it when next touched. It can wait because the copies hold the same numbers today.
- **The pointed tabs are drawn inline twice**: `Battle/VictoryDropsView` (the Items step) and the gold plan's `Trade/ShopListView` (Buy and Sell). Once the quests plan's L2 has made `Ui/PointedTabsView`, both draw through it when next touched. It can wait because each copy is drawn once per screen and looks the same.
- `ShopStockRepair` never caps a saved stock's `RolledAtLevel` by the experience table's top level (S6 review): after content lowers the max level below it, that shop never rerolls; bring it down with a report line (decision 360's rule).
- `ForgeView` and `ShopView` draw the same frame (scrim, header, footer, the two column areas from `MenuLayout`), the second copy (gold plan H1): share it in one type once `MenuLayout`'s frame numbers live in `Ui/` (Ui can't read `Menus`).
- `ForgeList` and `Menus/InventoryList` each hold the cursor-plus-gliding-highlight pair that `Ui/RowSelection` now packages for the Shop's lists (gold plan B2): switch them to it when next touched.
- `StatusSkillsView.DescriptionLayout` (a wrapped-text layout) has two users, the Status page and the Shop's detail column (gold plan L2a); move it to `Ui/` and fix its doc, which still names the Status page's font size.
- `ShopListView.Draw` and `ForgeListView.Draw` take 5 parameters (the limit is 4): pass the fixed area to their constructors, as `ShopDetailView` does (L2b review).
- `ShopScene` is at 286 lines after the sound pass's SH3 (the preview left in SH1): the next slice that adds to it splits the list's input (moves, category changes and their sounds) into its own type first.

## From the gold and shops plan (2026-11-01)

- `Game/Exploration/EventPresenter` is at 293 lines after the sound pass's FE3 (its page reading left for `MessageReading` in FE1); the next growth past about 300 lines moves the rest's handling (`ShowRest` and the rest's line in `StartReading`) beside `RestPlayback` first.
- `Scenes/ScreenFade`: one update longer than the 0.08 s hold at black jumps over it, so `EventPresenter.ShowWarp` never sees `Holding`, the warp's fade runs out and in over the old map, then starts again with its sound (found by a test stepping 0.1 s in the sound pass's FE3). The fixed 1/60 s step hides it; a long frame would show it: let a fade report a hold it passed.
- The "move down until the wanted row" test helper is written three times (`ShopSceneBuyTests`, `ShopSceneSellTests`, `ShopScenePreviewTests`): `Trade/ShopDriver.MoveDownUntil` (the sound pass, SH2) is the shared one they can switch to.
- Private draw helpers over four parameters: `ShopDetailView.DrawRow` (6), `DrawPreview` and `DrawPicker` (5), `ShopListView.DrawTabs`, `DrawRowSelection` and `DrawRow` (5); fold the area or the opacity into a per-frame record when each view is next touched.
- The gold plan's out-of-scope features (editing shops in the editor, gold in the save list, a `giveGold` step, Unique items in shops, the whole party's preview at once) are listed in `docs/plans/done/2026-10-29-gold-and-shops/README.md`, "Out of scope".
- The copied-content-root fixture (`CopiedContentRoot`, quests QD) exists; `ContentCatalogLoaderShopTests`, `ContentCatalogLoaderCraftingTests`, `ContentWriterTests` and `ContentWriterShopTests` still copy the content themselves: switch them when next touched.
- The "enum to its camelCase JSON name" one-liner is written five times (`ResistancesConverter`, `StatBonusesConverter`, `CraftingRulesValidator.NameOf`, `LootRulesValidator.NameOf`, `MapEventValidator.JsonNameOf`): one shared Data helper (QV review).
- Data enumerates "a layout's events" three times (`MapEventValidator.AllEvents`, `MapEventSteps.Of`, `WorldStateRepair`); `MapLayout.AllEvents` (quests QE1) is the shared member to move them to.
- `ObjectIds.ProblemOf` is the content id rule for any typed id (the Quests window uses it for quests and stages, QE2); rename it to a `Catalogs/ContentIds` shared by objects and quests.

## From the quests plan (2026-11-01, at the retro)

- `Core/Session/GameSession.cs` is at 289 lines (the quests plan added the journal and the log's capture and restore): the next member moves the save capture and restore into their own type first.
- `tests/Jrpg.Data.Tests/Content/ContentWriterTests.cs` (289) and `tests/Jrpg.Game.Tests/Exploration/EventPresenterTests.cs` (306): new tests go in sibling files, as `ContentWriterQuestTests` and `EventPresenterQuestTests` do.
- `Objects/ObjectIds.ProblemOf`'s message says "(like tree_4)" and the Quests window shows it for quest and stage ids (QE2): reword it with the `ContentIds` rename above.
- `CatalogDraft.SetQuests` builds its dictionary with `ToDictionary`, which throws on a repeated id; only `QuestCatalogViewModel.Problem` stands before it (QE1): refuse with the validator's message instead.
- `Ui/ResourceGaugeView.LabelSpacing` (2) duplicates `SheetRowLook.LabelSpacing`, which `QuestDetailView` reads too (L3 review): one constant in `Ui/`.
- The quests plan's out-of-scope features (the Quest giver preset, removing a saved quest or stage in the Quests window, a cell's touch event that changes with a quest, rewards, story flags, map markers) are listed in `docs/plans/done/2026-10-29-quests/README.md`, "Out of scope".

## From the enemy skills and bosses plan (2026-11-01)

- `BossFightSimulation.PlayFight` repeats the battle setup and loop of `ForestFightsSimulation.RunBattle`: share them when a third simulation needs them (S1 review).
- `Battle/ActionPlayback` is at 292 lines after the music plan's B2; `Read` kept its case count only by sending `EnemyPhaseEntered` through its default arm into `AddCue` (B3): the next event kind brings the handler table the `ActionPlayback.Read` line asks for.
- `BattleTurnSequenceTests` writes `new EnemyTactics(random, new Dictionary<string, SkillDefinition>())` five times (T1): a small helper, as `EnemyTacticsTests` has.
- The enemy plan's out-of-scope features (a battle event step and a boss beaten for good, more conditions, enemy resources, cooldowns, phases that change stats or art, a boss banner, skills for the goblin and the beetle) are listed in `docs/plans/done/2026-10-29-enemy-skills-and-bosses/README.md`, "Out of scope".

## From the music and sound effects plan (2026-11-01)

- `OptionsPageTests` (lines 53-88): the older text speed tests reach their row with one Down press; `PageOn(OptionsPage.TextSpeedRow)` now exists (V1 review).
- `SystemPageTests`' older tests (`Update_ConfirmQuit_AsksBeforeRequesting`, `Update_YesToQuit_RequestsQuit`, `Update_YesToTitleScreen_RequestsTitle`, `Update_AnswerNo_StaysInPage`) and the new `Update_ConfirmQuit_AsksNothing` reach rows by fixed presses: `SystemPage` exposes no selected entry (S3 review).
- `BattleSceneTests` and `BattleSceneSoundTests` reach command rows by fixed presses (Up wrapping to Run, Down counted to Defend), since `BattleScene` doesn't expose its menu's selection (B4 review).
- `Battle/BattleScene` is at 288 lines after the music plan's B5: the next change that grows it moves its outcome screens' update and finish into their own type first.
- `Menus/OptionsPage.DrawRow` is 25 lines since the volume pips (V2): extract the pips' drawing when the page is next touched.
- `UiArtFilesTests` checks with `File.Exists`, which ignores case on Windows; `UiSoundFilesTests` compares exact names: do the same for the art (P1).
- `InventoryPageSoundTests` and the older Inventory tests lean on the list opening on the potion: a small helper that moves to an item by id (S4 review).
- The music plan's out-of-scope features (settings kept between runs, composed music, crossfades, streaming, field sounds, a `playMusic` step, pausing on lost focus) are listed in `docs/plans/done/2026-10-29-music-and-sfx/README.md`, "Out of scope".
