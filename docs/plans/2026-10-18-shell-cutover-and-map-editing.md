# Legacy demo migration, part 3a: game shell, content cutover and map editing
Status: approved

Part 3 of the legacy migration is split into three plans, each approved on its own (decision 77). This file is **part 3a** and the only approved one. Parts 3b (game parity) and 3c (editor catalogs and retirement) are kept at the end, under "Follow-up plans (not approved)", as starting material for their own `/plan-feature`.

## Goal
At the end of part 3a the developer opens a map in the new WPF editor, paints tiles, places objects and events, saves, presses F5 in Visual Studio and sees the map in the MonoGame game.

Concretely:
1. `src/Jrpg.Game` starts from F5 or `dotnet run`, loads and validates content, and draws a map (tile layers, objects in depth order, canopies and roofs over the player) around the new game's start position. Text from the message table and keyboard/controller input work. Walking, events, battles and menus are part 3b.
2. Render data (tilesheets, sheet cells, object tiles, above-player zones, categories, layer Z, draw order) goes through the converter, then the **content cutover** makes `content/` the only source of truth: last converter run, files and art moved to their final places, converter deleted, ADR 0005 superseded.
3. The new editor (`src/Jrpg.Editor`, Core + Data, never Game) edits maps right after the cutover: viewport, undo and redo, tile painting with brush, rectangle and fill, object placement, map settings, object events, event presets and cell events. It saves through Data's validators.
4. F5 in Visual Studio launches the game and the editor together; both read the repository's `content/` in Debug.

**No map-editing gap** (decision 72): the legacy MapEditor and the converter stay in use until slice 10; the editor slices follow directly. The only time no tool can paint maps is while slices 11a-11e are implemented, back to back.

## Guiding rules
- **Port mechanisms, not legacy design** (parts 1 and 2). Legacy values become placeholders in content or in one named place. Every UI or design choice is a question for the developer; slices use the placeholder until it's answered.
- **Thin shells.** Game: input to Core commands, Core state to pixels. Editor: gestures to edit commands on a draft, saved through Data's validators. Any `if` that decides gameplay goes to Core with a test.
- **What can't be unit-tested is split out.** Each screen has a pure part (layout math, cursors, timings, edit commands) tested in `tests/Jrpg.Game.Tests` or `tests/Jrpg.Editor.Tests`, and a thin draw part checked with the manual checklist in its slice.
- **UI direction** (developer, part 1): Octopath-style menus, arrow-nav tabs, blue selection glow, no yellow bars; keyboard and controller first (decision 37). Mostly part 3b.

### Tunables and placeholders
| Name | Where | Placeholder | Meaning | Plan |
|---|---|---|---|---|
| `StickReader.Deadzone` | Game `Input/` | 0.2 | Below this tilt the stick counts as released (part 2 table). | 3a |
| virtual screen | Game `Rendering/VirtualScreen` | 1920 x 1080 | Drawn size before scaling to the window (decision 48). | 3a |
| `MapView.CellPixels` | Game `Exploration/` | 48 | Screen pixels per map cell (decision 48, legacy `TileSize`). | 3a |
| `fieldSpriteFile` | `content/new-game.json` | `player.png` | The one map sprite, as legacy (question 58). | 3a |
| editor zoom limits | Editor `Viewport/` | 0.1 to 8 | Legacy MapEditor. | 3a |
| brush sizes | Editor `Maps/Editing/` | 1, 2, 3, 4, 5, 7, 9 | Legacy MapEditor. | 3a |
| layer presets | Editor `Maps/` | Ground -5, Ground detail -4, Decoration -3, Same as player 1, Above player 5 | Legacy MapEditor (question 71). | 3a |
| camera easing, walk animation, typewriter, fades, banner, battle pacing and layout, cutscene stage cell, save slots | Game | legacy values | See part 3b notes. | 3b |

## Out of scope
- Everything in parts 3b and 3c (see "Follow-up plans").
- New features: autosave, battles started by events (see "New features, not parity"), audio, NPCs, shops, switches and branches, options screen, localization, hot reload, editor play-test button, database and dialogue editors, tile animation, minimap.
- Balance and mechanics: no placeholder rule from parts 1 and 2 changes.

## Design overview

### Current state checked
- Core: battle, party, items, maps, events, exploration, world, saving, session, all headless and tested. `Polygon` is internal. Map records hold only what parts 1-2 use (no sheets, object tiles, zones or Z).
- Data: `ContentCatalogLoader.Load`; `ContentFileWriter` is internal, visible to the converter only. `content/generated/` (tiles, objects, maps, report) is rewritten by `tools/Jrpg.ContentConverter` from `Assets/`; hand-written `content/maps/{id}.json` holds map settings.
- Game: `JrpgGame` opens a black 60 Hz window; no test project; content isn't loaded.
- Legacy facts that shape the design, checked against the files:
  - All 283 tiles come from a tilesheet cell; `Sprite`, `Frames` and `CollisionPolygon` are null everywhere. Only 2 of the ~10 referenced sheets are configured in `Tilesheets.json` (16 px and 64 px); the rest relied on a 48 px code default.
  - Object tile groups never use a per-tile Z offset and `ZOverrides` is empty on every placed object, so an object draws on one layer. Above-player zones are free-form concave polygons (canopies, roofs).
  - The legacy map sprite is `Assets/player.png` (rows Down, Left, Right, Up; 8 frames of 48 px) for every leader.
  - `Assets/Icons/` has PNGs for Attack, Skill, Item, Defend, Run, All, Consumable, Armor, Weapon and TargetArrow, and `Assets/Backgrounds/` has `GameMenu.png` and `StartMenu.png`. Only the menu-tab and stat icons exist solely as XAML vectors in `JRPG/UI/Styles/Art.xaml` (part 3b, question 51).
  - Battle attack effects are GIFs (part 3b, question 57).

### Projects
| Project | TFM | References | Slice |
|---|---|---|---|
| `tests/Jrpg.Game.Tests` | `net10.0` | Game (exe reference), Core, Data | 1a |
| `src/Jrpg.Editor` | `net10.0-windows`, `UseWPF` | Core, Data; `CommunityToolkit.Mvvm` (decision 47) | 11b |
| `tests/Jrpg.Editor.Tests` | `net10.0-windows` | Editor, Core, Data | 11b |
Removed in slice 10: `tools/Jrpg.ContentConverter`, `tests/Jrpg.ContentConverter.Tests`.

### Packages (decisions 46, 47)
- `FontStashSharp.MonoGame` (MIT) for runtime TTF fonts, in `Jrpg.Game` (slice 1c). Only openly licensed fonts (SIL Open Font License, e.g. Google Fonts), each with its license file next to it.
- `CommunityToolkit.Mvvm` for the editor (slice 11b).
Each is pinned to its latest stable version in `Directory.Packages.props` when added.

### One content root for both apps (decision 77)
- In Debug, Game and Editor read the **repository's** `content/`: each project writes the path at build time into `AssemblyMetadata("ContentRoot", "$(MSBuildThisFileDirectory)..\..\content")`, set only for the Debug configuration.
- A `--content <path>` argument overrides it (both apps).
- With neither (a published build), the root is `{AppContext.BaseDirectory}/content`; `content/**` is copied to the output **only on publish** (`CopyToPublishDirectory`), never on build.
- One implementation, `Data/Content/ContentRootLocator` (public), used by both apps, so the rule can't drift.
- Until the cutover (slice 10) legacy art isn't in `content/` yet: for legacy art kinds the art root is the repository's `Assets/` (a second Debug metadata, `ArtRoot`), and `Data/Art/ArtFolders` uses the legacy folder names (`Tiles`; `player.png` at the root). Slice 10 moves the files, removes `ArtRoot` (art root = `{content}/art`) and switches the folder names. No other code changes.

### Visibility rule (as parts 1 and 2)
- Core types become `public` only when Game or Editor calls them: `Geometry/Polygon` (canopy masks, editor shapes) and the new map records.
- Game and Editor types are `internal sealed`. `InternalsVisibleTo`: Game -> `Jrpg.Game.Tests`; Editor -> `Jrpg.Editor.Tests`.
- Data stays internal except entry points: `ContentCatalogLoader`, `ContentRootLocator`, `UiMessageLoader`, `ArtReferenceCheck`, `ContentWriter` and their exceptions.

### Core changes in part 3a
```
Jrpg.Core/
  Geometry/  Polygon (made public)
  Maps/      + TilesheetDefinition, ObjectTile, DrawOrderKey, MapLayers;
             TileDefinition + SheetId/Column/Row; ObjectDefinition + Category/Tiles/AbovePlayerZones;
             TileLayer + Z; PlacedObject + Z
  Content/   ContentCatalog + Tilesheets; NewGameDefinition + FieldSpriteFile
```
No new folder edges: `Maps` still depends on `Events` and `Geometry`; `Content` already depends on `Maps`. (Part 3b's Core edges are listed in its notes.)

### Game structure (ADR 0008)
Part 3a builds `Input`, `Rendering`, `Ui` (messages only), `Scenes`, `Exploration` (map view only) and `Flow`. The rest is part 3b's target, listed so the dependency table is checked once.
```
Jrpg.Game/
  Hosting/     GamePaths (save directory, 3b)
  Input/       GameButton, InputBindings, InputFrame, InputReader, StickReader
  Rendering/   VirtualScreen, TextureLibrary, FontLibrary, TileAtlas, ObjectSpriteBaker, CanopyMask
  Ui/          UiMessages, MessageArgument, MessageIds; 3b: ListCursor, TabStrip, TargetCursor, PlayTimeText, PanelView, SelectionGlow
  Scenes/      IScene, SceneStack; 3b: SceneRequest (+ records), ScreenFade
  Dialogue/    3b: Typewriter, SpeechPlayback, DialogueBox
  Exploration/ ExplorationScene, MapView, MapDrawList, MapCamera; 3b: WalkAnimation, InteractionPromptView, MapNameBanner
  Battle/      3b: BattleScene, BattleLayout, BattlePacing, BattleMenu, BattlePlayback, BattleHud, VictoryScreen,
               ExperienceBarAnimation, DefeatScreen, DefeatMenu
  Menus/       3b: GameMenuScene, IMenuPage, StatusPage, InventoryPage, InventoryFilter, PartyPage, PartyOrderEdit,
               EquipmentPage, StatPreview
  Saving/      3b: SaveSlots, SaveSlotList
  Title/       3b: TitleScene, TitleMenu
  Cutscenes/   3b: CutsceneScene
  Flow/        GameFlow, SessionTransitions; 3b: BattleTransitions, MenuTransitions
```
**Game folder dependencies, checked edge by edge (acyclic):**
| Folder | Depends on |
|---|---|
| `Hosting`, `Input`, `Rendering` | Core, Data only |
| `Ui` | Rendering, Input |
| `Scenes` | Input, Rendering |
| `Dialogue` | Ui, Rendering, Input |
| `Saving` | Hosting, Ui, Rendering, Input |
| `Exploration` | Scenes, Dialogue, Ui, Rendering, Input |
| `Battle` | Scenes, **Saving** (defeat: last save), Ui, Rendering, Input |
| `Menus` | Scenes, Saving, Ui, Rendering, Input |
| `Title` | Scenes, Saving, Ui, Rendering, Input |
| `Cutscenes` | Scenes, Dialogue, Rendering |
| `Flow` | all of the above; only `JrpgGame` depends on `Flow` |
Order from the bottom: Hosting/Input/Rendering -> Ui, Scenes -> Dialogue, Saving -> Exploration, Battle, Menus, Title, Cutscenes -> Flow. Every edge points down; `TargetCursor` and `PlayTimeText` live in `Ui` so Battle and Menus don't depend on each other.

### Editor structure (ADR 0012)
```
Jrpg.Editor/
  Hosting/        App
  Shell/          MainWindow, MainViewModel
  Viewport/       ViewportTransform
  Maps/           MapViewModel, MapDrawing (WPF DrawingVisual), MapCanvas, LayerPresets
  Maps/Editing/   MapDraft, IMapEdit (+ edits), EditHistory, BrushCells, TileFloodFill, PlacedObjectUids, EventIds, MapIds
  Maps/Settings/  MapSettingsViewModel
  Tiles/          TileBrowserViewModel
  Objects/        ObjectBrowserViewModel
  Events/         EventListViewModel, EventStepViewModel, EventPresets
```
`Maps/Editing` depends on Core only (no WPF types), so it's tested headless. `Viewport` depends on nothing. `Maps`, `Maps/Settings`, `Tiles`, `Objects`, `Events` depend on `Maps/Editing`, `Viewport`, Data. `Shell` depends on all; nothing depends on `Shell`.

### Key design decisions
1. **Scenes don't call each other** (ADR 0008). They return `SceneRequest`s (part 3b), and `Flow` applies them through small transition handlers (`SessionTransitions`, `BattleTransitions`, `MenuTransitions`), so no single switch grows past CA1502's limit. One `GameSession` at a time.
2. **Art is runtime-loaded PNG** through one `Rendering/TextureLibrary`, fonts through FontStashSharp (ADR 0009). The editor reads the same files. Final home: `content/art/<kind>/`.
3. **Player-facing text is content** (ADR 0010): `content/text/ui.json`, ids in `Ui/MessageIds`.
4. **Canopies are baked, not clipped.** At startup each object definition's tiles are composed into a body texture and a canopy texture (only pixels inside its zones), drawn over the player. No shader, no stencil; possible because every object draws on one layer (checked, and enforced by the converter and validator).
5. **Render data goes through the converter first** (2a-2c); the legacy MapEditor stays the map tool until slice 10.
6. **The cutover is one slice** (ADR 0011, superseding 0005): last converter run, `git mv` of generated files and art, Data paths and art folders updated, converter deleted.
7. **Editing logic lives in the editor, headless** (ADR 0012): a `MapDraft` copied from a `MapDefinition`, `IMapEdit` commands, `EditHistory`; saved through Data's `ContentWriter`, which runs the loader's validators first.
8. **Stable ids.** A new placed object gets the next free uid `{objectId}_{n}` on its map (no randomness needed); a new event gets `{uid}_{n}` with n one above the highest index on the object; nothing is renumbered when events are reordered or deleted.

### Legacy inventory for part 3a
Parts 1 and 2 ported every rule. This table covers what part 3a moves; the rest of the legacy game and editor is mapped in the follow-up notes.

| Legacy | New | Slice |
|---|---|---|
| `TilesheetDefinition`, `TilesheetDatabase` (48 px default, `GetSourceRect`) | Core `Maps/TilesheetDefinition`; every sheet listed explicitly, no code default | 2a |
| `TileDefinition.Tilesheet`, `SheetX`, `SheetY` | `TileDefinition.SheetId`, `Column`, `Row` | 2a |
| `TileDefinition.Name` | dropped: auto-generated names; the editor shows the sheet cell (question 69) | - |
| `ObjectDefinition.Tiles` / `TileGroupEntry` | `ObjectDefinition.Tiles` / `Maps/ObjectTile` (relative to the top-left; a Z offset is a conversion error) | 2b |
| `ObjectDefinition.AbovePlayerZones`, `Category` | same names | 2b |
| `Tile.Z`, `PlacedObject.Z` | `TileLayer.Z`, `PlacedObject.Z` | 2c |
| `PlacedObject.ZOverrides`, `GetTileZ` | dropped: empty everywhere (conversion error if present; question 69) | 2c |
| `PlacedObject.AbovePlayerZoneZ` | `MapLayers.CanopyZOf` (computed, as legacy) | 2c |
| `DrawSortKey`, `MapConstant.PlayerZ`/`AbovePlayerZ` | Core `Maps/DrawOrderKey`, `Maps/MapLayers` | 2c |
| `MapConstant.TileSize` | Game `MapView.CellPixels`, editor drawing constant | 3a, 11b |
| `MapConstant` paths, `Utils/AssetPathResolver` | Data `ContentRootLocator`, `Art/ArtFolders` | 1a, 3a |
| `App`, `MainWindow`, `GameLoopManager` | `Program`, `JrpgGame`, `VirtualScreen`, MonoGame fixed step (no interpolation needed at 60 Hz) | 1a |
| `InputManager`, `InputRouter` | `Input/*` | 1b |
| `MapRenderer` (tiles, objects, y-sort, zone clips) | `MapView`, `MapDrawList`, `TileAtlas`, `ObjectSpriteBaker`, `CanopyMask` | 3a, 3b |
| `CameraManager.SnapTo`, bounds clamp | `Exploration/MapCamera` (snap only; follow in 3b) | 3a |
| Legacy art in `Assets/` | `content/art/<kind>/` (ADR 0009 table) | 10 |
| `tools/Jrpg.ContentConverter`, `content/generated/` | deleted; files moved | 10 |
| `MapEditor` file menu, dirty tracking, confirm discard | `Shell/MainViewModel`, `Maps/MapViewModel` (maps picked by id) | 11b, 11d |
| map drawing, grid, overlays, dim other layers | `Maps/MapDrawing` with Core `DrawOrderKey` | 11b, 11e |
| camera (middle drag, Space drag, arrows, wheel zoom, fit, 100 %), status bar | `Viewport/ViewportTransform`, `Shell/MainViewModel` | 11c |
| `UndoManager`, `IEditorCommand`, `BatchCommand`, `Commands/*` | `EditHistory`, `IMapEdit` (one edit per stroke) | 11d-11g |
| brush 1-9, right-drag erase, Alt eyedropper, layer presets, free Z | `Maps/Editing` edits, `BrushCells`, `LayerPresets` | 11e |
| rectangle, flood fill (`FillTileCommand`), fill and clear layer | `BrushCells`, `TileFloodFill` | 11f |
| object browser, place, Shift-drag move, delete, Ctrl-drag select | `Objects/ObjectBrowserViewModel`, object edits | 11g |
| `PlacedObject.NewUid` (random hex) | `Maps/Editing/PlacedObjectUids` (next free) | 11g |
| `MapPropertiesWindow` (name, size, encounters flag) | `Maps/Settings/MapSettingsViewModel` (display name, size, encounter table) | 11h |
| `ObjectEventWindow`, presets, `MapEvent.Describe` | `Events/*` | 11i, 11j |
| `TriggerEditWindow`, `SetTriggerCommand` | cell events (warp, dialogue) | 11j |

## Slices
Sizes are estimates of hand-written code plus tests; moved files, generated content and deleted code aren't counted. A slice estimated over ~300 lines is split before it starts.

### Phase 1: game shell

- [x] **ADR 0008: Game shell structure** (before 1a). Options: (A) a scene stack whose scenes return `SceneRequest` records, applied by `Flow` through small transition handlers (`SessionTransitions`, `BattleTransitions`, `MenuTransitions`), each switching over only its own few request types, with pure presenters tested in `Jrpg.Game.Tests`; (B) scenes call a shared scene-manager service (the legacy statics; scenes know each other; folder cycles); (C) one state machine in `JrpgGame`. Recommendation: **A**. Record: one `GameSession` at a time; the flow's update path never touches `GraphicsDevice` or textures (so it runs headless in tests; drawing is separate); the Game seeds `SeededRandom` from the clock and writes the seed to the debug output; the Game passes `DateTimeOffset.Now` to `Capture` and every update's delta to `AddPlayTime`; startup errors go to the console and debug output until text exists; no allocation in `Update`/`Draw`.

- [x] **1a. Game host, content root and scene stack** (Data + Game, about 250 lines)
  - **Goal:** F5 in Visual Studio and `dotnet run --project src/Jrpg.Game` open the game window, which loads and validates the repository's `content/` and starts a new game straight away (a title screen is part 3b). A broken content file stops the start; the exception, naming the file, goes to the console and the debug output (no on-screen error until 1c).
  - **Project:** `Jrpg.Game.csproj`: Debug-only `AssemblyMetadata("ContentRoot", ...)`, `content/**` copied on publish only, a per-monitor DPI-aware `app.manifest` (part 1 follow-up), `InternalsVisibleTo("Jrpg.Game.Tests")`, `Properties/launchSettings.json`. New `tests/Jrpg.Game.Tests` in `Jrpg.slnx`.
  - **New types:**
    - Data `Content/ContentRootLocator` (public static): `string Find(IReadOnlyList<string> args, Assembly app)`: `--content <path>` if given, else the app's `ContentRoot` metadata, else `{AppContext.BaseDirectory}/content`. One rule for both apps.
    - Game `Rendering/VirtualScreen`: draws the game at 1920 x 1080 and fits it into the window (decision 48). `static Rectangle DestinationIn(Rectangle window)` (pure): aspect kept, letterboxed and centered; an integer scale when the window holds an exact integer multiple (1920 x 1080 at 1x, 3840 x 2160 at 2x, a 2560 x 1080 ultrawide at 1x with side bars), otherwise the fractional fit (smaller and in-between screens). Point sampling. *Interpretation of decision 48 to confirm at this slice's review.*
    - Game `Scenes/IScene` (one screen; many implementations): `Update(TimeSpan delta)`, `Draw(SpriteBatch batch)`. `Scenes/SceneStack`: drawn bottom-up, only the top updated; `Push`, `Pop`, `Replace`, `Update`, `Draw`.
    - Game `Flow/GameFlow` (owns the stack and adds play time every update) and `Flow/SessionTransitions` (starts the new game; `SeededRandom` from the clock, seed logged). `Exploration/ExplorationScene`: the scene the new game starts in; draws nothing until 3a.
  - **F5:** create a solution launch profile in Visual Studio for `Jrpg.slnx` with only `Jrpg.Game` and commit the file Visual Studio writes (expected `Jrpg.slnLaunch`; record the real name). If Visual Studio doesn't support launch profiles for `.slnx`, record that and document the fallback ("Configure Startup Projects") in CLAUDE.md instead. 11b adds the editor.
  - **Tests:** Data `ContentRootLocator_Find_ContentArgument_WinsOverMetadata`, `ContentRootLocator_Find_NoArgument_UsesMetadata`, `ContentRootLocator_Find_NoArgumentNoMetadata_UsesBaseDirectory`. Game `VirtualScreen_DestinationIn_ExactlyTwiceVirtualSize_ScalesByTwo`, `VirtualScreen_DestinationIn_UltrawideAtVirtualHeight_ScalesByOneWithSideBars`, `VirtualScreen_DestinationIn_2560x1440_FitsFractionally`, `VirtualScreen_DestinationIn_SmallerWindow_ScalesDownToFit`, `SceneStack_Update_OnlyTopSceneUpdates`, `SceneStack_Draw_DrawsBottomToTop`, `SceneStack_Pop_LastScene_Throws`.
  - **Verify by hand:** F5 opens the game; `dotnet run` too; resizing letterboxes; an edit to `content/` shows on the next start without rebuilding; breaking `content/items.json` stops the start with `items.json` named in the Output window.
  - **Done, with deviations:** `ContentRootLocator.Find` delegates to an internal `Choose(args, builtInRoot)` so tests need no assembly metadata. `VirtualScreen.DestinationIn` is a plain aspect fit, which is a whole-number scale exactly when the window is an exact multiple of 1920x1080; whether 2560x1440 should instead show 1x with bars (pixel art stays crisp) is the developer's call (decision 48). F5: `Jrpg.Game` is marked `DefaultStartup="true"` in `Jrpg.slnx` (accepted by the dotnet tools); a committed multi-project launch profile can't be created outside Visual Studio, so it's checked by the developer, and 11b adds the editor. Startup also reports an empty `--content` or an unreadable folder. Verified here: a missing content folder prints the reason and exits 1; the game runs on the repository content. Extra tests: `GameFlow` starts on the start map and adds every update's delta to play time (`Jrpg.Game.Tests` links `content/`).

- [x] **1b. Input mapping** (Game, about 200 lines)
  - **Goal:** keyboard and controller map to the same buttons and one `MoveIntent`; presses are edge-triggered; the stick has a 0.2 deadzone.
  - **New types (Game):** `Input/GameButton` (`Confirm`, `Cancel`, `Menu`, `Up`, `Down`, `Left`, `Right`, `PreviousTab`, `NextTab`, `DebugCutscene`), `Input/InputBindings` (legacy keys: arrows, E/Space/Enter confirm, Esc, F1; pad: A, B, Start, shoulders, D-pad; question 50), `Input/StickReader` (static: `Deadzone = 0.2f`; `FromStick(Vector2)` with screen Y down; `FromKeys(...)`, opposite keys cancel; the stick wins outside the deadzone), `Input/InputFrame` (`Move`, `IsPressed`, `IsHeld`), `Input/InputReader` (from `KeyboardState` and `GamePadState`, keeping the previous update). `IScene.Update` takes the `InputFrame`.
  - **Tests:** `StickReader_FromStick_InsideDeadzone_ReturnsNone`, `StickReader_FromStick_StickPushedUp_GivesNegativeY`, `StickReader_FromKeys_UpAndLeft_HasLengthOne`, `StickReader_FromKeys_OppositeKeys_CancelOut`, `InputReader_Read_KeyHeldTwoUpdates_PressedOnlyOnFirst`, `InputReader_Read_StickOutsideDeadzone_OverridesKeys`.
  - **Done, with deviations:** a small `Input/ButtonSet` bitset holds held and pressed buttons so reading input allocates nothing; `FromKeys` is `FromDirectionButtons(ButtonSet held)`, fed by both the arrows and the D-pad; the tilt exactly at the deadzone moves. Bindings are placeholders (question 50): Esc gives both Cancel and Menu as in legacy, and the tab buttons have only the controller shoulders (legacy menus were mouse-driven). The game reads the controller without MonoGame's dead zone. Follow-up for the first slice that acts on input: whether to ignore input while the window isn't focused.

- [x] **ADR 0009: Art location and asset loading** (before 1c; decides the content pipeline deferred by part 1). Options: (A) MonoGame content pipeline (`.mgcb`, two packages, `.xnb`; the WPF editor can't read `.xnb`, so PNGs stay the source anyway); (B) runtime PNG through one `TextureLibrary` (alpha premultiplied on load), TTF fonts through `FontStashSharp.MonoGame`; (C) runtime PNG + bitmap fonts with our own loader; (D) runtime PNG, pipeline for SpriteFonts only. Decision (developer, 46): **B**. Record: art in `content/art/<kind>/` (`tilesheets`, `field-sprites`, `battlers`, `portraits`, `icons`, `backgrounds`, `effects`, `fonts`, `ui`); content names art by file within its kind; until slice 10 the art root is `Assets/` with legacy folder names; fonts only under an open license with the license file next to them; GIF effects become PNG strips of square frames (part 3b); Data checks art references (`ArtReferenceCheck`) outside the content loader; test projects link art only where a test checks art.

- [x] **ADR 0010: Player-facing text** (before 1c). Options: (A) strings in Game code; (B) one message table `content/text/ui.json` (id -> text with named placeholders like `{item}`) loaded by Data, ids as constants in `Ui/MessageIds`; (C) one file per screen. Recommendation: **B** (CLAUDE.md: no player-facing text in C#; ready for localization). Save repair notes become structured records worded by the Game (part 3b, question 63).

- [x] **1c. Text on screen** (Data + Game + content, about 250 lines)
  - **Goal:** the game draws crisp text from the message table, and startup errors (content, art) are shown on screen from here on, as well as logged.
  - **Package:** `FontStashSharp.MonoGame` (decision 46).
  - **Data:** `Messages/UiMessageLoader` (public static): `IReadOnlyDictionary<string, string> Load(string contentRoot)` reads `content/text/ui.json`; fails naming the id for an empty text or an unbalanced `{`.
  - **Game:** `Rendering/FontLibrary` (loads `{content}/art/fonts/` directly: fonts are new files, so the transitional `Assets/` art root never applies to them), `Ui/MessageArgument` (`readonly record struct MessageArgument(string Name, string Value)`), `Ui/UiMessages` (`string Text(string id)`, `string Format(string id, params ReadOnlySpan<MessageArgument> arguments)`; unknown id or missing placeholder throws), `Ui/MessageIds`. Formatted strings are built when state changes, never per frame. A startup error screen shows the exception message.
  - **Content:** `content/text/ui.json`; one OFL font with its license file in `content/art/fonts/`.
  - **Tests:** Data `UiMessageLoader_Load_EmptyText_ThrowsNamingTheId`, `UiMessageLoader_Load_UnbalancedBrace_ThrowsNamingTheId`, `UiMessageLoader_Load_RepositoryContent_Succeeds`. Game `UiMessages_Format_NamedPlaceholder_IsReplaced`, `UiMessages_Format_MissingArgument_Throws`, `UiMessages_Text_UnknownId_Throws`, `MessageIds_EveryConstant_ExistsInRepositoryContent` (reflects over `MessageIds`).
  - **Done, with deviations:**
    - **Package and font:** `FontStashSharp.MonoGame` 1.6.2 is Zlib-licensed, not MIT. The font is Lato Regular (SIL OFL, from google/fonts), a placeholder until the developer picks one.
    - **Fonts:** `FontLibrary` loads every `.ttf` in `art/fonts/` in name order (first = main font), so no file name is in code; a file that isn't a font fails naming it.
    - **Startup:**
      - The message table and fonts load first; if either fails, the reason goes to the console only (exit 1), since nothing can be drawn.
      - A content failure shows `Scenes/StartupErrorScene` and is logged. Art errors wait for `ArtReferenceCheck`.
      - `JrpgGame` runs an `IScene`: `GameFlow` implements it, so the host runs the flow or the error scene.
    - **Message ids:** they use underscores like other content ids (`startup_failed`).
    - **Follow-up (decision 48):** the host point-samples the virtual screen into a smaller window, so text aliases at the 2/3 starting size.

### Phase 2: render data through the converter
The developer still edits maps in the legacy MapEditor (ADR 0005), so render data is converted like the rest and regenerated on every run.

- [x] **2a. Tilesheets and tile cells** (tool + Data + Core, about 280 lines)
  - **Goal:** every tile knows its sheet and cell, and every sheet a tile uses is listed with its grid.
  - **Core:** `Maps/TilesheetDefinition` (content record: `Id`, `File`, `TileWidth`, `TileHeight`, `Margin`, `Spacing`; `CellLeft(int column)`, `CellTop(int row)`; how one sheet image is cut). `TileDefinition` gains `SheetId`, `Column`, `Row`. `ContentCatalog` gains `Tilesheets`.
  - **Tool:** `Tiles/LegacyTilesheetJson`, `Tiles/TilesheetConversion`: one entry per sheet referenced by a tile; a sheet missing from `Tilesheets.json` gets the legacy 48 px grid and a report line; sheet ids from the file name by `LegacyIds`; a configured sheet no tile uses is reported and skipped. Writes `content/generated/tilesheets.json`.
  - **Data:** `Maps/TilesheetCatalogValidator` (duplicate ids, empty file, tile size >= 1, margin and spacing >= 0). `TileCatalogValidator`: unknown sheet id, column or row < 0.
  - **Tests:** Core `TilesheetDefinition_CellLeft_WithMarginAndSpacing_SkipsBoth`. Tool `TilesheetConversion_Convert_UnconfiguredSheet_Uses48PixelGridAndReports`, `TilesheetConversion_Convert_RepositoryTileset_ListsEveryReferencedSheet`. Data `TileCatalogValidator_Validate_UnknownSheet_ThrowsNamingTheTile`, `TilesheetCatalogValidator_Validate_ZeroTileWidth_ThrowsNamingTheSheet`. The converter drift test covers the regenerated files.
  - **Done, with deviations:** 10 sheets: `FG_Forest_Fall` at 16 px, the other 9 on the legacy 48 px grid. `Floor.png` is used only by the dropped `Maps/Back` maps, so it's reported as skipped, and question 49 (64 px shrunk to 48) has no sheet to apply to for now. Sheets are matched by file name ignoring case, as legacy did. A tile without a sheet cell is a conversion error (none exist). The validators also refuse an empty sheet `file` and a negative column or row. Core tests build tiles through `Builders/TestTiles`.

- [x] **2b. Object tiles, above-player zones and categories** (tool + Data + Core, about 250 lines)
  - **Goal:** objects carry their tiles, the parts drawn over the player, and the browser category.
  - **Core:** `Maps/ObjectTile` (`readonly record struct ObjectTile(int X, int Y, int TileId)`: one tile of an object, relative to its top-left). `ObjectDefinition` gains `Category`, `Tiles`, `AbovePlayerZones` (`IReadOnlyList<IReadOnlyList<Point2D>>`, normalized like the collision polygon).
  - **Tool:** `ObjectConversion` shifts the tile group to (0, 0) and converts zones like polygons; a non-zero tile Z offset is a conversion error naming the object (none exist).
  - **Data:** `ObjectCatalogValidator`: tiles non-empty, known tile ids, inside `Width` x `Height`, no two on one cell; zones of >= 3 points within 0..1; non-empty category.
  - **Tests:** Tool `ObjectConversion_Convert_TileGroupNotAtOrigin_ShiftsToOrigin`, `ObjectConversion_Convert_TileWithZOffset_ThrowsNamingTheObject`, `LegacyContentConversion_Convert_RepositoryAssets_TreeHasOneCanopyZone`. Data `ObjectCatalogValidator_Validate_TileOutsideBounds_ThrowsNamingTheObject`, `ObjectCatalogValidator_Validate_UnknownTileId_ThrowsNamingTheObject`.
  - **Done, with deviations:** legacy zones were already 0..1 over the tile group's bounds (as the collision polygon), so they carry over unchanged; only tiles are shifted. 8 of 12 objects have one zone each; categories "Chests", "General", "House" carry over as written. The validator also refuses an empty tile list, two tiles on one cell and a null zone; it's split into tile, zone and outline checks to stay under CA1502. Core tests fill the new fields with placeholders.

- [x] **2c. Layers and draw order** (tool + Data + Core, about 250 lines)
  - **Goal:** tile layers and placed objects carry their Z, and one Core rule decides what's drawn over what, for the game and the editor.
  - **Core:** `TileLayer` gains `Z`; `PlacedObject` gains `Z`. `Maps/MapLayers` (static): `PlayerZ = 1` (depth-sorted with the player by base Y), `AbovePlayerZ = 5` (always over the player), `CanopyZOf(int objectZ)` (`Max(AbovePlayerZ, objectZ)`). `Maps/DrawOrderKey` (`readonly record struct`, `IComparable`): Z, then tiles before sprites on one Z, then sprites by base Y, then insertion order (legacy `DrawSortKey`); `ForTile(int z, int order)`, `ForSprite(int z, double baseY, int order)`.
  - **Tool:** layers keep their legacy z, placed objects their `z`; a non-empty `zOverrides` is a conversion error naming the uid (none exist).
  - **Data:** `MapLayoutValidator`: layer Zs strictly ascending.
  - **Tests:** Core `DrawOrderKey_CompareTo_LowerZ_DrawsFirst`, `DrawOrderKey_CompareTo_SameZTileAndSprite_TileFirst`, `DrawOrderKey_CompareTo_SpritesSameZ_LowerBaseDrawsLater`, `DrawOrderKey_CompareTo_EqualKeys_InsertionOrderDecides`, `MapLayers_CanopyZOf_ObjectBelowPlayer_ReturnsAbovePlayerZ`. Tool `MapConversion_Convert_LayerKeepsLegacyZ`, `MapConversion_Convert_ZOverride_ThrowsNamingTheUid`. Data `MapLayoutValidator_Validate_LayersOutOfZOrder_ThrowsNamingTheMap`.
  - **Done, with deviations:** legacy Zs kept as they were: layers at -5 and -4, most placed objects at -3 (one at -2, one at -5), so today every object draws under the player and only canopies (`CanopyZOf` = 5) go over it. `DrawOrderKey` and `MapLayers` have no caller until 3a/3b and 11b. `DrawOrderKey` compares base Y for tiles too (always 0, so a tie), one branch fewer than legacy with the same order. Test builders give layers Z 0, 1, 2 and placed objects `PlayerZ`.

### Phase 3: the map on screen

- [x] **3a. Map tiles on screen** (Data + Core + Game + content, about 290 lines)
  - **Goal:** the game shows the start map's tile layers and the player sprite at the new game's start position, the view centered on it and clamped to the map.
  - **Core:** `NewGameDefinition.FieldSpriteFile` (the one legacy map sprite, `player.png`, until question 58).
  - **Data:** `Art/ArtFolders` (folder per art kind; legacy names until slice 10), `Art/PngSize` (width and height from the PNG header; fails naming the file), `Art/ArtReferenceCheck` (public static `IReadOnlyList<string> FindProblems(ContentCatalog catalog, string artRoot)`: every tilesheet and the field sprite exist, every tile cell lies inside its image). `NewGameValidator` rejects an empty `fieldSpriteFile`. `ContentRootLocator` gains the transitional art root (`ArtRoot` metadata).
  - **Game:** `Rendering/TextureLibrary` (the one loader, `game.md`: `Get(string folder, string file)`, premultiplied, cached, disposed), `Rendering/TileAtlas` (source rectangle per tile id, built once), `Exploration/MapCamera` (pure: `SnapTo(Point2D center, MapLayout layout)`, `Origin`; centers maps smaller than the view, clamps others), `Exploration/MapView` (draws visible cells; `static CellRange VisibleCells(Vector2 origin, MapLayout layout)`). `GameFlow` runs `ArtReferenceCheck` at startup; problems go to the console, debug output and the 1c error screen.
  - **Content:** `new-game.json` gains `"fieldSpriteFile": "player.png"`.
  - **Tests:** Data `PngSize_Read_RepositoryTilesheet_ReturnsItsSize`, `PngSize_Read_NotAPng_ThrowsNamingTheFile`, `ArtReferenceCheck_FindProblems_CellOutsideSheet_NamesTheTile`, `ArtReferenceCheck_FindProblems_RepositoryContent_FindsNone` (Data.Tests links the legacy art it checks), `NewGameValidator_Validate_EmptyFieldSprite_ThrowsNamingTheField`. Game `MapCamera_SnapTo_MapSmallerThanView_CentersMap`, `MapCamera_SnapTo_NearMapEdge_ClampsToEdge`, `MapView_VisibleCells_CameraAtOrigin_CoversScreenPlusOneCell`, `MapView_VisibleCells_NearMapEdge_ClampsToMap`.
  - **Verify by hand:** the start map's ground appears around (10, 3) with no seams or blur; the 16 px forest sheet is sharp at 3x; note how the 64 px `Floor.png` looks at 48 px (question 49).
  - **Done, with deviations:**
    - **Art check:** it runs in `Program` after the catalog loads, not in `GameFlow`, since `Program` chooses the error screen; `GameFlow` stays headless.
    - **Texture loading:** `JrpgGame` owns `TextureLibrary` (it needs the device) and `IScene.Draw` takes it. Textures load on first use through it, cached by (folder, file) so drawing builds no strings.
    - **Tile atlas:** `TileAtlas` is pure (`TileImage`: sheet file and source rectangle) and is built once in `GameFlow`.
    - **Scene:** `ExplorationScene` reads the location once (3b follows the player). Layers up to `PlayerZ` draw before the player, the rest after.
    - **Legacy data:** `player.png` is 384x288; legacy used rows 0-3 only. `Floor.png` isn't used (2a), so question 49 didn't come up.
    - **Publishing:** until slice 10, publishing also copies the legacy tilesheets and `player.png` into `content/art` with the legacy folder names.
    - **Verified by hand:**
      - The start map draws with no seams, and the player stands at (10, 3) facing down.
      - Cell (5, 13) is black: a hole in the legacy map (an empty ground tile), to fill in the editor.
      - The published build starts.
    - **Follow-up:** `ArtReferenceCheck` doesn't check the field sprite's size yet (question 58).

- [ ] **3b. Objects and depth** (Core + Game, about 260 lines; split into "baking" and "draw list" if it grows past ~300)
  - **Goal:** houses, trees, the chest and doors are drawn in depth order with the player, and canopies and roofs over the player, as in the legacy game.
  - **Core:** `Geometry/Polygon` made public.
  - **Game:** `Rendering/CanopyMask` (static, pure: `bool[] Build(IReadOnlyList<IReadOnlyList<Point2D>> zones, int width, int height)`, pixel centers tested with `Polygon.Contains`), `Rendering/ObjectSpriteBaker` (per object definition at startup: a body texture from its tiles at 48 px per cell and a canopy texture of the body pixels inside the mask), `Exploration/MapDrawList` (reusable drawables sorted by `DrawOrderKey` with a cached comparer, no per-frame allocation; objects sort by `Position.Y + Height`, canopies at `MapLayers.CanopyZOf(z)`, the player by its feet on `PlayerZ`).
  - **Tests:** Game `CanopyMask_Build_PixelOutsideEveryZone_IsFalse`, `CanopyMask_Build_PixelInsideZone_IsTrue`, `MapDrawList_Rebuild_SpriteLowerOnScreen_DrawnLater`, `MapDrawList_Rebuild_CanopyOfObjectBelowPlayer_DrawnAfterPlayer`.
  - **Verify by hand:** move `startLocation` in `new-game.json` next to a tree and a house (no rebuild needed, decision 77); compare with the legacy game at the same spot.

### Phase 4: content cutover

- [ ] **ADR 0011: Content after the converter** (before 10; supersedes ADR 0005). Options for maps: (A) two editor-written files per map: `content/maps/{id}.json` (settings, small, hand-readable) and `content/map-layouts/{id}.json` (layout); (B) one file per map. Catalogs at the content root: `tiles.json`, `tilesheets.json`, `objects.json`. Recommendation: **A** (the cutover is pure moves; `MapFolderReader` keeps composing; encounter tuning stays hand-editable). Record: `content/` is the only source of truth; `Assets/` and the converter are gone; the editor writes through `ContentWriter`; ids are never renumbered (decision 8 above); CRLF via `.gitattributes` on the new paths. ADR 0005 becomes "superseded by 0011".

- [ ] **10. Content cutover** (Data + tests + repository, about 200 lines plus moves and deletions)
  - **Goal:** content and art live in their final places, the converter is gone, and Game, tests and (next) the editor read only `content/`.
  - **Steps:** run the converter one last time; `git mv` `content/generated/tiles.json`, `tilesheets.json`, `objects.json` to `content/`, `content/generated/maps/` to `content/map-layouts/`, and the art referenced by content per ADR 0009's table (`Assets/Tiles/*.png` -> `content/art/tilesheets/`, `Assets/player.png` -> `content/art/field-sprites/`, `Sprites/` -> `battlers/`, `Portraits/` -> `portraits/`, `Icons/` -> `icons/`, `Backgrounds/` -> `backgrounds/`; GIF effects wait for their strips in part 3b; unused files stay in `Assets/` per question 74); delete `conversion-report.txt`, `tools/Jrpg.ContentConverter`, `tests/Jrpg.ContentConverter.Tests` and their `Jrpg.slnx` entries; Data path constants; `ArtFolders` to the final names and the art root to `{content}/art` (drop `ArtRoot` metadata); `Jrpg.Data` drops `InternalsVisibleTo("Jrpg.ContentConverter")`; Data.Tests links `content/art/**` instead of `Assets/`; `.gitattributes` rule on the new paths; CLAUDE.md loses the converter lines and `tools/`.
  - **Tests:** no new behavior; existing loader, save and art tests pass unchanged on the new paths. Converter tests go with the converter.
  - **From here on** the legacy MapEditor and `Assets/` are frozen (left in place until part 3c moves them under `legacy/`).

### Phase 5: map editing

- [ ] **ADR 0012: Editor architecture** (before 11a). Options for MVVM: (A) `CommunityToolkit.Mvvm` source generators; (B) a hand-written `ObservableObject`/`RelayCommand` base, which `.claude/rules/editor.md` rules out ("don't hand-roll `INotifyPropertyChanged` boilerplate"); (C) code-behind like legacy, also against `editor.md`; (D) an external editor such as Tiled with an importer, rejected because the developer wants a WPF editor sharing Data's validators. Options for editing logic: (1) headless `MapDraft` + `IMapEdit` + `EditHistory` in the editor's `Maps/Editing`, tested in `Jrpg.Editor.Tests`; (2) editing operations in Core. Decision: **A** (developer, 47) **+ 1** (edits are tool logic, not game rules). Record: content I/O only through Data (`ContentCatalogLoader`, `ContentWriter`); validation on save with the loader's validators; drawing with WPF `DrawingVisual` and Core `DrawOrderKey`, canopies clipped with WPF geometry as legacy; content root through `ContentRootLocator` (Debug metadata or `--content`); never references Game (guard test); view models hold no `DispatcherObject`, so tests need no STA thread.

- [ ] **11a. Writing maps with validation** (Data, about 250 lines)
  - **Goal:** Data saves a map so that the game will load it, or refuses with the loader's message and writes nothing.
  - **Data:** `Content/ContentWriter` (public): `ContentWriter(string contentRoot)`, `void WriteMap(MapDefinition map, ContentCatalog catalog)`: validates layout, settings and events against the catalog with this map replaced (`MapLayoutValidator`, `MapSettingsValidator`, `MapEventValidator`), then writes both files atomically (temporary file, then move) through `ContentFileWriter`; throws `ContentValidationException` on failure.
  - **Tests:** `ContentWriter_WriteMap_ThenLoad_ReturnsEqualMap`, `ContentWriter_WriteMap_InvalidWarpTarget_ThrowsAndLeavesFilesUntouched`, `ContentWriter_WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes` (saving an untouched map produces no diff).

- [ ] **11b. Editor skeleton and map view** (Editor + tests, about 300 lines)
  - **Goal:** F5 launches the game and the editor together; the editor lists maps by id and draws the chosen one as the game does (layers, objects, canopies, depth order).
  - **Projects:** `src/Jrpg.Editor` (`net10.0-windows`, `UseWPF`, `CommunityToolkit.Mvvm`, Debug `ContentRoot` metadata, `InternalsVisibleTo("Jrpg.Editor.Tests")`, `Properties/launchSettings.json`), `tests/Jrpg.Editor.Tests`; both in `Jrpg.slnx`. The 1a launch profile gains `Jrpg.Editor` (or the documented fallback lists both projects).
  - **New types:** `Hosting/App`, `Shell/MainWindow`, `Shell/MainViewModel` (loads the catalog; a load error is shown with its message, not a crash), `Maps/MapViewModel`, `Maps/MapDrawing` (one `DrawingVisual` per redraw; tile images cut by `TilesheetDefinition`; `DrawOrderKey` order; canopies clipped to zone geometry; 48 px per cell at 100 %).
  - **Tests:** `EditorAssembly_ReferencedAssemblies_DoNotIncludeJrpgGame`, `MapViewModel_Open_MapId_ExposesItsLayersInZOrder`.
  - **Verify by hand:** F5 starts both apps; every map draws like the game; `dotnet run --project src/Jrpg.Editor` works.

- [ ] **11c. Viewport** (Editor, about 250 lines)
  - **Goal:** pan (middle drag, Space + drag, arrows), wheel zoom toward the cursor, fit (Ctrl+0), 100 % (Ctrl+1), grid toggle, status bar with cell, tile id and layer under the cursor.
  - **New types:** `Viewport/ViewportTransform` (pure: zoom limits 0.1-8, `ZoomAt(Point screen, double zoom)`, `PanBy`, `Fit(Size map, Size view)`, `ScreenToCell`).
  - **Tests:** `ViewportTransform_ZoomAt_KeepsPointUnderCursorFixed`, `ViewportTransform_ZoomAt_BeyondMax_ClampsToMax`, `ViewportTransform_Fit_WideMap_FitsWidthAndCenters`, `ViewportTransform_ScreenToCell_AfterPan_AccountsForOffset`.

- [ ] **11d. Map documents: draft, undo and save** (Editor, about 300 lines)
  - **Goal:** an opened map becomes an editable draft; edits undo and redo (Ctrl+Z, Ctrl+Y); Ctrl+S saves through `ContentWriter` and shows its message on failure; switching maps or closing with unsaved changes asks first.
  - **New types (`Maps/Editing`):** `MapDraft` (mutable copy of a `MapDefinition`: layer grids by Z, placed objects, cell events, settings; `static From(MapDefinition)`, `ToDefinition()`), `IMapEdit` (`Apply(MapDraft)`, `Revert(MapDraft)`; many implementations), `EditHistory` (`Do`, `Undo`, `Redo`, `IsDirty` relative to the last save, `MarkSaved`; a new edit clears redo), `SetTilesEdit` (a batch of cell changes; one stroke = one undo step, legacy).
  - **Tests:** `MapDraft_ToDefinition_Unchanged_EqualsSource`, `EditHistory_Undo_RevertsLastEdit`, `EditHistory_DoAfterUndo_ClearsRedo`, `EditHistory_IsDirty_AfterUndoToSavedPoint_IsFalse`, `SetTilesEdit_Revert_RestoresPreviousIds`.

- [ ] **11e. Tile painting** (Editor, about 300 lines)
  - **Goal:** pick a tile in the tile browser (sheets with their grid; cells that are tiles marked) and paint with the brush (sizes 1-9, `[` and `]`), erase with right drag, pick with Alt+click (tile and layer), choose the layer from the presets or a free Z, dim other layers.
  - **New types:** `Tiles/TileBrowserViewModel`, `Maps/LayerPresets` (legacy list, question 71), `Maps/Editing/BrushCells` (pure: square brush cells clipped to the map; cells along a drag with no gaps, legacy `LineCells`). Painting onto a Z without a layer inserts the layer in Z order; a layer left empty is omitted on save.
  - **Tests:** `BrushCells_Square_SizeThree_CoversNineCells`, `BrushCells_AtMapEdge_ClipsToMap`, `BrushCells_Line_DiagonalDrag_HasNoGaps`, `MapDraft_SetTile_NewZ_InsertsLayerInZOrder`, `MapDraft_ToDefinition_EmptyLayer_IsOmitted`.

- [ ] **11f. Rectangle, flood fill, fill and clear layer** (Editor, about 250 lines)
  - **Goal:** rectangle tool (R), flood fill (F, or Shift+click) on the current layer, Fill layer and Clear layer, each one undo step.
  - **New types:** `Maps/Editing/TileFloodFill` (pure: 4-connected cells with the same id on one layer, legacy `FillTileCommand`); rectangle cells in `BrushCells`.
  - **Tests:** `TileFloodFill_Cells_StopsAtDifferentTile`, `TileFloodFill_Cells_EmptyCells_FillsConnectedEmptyArea`, `TileFloodFill_Cells_SameIdAsStart_ReturnsRegionOnce`, `BrushCells_Rectangle_DraggedUpLeft_NormalizesCorners`.

- [ ] **11g. Placing objects** (Editor, about 300 lines)
  - **Goal:** the object browser lists object definitions by category with a preview; click places on the chosen Z, Shift+drag moves, right click or Delete removes, Ctrl+drag selects several for Delete; all undoable.
  - **New types:** `Objects/ObjectBrowserViewModel`, `Maps/Editing/PlacedObjectUids` (`string Next(string objectId, IReadOnlyCollection<string> taken)`: `{objectId}_{n}` with the lowest free n on the map, e.g. `chest_2`; existing hex uids stay as they are), `PlaceObjectEdit`, `MoveObjectEdit`, `RemoveObjectsEdit`.
  - **Tests:** `PlacedObjectUids_Next_Chest1Taken_ReturnsChest2`, `PlacedObjectUids_Next_NoneTaken_ReturnsSuffixOne`, `RemoveObjectsEdit_Revert_RestoresObjectsInPlacementOrder`, `MoveObjectEdit_Revert_RestoresPosition`, `MapDraft_ObjectsInRectangle_ReturnsOnlyOverlapping`.

- [ ] **11h. Map settings, resize and new maps** (Editor, about 250 lines)
  - **Goal:** edit a map's display name, size (shrinking warns and drops what falls outside) and random encounter table (odds, grace steps, encounters, or none); create a new map whose id is fixed at creation (question 70).
  - **New types:** `Maps/Settings/MapSettingsViewModel`, `Maps/Editing/ResizeMapEdit` (keeps what it drops, for undo), `Maps/Editing/MapIds` (`string FromName(string name, IReadOnlyCollection<string> taken)`: snake_case, unique).
  - **Tests:** `ResizeMapEdit_Shrink_DropsTilesAndObjectsOutside`, `ResizeMapEdit_Revert_RestoresDroppedContent`, `MapIds_FromName_TakenId_AppendsNumber`, `MapSettingsViewModel_NoEncounters_ProducesNullTable`.

- [ ] **11i. Object events** (Editor, about 300 lines)
  - **Goal:** clicking a placed object in event mode opens its events: activation, prompt, once, "only while the object is", and steps (dialogue, give item, swap object, warp) with add, remove and reorder; ids stay stable. Map markers: yellow for interact, cyan for touch (legacy).
  - **New types:** `Events/EventListViewModel`, `Events/EventStepViewModel` (one step's fields and its one-line label, legacy `Describe`), `Maps/Editing/EventIds` (`string Next(PlacedObject placed)`: `{uid}_{n}`, n one above the highest index on the object), `SetObjectEventsEdit`.
  - **Tests:** `EventIds_Next_AfterDeletingFirst_DoesNotReuseItsId`, `EventIds_Next_ReorderedEvents_KeepTheirIds`, `EventStepViewModel_Label_Warp_NamesMapAndCell`, `SetObjectEventsEdit_Revert_RestoresPreviousEvents`. Save-time checks (unknown items, warps outside maps) come from `ContentWriter`.

- [ ] **11j. Event presets and cell events** (Editor, about 250 lines)
  - **Goal:** presets Chest, Door, Door you open and Sign fill an object's events (with the legacy guess of the "opened" object); clicking a cell in event mode adds or edits its touch event (warp or dialogue); right click removes it.
  - **New types:** `Events/EventPresets` (pure: a preset's events for an object, and the likely opened object id from the object list), `SetCellEventEdit`.
  - **Tests:** `EventPresets_Chest_GivesItemThenSwapsToOpenedObject`, `EventPresets_GuessOpenedObject_ChestFindsChestOpened`, `EventPresets_Door_TouchWarpOnlyWhenOpen`, `SetCellEventEdit_SecondEventOnSameCell_ReplacesFirst`.
  - **Verify by hand (end of part 3a):** open `start`, paint a path with brush, rectangle and fill on two layers, place a tree and a chest with the Chest preset, add a warp cell, set encounters, undo and redo across it, save, press F5: the game shows the change.

## Decisions (developer, 2026-10-18)
Numbering continues from part 2 (questions and decisions share numbers).
46. **Fonts:** `FontStashSharp.MonoGame` (MIT) approved. Only openly licensed font files (SIL Open Font License, e.g. Google Fonts), with the license file kept next to the font.
47. **Editor MVVM:** `CommunityToolkit.Mvvm` approved for the editor.
48. **Screen:** 1920 x 1080 virtual screen, 48 px cells. The game must play on smaller and bigger screens: scale to fit keeping the aspect ratio, with letterbox bars; integer scaling only when the window allows it. Windowed or fullscreen stays open (question 78).
72. **No map-editing gap:** the legacy MapEditor is used until the cutover (slice 10); the new editor's map editing lands in part 3a right after it.
73. **Legacy code:** moves under `legacy/` as read-only reference, outside the solution and the build (part 3c, slice 13a).
77. **Plan split and review fixes:** part 3 is split into 3a (this plan, approved), 3b (game parity) and 3c (editor catalogs and retirement), each approved separately. From the devil's-advocate review: one content root for both apps (repository `content/` in Debug through build-time metadata, `--content` override, copied to output only on publish); startup errors logged to console and debug output until text exists; `GameFlow` split into transition handlers; `PlayTimeText` and `TargetCursor` in `Ui/`, Battle -> Saving edge added; `UiMessages.Format` takes `MessageArgument`s; no bool flag in `MapCamera` or `WalkAnimation`; F5 checked in 1a with a documented fallback; autosave and event-started battles leave parity and become new features; next-free uids for placed objects.

## Open design questions (developer decides)
**Blocking part 3a:** none. Slices use these placeholders until answered:
- **49. Mixed sheet sizes:** the 64 px `Floor.png` shrinks to 48 px with point sampling (uneven pixels). Accept for now? (seen in 3a)
- **50. Controls:** keys (legacy arrows only; add WASD?), confirm (E, Space, Enter), cancel and menu (Esc; Backspace?), controller layout. Mouse in menus and battle (legacy was mouse-driven)? (placeholder in 1b; matters in 3b)
- **58. Map sprite:** one global sprite (placeholder in `new-game.json`, as legacy) or one per member, the map showing the leader's?
- **69. Dropped editor features:** NPC editing, per-tile Z overrides, tile collision polygons, tile animation, tile names, generic asset preview have no data using them. Drop them? (3a doesn't build them; 3c confirms)
- **70. Map ids:** fixed at creation, display name editable, since saves and warps refer to them? (placeholder in 11h)
- **71. Layer presets:** keep the legacy list plus a free Z? (placeholder in 11e)

**Part 3b (game parity):**
- **51. Menu-tab and stat icons:** Inventory, Party, Equipment, Status, System, Save, Load, Options, HP, MP, Attack, Defense exist only as XAML vectors. Make PNGs of them, or text-only tabs for now? (Battle command icons, category icons, the target arrow and both menu backgrounds already exist as PNGs.)
- **52. Blur:** the title's load overlay and the victory background were blurred; MonoGame needs a shader. Dim instead?
- **53. Map name banner** (decision 33): duration (placeholder 2.5 s), position, style; on every entry, including back and forth through a door?
- **54. Dialogue:** typewriter speed (legacy 30 ms per character); confirm completes the page, a second confirm advances?
- **55. Found items:** wording of "Obtained Potion!", "Obtained Potion x2!", and copies lost to a full stack (decision 29).
- **56. Battle:** legacy layout and pacing as placeholders? Targeting with Left/Right? Skills with no icon: none or a default?
- **57. Attack effects:** convert the two GIFs to PNG strips by hand or with a one-off script? Frame time (placeholder 0.08 s)?
- **59. Menu tabs:** keep the legacy set (Inventory, Party, Equipment, Status, System: Save, Load, Options, Title screen, Quit)? Drop the empty Options page?
- **60. Inventory and equipment:** keep the categories All, Consumables, Weapons, Armor? Keep "Remove all"?
- **61. Party order:** select a member, then the place to swap with?
- **62. Saving:** anywhere from the menu (legacy)? Overwrite asks first? Labels "File 1" to "File 20"?
- **63. Save repair notes:** show the player a summary, or only log them (decision 40)? Blocks the Data half of 6a.
- **64. Save folder:** `%LocalAppData%/Jrpg/Saves` (placeholder) or next to the executable?
- **65. "Load last save" after a defeat:** the most recent save of any slot? With none, only "Main menu"?
- **66. Loading screen** (decision 31): black with "Loading…"?
- **68. F1 intro hook:** release builds too, or debug only?
- **78. Window mode:** windowed, borderless fullscreen (legacy), or a choice? (1a uses a resizable window)

**Part 3c (editor catalogs and retirement):**
- **74. Leftover assets:** `Assets/Maps/Back/*` (decision 39) and unused PNGs (`Objects/Tree1-3.png`, `playerx64.png`): move with the legacy code to `legacy/`, or delete?
- **75. "Legacy" in code comments:** rewrite them to state the rule, or keep them as history (the code they cite stays under `legacy/`)?

**New features (not parity, own plans):**
- **30. Battle inside an event:** do the remaining steps run after a victory? What on escape and defeat?
- **44. Autosave slot:** one shared by all playthroughs, or one per playthrough?
- **67. Autosave moments:** when does the autosave happen?
- **76. Audio:** legacy has none; its own plan when wanted.

## Risks
- **Two apps, one content root.** In Debug both apps read and the editor writes the same `content/`. The game reads content only at startup, so a save in the editor shows on the next game start; no file locking issue (atomic writes, reads at startup only).
- **Cutover timing.** After slice 10 the converter is gone; a map edit wanted between 10 and 11e waits for those slices (decision 72 accepts this as implementation time, not a gap in tooling plans).
- **Canopy baking.** Textures are baked at 48 px per cell; tiles from 16 px and 64 px sheets are scaled per tile before baking. If an object ever spans several layers, baking no longer works as is (converter and validator refuse per-tile Z).
- **Content churn from the editor.** Every save rewrites JSON; `ContentWriter_WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes` and `.gitattributes` keep diffs to real changes.
- **Stable ids.** Uids and event ids are save keys (ADR 0005). New uids are next-free `{objectId}_{n}`, so deleting `chest_2` and placing a new chest reuses `chest_2`: an old save's "opened" state would apply to the new chest. Acceptable while saves are play-test saves; part 3b can revisit (e.g. highest n + 1) if it matters. Event ids use the highest index + 1, so only deleting an object's last event can free its id.
- **Visual Studio launch profiles.** Support for `.slnx` is checked in 1a; the fallback is documented.
- **Analyzers on WPF and MonoGame projects.** Warnings-as-errors and code style apply to code-behind and MonoGame templates; generated XAML files may need an `.editorconfig` section marking them generated (with a comment).
- **Test projects.** `Jrpg.Game.Tests` references a WinExe (supported). `Jrpg.Editor.Tests` relies on view models free of `DispatcherObject`s.
- **Art in test outputs.** Only Data.Tests links art (for the art check).
- **Save format.** No change in part 3a.

## ADRs needed
Each is a checkbox step in the slice list, accepted before the first slice that needs it.
- **0008 Game shell structure** (before 1a): scene stack with returned requests and small transition handlers (recommended) vs. a shared scene-manager service vs. one state machine; headless flow; seeding and time.
- **0009 Art location and asset loading** (before 1c): content pipeline vs. runtime PNG + FontStashSharp (decided, 46) vs. bitmap fonts vs. pipeline for fonts only; `content/art/<kind>/`; transitional art root; font licensing.
- **0010 Player-facing text** (before 1c): strings in code vs. one message table (recommended) vs. per-screen files.
- **0011 Content after the converter** (before 10, supersedes 0005): two files per map (recommended) vs. one; final locations; id stability.
- **0012 Editor architecture** (before 11a): `CommunityToolkit.Mvvm` (decided, 47) vs. hand-written base (against `editor.md`) vs. code-behind vs. Tiled (rejected: WPF editor wanted); headless `Maps/Editing` vs. Core; validation on save; content root.

---

## Follow-up plans (not approved)
Starting material only. Nothing below is approved; each part gets its own `/plan-feature`, which revisits sizes, ADRs and questions. Slice ids are kept from the first draft.

### Part 3b: game parity (not approved)
Goal: the game does everything the legacy WPF game did. Uses part 3a's shell, input, text, map view and content root.

**Core edges added by these notes (checked acyclic):** `Cutscenes` -> Dialogue, Geometry; `Content` -> Cutscenes; `Saving` -> Content (`SaveSlotSummary`); `Session` -> Battle. Nothing lower points back up.

**Legacy inventory for 3b:** `GameManager` prompt, item message, warp fades, encounter hand-off -> 3d, 3e, 4b; `CameraManager` follow and `PlayerManager` animation -> 3c; `DialogManager`, `DialogueBox` -> 3d; `CutsceneManager`, `CutscenePlayer`, F1 -> 7a-7c; `BattleUI`, `TurnOrderBar`, `CharacterPanel`, `EnemyTag`, `CommandPanel`, `BattleBanner`, `FloatingText` -> 4b-4d; `VictoryScreen`, `ExpGainRow`, escaped screen -> 4e; `DefeatScreen`, `BattleExit` -> 6d; `GameMenuControl`, `MenuTab`, `MenuHeader`, `PartySidebar`, `StatusView`, `InventoryView`, `PartyView`, `EquipmentView`, `EquipmentSelectionView` -> 5a-5d; `SaveLoadView`, `SaveSlotPanel`, `ConfirmDialog`, `SaveDataPreview` -> 6a, 6b; title screen -> 6c; `OptionsView` (empty) -> question 59; `SceneManager`, `GameFlowManager`, `GameStateManager` -> `SceneRequest`s and `Flow` handlers; `ImageLoader`, `UiArt`, `FadeHelper`, `UI/Styles/*` -> `TextureLibrary`, `ScreenFade`, `Ui/*`.

**Tunables for 3b:** camera easing 0.35 moving / 0.1 still per 60 Hz update (passed as an easing fraction); walk animation 8 frames at 10 fps, rows Down/Left/Right/Up; typewriter 30 ms per character, blink 0.5 s; warp fade 0.25 s out, 0.08 s black, 0.25 s in; scene change 0.15 s black; map name banner 2.5 s; battle pacing step 0.2 s, effect 1.0 s, 2.0 s with a defeat, escaped 1.5 s, effect frame 0.08 s; battle layout sprite 96 px, row spacing 136, row shift 50; cutscene stage cell 96 px; 20 manual save slots (decision 34).

**Headless game-flow test (ADR 0008):** `GameFlow_SeededNewGame_WalkIntoForestUntilBattle_RequestsBattleAndFinishes`: a seeded new game, scripted inputs walk through the warp into the forest until a `BattleRequested`, the battle is driven to an end with `EnemyTactics` and an attack-first policy, the flow returns to exploration. The flow's update path uses no `GraphicsDevice` or textures.

- **3c. Walking, animation and camera.** Core `ExplorationState.IsPlayerMoving`. Game `MapCamera.Follow(Point2D target, MapLayout layout, double easing)` (the scene passes 0.35 or 0.1), `Exploration/WalkAnimation` with `Advance(TimeSpan delta)` while moving and `Stand()` when not, `static int RowOf(Facing facing)`. Per-member `fieldSpriteFile` only if question 58 says so. Tests: `ExplorationState_IsPlayerMoving_PushingIntoWall_IsTrue`, `MapCamera_Follow_OneUpdate_ClosesEasingFractionOfDistance`, `MapCamera_Follow_NearMapEdge_ClampsToEdge`, `WalkAnimation_Stand_ShowsFirstFrame`, `WalkAnimation_Advance_TenthOfASecond_AdvancesOneFrame`, `WalkAnimation_RowOf_EveryFacing_FollowsSheetOrder`. Verify: keys and stick, diagonals not faster, half tilt slower, collisions as legacy, interior centered.
- **3d. Interaction prompt, dialogue and found items.** Core `Exploration/InteractionTarget`, `ExplorationState.InteractionInFront` (cached; refreshed after walking, map entry, event and encounter end), `ItemReceipt` public, `ExplorationState.CurrentItemReceipt`, `ExplorationState.CurrentObjectIdOf(PlacedObject)` (swaps, so the opened chest draws opened). Game `Dialogue/Typewriter`, `SpeechPlayback`, `DialogueBox`, `Exploration/InteractionPromptView`; messages per question 55. Tests: `ExplorationState_InteractionInFront_FacingChest_ReturnsItsPrompt`, `ExplorationState_InteractionInFront_WhileEventRuns_IsNull`, `ExplorationState_CurrentItemReceipt_OnGiveItemStep_ReportsAddedCopies`, `ExplorationState_CurrentObjectIdOf_AfterChestOpened_ReturnsOpenedChest`, `Typewriter_Update_OneCharacterTime_RevealsOneCharacter`, `Typewriter_RevealAll_CompletesPage`, `SpeechPlayback_Confirm_WhileTyping_CompletesPageWithoutAdvancing`, `SpeechPlayback_Confirm_OnLastCompletePage_Finishes`.
- **3e. Warps, fades and map names.** `Scenes/SceneRequest` introduced here; `Scenes/ScreenFade` (pure), `Exploration/MapNameBanner` (pure timing). Tests: `ScreenFade_Update_PastFadeOut_IsFullyBlackDuringHold`, `ScreenFade_Update_AfterFadeIn_IsDone`, `MapNameBanner_Update_PastDuration_IsHidden`, `MapNameBanner_Show_WhileShowing_RestartsTimer`.
- **4a. Battle seams in Core.** `Effects/TargetChoice<T>(IReadOnlyList<T> Candidates, bool TakesAll)`; `BattleState.UpcomingActors` (part 1's `TurnOrder.Upcoming`), `BattleState.TargetChoiceFor(Targeting)`; `CanUse(skill/item)` also needs a legal target; `FieldItemUse.CanUse` likewise and `FieldItemUse.TargetChoiceFor(item, user)`; `EnemyTactics` public; `GameSession.StartPendingBattle()`, `GameSession.EnemyTactics`, `GameSession.FinishBattle(BattleState)` (gains after a victory, clears the encounter). Tests as in the first draft (`BattleState_UpcomingActors_*`, `BattleState_TargetChoiceFor_*`, `BattleState_CanUse_ReviveWithNobodyFallen_ReturnsFalse`, `FieldItemUse_*`, `GameSession_StartPendingBattle_NoEncounterPending_Throws`, `GameSession_FinishBattle_*`).
- **4b. Battle screen: attack, defend, run.** `BattleRequested`/`BattleFinished` handled by `Flow/BattleTransitions`; `Battle/BattleLayout`, `Battle/BattleMenu`, `Ui/TargetCursor`, `Battle/BattleHud`, `Battle/BattleScene`; art check covers battlers and backgrounds. Tests: `BattleLayout_PositionOf_SecondEnemy_IsOneRowAndShiftAway`, `BattleMenu_ConfirmAttack_OpensTargetsOnLivingEnemies`, `BattleMenu_ConfirmTarget_ProducesAttackCommand`, `BattleMenu_ConfirmDisabledCommand_DoesNothing`, `BattleMenu_CancelOnTargets_ReturnsToCommands`, `TargetCursor_Next_LastCandidate_WrapsToFirst`, `TargetCursor_TakesAllChoice_SelectsEveryCandidate`.
- **4c. Battle playback.** `Battle/BattlePacing`, `Battle/BattlePlayback` (pure); effect strips replace the GIFs (question 57), moved to `content/art/effects/`. Tests: `BattlePlayback_Start_AttackEvents_ActorStepsForwardFirst`, `BattlePlayback_Update_EffectApplied_AddsFloatingNumberWithAmount`, `BattlePlayback_Update_TargetDefeated_UsesLongerDelay`, `BattlePlayback_Update_AllBeatsDone_IsNotPlaying`, `ArtReferenceCheck_FindProblems_EffectStripWithPartialFrame_NamesTheFile`.
- **4d. Skills and items in battle.** `BattleMenu` lists; icons from `content/art/icons/` (empty icon per question 56). Tests: `BattleMenu_SkillWithoutEnoughMp_IsDisabled`, `BattleMenu_ConfirmSkill_OpensTargetsForItsTargeting`, `BattleMenu_AllAlliesItem_ProducesCommandOnEveryLivingMember`, `BattleMenu_CancelOnSkillList_ReturnsToCommands`.
- **4e. Victory and escape screens.** `Battle/ExperienceBarAnimation` (pure), `Battle/VictoryScreen`. Tests: `ExperienceBarAnimation_Update_TwoLevelUps_PassesThroughBothSteps`, `ExperienceBarAnimation_Skip_JumpsToFinalStep`, `ExperienceBarAnimation_NoSteps_IsComplete`.
- **5a. Menu shell and status.** `Ui/ListCursor`, `Ui/TabStrip`, `Ui/PlayTimeText`, `Ui/PanelView`, `Ui/SelectionGlow`, `Menus/IMenuPage`, `Menus/GameMenuScene`, `Menus/StatusPage`; menu requests via `Flow/MenuTransitions`; tab icons per question 51. Tests: `ListCursor_MoveDown_OnLastEntry_WrapsToFirst`, `ListCursor_MoveDown_SkipsDisabledEntry`, `TabStrip_Next_OnLastTab_WrapsToFirst`, `PlayTimeText_Format_OverAnHour_ShowsHoursMinutesSeconds`.
- **5b. Inventory page.** `Menus/InventoryFilter`, `Menus/InventoryPage` over `FieldItemUse` (reserve included, decision 25). Tests: `InventoryFilter_Weapons_KeepsOnlyWeaponSlotItems`, `InventoryFilter_Consumables_KeepsItemsWithEffect`, `InventoryFilter_All_KeepsPickupOrder`.
- **5c. Party page.** `Menus/PartyOrderEdit`, `Menus/PartyPage` (question 61). Tests: `PartyOrderEdit_Swap_TwoMembers_SwapsTheirPlaces`, `PartyOrderEdit_Swap_SameMember_KeepsOrder`.
- **5d. Equipment page.** Core `PartyEquipment.WearableFor(member, slot, items)`; Game `Menus/StatPreview`, `Menus/EquipmentPage`. Tests: `PartyEquipment_WearableFor_OtherClassItem_IsExcluded`, `PartyEquipment_WearableFor_ArmorSlot_ExcludesWeapons`, `StatPreview_Of_StrongerWeapon_ShowsPositiveAttack`, `StatPreview_Of_EmptySlotItem_ShowsFullBonus`.
- **6a. Save slot summaries and structured repair notes.** Core `Saving/SaveSlotMember`, `Saving/SaveSlotSummary.Of(save, catalog)`. Data half (depends on question 63): `Saving/SaveRepairKind`, `Saving/SaveRepairChange`, `RepairedSave.Changes` as records; existing repair tests assert kinds. Tests: `SaveSlotSummary_Of_UsesMapDisplayName`, `SaveSlotSummary_Of_ListsActiveMembersInOrder`.
- **6b. Save and load from the menu.** `Hosting/GamePaths.SaveDirectory` (question 64), `Saving/SaveSlots`, `Saving/SaveSlotList`, `LoadRequested`. Tests: `SaveSlots_ManualSlots_AreTwentyInOrder`, `SaveSlots_NameOf_FirstSlot_IsSlot01`.
- **6c. Title screen.** `Title/TitleMenu` (pure), `Title/TitleScene` (background `StartMenu.png`, already a PNG); removes 1a's auto-start. Tests: `TitleMenu_ConfirmStart_RequestsNewGame`, `TitleMenu_ConfirmExit_RequestsQuit`, `TitleMenu_ConfirmLoad_OpensSlotList`.
- **6d. Defeat flow.** `Saving/SaveSlots.MostRecent` (question 65), `Battle/DefeatMenu`, `Battle/DefeatScreen` (Battle -> Saving edge), loading screen (question 66). Tests: `SaveSlots_MostRecent_PicksLatestSavedAt`, `SaveSlots_MostRecent_NoReadableSave_ReturnsNull`, `DefeatMenu_NoSave_OffersOnlyMainMenu`.
- **7a. Cutscene playback (Core).** `Cutscenes/CutsceneStep` + `FadeOutStep`, `FadeInStep`, `ShowBackgroundStep`, `ShowActorStep`, `MoveActorStep`, `CutsceneDialogueStep`; `CutsceneDefinition`, `CutsceneActor`, `CutscenePlayback`. Tests: `CutscenePlayback_Update_HalfwayThroughFadeOut_BlacknessIsHalf`, `CutscenePlayback_Update_LeftoverTime_CarriesIntoNextStep`, `CutscenePlayback_Update_OnDialogue_WaitsForContinue`, `CutscenePlayback_Update_MoveActor_InterpolatesPosition`, `CutscenePlayback_Update_PastLastStep_IsFinished`.
- **7b. Cutscene content.** Discriminators, `Cutscenes/CutsceneValidator`, `content/cutscenes/intro.json` (legacy intro verbatim). Tests: `CutsceneValidator_Validate_MoveBeforeShow_ThrowsNamingTheStep`, `ContentCatalogLoader_Load_RepositoryContent_HasIntroCutscene`.
- **7c. Cutscene scene and F1 hook** (decision 36, question 68). `Cutscenes/CutsceneScene`. Test: `CutsceneScene_StagePositionToScreen_ScalesByStageCell`.
- **9. Game parity check** (manual): legacy and new game side by side over title, walking, events, warps, battles (all commands, victory, escape, defeat), every menu tab, save and load, F1 intro, play time, F5 and `dotnet run`.

### Part 3c: editor catalogs and retirement (not approved)
Goal: the editor manages tilesheets, tiles and object definitions, matches the legacy MapEditor, and the legacy demo leaves the build.

**Legacy inventory for 3c:** `TilesetManagerWindow`, `ZoomableImageViewer` -> 11k, 11l; `ObjectEditorWindow`, `CollisionEditorWindow`, `PropertyEditor` -> 11m; selection mode + `NewObjectWindow`, `TileGroupObjectCreator` -> 11n; F1 controls help -> 11n; `NpcEditWindow`, `ZEditPopup`, folder browser -> dropped (question 69); `JRPG.sln`, `JRPG/`, `JRPG.Core/`, `MapEditor/`, remaining `Assets/` -> `legacy/` (decision 73, question 74).

- **11k. Tilesheet manager.** `ContentWriter.WriteTilesheets` (refuses removing a used sheet); `Tiles/TilesheetManagerViewModel`, `Tiles/TilesheetCells` (columns and rows; `GuessTileSize` preferring 48, then 32, 16, 64, 24). Tests: `ContentWriter_WriteTilesheets_RemovingUsedSheet_ThrowsNamingTheSheet`, `TilesheetCells_Columns_WithMarginAndSpacing_CountsWholeCells`, `TilesheetCells_GuessTileSize_DivisibleBy48_Prefers48`.
- **11l. Tiles from sheet cells.** `ContentWriter.WriteTiles` (refuses removing a tile a map or object uses); `Tiles/TileIds` (one above the highest; reuse is harmless since tile ids aren't save keys); transparent cells skipped. Tests: `ContentWriter_WriteTiles_RemovingTileUsedByMap_ThrowsNamingTheMap`, `TileIds_Next_ReturnsOneAboveHighest`, `TilesheetCells_IsTransparent_AllZeroAlpha_ReturnsTrue`.
- **11m. Object definitions: collision, zones, category.** `ContentWriter.WriteObjects` (refuses removing an object a map places or an event names); `Objects/ObjectEditorViewModel`, `Objects/ShapeEditing`. Tests: `ContentWriter_WriteObjects_RemovingPlacedObject_ThrowsNamingTheMap`, `ShapeEditing_InsertIndexNear_PointOnSecondEdge_ReturnsTwo`, `ShapeEditing_Normalize_PointOutsideObject_ClampsToUnitSquare`.
- **11n. Objects from tiles, overlays and help.** `Objects/ObjectFromTiles`; overlay toggles; F1 help. Tests: `ObjectFromTiles_Create_SelectionWithEmptyCells_SkipsThem`, `ObjectFromTiles_Create_TakenId_AppendsNumber`, `ObjectFromTiles_Create_SizeFromSelectionBounds`.
- **12. Editor parity check** (manual): a full legacy MapEditor session redone in the new editor, then played in the game.
- **13a. Move the legacy demo under `legacy/`** (decision 73): tag `legacy-demo-final`; `git mv` `JRPG.sln`, `JRPG/`, `JRPG.Core/`, `MapEditor/` and the rest of `Assets/` under `legacy/` with a README (read-only reference, built by nothing); `Jrpg.slnx` and every build ignore it. Verify: build, test and format on `Jrpg.slnx` pass; nothing outside `legacy/`, ADRs and plans names `JRPG.sln`, `JRPG.Core`, `MapEditor` or `Assets/`.
- **13b. Docs, hooks and launch setup:** CLAUDE.md (drop "Current state (migration in progress)"; add `tests/Jrpg.Game.Tests`, `tests/Jrpg.Editor.Tests`, editor run command, F5 profile), `.claude/hooks/verify-on-stop.ps1` (only `Jrpg.slnx`), `src/Directory.Build.props` comment, `.editorconfig` (its comment near line 75 mentions the legacy demo), `AI-SETUP.md`, `README.md`, `.claude/agents/architect.md`, agent memories naming legacy paths; "legacy" comments in code per question 75.

### New features, not parity (own plans)
- **Autosave** (questions 44, 67): the legacy game had none. Writes an `autosave` slot at the chosen moments and lists it with the manual slots.
- **Battles started by events** (question 30): legacy event battles only started three hardcoded goblins and skipped the rest of the event, and no content uses them. Would add `Events/StartBattleStep`, a `startBattle` discriminator and validator, and the editor's battle step.
