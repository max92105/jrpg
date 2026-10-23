# Retiring the WPF demo: design

Part of [the demo retirement plan](README.md): the tunables, the gap survey, the design overview and the risks. Slices point here for the details they build on. "The demo" is the WPF program under `legacy/` (`JRPG/`, `JRPG.Core/`, `MapEditor/`).

## Tunables and placeholders
"Where" is the one place the value lives. The demo's WPF units were 1:1 on 1920 x 1080 (battle plan), so its sizes carry over unchanged.

| Name | Where | Value | Meaning | Slice |
|---|---|---|---|---|
| device in use | `Input/InputReader` | keyboard at start; switches on the first press of the other device (a pad button or the stick past its dead zone) | Question 147. | I1 |
| button names | `Input/ButtonLabels` | the first binding of the button on that device, by its key or button name (`E`, `A`) | Decision 138, question 147. Names of physical keys, not translated text. | I1 |
| prompt position | `Exploration/PromptPlacement` | centered over the object, its bottom 6 px above the object's top edge | The demo's prompt. | G1a |
| prompt panel | `Ui/UiTheme` (`PromptFill`, `PromptStroke`, `PromptTextSize`, `PromptPadding`, `KeyCapFill`) | fill `#C8000000`, 1 px stroke `#A0FFFFFF`, corner radius 4, padding 8 x 3, text 16; the key in a small light key cap | The demo's box plus the key cap; placeholder look, checked against `docs/ui_ux_exemples/`. | G1b |
| prompt fade | `Exploration/PromptFade` | 0.15 s in, 0.1 s out | New (the demo popped it); placeholder. | G1b |
| footer hints | `content/text/ui.json` `menu_hint_*` | main menu "Choose a menu.", Inventory "Choose an item to read about it. Confirm to use it.", Party "Choose who fights, and in what order.", Equipment "Choose a slot to change what is worn.", Status "Choose a character.", Save "Choose a file to save to.", Load "Choose a file to load.", Options "Choose a setting." | The demo's hints, its mouse words ("hover", "click") replaced; placeholders. | H1 |
| footer hint text | `Menus/GameMenuScene` | left-aligned in the 64 px footer, 40 px in, `UiTheme` secondary text | The demo's footer. | H1 |
| page transition | `Menus/PageTransition` | 24 px slide from the right and a fade in, 220 ms, cubic ease-out | The demo's `PlayPageTransition`. | T1 |
| sidebar placement | `Menus/GameMenuScene` | right column 460 px wide, 24 px gap; on every page except Party; not on the main menu; hidden while a page shows its own side panel | Question 145, the demo's layout. | P1 |
| sidebar cards | `Menus/PartySidebar`, `Ui/MemberCardView` | active members in party order: portrait, name, level, HP and MP bars with numbers, 12 px apart | Question 146, the demo's `PartyMemberCard`. | P1, P2 |
| who can give an item up | `Core/Party/PartyEquipment.TakeableFor` | any other member, active or reserve, in `PartyState.Members` order; not an item the member already wears in that slot | Decision 137. | Q1 |
| the receiver's own item | `PartyEquipment.TryTakeFrom` | back to the bag; refused when the bag has no room for it | Decision 137; same rule as `TryEquip` (ADR 0003). | Q1 |
| worn row | `content/text/ui.json` `equipment_worn_row`, `Menus/EquipmentChoiceView` | "{item} ({member})", in the secondary text color | Decision 137. | Q2 |
| take hint | `content/text/ui.json` `equipment_take_hint` | "Taking it leaves {member} without it." in the footer while a worn row is highlighted | Placeholder wording; needs H1. | Q2 |

## Gap survey

Checked on 2026-10-22, file by file, against `legacy/` (153 `.cs`, 51 `.xaml`). Each feature is one of:
- **ported**: same behavior;
- **differently**: ported in another shape, which is fine;
- **gap**: ported by this plan, which names the slice;
- **dropped**: by the decision named;
- **new**: the demo didn't have it; a later plan.

### Game systems (`legacy/JRPG/Managers`, `legacy/JRPG.Core`)
| Demo | Now | Class |
|---|---|---|
| `PlayerManager` walking, collision, diagonal sliding | `PlayerWalker`, `MapCollision`, `MoveIntent` (diagonals no longer faster) | differently |
| `PlayerManager` animation; `CameraManager` easing | `WalkAnimation`, `MapCamera.Follow` (time-based) | ported |
| `GameManager` interact (E/Space/Enter on the object in front), touch events, "once" events | `ExplorationState.TryInteract`, `InteractionFinder`, `EventRun` (ADR 0006) | ported |
| `GameManager.UpdatePrompt`: a "[E] Check" box over the object in front | nothing; `MapEvent.Prompt` is required by `MapEventValidator` and never shown | **gap: I1, G1a, G1b** (with the device's key, decision 138) |
| `GameManager.WarpAsync`, `FindArrivalPoint`, touch disarmed after a warp | `EventPresenter` + `ScreenFade`, `WarpArrival`, `_isTouchArmed` | ported |
| `GameManager` random battles (1 in 500 per update, 1 s grace after a warp) | `RandomEncounters` per step with grace steps (decision 26) | differently |
| `TriggerHandler` warp, dialogue, item tiles | cell events with `WarpStep`, `DialogueStep`, `GiveItemStep` | differently |
| `TriggerHandler` / event `Battle` action: three hardcoded goblins, rest of the event skipped | nothing | new (question 30) |
| `DialogManager`, `DialogueBox` | `SpeechPlayback`, `Typewriter`, `DialogueBox` | ported |
| `CutsceneManager.StartIntroCutscene` (F1), `CutscenePlayer`, `Cutscenes/*` steps | `GameButton.DebugCutscene` bound to F1, read by nothing | dropped (decision 133); X1 removes the button; sketch below |
| `SceneManager`, `GameFlowManager`, `GameStateManager`, `GameLoopManager` | `GameFlow`, `SceneStack`, transitions, MonoGame's loop (ADR 0008) | differently |
| `SceneManager` blur behind overlays | dim (question 52) | differently |
| `InputManager`, `InputRouter` (keyboard, mouse in menus and battle) | `InputReader`, `InputBindings`; keyboard and controller only | differently (decision 50) |
| `MapRenderer` draw order, canopies; `MapManager` walkability | `MapDrawList`, `MapView`, `CanopyMask`, `WalkabilityGrid` | ported |
| `MapRenderer` tile animation timer (0.2 s) | none; no tile had frames | dropped (question 69 rest, placeholder) |
| `BattleManager`, `Battles/*` | `Battle/*`, `Effects/*`, `Progression/*` (ADR 0004) | ported |
| `Battles/Models/Enemies/Goblin` (stats in code) | `content/enemies.json` | differently |
| `Systems/Inventories`, `Equipments`, `Parties` (`GameSystems` static singletons) | `Inventory`, `PartyEquipment`, `PartyState` in one `GameSession` | differently |
| `EquipmentManager`: taking a worn copy from another member | `PartyEquipment` only moves between bag and member | **gap: Q1** |
| `SaveGameManager`, `SaveData`, `SaveDataPreview` | `FileSaveStore`, `SaveGame`, `SaveSummary` (ADR 0007) | differently |
| `SaveData.Gold` (never set) | none | new (decision 103) |
| `WorldState` | `WorldState` | ported |
| `Utils/AssetPathResolver`; `Utils/TileGroupObjectCreator` | `ContentRootLocator`, `ArtFolders`; editor `ObjectFromCells` | differently; ported |

### Game screens (`legacy/JRPG/UI`)
| Demo | Now | Class |
|---|---|---|
| Start screen (`MainWindow`), load overlay | `Title/TitleScene`, `TitleMenu`, `Saving/SaveFilesPanel` | ported |
| `GameMenuControl`, `MenuTab`, `MenuHeader`, `ConfirmDialog`, `ListRow` | `Menus/GameMenuScene`, `MenuHeader`, `Ui/ConfirmPrompt` | ported |
| `GameMenuControl.HintText`, `pageHint`, `MenuPage.HintRequested` / `ShowHint`: a one-line hint in the footer, per page, replaced while a page asks | `GameMenuScene` draws an empty footer bar; `.claude/rules/game.md` asks for the hint | **gap: H1** (review find, can be struck) |
| `GameMenuControl.PlayPageTransition`: a page slides 24 px and fades in over 220 ms | pages appear at once | **gap: T1** (review find, can be struck) |
| `PartySidebar` (`PartyMemberCard` per active member, beside every page but Party) | nothing | **gap: P1, P2** (decision 139) |
| `StatusView`, `StatsPanel`, `StatLine` | `Menus/StatusPage`, `Ui/StatBar` | ported |
| `InventoryView` (categories, use on a target) | `Menus/InventoryPage`, `ItemTargetPicker` | ported |
| `EquipmentView`, `EquipmentSelectionView` (choices, Remove, Remove all, stat preview) | `Menus/EquipmentPage`, `EquipmentChoice`, `StatPreview` | ported |
| `EquipmentSelectionView.AskWhoToTakeFrom` | nothing | **gap: Q2** (one row per wearer, decision 137) |
| `EquipmentSelectionView.ShowEquippedBy` ("Equipped by" portraits) | each worn row names its wearer | differently (decision 137) |
| `PartyView`, `PartyMemberCard` | `Menus/PartyPage`, `PartyColumns` | ported |
| `OptionsView` (empty) | `Menus/OptionsPage` (full screen, text speed) | differently |
| `SaveLoadView`, `SaveSlotPanel` | `Menus/SavePage`, `LoadPage`, `Saving/*` | ported |
| `BattleUI` and its controls; `VictoryScreen`, `ExpGainRow`, `DefeatScreen` | `Battle/*` (ADR 0013) | ported |
| `UI/Styles/Theme.xaml`, `Bars.xaml`, `MenuStyles.xaml`, `BattleStyles.xaml` | `Ui/UiTheme`, `UiTextures` | ported (decision 82) |
| `UI/Styles/Art.xaml` vector icons (menu tabs, stats) | text-only tabs (decision 51) | dropped (decision 140: kept in git history) |

### Editor (`legacy/MapEditor`)
| Demo | Now | Class |
|---|---|---|
| `EditorMainWindow` menus (New, Open, Save, Undo/Redo, Delete, Grid, Collision and zones, Fit, Actual size, Map properties, Tileset manager, Edit object, F1) | `Shell/MainWindow` menu bar, map list, F1 help | ported (editor plan) |
| Save As | none | dropped (decision 70) |
| Paint, rectangle, fill, sizes, fill/clear layer, dim other layers | `Maps/Editing`, `TilePaintingViewModel` | ported |
| `Commands/*`, `UndoManager` | `IMapEdit`, `EditHistory`, `CatalogHistory`, `UndoJournal` (ADR 0012, 0014) | differently |
| `TilesetManagerWindow`, `ZoomableImageViewer` | Tiles panel, Cut panel, import, Mark blocked | differently (ADR 0014) |
| Tileset manager: delete tile, tile names, tile collision polygons, animated tiles | none | dropped (question 69 rest, placeholder) |
| `ObjectEditorWindow`, `CollisionEditorWindow` | `Objects/ObjectShapesWindow` | ported |
| `NewObjectWindow`, selection mode | `NewObjectWindow`, `ObjectFromCells` | ported |
| `PropertyEditor` (object name, `IsNpc`, `Meta`) | category only; NPCs are field-sprite objects | differently (ADR 0015); names: question 69 |
| `NpcEditWindow` | `NewNpcWindow` + a Talk event | differently (ADR 0015) |
| `TriggerEditWindow`, `ObjectEventWindow`, `MapPropertiesWindow` | cell and object event dialogs with presets; `Maps/Settings` | ported |
| "Tile Z" and "Object Part Z" modes (`ZEditPopup`) | none; `zOverrides` empty in every map | dropped (question 69 rest, placeholder) |

### Content and art
| Demo | Now | Class |
|---|---|---|
| `Assets/Maps/*.json`, tileset, objects, tilesheets JSON | `content/` (ADR 0011) | differently |
| `Assets/Maps/Back/*` | none | dropped (decision 39) |
| Attack GIFs | PNG strips in `content/art/effects/` | differently (decision 57) |
| `Objects/Tree1-3.png`, `playerx64.png` | none | dropped (decision 74) |
| Default dialogue portrait `Fighter.png`; `TargetArrow.png` | in `content/art/`, referenced by nothing | dropped (decision 141; X1 deletes them) |

## Design overview

### What exists, checked (2026-10-22)
- **Input.** `InputReader.Read(keyboard, pad)` builds an `InputFrame(Move, held, pressed)`. Nothing knows which device the player is using. `InputBindings` lists key and pad bindings per `GameButton`; Confirm's first bindings are `Keys.E` and `Buttons.A`.
- **Prompt.**
  - `MapEvent.Prompt` is required on Interact events by `MapEventValidator` (8 prompts in `start` and `interior_1`). The editor's presets fill it in ("Open", "Enter", "Read", "Talk", default "Check").
  - `InteractionFinder.FindInFront` (internal) returns a `TriggeredEvent(MapEvent, PlacedObject?)`, a sealed record, so every call that finds something allocates. `Walk` already calls it every moving frame.
  - `ExplorationScene`'s constructor is at four parameters: `ExplorationState`, `ContentCatalog` (read only for `NewGame.FieldSpriteFile`), `MapArt`, `EventPresenter`.
- **Menu.**
  - `GameMenuScene` draws header and footer bars (footer 64 px, empty) and gives the open page a fixed `_pageArea`, through `IMenuPage.Draw(batch, textures, area, opacity)`.
  - Pages appear at once.
  - `Ui/MemberCardView`, `MemberVitalBars`, `Diamond` and `StatBar` exist, and the Party page and battle already use them.
- **Equipment.**
  - `PartyEquipment(Inventory)` has `TryEquip`, `TryUnequip` and `WearableFor(member, slot, items)` (owned copies only). `PartyMember.Wear`/`TakeOff` are internal. `PartyState.Members` lists everyone, active then reserve.
  - `Menus/EquipmentPage` (215 lines) opens an `EquipmentChoice(IReadOnlyList<ItemDefinition>)` and applies an `EquipmentPick(ItemDefinition? Item)` in `Apply`.
- **Failing tests.** 5 tests pin repository totals: 283 tiles and 13 objects (`ContentCatalogLoaderTests`), the whole object list (`ObjectBrowserViewModelTests`), and the whole layer list of the opened map (`MainViewModelTests`, two).
- **The word.** Matching lines, case-insensitive:

| Area | Lines | Files | Slice |
|---|---|---|---|
| `src/Jrpg.Core` | 15 | 11 | W1 |
| `src/Jrpg.Data` | 3 | 2 | W1 |
| `tests/Jrpg.Data.Tests`, `tests/Jrpg.Simulations` | 5 | 4 | W1 |
| `src/Jrpg.Game/Ui` (`UiTheme` alone 119) | 149 | 16 | W2 |
| `src/Jrpg.Game/Battle` | 48 | 31 | W3 |
| `src/Jrpg.Game`: Menus 38, Exploration 14, Title 8, Saving 6, Dialogue 5, Input 3, Flow 1, Scenes 1 | 76 | 37 | W4 |
| `tests/Jrpg.Game.Tests` | 6 | 4 | W2, W3, W4 |
| `src/Jrpg.Editor` | 54 | 33 | W5 |
| `tests/Jrpg.Editor.Tests` | 4 | 4 | W5 |
| `src/Directory.Build.props` | 2 | 1 | X1 |
| `content/` | 0 | 0 | - |
| `.claude/` (rules 1, agents 2, agent memory 16) | 19 | 8 | W6 |
| `docs/plans/README.md`, `docs/decisions/README.md` | 3 | 2 | W6 |
| `CLAUDE.md` 3, `README.md` 1, `AI-SETUP.md` 3, `.editorconfig` 1 | 8 | 4 | X1 |
| `docs/decisions/0001`-`0015` | 44 | 13 | history (decision 134) |
| `docs/plans/done/**`, the 2026-10-18 plan | 395 | 38 | history (decision 134) |

  The word also appears in identifiers and names:
  - identifiers: `UiTheme.FromLegacy` (private, about 70 uses) and five test names (W1, W2, W5);
  - names on disk: `legacy/`, `docs/plans/done/2026-10-17-legacy-core-migration.md`, `.claude/agent-memory/architect/project_legacy_content_quirks.md`.
- **Nothing builds or references `legacy/`.** `Jrpg.slnx`, the csproj files, `verify.ps1` and the hooks don't name it; only docs and comments do. Untracked leftovers of old demo builds sit at the root (`MapEditor/bin`, `packages/`, `.vs/JRPG`); `packages/` is in `.gitignore`.

### Core changes
| Member | Why | Slice |
|---|---|---|
| `Exploration/TriggeredEvent` becomes a `readonly record struct` | Probing every frame (the prompt, and touch events already) allocates nothing. | G1a |
| `Exploration/InteractionTarget` (public sealed record: `PlacedObject Object`, `MapEvent Event`) | What the player can start by pressing Confirm, for the prompt. | G1a |
| `ExplorationState.InteractionInFront` (`InteractionTarget?`) | Refreshed at the end of every public command (`Walk`, `TryInteract`, `ContinueEvent`, `FinishEncounter`), on every return path, and on map entry. Null while an event runs or a battle is pending. A new record only when the object or event changes. | G1a |
| `Party/WornItem` (public sealed record: `PartyMember Wearer`, `ItemDefinition Item`) | One item worn by one member: a row the player can take from. | Q1 |
| `PartyEquipment.TakeableFor(PartyMember member, EquipmentSlot slot, PartyState party)` | What other members wear in that slot that the member's class can wear, except an item the member already wears there; one per wearer, in `Members` order. | Q1 |
| `PartyEquipment.TryTakeFrom(PartyMember giver, PartyMember receiver, EquipmentSlot slot)` | Moves the giver's item to the receiver; the receiver's own item goes to the bag. False and no change when the giver is the receiver, the giver's slot is empty, the receiver's class can't wear the item, or the bag has no room for the receiver's item. | Q1 |

No new Core folder and no new folder edge.

### Game structure added by this plan
```
Jrpg.Game/
  Input/        InputDevice, InputFrame.Device, ButtonLabels (I1)
  Exploration/  PromptPlacement, InteractionPromptView (G1a; look G1b), PromptFade (G1b)
  Rendering/    MapArt.PlayerSpriteFile (G1a)
  Menus/        IMenuPage.Hint (H1); PageTransition (T1); IMenuPage.ShowsPartySidebar, PartySidebar, SidebarCard (P1);
                PartySidebarView (P2); EquipmentPick.Giver (Q2)
  Ui/           Easing (T1); UiTheme prompt and key-cap values (G1b)
```
- `Input/InputDevice` (enum): the keyboard or a controller.
- `Input/ButtonLabels`: the name of the key or pad button bound to a game button on a device (pure; first binding in `InputBindings`).
- `Exploration/PromptPlacement`: where the prompt box goes on the screen (pure, static).
- `Exploration/InteractionPromptView`: draws the key and the prompt of `ExplorationState.InteractionInFront`. Text is measured only when the target or device changes.
- `Exploration/PromptFade`: the prompt's opacity over time as its target appears, changes and goes (pure).
- `Menus/PageTransition`: a page's slide offset and opacity over time after it opens (pure).
- `Ui/Easing`: the clamped time share and the cubic ease-out (pure, static). The backlog's six copies can move to it later.
- `Menus/PartySidebar`: the active members' card texts, rebuilt only when a shown value changes (pure).
- `Menus/SidebarCard` (record): one sidebar card's member and its formatted texts.
- `Menus/PartySidebarView`: draws the sidebar's cards.

**Folder dependencies (Game), checked edge by edge:** no new edge.
| Folder | Depends on | This plan uses |
|---|---|---|
| `Input` | Core | nothing new (Microsoft.Xna input types it already reads) |
| `Ui` | Rendering, Input | `Easing` depends on nothing |
| `Exploration` | Scenes, Dialogue, Hosting, Ui, Rendering, Input | `InteractionPromptView` uses `Ui`, `Rendering`, `Input` (`ButtonLabels`) |
| `Menus` | Scenes, Saving, Hosting, Ui, Rendering, Input | `PageTransition` uses `Ui/Easing`; `PartySidebarView` uses `Ui/MemberCardView` |

**`ExplorationScene` stays at four constructor parameters.** It takes `InteractionPromptView` in place of `ContentCatalog`, which it read only for the player's field sprite. `MapArt` now carries that file as `PlayerSpriteFile`, read from `catalog.NewGame` when it is built.

### Key decisions
1. **Core says what the prompt is about; the Game words it.** `InteractionInFront` gives the event, whose `Prompt` is content text. The Game draws the device's Confirm key (decision 138) before it. The prompt is hidden while an event runs or its fade plays (`EventPresenter.IsBusy`) and while a battle is pending.
2. **One query, cached, refreshed on every exit of a command.** Each public command of `ExplorationState` ends through one private refresh (a `try`/`finally`, or a single exit), so an early return can't leave a stale target. For example, an Interact event that finishes inside `TryInteract`. `TriggeredEvent` as a struct removes the per-probe allocation.
3. **The input device is a Game fact.** `InputReader` knows the last device used. Key names come from the bindings, so a changed binding changes the prompt with it. They are names of physical keys (`E`, `A`), not translated text (ADR 0010 covers sentences).
4. **Pages tell the menu what they need.** `IMenuPage` gains two read-only properties:
   - `Hint`: the page's current footer line, already formatted;
   - `ShowsPartySidebar`: false on Party, and false while the page shows its own side panel.

   `GameMenuScene` reads them each update. Pages update their hint only when it changes, so nothing is allocated per frame.
5. **The sidebar narrows the page.** With the sidebar shown, the scene gives the page a narrower `area` (question 145). Pages already draw into `area`. A page that assumes the full width is fixed in P1 (risk).
6. **Taking is a Core move** (ADR 0003: a worn copy isn't in the bag). `TryTakeFrom` moves the item between members; the bag only takes the receiver's old item. The Game never calls `TryUnequip` then `TryEquip`.
7. **One row per wearer** (decision 137). `EquipmentChoice` lists "Remove", the owned items (`WearableFor`), then one row per `WornItem` (`TakeableFor`). `EquipmentPick` gains `Giver` (`PartyMember?`). Confirming a worn row calls `TryTakeFrom(giver, shown member, slot)`.
8. **Wording rules (decision 135).**
   - A comment that cited the demo now states the rule, the decision, or "placeholder".
   - Comments never point to a demo file; they say the role of the value instead.
   - Test names lose the word and keep their scenario.
   - Behavior and values don't change.
9. **The guard is a plain text scan with an explicit allow list** (decision 136, X2).
   - The pattern is built from two pieces (`'leg' + 'acy'`), so the script doesn't flag itself.
   - The history it skips is named file by file for ADRs 0001-0015, and by folder for `docs/plans/done/` and this plan.

### Cutscenes later (decision 133: a sketch for a later plan, no code here)
The demo's cutscene was a hardcoded list (fade, background, sprites at 96 px cells, dialogue, a timed move) behind F1. A real one should build on what the game already has.
- **A cutscene is a map event.** It reuses `MapEvent`, its activations (plus a new "on entering the map" activation) and "once" marking (ADR 0006). It is not a separate player or a separate file format. Saves already refuse a mid-event capture, so they need no change.
- **New event steps, beside `DialogueStep`, `WarpStep` and the others:**
  - `MoveActorStep(actor, path, speed)`, `FaceStep(actor, facing)`, `WaitStep(seconds)`;
  - `FadeStep(out/in, seconds)`, `CameraStep(target or cell, seconds)`;
  - `ShowPictureStep(file)` for a full-screen background like the demo's.

  Dialogue is the existing step.
- **Actors are placed NPC objects** (ADR 0015) and the player. Moving them needs runtime positions in Core, the same need as NPC movement (question 127). One design should serve both.
- **Time enters Core.** Today `EventRun` only waits for `Continue()`. Timed steps need `Update(TimeSpan delta)` on the run, or a step that reports its duration while the Game times it. Core must stay deterministic (delta only).
- **The Game plays steps** in `EventPresenter`, as it does now: exhaustive over step types, each step a beat.
- **The editor** gets a step editor per new step, and a way to pick actors on the map.
- **What its plan and ADR decide:**
  - where runtime actor positions live (`ExplorationState` or a run-owned stage);
  - who owns time (Core run or Game beat);
  - whether a cutscene can be skipped, and what then happens to its remaining effects;
  - whether battles started by events (question 30) are a step of the same system;
  - the camera's rules while a cutscene runs.

### Extension points
- `InteractionTarget` is where a later per-NPC prompt or "!" bubble reads from.
- `ButtonLabels` and `InputFrame.Device` serve every later key hint (footer hints showing keys, battle help).
- `IMenuPage.Hint` serves any page's contextual help.
- `TryTakeFrom` is the move a later "optimize equipment" would reuse.
- The guard's allow list is one array in `verify.ps1`; each new history location goes there.

### Port mapping (demo to new)
| Demo | New | What changes |
|---|---|---|
| `GameManager.UpdatePrompt`, `promptBox` | `ExplorationState.InteractionInFront`, `Exploration/InteractionPromptView`, `PromptPlacement`, `PromptFade` | The target is a cached Core query. The key follows the device. A fade replaces the pop. |
| `GameManager.FindEventInFront` (prompt use) | `InteractionFinder.FindInFront` | Reused as is (struct result). |
| `GameMenuControl.ShowHint`, `pageHint`, `MenuPage.HintRequested` | `IMenuPage.Hint`, read by `GameMenuScene` | A property the scene reads, not an event. |
| `GameMenuControl.PlayPageTransition` | `Menus/PageTransition` | Time-based, pure, tested. |
| `PartySidebar`, `PartyMemberCard`, `showPartySidebar`, `ShowSecondary` hiding it | `Menus/PartySidebar`, `PartySidebarView` over `Ui/MemberCardView`; `IMenuPage.ShowsPartySidebar` | Pages say whether they want it, instead of the menu toggling it on events. |
| `EquipmentSelectionView.AskWhoToTakeFrom`, `ShowEquippedBy` | `PartyEquipment.TakeableFor`, `TryTakeFrom`, one row per wearer | No locked list or portrait click. The row is the choice. |
| `InventoryEntry.EquippedTo` | `WornItem` from the members | Wearers are read from the members (ADR 0003), never stored twice. |

### Backlog items near this plan
- `EquipmentPage.Apply` works out "no room in the bag" itself (menu cleanup backlog). Q2 adds the take case to `Apply`. Not pulled in; Q2 notes it if `Apply` passes about 20 lines.
- "The clamped time share is copied 6 times" (battle backlog): T1 creates `Ui/Easing` for its own use, and updates that backlog line to point there. It doesn't move the six copies.
- "The footer bar with its hairline is drawn 3 times" (battle backlog): H1 draws text into the menu's footer only; not pulled in.
- `Ui/MessageIds.cs` is 305 lines. H1 and Q2 add about ten ids. Not split here.
- The content-total tests line in `backlog.md` is slice 0, which removes it.

## After this plan
The base is one codebase with no reference program. Candidate new-system plans (the developer chooses the order):
1. cutscenes and event scripting, with battles started by events (133, 30; sketch above);
2. NPC movement and facing (127, 128; ADR; shares runtime actors with 1);
3. gold, drops and shops (103);
4. the map name banner and loading screen (33/53, 66);
5. autosave (44, 67);
6. animated battle sprites (100; ADR);
7. audio (76).

## Risks
- **Wording churn hides a behavior change.** About 300 lines over five slices touch comments next to code. The guard against it: words only, "existing tests pass unchanged", and `/review-slice` on the diff. The one code rename is the private `UiTheme.FromLegacy`, done with the IDE's rename.
- **A reason gets lost.** Some comments are the only record of why a value is what it is. The slice files list those lines with replacement wording. "Don't invent a reason": unknown reasons become "placeholder".
- **The prompt flickers** at the edge of an object's reach. `PromptFade` smooths it (G1b); G1a's hand check watches for it.
- **Per-frame allocation.** `InteractionInFront` must not allocate while standing or walking past the same object, and the prompt view must not re-measure. Tests check record identity across walks; the view re-formats only on a target or device change.
- **Device flapping.** A pad with a drifting stick could switch the device every frame. The stick counts only past `StickReader`'s dead zone, and the label text is cached per device.
- **The sidebar narrows pages** (question 145). Status, Inventory and Equipment were laid out for the full width. P1 checks each at the narrower area and fixes overlaps by reading `area` (no new layout system). If a page can't fit, that is a question for the developer, not a silent overlap.
- **`EquipmentPage` size** (215 lines). Q2 adds only routing (the pick's giver). The row logic is in `EquipmentChoice`.
- **Save format.** No change: equipment is already saved per member.
- **Determinism.** No randomness or time in Core here. Fades and transitions use the update's delta.
- **Deleting the folder.** The annotated tag and this survey are the record. The XAML icons live only in history (decision 140).
- **The guard's false positives.** A package, API or file name could contain the word. The guard prints `path:line`, and the allow list is one array.
- **A stale exception.** This plan's folder is on the guard's skip list while it is open. When it moves to `done/` (after the retro), remove that entry: the retro checklist names it (X2).
