# Game: walking, title screen and game menu
Status: approved

First plan of part 3b (game parity). It takes from the part 3b notes at the end of `2026-10-18-shell-cutover-and-map-editing.md`:
- walking and map events (notes 3c-3e);
- the title screen (6c);
- the game menu with its pages (5a-5d, 6b).

Names, tunables and question numbers are reused from there. Battles, cutscenes and the defeat flow stay in those notes for later plans. Revised after the developer's answers and a devil's-advocate review (2026-10-18).

## Goal
At the end of this plan the developer presses F5 and the game opens **full screen** on a **title screen** with New game, Continue, Load and Quit. From there:
1. **Walking.** Keyboard or controller walks the player:
   - walls block, the sprite animates and the camera follows;
   - the chest opens with "Obtained ..." and stays open;
   - the door opens;
   - warp cells fade to the forest and back;
   - no random battles happen yet.
2. **The game menu** (Menu button). It looks like the legacy menu (Octopath Traveler 2 inspired) and has every legacy page:
   - **Status;**
   - **Inventory:** browse by category, use consumables;
   - **Equipment:** wear, swap, remove;
   - **Party:** active members, reserve, order;
   - **Options:** full screen, text speed;
   - **System:** save to one of 20 files, load, back to the title, quit (both ask first).

Saves made here load in the next session from Continue, Load or the menu.

## Order, and why
1. **Movement and map events (M1-M4).** The game can't be played without them, and they need nothing from the menus. They bring the pieces later screens reuse:
   - the screen fade (warps here, screen changes later);
   - the legacy theme and panels (the message box here, every menu later);
   - non-allocating reads on `ExplorationState`.
2. **Full screen (W1).** The game is meant to run full screen (decision 78), and the menus are laid out and checked by hand at full size from then on.
3. **Title screen (T1a-T2).** It introduces the requests scenes send to the flow, and a flow that can run with no game started. Leaving to the title and loading both need this.
4. **Menu shell (G1-G4).** The tabs, the confirmation prompt, the header and Status: what every page and the save pages stand on.
5. **Saving and loading (E1, S1-S3).** They come before the other pages so the developer can keep test sessions between runs. E1 (stable object ids) comes first because saves become real here.
6. **Menu pages (P1-P5).** Inventory, item use, Equipment, Party and Options last. Each is independent, and Options needs the typewriter (M4) and full screen (W1).

## Guiding rules
- **Port mechanisms, not legacy design.** Legacy values become placeholders, each listed below. Every undecided UI or design choice is a numbered question; slices use the placeholder until it's answered.
- **Port the legacy look** (decision 82). Colors, sizes and layouts come from `legacy/JRPG/UI/Styles/Theme.xaml`, `MenuStyles.xaml`, `Bars.xaml` and the menu controls' XAML. Legacy sizes were WPF units on a 1920 x 1080 window, so they carry over 1:1 to the virtual screen. PNGs are used where legacy used PNGs (backgrounds, item and category icons); panels, gradients and glows are drawn in code.
- **Thin shell (ADR 0008).** Scenes return `SceneRequest`s; `Flow` applies them through `SessionTransitions` and `MenuTransitions`. A scene never builds or calls another scene. Any `if` that decides gameplay goes to Core with a test.
- **Pure presenter + thin draw.** Each screen has a pure part (cursor, page state, timings, the request it returns) tested in `tests/Jrpg.Game.Tests`, and a draw part checked by hand with the slice's checklist.
- **No allocation in `Update`/`Draw`.** Text is formatted when the state changes (a page opens, an item is used, the play-time second changes), never every frame. Per-frame reads use the new `ExplorationState` members (`Location` builds a record each call).
- **Keyboard and controller first** (decision 37). No mouse (decision 50).
- **Esc is both Cancel and Menu** (1b). **Cancel wins inside anything nested:** a page, a list, the title's slot list, a confirmation prompt. Menu only opens the menu from the map, and only closes it from the tab list.

### Tunables and placeholders
| Name | Where | Placeholder | Meaning | Slice |
|---|---|---|---|---|
| camera easing | `Exploration/ExplorationScene` | 0.35 moving, 0.1 standing, per 1/60 s, applied as `1 - (1 - share)^(delta x 60)` | Legacy `CameraManager` (decision 86), made time-based. | M1 |
| walk animation | `Exploration/WalkAnimation` | 8 frames, 10 fps; rows Down, Left, Right, Up | Legacy `PlayerManager`, `player.png`. | M1 |
| map sprite | `content/new-game.json` `fieldSpriteFile` | `player.png` for everyone | Decision 58: easy to change; one field. | - |
| warp fade | `Exploration/EventPresenter` | 0.25 s out, 0.08 s black, 0.25 s in | Legacy `SceneManager.TransitionAsync`. | M2 |
| screen-change fade | `Flow/GameFlow` | 0.5 s out, 0.15 s black, 0.5 s in | Legacy new game, load, title. | T1a |
| menu open fade | `Menus/GameMenuScene` | 0.15 s, opacity only | Legacy `GameMenuControl.OpenDuration` (decision 81). | G1 |
| theme colors, text sizes | `Ui/UiTheme` | legacy `Theme.xaml` (text 28/22/18/14 px, accent `#D6DCFF`, selection gradient `#1A1F5C` to `#C8CEFF`, ...) | Decision 82. | M3b |
| nav tab shape | `Ui/UiTextures` | legacy `Shape.NavTab` 410 x 56 arrow | Decision 82. | G1 |
| message box | `Dialogue/DialogueBox` | legacy `DialogStyle`, 1200 x 240 px, 40 px from the bottom | Legacy box doubled for the virtual screen. | M3b |
| typewriter | `Dialogue/Typewriter` | 30 ms per character; marker blinks every 0.5 s | Decision 54. | M4 |
| text speeds | `Hosting/PlayerSettings` | Slow 45 ms, Normal 30 ms, Fast 15 ms, Instant | Question 90. | P5 |
| full screen | `JrpgGame` | borderless full screen (legacy `WindowStyle=None`, maximized); Alt+Enter toggles | Decision 78. | W1 |
| save slots | `Saving/SaveSlots` | 20, `slot_01` to `slot_20`, shown "File 1" to "File 20" | Decisions 34, 62. | S1 |
| visible list rows | `Ui/ListCursor` users | slots 5, items 10, party 7 | How many rows fit on a page. | S1, P1, P4 |

## Out of scope (follow-ups)
- **Battles.** Random battles are off (decision 79). Event battles are a new feature (question 30).
- **Cutscenes and the F1 hook** (notes 7a-7c, question 68).
- **Editor:** only E1 (object uids); the editor catalogs are part 3c.
- **Equipment "take it from whoever wears it"** (legacy `AskWhoToTakeFrom` when every copy is worn). It moves an item between two members, a rule Core doesn't have yet (`PartyEquipment` only moves between inventory and member). Follow-up with a Core method.
- **Party sidebar** (legacy `PartySidebar` next to pages): the Status and Party pages already show the party. Follow-up if wanted.
- **Interaction prompt** (legacy "Check" bubble), **map name banner** (question 53), **loading screen** (question 66), **defeat flow** (question 65), **repair notes shown to the player** (decision 63: logged), **autosave** (questions 44, 67), **blur** (question 52), audio, mouse, saving options between runs (question 89).

## Design overview

### What exists, checked
- **Core:**
  - Walking: collision and diagonal sliding (`PlayerWalker`, `MapCollision`).
  - Events: touch events start on entering a cell and on bumping an object; `TryInteract` starts interact events. `EventRun` (ADR 0006) makes dialogue, give-item and warp steps wait for `ContinueEvent`, and a warp moves the player on `ContinueEvent`.
  - Random battles roll per step and do nothing on a map whose table is null. `Walk` and `TryInteract` throw while an event runs or a battle is pending.
  - Sessions are captured and restored (`GameSession.Capture`, `Restore`).
  - Menu rules:
    - `FieldItemUse.CanUse` and `Use(item, user, targets)`, which checks legality but doesn't list the legal targets;
    - `PartyEquipment.TryEquip`/`TryUnequip`, but no list of what a member can wear;
    - `PartyMember.StatsWith(item)` for the stat preview;
    - `PartyState.TryAddToActive`, `TryMoveToReserve`, and `Reorder` with the leader fixed first.
- **Data:** `FileSaveStore` (slot names `[a-z0-9_]+`, atomic writes, repair with a list of changes). `ArtReferenceCheck` checks tilesheets and the field sprite only.
- **Game:**
  - `ExplorationScene` takes a `MapLocation` and only draws.
  - `MapCamera` snaps.
  - `MapDrawList` rebuilds without allocating.
  - `IScene.Update` returns nothing.
  - `GameFlow` starts a new game in its constructor.
  - `InputFrame` gives `Move` and edge-triggered buttons.
  - `Rendering/CanopyMask.Build` turns normalized polygons into a pixel mask, reused here for the nav-tab arrow.
- **Content:**
  - `start` has the chest (interact: give item, then swap to the opened chest), the door (interact: swap to open; touch: warp while open) and warp cells (0, 14) and (0, 15).
  - `forest` has a random battle table (kept, unused).
  - Items name `Consumable.png`, `Weapon.png` or `Armor.png` icons.
  - Art: `StartMenu.png`, `GameMenu.png`, `All.png` and the party portraits exist. Lato Regular is the only font.
  - No map has a dialogue step yet.

### Core changes (no new folder edges)
| Member | Why | Slice |
|---|---|---|
| `ExplorationState.IsPlayerMoving`, `PlayerPosition`, `PlayerFacing` | Animation and per-frame reads without allocating. | M1 |
| `ExplorationState.CurrentMap` (`MapDefinition`) | Redraw after a warp; the menu header's location. | M2 |
| `ExplorationState.CurrentDefinitionOf(PlacedObject)` | Swapped objects draw swapped; forwards `ActiveMap.CurrentDefinitionOf`, no new logic. | M3a |
| `ItemReceipt` public; `ExplorationState.CurrentItemReceipt` | The message says what a give-item step really added (ADR 0006). | M3b |
| `Effects/TargetChoice<T>` (public record: `Candidates`, `TakesAll`); `FieldItemUse.TargetChoiceFor(item, user)`; `CanUse` also needs a legal target | The item-use screen lists legal targets; a revive with nobody fallen is greyed (notes 4a). | P2a |
| `PartyEquipment.WearableFor(PartyMember member, EquipmentSlot slot)` | Inventory items the member's class can wear in that slot (notes 5d). | P3b |

### Game structure added by this plan
```
Jrpg.Game/
  Hosting/     PlayerSettings, FullScreenShortcut (W1); GamePaths (S1)
  Rendering/   MapArt, TextureLibrary.Pixel (M2)
  Ui/          ScreenText, UiTheme, UiTextures, PanelView, GradientStrip (M3b); ListCursor (T1b); UiArtFiles (T2);
               TabStrip (G1); ConfirmPrompt, ConfirmPromptView (G2); PlayTimeText (G3); StatBar (G4)
  Scenes/      ScreenFade, FadePhase (M2); SceneRequest, NewGameRequested (T1a); QuitRequested (T1b);
               MenuRequested, MenuClosed, TitleRequested (G1); LoadRequested (S2a)
  Dialogue/    FoundItemText, DialogueBox (M3b); SpeechPlayback (M3c); Typewriter (M4)
  Exploration/ WalkAnimation (M1); EventPresenter (M2)
  Title/       TitleMenu, TitleScene (T1b)
  Menus/       GameMenuScene, IMenuPage, SystemPage (G1); MenuHeader (G3); StatusPage (G4); SavePage (S1);
               LoadPage (S2a); InventoryPage, InventoryFilter (P1); ItemTargetPicker (P2b); EquipmentPage (P3a);
               EquipmentChoice, StatPreview (P3b); PartyPage, PartyOrderEdit (P4); OptionsPage (P5)
  Saving/      SaveSlots, SaveSlotEntry, SaveSlotList, SaveSlotListView (S1)
  Flow/        SceneFactory (T1a); MenuTransitions (G1); EncounterFreeContent (M2)
```
**Folder dependencies, checked edge by edge (acyclic):**
| Folder | Depends on |
|---|---|
| `Hosting`, `Input`, `Rendering` | Core, Data only |
| `Ui` | Rendering, Input |
| `Scenes` | Input, Rendering |
| `Dialogue` | Ui, Rendering, Input |
| `Saving` | Hosting, Ui, Rendering, Input |
| `Exploration` | Scenes, Dialogue, **Hosting** (text speed), Ui, Rendering, Input |
| `Menus` | Scenes, Saving, **Hosting** (options), Ui, Rendering, Input |
| `Title` | Scenes, Saving, Ui, Rendering, Input |
| `Flow` | all of the above; only `JrpgGame` and `Program` depend on `Flow` |

`SaveSlotList` lives in `Saving`, so `Menus` and `Title` both use it without depending on each other. Request records live in `Scenes`, the lowest folder every scene already uses.

### Key design decisions
1. **Scenes return requests** (ADR 0008). `IScene.Update` becomes `SceneRequest? Update(TimeSpan delta, InputFrame input)`: null on almost every frame, so nothing is allocated. `SceneStack.Update` returns the top scene's request. `GameFlow` hands it to `SessionTransitions` (new game, load, title) or `MenuTransitions` (open, close), and passes `QuitRequested` up to `JrpgGame`, which calls `Exit()`.
2. **Screen changes happen behind the flow's fade.** A session request starts the flow's `ScreenFade`. The flow applies the request when the screen is black, and ignores input until the fade-in ends. `SceneStack.ReplaceAll(IScene)` swaps the whole stack, for example the menu and the map for the title.
3. **Event steps are shown by `Exploration/EventPresenter`.** The scene walks, follows with the camera and draws. The presenter turns the running event's current step into a fade (warp) or a message box (dialogue, found item), and calls `ContinueEvent` when the screen is black or the player has read the step.
   - Its switch over step types is exhaustive: a step type it doesn't handle throws `InvalidOperationException` naming the type, as `EventRun` does.
   - `IsBusy` (an event runs or its fade isn't idle) blocks walking, interacting and opening the menu.
4. **The menu opens only between events.** So `GameSession.Capture` (which refuses mid-event saves, ADR 0007) is always legal from the menu.
5. **`SceneFactory` builds scenes**, so transition handlers stay at four parameters or fewer. Its constructor takes the catalog, `ScreenText`, `SaveSlots` and `PlayerSettings`, and builds `MapArt` from the catalog itself. It has one method per scene.
6. **No session on the title.** `GameFlow.Session` becomes nullable. Play time is added only while a session exists, menus included (decision 35).
7. **Seeds and time.** New and loaded games get a clock seed, written to the debug output (ADR 0008). Save time comes from one injected source: `Program` gives `SaveSlots` a `Func<DateTimeOffset>` (`() => DateTimeOffset.Now`), which it passes to `Capture`. That is ADR 0008's "the Game passes `DateTimeOffset.Now` to `Capture`"; tests pass a fixed value.
8. **Random battles off** (decision 79). `Flow/EncounterFreeContent.Of(catalog)` gives the game a copy of the catalog whose maps have no random battle table. Content files are untouched, Core never rolls, and the editor still shows the tables. The battle plan deletes this type.
9. **Player settings** (`Hosting/PlayerSettings`: full screen, text speed) are one mutable object owned by `Program`. `JrpgGame` applies the window mode when it changes, the Options page edits it, and `EventPresenter` reads the text speed. Not saved between runs yet (question 89).
10. **Legacy look in code.**
    - `Ui/UiTheme` holds the legacy colors and text sizes.
    - `Ui/UiTextures` builds the textures once: the nav-tab arrow mask through `CanopyMask.Build`, gradient strips through the pure `Ui/GradientStrip`, and a rounded-panel 9-slice.
    - The title's white glow is the text drawn several times at small offsets in low alpha (no shader).

### Legacy mapping
| Legacy | New | Slice |
|---|---|---|
| `PlayerManager` animation | `Exploration/WalkAnimation` | M1 |
| `CameraManager.Update`, `SetEasing` | `MapCamera.Follow` (time-based) | M1 |
| `InputManager` keys | `InputBindings` + WASD, Backspace (decision 50) | M1 |
| `GameManager.WarpAsync`, `isWarping`, `TransitionAsync`, `FadeHelper` | `EventPresenter`, `Scenes/ScreenFade` | M2 |
| `GameManager` encounter start | off (decision 79) | M2 |
| `GameManager.RunEventAsync` (interact, item message, swaps) | `ExplorationScene` + `EventPresenter`, `Dialogue/FoundItemText` | M3a, M3b |
| `DialogManager`, `DialogueBox` | `Dialogue/SpeechPlayback`, `Typewriter`, `DialogueBox` | M3b-M4 |
| `MainWindow` `WindowStyle=None`, maximized | borderless full screen, Alt+Enter | W1 |
| `MainWindow` start screen, `StartMenuButtonStyle` | `Title/TitleMenu`, `Title/TitleScene` | T1b, T2 |
| `SceneManager.StartNewGameAsync`, `ReturnToMainMenuAsync`, `LoadGameAsync` | `SessionTransitions` behind the flow's fade | T1a, G1, S2a |
| `SceneManager.ShowLoadScreenAsync` (blurred title + overlay) | slot list over the dimmed title (question 52) | S2b |
| `InputRouter` Esc rules | Esc rule above | G1, S2b |
| `GameMenuControl`, `MenuTab`, `NavTabStyle`, `GlassPanelStyle`, header/footer bars | `Menus/GameMenuScene`, `Ui/TabStrip`, `UiTextures`, `PanelView` | G1 |
| System sub menu | `Menus/SystemPage` | G1, S1, S2a |
| `ConfirmDialog` | `Ui/ConfirmPrompt`, `ConfirmPromptView` | G2 |
| `MenuHeader` | `Menus/MenuHeader`, `Ui/PlayTimeText` | G3 |
| `StatusView`, `StatsPanel`, `StatLine`, `Bars.xaml` | `Menus/StatusPage`, `Ui/StatBar` | G4 |
| `SaveLoadView`, `SaveSlotPanel`, `SaveDataPreview` | `Saving/*`, `Menus/SavePage`, `LoadPage` | S1, S2a |
| `PlacedObject.NewUid` / editor `PlacedObjectUids` (next free) | highest n + 1 (decision 87) | E1 |
| `InventoryView`, category rail (`CategoryTabStyle`, `FilterOption`) | `Menus/InventoryPage`, `InventoryFilter` | P1 |
| `InventoryView` use popup, `CharacterPortrait` targets | `Menus/ItemTargetPicker` over Core `TargetChoice` | P2a, P2b |
| `EquipmentView` (character rail, slots, Remove all) | `Menus/EquipmentPage` | P3a |
| `EquipmentSelectionView` (choices, Remove, stat preview) | `Menus/EquipmentChoice`, `StatPreview` | P3b |
| `EquipmentSelectionView.AskWhoToTakeFrom` | follow-up (needs a Core transfer rule) | - |
| `PartyView`, `PartyMemberCard` | `Menus/PartyPage`, `PartyOrderEdit` | P4 |
| `OptionsView` (empty) | `Menus/OptionsPage` (full screen, text speed) | P5 |
| `PartySidebar` | follow-up | - |

## Slices
Sizes are estimates of hand-written code plus tests. A slice estimated past ~300 lines is split before it starts.

### Phase M: walking and map events

- [x] **M1. Walking, walk animation and camera follow** (Core + Game, about 270 lines)
  - **Done, with deviations:** the Core wall test is `IsPlayerMoving_PushingIntoMapEdge_IsTrue` (the test map has no walls; the edge blocks). Extra tests: `PlayerPosition_AfterWalking_MatchesLocation`, `WalkAnimation_Advance_LeftoverFromLastFrame_CountsTowardNext`. The camera keeps an unrounded origin and only rounds `Origin`. Input is still read while the window is unfocused (so held keys don't re-press), but the scene gets an empty frame. While an event runs, the sprite stands on frame 0. The hand checks below are still to do.
  - **Goal:** arrows, WASD, D-pad and stick walk the player. Walls and objects block, and diagonals slide along walls (Core, part 2). The sprite faces its direction and animates while pushing. The camera eases after the player and stays inside the map.
  - **Core:** `ExplorationState.IsPlayerMoving`, `PlayerPosition`, `PlayerFacing`.
  - **New types (Game):**
    - `Exploration/WalkAnimation`: which frame of the field sprite to show.
      - `Advance(TimeSpan delta)` while moving, carrying the remainder so the timing is exact.
      - `Stand()` shows frame 0.
      - `Frame`, and `static int RowOf(Facing facing)`, which replaces `ExplorationScene.SpriteRowOf`.
    - `MapCamera.Follow(Point2D target, MapLayout layout, TimeSpan delta, double sharePerTick)`: the share of the gap to the clamped target closed per 1/60 s, applied time-based. `Origin` stays in whole pixels.
  - **Changes:**
    - `ExplorationScene` now takes the session's `ExplorationState` (not a `MapLocation`). In `Update`, unless an event runs, it walks, advances or stops the animation, follows, and rebuilds the draw list.
    - `InputBindings` adds WASD (directions) and Backspace (Cancel) (decision 50).
    - `JrpgGame` passes an empty `InputFrame` while the window isn't focused (decision 83).
  - **Tests:** Core `ExplorationState_IsPlayerMoving_PushingIntoWall_IsTrue`, `ExplorationState_IsPlayerMoving_NoIntent_IsFalse`. Game `WalkAnimation_Stand_ShowsFirstFrame`, `WalkAnimation_Advance_TenthOfASecond_AdvancesOneFrame`, `WalkAnimation_Advance_PastLastFrame_WrapsToFirst`, `WalkAnimation_RowOf_EveryFacing_FollowsSheetOrder`, `MapCamera_Follow_OneTick_ClosesShareOfDistance`, `MapCamera_Follow_TwoHalfTicks_EqualOneTick`, `MapCamera_Follow_NearMapEdge_ClampsToEdge`, `MapCamera_Follow_MapSmallerThanView_StaysCentered`, `InputReader_Read_WKey_MovesUp`, `InputReader_Read_Backspace_PressesCancelOnly`, `GameFlow_Update_HoldingRight_MovesPlayerRight`.
  - **Verify by hand:**
    - [ ] Arrows, WASD, D-pad and stick walk; a half tilt is slower; diagonals aren't faster.
    - [ ] Houses, trees and the map edge block.
    - [ ] The sprite animates against a wall and stands on frame 0 when released.
    - [ ] The camera lags slightly and settles.
    - [ ] Focusing another window stops walking.
  - **Known gap until M2:** stepping on a warp cell starts its event, and the player stays frozen.

- [x] **M2. Warps with a fade; random battles off** (Core + Game, about 290 lines)
  - **Done, with deviations:** `MapArt` takes the cell size (so Rendering doesn't depend on Exploration). The two repository-content tests share a test helper, `ForestWarpStart` (renamed `StartMapScenarios` in M3a). Extra tests: `ScreenFade_Update_LeftoverPastHold_CarriesIntoFadeIn`, `ScreenFade_Update_Idle_StaysClear`. The scene re-enters a map when the `CurrentMap` reference changes, so a warp within the same map would ease the camera instead of snapping it (no content does this yet). **The forest has no warp back** (`forest.json` has no cell events), so the "Walk back" check can't be done until content adds one. The hand checks below are still to do.
  - **Goal:** warp cells fade to black, change the map and fade back in, with the camera snapped to the arrival point. Walking, interacting and the menu are blocked until the fade-in ends. Random battles never start (decision 79).
  - **Core:** `ExplorationState.CurrentMap`.
  - **New types (Game):**
    - `Scenes/ScreenFade` (pure): `ScreenFade(TimeSpan fadeOut, TimeSpan hold, TimeSpan fadeIn)`, `Start()` (throws unless idle), `Update(TimeSpan delta)`, `Phase`, `Blackness` (0..1).
    - `Scenes/FadePhase`: `Idle`, `FadingOut`, `Holding`, `FadingIn`.
    - `Exploration/EventPresenter`: shows the running event's current step and continues it.
      - In this slice it handles warps: a warp step starts the fade, and `ContinueEvent` is called when the phase reaches `Holding`.
      - `IsBusy`: an event runs or the fade isn't idle.
      - Its switch is exhaustive: any other step type throws `InvalidOperationException` naming it.
    - `Rendering/MapArt`: the tile atlas and canopy textures, built once per game.
    - `TextureLibrary.Pixel`: a 1 x 1 white texture for fades and panels.
    - `Flow/EncounterFreeContent` (static): `ContentCatalog Of(ContentCatalog catalog)`, the catalog with every map's random battle table removed. `GameFlow` starts sessions with it. A `TODO` names the battle plan.
  - **Changes:** `ExplorationScene` rebuilds its `MapView`, draw list and camera snap when `CurrentMap` changes, and draws the fade over the map.
  - **Known gap until M3a/M3b:** dialogue and give-item steps throw. None can be reached yet: the chest needs interact (M3a), and no map has a dialogue step.
  - **Tests:** `ScreenFade_Update_HalfwayThroughFadeOut_IsHalfBlack`, `ScreenFade_Update_PastFadeOut_IsFullyBlackDuringHold`, `ScreenFade_Update_AfterFadeIn_IsIdle`, `ScreenFade_Start_WhileFading_Throws`, `EventPresenter_Update_WarpStep_ContinuesOnlyOnceBlack` and `ExplorationScene_Update_DuringWarpFadeIn_DoesNotWalk` (both on repository content with the start moved next to cell (0, 14) through `catalog with { NewGame = ... }`, holding Left), `EncounterFreeContent_Of_RepositoryContent_LeavesNoRandomBattleTable`, Core `ExplorationState_CurrentMap_AfterWarp_IsTargetMap`.
  - **Verify by hand:**
    - [ ] Walk onto (0, 14): fade, then the forest at (1, 1), camera on the player.
    - [ ] Hold a direction through the fade: the player doesn't move until the fade-in ends.
    - [ ] Walk 200 steps in the forest: no battle.
    - [ ] Walk back.

- [x] **M3a. Interact and swapped objects** (Core + Game, about 150 lines)
  - **Done, with deviations:** `MapView` and `MapDrawList.Rebuild` take a `Func<PlacedObject, ObjectDefinition>` (the scene passes `CurrentDefinitionOf`, built once) instead of the definition dictionary. The Core tests use the lever fixture: `CurrentDefinitionOf_AfterSwap_ReturnsSwappedDefinition`, `CurrentDefinitionOf_NeverSwapped_ReturnsPlacedDefinition`. The M2 test helper became `StartMapScenarios`, shared by both scene tests. The hand checks below are still to do.
  - **Goal:** Confirm in front of an object starts its interact event. The closed door opens on Confirm (a swap, which doesn't wait), draws open, and warps when walked into.
  - **Core:** `ExplorationState.CurrentDefinitionOf(PlacedObject)` (forwards `ActiveMap.CurrentDefinitionOf`).
  - **Changes:**
    - `ExplorationScene`: Confirm, when the presenter isn't busy, calls `TryInteract`.
    - `MapView` and `MapDrawList` draw and sort each placed object by its current definition (tiles, height, canopy).
  - **Known gap until M3b:** the chest's give-item step throws. Don't open the chest.
  - **Tests:** Core `ExplorationState_CurrentDefinitionOf_AfterDoorOpened_ReturnsOpenDoor`, `ExplorationState_CurrentDefinitionOf_NeverSwapped_ReturnsPlacedDefinition`. Game `MapDrawList_Rebuild_SwappedObject_UsesCurrentDefinitionHeight`, `ExplorationScene_Update_ConfirmFacingDoor_OpensIt`.
  - **Verify by hand:**
    - [ ] The door opens and stays open after walking away.
    - [ ] Walking into the open door warps to `interior_1` and back.

- [x] **M3b. Legacy theme, message box and found items** (Core + Game, about 300 lines)
  - **Done, with deviations (developer, 2026-10-19):** the box copies the legacy in-game `DialogueBox` (what legacy showed for found items), not the menu's `DialogStyle`: `#AA000000` fill, white border, rounded corners, doubled to 1200 x 240 px, 40 px from the bottom, radius 20, border 4, text 32 px. `GradientStrip` and `UiTextures` move to G1 (no user here); `PanelView` bakes its own rounded texture from a `PanelStyle`. `EventPresenter` now draws the message and the fade. Tests are named `EventPresenter_Update_ConfirmOnFoundItem_ContinuesEvent`; extra: `Update_FirstUpdateOnFoundItem_WaitsEvenWithConfirmPressed`, `ExplorationScene_Update_ConfirmClosingFoundItem_DoesNotInteractAgain`, `FoundItemText_Of_AllLost_OnlyMentionsLostCopies`, `PanelViewTests` (4). Open: the all-lost message doesn't name the item (decision 55 wording). Follow-up: `FontLibraryTests.Load_FileThatIsNotAFont_ThrowsNamingTheFile` sometimes crashes the test host (native read past a 23-byte non-font); `FontLibrary` should reject too-short files first. The hand checks below are still to do.
  - **Goal:** opening the chest shows "Obtained {item}!" in a message box styled like legacy (`DialogStyle`). Confirm closes it, and the chest draws opened.
  - **Core:** `ItemReceipt` public; `ExplorationState.CurrentItemReceipt`.
  - **New types (Game):**
    - `Ui/ScreenText`: the message table and the fonts screens write with. Built in `Program`; `GameFlow` takes it.
    - `Ui/UiTheme`: the legacy `Theme.xaml` colors and text sizes as named constants.
    - `Ui/GradientStrip` (pure): the colors of a horizontal gradient from its stops, for gradient textures.
    - `Ui/UiTextures`: the UI's code-drawn textures, built once (here: the rounded panel).
    - `Ui/PanelView`: draws a 9-slice panel at any size.
    - `Dialogue/FoundItemText` (static): `item_found`, `item_found_many` or `item_found_lost` from `ui.json` (decision 55), built once when the step becomes current.
    - `Dialogue/DialogueBox`: draws one page in the box.
  - **Changes:**
    - `EventPresenter` handles give-item steps; Confirm calls `ContinueEvent`, and the same press doesn't also interact.
    - `MessageIds` gains the new ids.
  - **Known gap until M3c:** dialogue steps throw (only an editor-placed sign has one).
  - **Tests:** Core `ExplorationState_CurrentItemReceipt_OnGiveItemStep_ReportsAddedCopies`, `ExplorationState_CurrentItemReceipt_AfterContinue_IsNull`. Game `GradientStrip_Colors_TwoStops_InterpolatesMidpoint`, `FoundItemText_Of_OneCopy_UsesFoundMessage`, `FoundItemText_Of_TwoCopies_UsesManyMessage`, `FoundItemText_Of_SomeLost_MentionsLostCopies`, `EventPresenter_Confirm_OnFoundItem_ContinuesEvent`.
  - **Verify by hand:**
    - [ ] Open the chest: message, opened chest, item in the inventory.
    - [ ] A held Confirm doesn't reopen anything.
    - [ ] The box matches the legacy box's colors and border.

- [ ] **M3c. Dialogue steps** (Game, about 120 lines)
  - **Goal:** a dialogue step shows its lines one page per Confirm.
  - **New types (Game):** `Dialogue/SpeechPlayback` (pure): the pages of one step. `Confirm()` moves to the next page; `IsFinished`. Found items become a one-page playback.
  - **Tests:** `SpeechPlayback_Confirm_NotLastPage_ShowsNextPage`, `SpeechPlayback_Confirm_OnLastPage_Finishes`, `EventPresenter_Confirm_OnLastPage_ContinuesEvent`.
  - **Verify by hand:**
    - [ ] A two-line sign placed in the editor shows two pages.

- [ ] **M4. Typewriter, speaker and portrait** (Data + Game, about 200 lines)
  - **Goal:**
    - Dialogue pages type out at 30 ms per character. Confirm completes the page first, then advances (decision 54).
    - Speaker and portrait show when the step has them; a blinking marker shows when the page is complete.
    - Found-item messages appear whole.
  - **New types (Game):** `Dialogue/Typewriter` (pure): `Update(TimeSpan delta)`, `RevealAll()`, `VisibleLength`, `IsComplete`. Its character time is a constructor value, so Options (P5) can change it.
  - **Data:** `ArtReferenceCheck` covers portraits named by dialogue steps.
  - **Tests:** `Typewriter_Update_OneCharacterTime_RevealsOneCharacter`, `Typewriter_RevealAll_CompletesPage`, `SpeechPlayback_Confirm_WhileTyping_CompletesPageWithoutAdvancing`, Data `ArtReferenceCheck_FindProblems_MissingDialoguePortrait_NamesTheEvent`.
  - **Verify by hand:**
    - [ ] The sign types out; Confirm mid-page completes it.
    - [ ] Speaker and portrait show; the marker blinks.
    - [ ] Partial text is drawn without a new string per frame (FontStashSharp's API is checked first).

### Phase W: full screen

- [ ] **W1. Full screen and Alt+Enter** (Game, about 140 lines)
  - **Goal:** the game starts in borderless full screen (decision 78). Alt+Enter switches between full screen and the resizable window. The 1920 x 1080 virtual screen stays letterboxed in both.
  - **New types (Game):**
    - `Hosting/PlayerSettings`: the options the player chose. Here only `IsFullScreen`; created by `Program`, given to `JrpgGame`.
    - `Hosting/FullScreenShortcut` (pure): Alt+Enter pressed this update, from two keyboard states.
  - **Changes:** `JrpgGame` keeps its `GraphicsDeviceManager` (`HardwareModeSwitch = false`), applies `IsFullScreen` when it differs from the window, and keeps the last window size.
  - **Tests:** `FullScreenShortcut_IsPressed_AltHeldEnterGoesDown_IsTrue`, `FullScreenShortcut_IsPressed_EnterWithoutAlt_IsFalse`, `FullScreenShortcut_IsPressed_EnterHeldTwoUpdates_OnlyFirst`.
  - **Verify by hand:**
    - [ ] F5 opens full screen on the main monitor.
    - [ ] Alt+Enter goes windowed and back without stretching.
    - [ ] Alt+Tab away and back works.
    - [ ] Debugging with a breakpoint is still usable (question 93).

### Phase T: title screen

- [ ] **T1a. Scene requests and the flow's fade** (Game, about 220 lines)
  - **Goal:** no visible change; the game still starts on the map. Underneath:
    - scenes return requests;
    - the flow starts the game by applying `NewGameRequested` through `SessionTransitions`, behind its own fade;
    - `SceneFactory` builds the scenes.
  - **New types (Game):**
    - `Scenes/SceneRequest`: abstract record, what a scene asks the flow for. Record: `NewGameRequested`.
    - `IScene.Update` returns `SceneRequest?`; `SceneStack.Update` returns the top scene's request; `StartupErrorScene` returns null.
    - `SceneStack.ReplaceAll(IScene scene)`.
    - `Flow/SceneFactory`: builds each scene from the catalog, screen text, save slots (S1) and settings (P5).
  - **Changes:** `GameFlow`'s `Session` becomes nullable and play time is added only while one exists. The flow ignores input while its fade plays.
  - **Tests:** `SceneStack_Update_ReturnsTopSceneRequest`, `SceneStack_ReplaceAll_LeavesOnlyNewScene`, `GameFlow_Constructor_StartsNewGameBehindFade`, `GameFlow_Update_DuringFade_IgnoresInput`. `GameFlowTests` are updated: a session exists only once the fade reaches black.

- [ ] **T1b. Title menu** (Game, about 220 lines)
  - **Goal:**
    - The game opens on the title with New game and Quit (Continue and Load come in S2b), as plain text with the selected entry marked.
    - New game fades to the start map.
    - Quit closes the window.
  - **New types (Game):**
    - `Scenes/QuitRequested`.
    - `Ui/ListCursor` (pure): the selected entry of a vertical list. `MoveUp`, `MoveDown`, wraps, skips disabled entries.
    - `Title/TitleMenu` (pure): the entries and cursor. `SceneRequest? Update(InputFrame input)`.
    - `Title/TitleScene`: draws the title menu.
  - **Changes:**
    - `GameFlow` starts on the title.
    - `JrpgGame` exits on `QuitRequested`.
  - **Tests:** `ListCursor_MoveDown_OnLastEntry_WrapsToFirst`, `ListCursor_MoveUp_OnFirstEntry_WrapsToLast`, `ListCursor_MoveDown_SkipsDisabledEntry`, `TitleMenu_ConfirmNewGame_RequestsNewGame`, `TitleMenu_ConfirmQuit_RequestsQuit`, `GameFlow_Constructor_StartsOnTitleWithoutSession` (replaces T1a's constructor test: the requested behavior changed), `GameFlow_Update_QuitConfirmed_ReturnsQuitRequest`.
  - **Verify by hand:**
    - [ ] F5 shows the title.
    - [ ] Up/Down, W/S and the D-pad move the marker.
    - [ ] New game fades to the map.
    - [ ] Quit closes the window.

- [ ] **T2. Title look** (Game, about 160 lines)
  - **Goal:** the title looks like legacy (decision 82):
    - `StartMenu.png` fills the screen (cover-fit, no stretching);
    - "The Lost Realm" (`game_title` in `ui.json`, placeholder, decision 80) at 100 px with a drop shadow;
    - entries at 48 px, white; the selected one glows white and is drawn at 110 % (legacy `StartMenuButtonStyle`).
  - **New types (Game):** `Ui/UiArtFiles`: the UI images the screens draw (`StartMenu.png`, `GameMenu.png`, `All.png`), named in one place (decision 84).
  - **Tests:** `UiArtFiles_EveryFile_ExistsInRepositoryContent` (as `MessageIds`).
  - **Verify by hand:**
    - [ ] Side by side with a legacy screenshot if one exists: layout, sizes, glow.
    - [ ] Readable in full screen and in the window.

### Phase G: menu shell

- [ ] **G1. Menu shell and System page** (Game, about 300 lines)
  - **Goal** (decision 81):
    - Menu (Esc, Start) on the map opens the game menu with a 0.15 s fade. It is ignored while the presenter is busy.
    - The menu has the legacy background (`GameMenu.png` with the edge scrim), header and footer bars, and vertical arrow-shaped tabs on the left. Tabs are text only for now (decision 51). The selected tab has the legacy blue selection gradient and line.
    - Confirm enters a page; Cancel goes back to the tabs.
    - On the tabs, Cancel or Menu closes the menu. In a page, Esc only goes back to the tabs (Esc rule).
    - First tab: System, with Title screen and Quit (no confirmation until G2).
  - **New types (Game):**
    - Request records `MenuRequested`, `MenuClosed`, `TitleRequested`.
    - `Flow/MenuTransitions`: pushes and pops the menu.
    - `SessionTransitions` handles `TitleRequested`: ends the session, `ReplaceAll` with the title, behind the fade.
    - `Ui/TabStrip` (pure): the selected tab; `Previous`, `Next`, wraps.
    - `Ui/UiTextures` gains the nav-tab arrow (legacy `Shape.NavTab`, masked through `CanopyMask.Build`) and the selection gradients. Moved here from M3b: `UiTextures` itself and the pure `Ui/GradientStrip`, with `GradientStrip_Colors_TwoStops_InterpolatesMidpoint`.
    - `Menus/IMenuPage`: one page of the game menu. Title message id, `SceneRequest? Update(InputFrame input)`, `Draw`, and whether Cancel leaves it. Implemented by every page in this plan.
    - `Menus/GameMenuScene`: the tabs and the open page.
    - `Menus/SystemPage`: the System entries.
  - **Tests:** `TabStrip_Next_OnLastTab_WrapsToFirst`, `TabStrip_Previous_OnFirstTab_WrapsToLast`, `GameMenuScene_Cancel_OnTabs_RequestsMenuClosed`, `GameMenuScene_Confirm_OnTab_EntersPage`, `GameMenuScene_EscapeInPage_ReturnsToTabs` (Cancel and Menu both pressed), `SystemPage_ConfirmTitleScreen_RequestsTitle`, `SystemPage_ConfirmQuit_RequestsQuit`, `ExplorationScene_Update_MenuPressedWhileIdle_RequestsMenu`, `ExplorationScene_Update_MenuPressedDuringWarpFade_RequestsNothing`, `GameFlow_Update_TitleRequested_EndsSessionOnTitle`.
  - **Verify by hand:**
    - [ ] Esc and Start open the menu; Esc inside System goes back to the tabs, then closes.
    - [ ] Title screen goes to the title, and New game starts fresh.
    - [ ] The layout matches `GameMenuControl.xaml`.

- [ ] **G2. Confirmation prompt; Title screen and Quit ask first** (Game, about 160 lines)
  - **Goal:** Title screen and Quit always ask first (decision 85; unsaved progress isn't tracked). The prompt is the legacy `ConfirmDialog` look, with Yes/No starting on No. Cancel or Esc answers No.
  - **New types (Game):**
    - `Ui/ConfirmPrompt` (pure): the Yes/No cursor and the answer.
    - `Ui/ConfirmPromptView`: draws it over a scrim.
  - **Tests:** `ConfirmPrompt_Start_SelectsNo`, `ConfirmPrompt_Cancel_AnswersNo`, `ConfirmPrompt_ConfirmOnYes_AnswersYes`, `SystemPage_ConfirmQuit_AsksBeforeRequesting`, `SystemPage_AnswerNo_StaysInPage`.
  - **Verify by hand:**
    - [ ] Both entries ask; No and Esc stay; Yes leaves.

- [ ] **G3. Menu header: location and play time** (Game, about 130 lines)
  - **Goal:** the header shows the page title, the current map's display name and the play time (hh:mm:ss, ticking), as legacy `MenuHeader`.
  - **New types (Game):**
    - `Ui/PlayTimeText`: the play time as text, formatted only when the shown second changes. `Update(TimeSpan playTime)`, `Text`.
    - `Menus/MenuHeader`: draws the header.
  - **Tests:** `PlayTimeText_Update_OverAnHour_ShowsHoursMinutesSeconds`, `PlayTimeText_Update_SameSecond_KeepsSameText` (same instance: nothing formatted).
  - **Verify by hand:**
    - [ ] Play time ticks with the menu open.
    - [ ] The location reads "start", and "Forest" after the warp.

- [ ] **G4. Status page** (Data + Game, about 280 lines)
  - **Goal:** the first tab, Status, shows one member at a time in the legacy `StatusView` layout:
    - portrait, name, class, level;
    - HP, MP and experience bars (legacy `Bars.xaml` gradients);
    - attack and defense with equipment;
    - worn weapon and armor.
    PreviousTab/NextTab or Left/Right change the member (legacy character rail; question 92). Members are in active order, then the reserve.
  - **New types (Game):**
    - `Ui/StatBar`: draws a filled bar with a legacy gradient.
    - `Menus/StatusPage`.
  - **Data:** `ArtReferenceCheck` covers party portraits.
  - **Tests:** `StatusPage_NextMember_OnLastMember_WrapsToFirst`, `StatusPage_Members_ActiveFirstThenReserve`, Data `ArtReferenceCheck_FindProblems_MissingPartyPortrait_NamesTheMember`.
  - **Verify by hand:**
    - [ ] Every member shows the right portrait, stats and bars.

### Phase S: save and load

- [ ] **E1. Placed-object uids never reuse a lower number** (Editor, about 60 lines)
  - **Goal:** a newly placed object gets `{objectId}_{n}` with n one above the highest n on the map (decision 87). Deleting `chest_1` while `chest_2` exists gives `chest_3`, not `chest_1`, so a new object doesn't inherit an old save's "opened" state.
  - **Change:** `Maps/Editing/PlacedObjectUids.Next` (same signature). `EventIds` already uses highest + 1, so it needs no change.
  - **Remaining case:** both schemes still reuse the highest id after deleting it (question 88).
  - **Tests:** `PlacedObjectUids_Next_LowerNumberFree_ReturnsHighestPlusOne`. The existing `PlacedObjectUids_Next_NoneTaken_ReturnsSuffixOne` and `PlacedObjectUids_Next_Chest1Taken_ReturnsChest2` still pass unchanged.
  - **Verify by hand:**
    - [ ] Place two chests, delete the first, place another: it's `chest_3`.

- [ ] **S1. Save slots and saving from the menu** (Game, about 300 lines)
  - **Goal:** System gains Save. It opens the 20 files in the legacy `SaveSlotPanel` look: empty, damaged, or with location, play time, save date and the active party's names and levels. Confirm writes the save and the entry updates. Overwrite doesn't ask yet (S3).
  - **New types (Game):**
    - `Hosting/GamePaths`: where the game keeps its files. `SaveDirectory` = `%LocalAppData%/Jrpg/Saves` (decision 64).
    - `Saving/SaveSlots`: the 20 manual slots over `FileSaveStore`.
      - `SaveSlots(FileSaveStore store, ContentCatalog catalog, Func<DateTimeOffset> clock)`.
      - `static string NameOf(int number)`.
      - `IReadOnlyList<SaveSlotEntry> ReadAll()`: when a list opens, never per frame; a damaged file's reason goes to the debug output.
      - `void Write(int number, GameSession session)`: captures at `clock()`.
    - `Saving/SaveSlotEntry`: one file's summary from a repaired `SaveGame` and the catalog.
    - `Ui/ListCursor` gains a visible window: `VisibleRows`, `FirstVisible`, scrolling with the cursor.
    - `Saving/SaveSlotList` (pure): the entries with a cursor. `Saving/SaveSlotListView` draws it.
    - `Menus/SavePage`.
  - **Changes:** `Program` builds `FileSaveStore` and `SaveSlots`; `SceneFactory` takes them.
  - **Tests:** `SaveSlots_NameOf_FirstSlot_IsSlot01`, `SaveSlots_ReadAll_EmptyFolder_GivesTwentyEmptyEntriesInOrder`, `SaveSlots_Write_ThenReadAll_SlotShowsMapNameAndPlayTime` (temporary folder, fixed clock), `SaveSlots_ReadAll_DamagedFile_IsUnreadable`, `ListCursor_MoveDown_PastVisibleRows_ScrollsWindow`, `ListCursor_MoveUp_OnFirstEntry_WrapsToLastAndScrolls`, `SavePage_ConfirmSlot_WritesSave`.
  - **Verify by hand:**
    - [ ] Save to File 1 and File 20: the files appear in `%LocalAppData%/Jrpg/Saves`, and the entries show "start", the play time and the date.
    - [ ] A file broken by hand shows as damaged.

- [ ] **S2a. Loading from the menu** (Game, about 200 lines)
  - **Goal:** System gains Load, the same list where only readable files can be chosen. Loading fades to the saved map, position and facing, with party, inventory, opened chests and play time.
  - **New types (Game):**
    - `Scenes/LoadRequested(int SlotNumber)`.
    - `SessionTransitions` handles it: reads, restores with a fresh clock seed (logged), logs the repair changes (decision 63), and `ReplaceAll`s with exploration.
    - `Menus/LoadPage`.
  - **Edge:** a file that became unreadable since the list opened is logged, and the list stays open.
  - **Tests:** `LoadPage_ConfirmEmptySlot_RequestsNothing`, `GameFlow_Update_LoadRequested_RestoresSavedLocationAndPlayTime` (temporary folder: walk, save, start a new game, load, compare).
  - **Verify by hand:**
    - [ ] Open the chest, save, start a new game, load: the chest is open and the item owned.

- [ ] **S2b. Continue and Load on the title** (Game, about 230 lines)
  - **Goal** (decision 80): the title's entries become New game, Continue, Load and Quit.
    - **Continue** loads the most recent readable save (by save date).
    - **Load** opens the slot list over the dimmed title (question 52); Cancel or Esc returns to the entries.
    - With no readable save, both are disabled (shown, skipped by the cursor).
  - **Changes:** `Saving/SaveSlots.MostRecent(IReadOnlyList<SaveSlotEntry> entries)` (notes 6d); `TitleMenu` gains the entries and the slot-list state.
  - **Tests:** `SaveSlots_MostRecent_PicksLatestSavedAt`, `SaveSlots_MostRecent_NoReadableSave_ReturnsNull`, `TitleMenu_NoReadableSave_ContinueAndLoadDisabled`, `TitleMenu_ConfirmContinue_RequestsMostRecentSlot`, `TitleMenu_ConfirmLoad_OpensSlotList`, `TitleMenu_CancelOnSlotList_ReturnsToEntries`.
  - **Verify by hand:**
    - [ ] Quit, start the game: Continue lands where the last save was.
    - [ ] With an empty save folder, both entries are greyed.

- [ ] **S3. Overwrite asks first; failed saves say so** (Game, about 130 lines)
  - **Goal:**
    - Saving onto a used file asks "Overwrite File n?" (decision 62, G2's prompt, starting on No).
    - A failed write (`IOException`, `UnauthorizedAccessException`) shows "Couldn't save to File n." (message id `save_failed`), keeps the list open, and logs the reason.
  - **Tests:** `SavePage_ConfirmFilledSlot_AsksBeforeWriting`, `SavePage_AnswerYes_Overwrites`, `SavePage_WriteFails_KeepsListOpen` (the save folder is a file, so writing throws).
  - **Verify by hand:**
    - [ ] Overwriting asks; No keeps the old date.
    - [ ] A read-only save file shows the message.

### Phase P: menu pages

- [ ] **P1. Inventory page** (Data + Game, about 280 lines)
  - **Goal:** an Inventory tab in the legacy `InventoryView` layout:
    - a category rail with the legacy icons: All (`All.png`), Consumables, Weapons, Armor (`Consumable.png`, `Weapon.png`, `Armor.png`; decision 51, question 60);
    - item rows with icon, name and quantity, in pickup order;
    - the selected item's description.
    PreviousTab/NextTab change the category (question 92). Read-only here; using items is P2b.
  - **New types (Game):**
    - `Menus/InventoryFilter`: which items a category keeps (consumables: has an effect; weapons and armor: by equipment slot).
    - `Menus/InventoryPage`.
  - **Data:** `ArtReferenceCheck` covers item icons.
  - **Tests:** `InventoryFilter_Weapons_KeepsOnlyWeaponSlotItems`, `InventoryFilter_Consumables_KeepsItemsWithEffect`, `InventoryFilter_All_KeepsPickupOrder`, `InventoryPage_NextCategory_ResetsCursorToFirstRow`, Data `ArtReferenceCheck_FindProblems_MissingItemIcon_NamesTheItem`.
  - **Verify by hand:**
    - [ ] The starting items show under every category with the right icons and counts.
    - [ ] The list scrolls.

- [ ] **P2a. Field item targets in Core** (Core, about 150 lines)
  - **Goal:** Core says who an item can be used on, so the menu never offers an illegal choice.
  - **Core:**
    - `Effects/TargetChoice<T>` (public record): the legal targets of one effect, and whether it takes them all.
    - `FieldItemUse.TargetChoiceFor(ItemDefinition item, PartyMember user)`.
    - `CanUse` is false when no target is legal, such as a revive with nobody fallen.
  - **Tests:** `FieldItemUse_TargetChoiceFor_HealItem_ListsLivingMembersIncludingReserve`, `FieldItemUse_TargetChoiceFor_ReviveItem_ListsOnlyFallen`, `FieldItemUse_TargetChoiceFor_AllAlliesItem_TakesAll`, `FieldItemUse_TargetChoiceFor_SelfItem_ListsOnlyUser`, `FieldItemUse_CanUse_ReviveWithNobodyFallen_ReturnsFalse`.

- [ ] **P2b. Using items from the inventory** (Game, about 270 lines)
  - **Goal:** Confirm on a usable consumable opens the target picker (legacy use popup: portraits with HP/MP).
    - A single-target item picks one member; an "all" item selects everyone and confirms once.
    - The user is the chosen target (question 91).
    - The result shows as "{member} recovers {amount} HP." (placeholder wording, question 91), and the counts and bars update.
    - Unusable items (bombs, equipment) are greyed.
  - **New types (Game):** `Menus/ItemTargetPicker` (pure: over a `TargetChoice<PartyMember>`; `Confirm` gives the targets, `Cancel` closes). Messages `item_used_hp`, `item_used_mp`, `item_used_revive`.
  - **Tests:** `ItemTargetPicker_TakesAllChoice_SelectsEveryCandidate`, `ItemTargetPicker_MoveDown_SkipsNothingOutsideCandidates`, `InventoryPage_ConfirmUnusableItem_OpensNothing`, `InventoryPage_UseLastPotion_RemovesRow`.
  - **Verify by hand:**
    - [ ] Use a potion on a member at full HP and on a hurt one (set by hand in a save).
    - [ ] Use an all-allies item.
    - [ ] The revive is greyed with nobody fallen.

- [ ] **P3a. Equipment page: members, slots and stats** (Game, about 220 lines)
  - **Goal:** an Equipment tab in the legacy `EquipmentView` layout: one member at a time (PreviousTab/NextTab), the weapon and armor slots with what's worn, the member's stats, and "Remove all" (question 60; through `PartyEquipment.TryUnequip`).
  - **New types (Game):** `Menus/EquipmentPage`.
  - **Tests:** `EquipmentPage_NextMember_OnLastMember_WrapsToFirst`, `EquipmentPage_RemoveAll_EmptiesBothSlots`, `EquipmentPage_RemoveAll_StackFull_KeepsThatItemWorn`.
  - **Verify by hand:**
    - [ ] Worn items show per member; Remove all puts them back in the inventory.

- [ ] **P3b. Changing equipment with a stat preview** (Core + Game, about 290 lines)
  - **Goal:** Confirm on a slot lists the items the member can wear there, plus "Remove", in the legacy `EquipmentSelectionView` look. Moving the cursor previews attack and defense (legacy positive/negative colors). Confirm equips through `TryEquip`. An item that can't be worn because the replaced one doesn't fit back is refused with a message.
  - **Core:** `PartyEquipment.WearableFor(PartyMember member, EquipmentSlot slot)`: inventory items of that slot the member's class can wear, in pickup order.
  - **New types (Game):**
    - `Menus/StatPreview` (pure): the stat change if an item replaced the slot's item, from `PartyMember.StatsWith`.
    - `Menus/EquipmentChoice` (pure): the list and cursor.
  - **Tests:** Core `PartyEquipment_WearableFor_OtherClassItem_IsExcluded`, `PartyEquipment_WearableFor_ArmorSlot_ExcludesWeapons`. Game `StatPreview_Of_StrongerWeapon_ShowsPositiveAttack`, `StatPreview_Of_EmptySlotItem_ShowsFullBonus`, `EquipmentChoice_ConfirmRemove_UnequipsSlot`.
  - **Verify by hand:**
    - [ ] Swap swords and see the preview.
    - [ ] Another class's armor isn't listed.
    - [ ] Remove works.

- [ ] **P4. Party page** (Game, about 260 lines)
  - **Goal:** a Party tab in the legacy `PartyView` layout: the active party (n / 4) and the reserve as member cards.
    - On an active member: Confirm offers "Change order" or "To reserve".
    - On a reserve member: Confirm adds them if the party isn't full; otherwise it shows "The party is full."
    - The leader never moves (Core rule; legacy hint text).
    - Reordering: pick the member, then the place to swap with (question 61).
  - **New types (Game):** `Menus/PartyOrderEdit` (pure: builds the new order for `PartyState.Reorder`), `Menus/PartyPage`.
  - **Tests:** `PartyOrderEdit_Swap_TwoMembers_SwapsTheirPlaces`, `PartyOrderEdit_Swap_SameMember_KeepsOrder`, `PartyPage_ConfirmReserveMember_PartyFull_AddsNobody`, `PartyPage_LeaderSelected_OffersNoMove`.
  - **Verify by hand:**
    - [ ] Add three reserve members, reorder, send one back.
    - [ ] Status and save reflect the new order.

- [ ] **P5. Options page** (Game, about 200 lines)
  - **Goal:** an Options tab with the settings that exist: Full screen (on/off) and Text speed (Slow, Normal, Fast, Instant; question 90). Left/Right changes a value, and it takes effect at once. Not saved between runs (question 89).
  - **Changes:**
    - `Hosting/PlayerSettings` gains `TextSpeed`; `SceneFactory` passes the settings to `EventPresenter` (typewriter character time).
    - `Menus/OptionsPage`.
  - **Tests:** `OptionsPage_RightOnFullScreen_TogglesSetting`, `OptionsPage_RightOnTextSpeed_LastValue_StaysOnLast`, `EventPresenter_TextSpeedInstant_ShowsWholePage`.
  - **Verify by hand:**
    - [ ] Toggle full screen from the menu.
    - [ ] Change the text speed, then read the sign.

- [ ] **Z. Play-through check** (manual)
  - [ ] Title, New game.
  - [ ] Walk, open the chest and the door, warp to the forest and back.
  - [ ] Every menu tab: Status, use a potion, equip, reorder the party, Options.
  - [ ] Save, Title screen (asks), Continue: everything is where it was.
  - [ ] Load another file, then Quit (asks).
  - [ ] Again with a controller only, and once windowed with Alt+Enter.
  - [ ] Then fill in "Done, with deviations" on each slice.

## Decisions (developer, 2026-10-18)
Numbering continues from part 3a; questions and decisions share numbers.
- **50. Controls:** WASD alongside the arrows; Backspace also cancels. Esc stays Cancel + Menu (Esc rule above); Start opens the menu. No mouse.
- **51. Icons:** text-only menu tabs for now; the icon PNGs in `content/art/icons/` are used where they fit (item category rail, item rows; battle commands later).
- **54. Dialogue:** 30 ms per character; Confirm completes the page, then advances.
- **55. Found items:** "Obtained {item}!", "Obtained {item} x{count}!", "{lost} didn't fit and were left behind." for now.
- **58. Map sprite:** one global map sprite, kept as the single `fieldSpriteFile` in `new-game.json` so it's easy to change (the developer is unsure).
- **59. Menu pages:** as many legacy pages as possible in this plan, for testing: Status, Inventory (with item use), Equipment, Party, Options, System. What Core can't support yet is a named follow-up ("take it from whoever wears it", party sidebar).
- **62. Saving:** anywhere from the menu; overwrite asks, starting on No; labels "File 1" to "File 20".
- **63. Repair notes:** logged only.
- **64. Save folder:** `%LocalAppData%/Jrpg/Saves`.
- **78. Window mode:** full screen, borderless (placeholder: legacy was a borderless maximized window; exclusive mode would also change the monitor's resolution). Letterboxing stays; Alt+Enter and Options switch to the resizable window.
- **79. Random battles:** off until the battle plan; the game doesn't roll them. Map content keeps its tables (`Flow/EncounterFreeContent`).
- **80. Title screen:** New game / Continue (most recent save) / Load / Quit; Continue and Load disabled without saves; "The Lost Realm" on screen (placeholder).
- **81. Game menu:** as legacy: vertical tabs on the left, `GameMenu.png` background, 0.15 s opening fade, Confirm enters a page, Cancel goes back.
- **82. UI look:** Octopath Traveler 2 inspired, by porting the legacy theme and layouts (`Theme.xaml`, `MenuStyles.xaml`, `Bars.xaml`, menu controls). PNGs where legacy used PNGs, code-drawn panels and glows otherwise.
- **83. Unfocused window:** input is ignored.
- **84. UI image names:** constants in `Ui/UiArtFiles`, checked by a test; no ADR.
- **85. Leaving:** Title screen and Quit always ask first; unsaved progress isn't tracked (play time changes every frame, so tracking it isn't trivial).
- **86. Walking feel:** legacy camera easing and 10 fps animation; the easing becomes time-based.
- **87. Placed-object uids:** highest n + 1 (slice E1). Event ids already work this way.

## Open design questions (developer decides)
**Approved (developer, 2026-10-19) with these questions still open:** each slice uses the placeholder below until the developer answers.

**Earlier questions still open, with the placeholder used:**
- **52. Blur:** the title's slot list goes over a dimmed title (no shader). OK?
- **60. Inventory and equipment:** keep the legacy categories All, Consumables, Weapons, Armor (placeholder: yes)? Keep "Remove all" (placeholder: yes)?
- **61. Party order:** pick a member, then the place to swap with (placeholder)?
- **Font:** Lato Regular everywhere until chosen. Legacy used Segoe UI (not openly licensed) for menus and Cinzel Decorative (OFL) for the title.

**New:**
- **88. Reusing the highest id:** both object uids and event ids still reuse the highest one after it's deleted, so an object or event placed later can inherit an old save's state. Accept (placeholder), or store a per-map counter in the map settings file (a content format change)?
- **89. Options between runs:** saved to a settings file in `%LocalAppData%/Jrpg/` (a new file format), or for this run only (placeholder)? Any other options wanted now (none exist without audio)?
- **90. Text speeds:** Slow 45 ms, Normal 30 ms, Fast 15 ms, Instant (placeholder)?
- **91. Using items:** the user is the chosen target (placeholder; it only matters for "self" items)? Result wording "{member} recovers {amount} HP." (placeholder)? Use on a member already at full HP: allowed (Core allows it, placeholder) or greyed?
- **92. Tab and member switching on the keyboard:** the controller uses the shoulders. Keyboard placeholder: Q and R for PreviousTab/NextTab (E is Confirm), plus Left/Right on pages without sideways lists. Other keys?
- **93. Full screen while debugging:** Debug builds also start full screen (placeholder), or windowed with `--fullscreen` to force it?

## Risks
- **Many slices** (26). Each is independent from P1 on; pages can stop at any slice without blocking the others.
- **`ExplorationScene` growth.** Walking, camera, map switching, interact and menu requests stay in the scene; event steps live in `EventPresenter`. If the scene still passes CA1502, extract the per-map view (map view, draw list, camera) at that slice's review.
- **`GameMenuScene` growth.** Six pages behind `IMenuPage`; the scene only routes input and draws the frame. Each page keeps its own state.
- **Per-frame allocation.** `Location` allocates, so the new members are used instead. Text is formatted only on change. The typewriter must not build substrings per frame (M4).
- **`IScene` signature change.** Touches `SceneStack`, `GameFlow`, `StartupErrorScene` and their tests, in T1a only.
- **Changed tests.** `GameFlowTests` change twice for requested behavior changes: T1a (start behind a fade), T1b (start on the title). Each change is named in its slice.
- **Full screen on the developer's machine.** Borderless full screen can hide Visual Studio during debugging (question 93); Alt+Enter is the escape.
- **Saves become real.** First real saves outside tests; format version 1 is unchanged. Uid reuse is reduced by E1, not removed (question 88). Saves outside the repository are invisible to git. Tests use temporary folders, never `%LocalAppData%`.
- **Temporary encounter switch.** `EncounterFreeContent` makes the game's catalog differ from content. The battle plan must delete it; its `TODO` names that plan.
- **Legacy look without WPF effects.** Drop shadows, blur and glows have no shader here. Approximations: repeated text for glows, scrims for blur. Expect small visual differences from legacy.
- **Fonts.** Lato lacks the legacy ▶/✓ markers; markers are drawn as shapes.
- **No dialogue content.** M3c/M4 are checked with an editor-placed sign. If the developer doesn't want it committed, it's removed after the check.

## ADRs needed
None. Shell structure (0008), text (0010), art (0009) and save format (0007) cover this plan, and UI image names are code constants (decision 84). An ADR becomes needed if question 89 adds a settings file (a new persisted format) or question 88 adds a per-map uid counter (a content format change).
