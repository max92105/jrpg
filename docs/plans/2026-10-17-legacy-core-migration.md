# Legacy demo migration, part 1: solution skeleton, party, inventory and battle core
Status: approved

## Goal
Create `Jrpg.slnx` with the target project layout, then port the legacy demo's party, inventory, equipment, progression and battle rules into `src/Jrpg.Core`, with all hardcoded content moved to `content/` JSON loaded and validated by `src/Jrpg.Data`. The end state of this plan: a battle can be set up from content and played to victory, defeat or escape, headless, from a unit test or a simulation with a fixed seed. Nothing uses static `GameSystems`, `*Manager` classes, or unseeded randomness.

The legacy demo has more logic than fits in one plan of ~300-line slices. This plan covers the solution and the party/battle domain. World, exploration, events, saves and cutscenes are listed under **Follow-up plans**, with a destination for every legacy type, so the inventory below is complete.

## Guiding rule: port mechanisms, not design
The legacy demo is a proof of concept. Most of its rules were written to check that something happens at runtime, not as designed mechanics (developer, 2026-10-17). So this plan:
- ports each **mechanism** (level-up, damage, defend, flee, turn order, experience split) in its simplest working form;
- keeps every number in `content/` JSON, never in C#;
- marks each undesigned rule as a **placeholder** in its slice, so later design work knows what it replaces;
- doesn't invent mechanics the developer hasn't designed (class growth, class resources, escape chance, speed stat).

Placeholders in this plan: level-up grants no stat growth, the experience table values, the damage formula, Defend (no effect), Flee (always succeeds), shuffled turn order, goblin stats, all starting values in `content/new-game.json` and `content/party-members.json`, MP as the only skill resource (classes will get their own resources such as Rage or Faith).

## Out of scope
- All rendering, input, audio, and the MonoGame scene flow. Slice 1b only gets a window open.
- The editor project (`src/Jrpg.Editor`). Created in part 4, when it has something to do.
- Maps, movement, collision, map events, warps, random encounters, world state, save/load, play time, cutscenes (follow-up plan "part 2").
- Moving art out of `Assets/`. Only JSON content moves, and only for the types ported here.
- Balancing and designing mechanics (see the guiding rule).
- New mechanics (status effects, speed stat, class resources, gold, shops).

## Design overview

### Current state checked
- `src/` and `tests/` hold only `Directory.Build.props` (warnings as errors, `latest-recommended` analyzers, code style in build, CA1502 at 10). `.editorconfig` requires `_camelCase` private fields and namespace = folder (IDE0130). No `.slnx`, no projects, no `content/` folder yet.
- ADR 0001 (MonoGame + WPF + pure Core) is accepted. No other ADRs.
- Legacy `JRPG.Core` uses Newtonsoft.Json and WPF (`Point`, `Int32Rect`). The new Core uses only System.Text.Json (in-box) and no UI types.

### Packages (approved 2026-10-17)
- Central package management: one `Directory.Packages.props` at the repo root holds every version.
- Tests: `xunit.v3` (4.0.2), on Microsoft.Testing.Platform selected in `global.json`. `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk` were approved but aren't needed on that platform (see slice 1a).
- Game: `MonoGame.Framework.DesktopGL` (latest stable 3.8.x).
- Pin each to the latest stable version when it is added.
- Deferred: the MonoGame content pipeline (`MonoGame.Content.Builder.Task` + `dotnet-mgcb`) is decided in part 3; `CommunityToolkit.Mvvm` in part 4.

### Project layout
| Project | TFM | References | Slice |
|---|---|---|---|
| `src/Jrpg.Core` | `net10.0` | none | 1a. Game rules. No packages. |
| `src/Jrpg.Data` | `net10.0` | Core | 1a. Content loaders and validators (System.Text.Json). |
| `src/Jrpg.Game` | `net10.0` | Core, Data | 1b. MonoGame shell. |
| `tests/Jrpg.Core.Tests` | `net10.0` | Core | 1a. |
| `tests/Jrpg.Data.Tests` | `net10.0` | Core, Data | 1a. Loader and validator tests. |
| `tests/Jrpg.Simulations` | `net10.0` | Core, Data | 1a. Headless aggregate runs. |
| `src/Jrpg.Editor` | `net10.0-windows`, `UseWPF` | Core, Data | Part 4. |

Test projects that read real content link `content/**` into their output (`<None Include="..\..\content\**" LinkBase="content" CopyToOutputDirectory="PreserveNewest" />`), so tests never walk up the directory tree looking for the repo root.

### Visibility rule
- `public`: content definitions (`*Definition`, `Effect`, `EquipmentStats`, `ContentCatalog`, `GameRules`), feature state roots and what callers hold (`PartyState`, `PartyMember`, `Inventory`, `PartyEquipment`, `BattleState`, `Combatant`, `GameSession`, `FieldItemUse`), commands and results (`BattleCommand` records, `TurnResult`, `EffectOutcome`), and `IRandom`/`SeededRandom`. Data, Game and the simulations need these.
- Everything else is `internal sealed` (`TurnOrder`, `DamageFormula`, `EffectResolver`, `TargetRules`, `EnemyTactics`, `BattleRewards`, helpers).
- `Jrpg.Core` declares `InternalsVisibleTo` for `Jrpg.Core.Tests` and `Jrpg.Simulations` only.

### Core feature folders introduced by this plan
```
Jrpg.Core/
  Randomness/  IRandom, SeededRandom
  Stats/       Vitals, CombatStats
  Progression/ ExperienceTable, LevelProgressStep, ExperienceGain
  Effects/     Effect, EffectKind, Targeting, TargetRelation, TargetRules, EffectResolver
  Items/       ItemDefinition, EquipmentStats, EquipmentSlot, Inventory, InventoryEntry, FieldItemUse
  Skills/      SkillDefinition
  Party/       PartyMemberDefinition, ClassDefinition, PartyRules, PartyMember, PartyState, PartyEquipment
  Battle/      BattleState, BattlePhase, BattleSide, Combatant, TurnOrder, DamageFormula,
               BattleCommand (+ Attack/Defend/Flee/Skill/Item commands), TurnResult, EffectOutcome,
               EnemyDefinition, EnemyTactics, EncounterDefinition, BattleRewards
  Content/     ContentCatalog, GameRules
  Session/     GameSession, NewGameDefinition
```
Naming choices, made once:
- **PartyMember**, never "Character" or "Hero". **Enemy**, never "Monster". **Combatant** for either side inside a battle.
- **Experience** in code (`Experience`, `ExperienceToNextLevel`). "EXP" stays a display string.
- A type never has the same name as its feature folder (CA1724 and ambiguous references). So the runtime battle is `Battle/BattleState` (as CLAUDE.md suggests); the legacy enum `BattleState` becomes `BattlePhase`. The roster is `Party/PartyState`. The inventory lives in `Items/Inventory`.
- Definitions (immutable records loaded from content) end in `Definition`. Runtime state is a class with no suffix (`PartyMember`, `Combatant`, `Inventory`) or `State` when it's the root of a feature (`PartyState`, `BattleState`).
- Content ids are snake_case (`hi_potion`, `bronze_sword`, `sarxian`, `goblin_trio`). `Assets/Maps/start.json` already references `"potion"`, which fits.

### Key design decisions in this plan
1. **Composition instead of the legacy inheritance trees.** `BattleActor` -> `Character`/`Enemy` -> `Goblin` and `Item` -> `Consumable`/`Equipment` -> `Weapon`/`Armor` become:
   - `ItemDefinition` with an optional `Effect` (consumables) and optional `EquipmentStats` (weapon or armor via `EquipmentSlot`).
   - A single sealed `Combatant` built by `Combatant.FromPartyMember(...)` or `Combatant.FromEnemy(...)`.
   - Enemies are content (`EnemyDefinition`), not subclasses.
2. **Vitals are shared, not copied.** `Combatant.FromPartyMember` uses the same `Vitals` instance as the `PartyMember`, so damage taken in battle persists after it (legacy behavior, since legacy battles mutated `Character` directly). One source of truth, no write-back step. A test covers it.
3. **Battle API returns results** (ADR before slice 5b). `BattleState.Execute(BattleCommand)` validates the command for the current actor, applies it, ends the turn, and returns `TurnResult(Combatant Actor, IReadOnlyList<EffectOutcome> Outcomes)`, so the renderer knows who acted after the turn has moved on. This replaces the legacy `ExecuteCurrentTurnAction(CombatAction, targets, object param)` + `AdvanceTurn()` + `IsBattleOver()` (a query that also changed state).
4. **Commands are typed records**, not an enum plus an `object` parameter: `AttackCommand`, `DefendCommand`, `FleeCommand`, `SkillCommand`, `ItemCommand`. Every party action goes through `Execute`.
5. **Enemy choice is separate from battle rules.** `EnemyTactics` picks a command for an enemy turn with `IRandom`. The legacy UI picked a target with `new Random()`.
6. **Equipment uses the move model** (developer decision 2026-10-17, ADR before slice 4). Equipping takes the copy out of `Inventory`; unequipping puts it back. `Inventory` only ever holds unworn copies, so nothing needs to check who wears what. This replaces `InventoryItem.EquippedTo` and `AvailableStock`.
7. **Effects are their own feature.** `Effects/EffectResolver` and `Effects/TargetRules` work on `Vitals` and a `TargetRelation` (`Self`, `Ally`, `Enemy`), not on `Combatant`. Battle and the field menu both depend on `Effects/`; `Items/` never depends on `Battle/`.
8. **One shared content entry point** (ADR before slice 3a). `ContentCatalog` (Core record) holds every loaded definition dictionary plus `GameRules`. `Jrpg.Data.Content.ContentCatalogLoader.Load(contentRoot)` builds it and runs every validator. `GameSession.StartNew(ContentCatalog)` replaces `GameSystems.Initialize()`.

### Legacy inventory: where every type goes
Slice numbers refer to this plan. P2 = follow-up "world and exploration core", P3 = rendering, P4 = editor port.

**`JRPG.Core/Battles`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `BattleManager` | `Battle/BattleState`, `Battle/TurnOrder`, `Battle/BattleRewards`, `Battle/DamageFormula` | 5a, 5b, 6, 7 | Split by responsibility. `Guid` shuffle becomes a seeded Fisher-Yates through `IRandom`. `Execute` ends the turn. |
| `BattleActor` (abstract) | `Battle/Combatant` (sealed) | 5a | Composition: `Vitals` + `CombatStats` + skills + side. |
| `Character` | `Party/PartyMember` + `Party/PartyMemberDefinition` | 2, 4 | Runtime vs content split. `IsActive`/`IsMainCharacter` flags move to `PartyState`. |
| `Enemy`, `Enemies/Goblin` | `Battle/EnemyDefinition` + `content/enemies.json` | 5a, 7 | No subclass per enemy. `SlotIndex` = position in the encounter. `SpriteFlip` -> `IsSpriteFlipped`. |
| `Skill` | `Skills/SkillDefinition` + `content/skills.json` | 6 | Uses the shared `Effect` record. |
| `ActionResult` | `Battle/EffectOutcome` | 5b | Reports the amount actually applied. Legacy reported raw power for skills and items. |
| `TurnEntry` | removed | 5a | `TurnOrder` holds combatants. `IsPlayer` -> `Combatant.Side`. |
| `ExpStep`, `ExpGain` | `Progression/LevelProgressStep`, `Progression/ExperienceGain` | 2, 7 | Immutable records. |
| `Enums/BattleState` | `Battle/BattlePhase` | 5b | Adds `Escaped`. |
| `Enums/CombatAction` | `Battle/BattleCommand` records | 5b, 6 | Run becomes `FleeCommand`. |
| `Enums/EffectType` | `Effects/EffectKind` | 3a | `ManaRecovery` -> `RestoreMp` (legacy restored HP instead, a bug). Adds `Revive`, since heals no longer revive. |
| `Enums/TargetType` | `Effects/Targeting` | 3a | `None` dropped (a non-consumable has no `Effect`). |

**`JRPG.Core/Systems`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `GameSystems` (static) | `Session/GameSession` (instance, passed in) | 8 | No statics. |
| `Parties/PartyManager` | `Party/PartyState` | 4 | Hardcoded 7 members -> `content/party-members.json`. Starting active flags -> `content/new-game.json` (8). Active order is saved by id, not name (P2). |
| `Inventories/InventoryManager` | `Items/Inventory` | 3a, 3b | `CreateItemById` switch and `KnownItemIds` -> `content/items.json` + `ContentCatalog.Items`. `InitializeDefaultItems` -> `new-game.json` (8). Selection state (`SelectItem`/`UseSelectedItem`) is UI state and goes away: battle uses `ItemCommand` (6), the menu uses `Items/FieldItemUse` (8). Save mapping -> P2. |
| `Inventories/Models/Item`, `Consumable` | `Items/ItemDefinition` + `Effects/Effect` | 3a | `CanBeUsedFromInventory` -> `IsUsableFromMenu`. `MaxStack` is enforced. |
| `Inventories/Models/InventoryItem` | `Items/InventoryEntry` | 3a | `EquippedTo`/`AvailableStock` disappear with the move model. |
| `Inventories/Enums/ItemType` | removed | - | Unused. |
| `Equipments/EquipmentManager` | `Party/PartyEquipment` | 4 | Takes the inventory through its constructor, not `GameSystems.Inventory`. |
| `Equipments/Models/Equipment`, `Weapon`, `Armor` | `Items/EquipmentStats` + `Items/EquipmentSlot` | 3a | |
| `Equipments/Models/EquipmentEntry` | removed | - | Unused. |
| `GameSystemManager` | `Session/GameSession` fields + `Session/PlayTime` | P2 | `DateTime.Now` play time -> accumulated game-loop delta. Save orchestration -> `Saving/`. |
| `SaveGames/*` (`SaveGameManager`, `SaveData`, `SaveDataPreview`, `SavedInventoryEntry`) | `Saving/SaveGame`, `Saving/ISaveStore`, `Saving/SaveSlotSummary` | P2 | Versioned format. File I/O behind `ISaveStore`. `GetLocationName` switch -> map display name from content. `SaveTimestamp` passed in by the Game. Legacy saves are not migrated. |
| `WorldState` | `World/WorldState` (instance in `GameSession`) | P2 | |

**`JRPG.Core/Maps`, `Events`, `Triggers`, `Cutscenes`, `Utils`**
| Legacy | New | Where | Change |
|---|---|---|---|
| `Map`, `Tile`, `TileGroupEntry`, `PlacedObject`, `ObjectDefinition`, `TileDefinition`, `TilesheetDefinition` | `Maps/*` with the same names, except `Tile` -> `MapTile` | P2 | WPF `Point` -> `Geometry/Point2D`. `Int32Rect` -> a Core rect record. `PlacedObject.NewUid` (Guid) -> editor only. |
| `AnimatedTileDefinition`, `IO/TilesetLoader` | removed | - | Unused. |
| `DrawSortKey`, `MapConstant` | `Maps/DrawOrderKey`, `Maps/MapLayers` | P2 | `TileSize` (pixels) -> Game/Editor. File paths -> Data. |
| `IO/MapSerializer`, `TileDatabase`, `TilesheetDatabase`, `ObjectDatabase` (static) | Jrpg.Data map/tile/object readers -> immutable `TileCatalog`, `ObjectCatalog`, `TilesheetCatalog` | P2 | Legacy uid derivation and `event` -> `events` migration move to the Data reader. |
| `Events/Models/MapEvent` (+ `EventAction`, enums) | `Events/MapEvent` + typed `EventStep` records | P2 | `Describe()` and `Clone()` -> Editor. |
| `Triggers/*`, `Enums/TriggerType`, `TriggerJsonConverter` | merged into map events (tile events with touch activation) | P2 | Developer decision: one event system. Converter -> Data. |
| `Cutscenes/*` | `Cutscenes/CutsceneScript` + step records, advanced by delta | P2 | Intro script -> `content/cutscenes/intro.json`. `StepType` enum is unused and dropped. |
| `Utils/AssetPathResolver` | Game asset loader (P3) and Data content root | P3 | |
| `Utils/MathExtension` | `Math.Clamp` | - | Removed. |
| `Utils/TileGroupObjectCreator` | Editor | P4 | |

**`JRPG/` (WPF game): logic found outside the core library**
| Legacy | New | Where |
|---|---|---|
| `UI/Battle/BattleUI`: enemy target via `new Random()` | `Battle/EnemyTactics` | 5b |
| `BattleUI`: Defend, Run | `DefendCommand`, `FleeCommand` | 5b |
| `BattleUI` + `InventoryView`: target validity per `TargetType` | `Effects/TargetRules` | 6 |
| `UI/GameMenu/InventoryView`: using a consumable on party members | `Items/FieldItemUse` | 8 |
| `UI/GameMenu/PartyView`, `EquipmentSelectionView` (including "take it from another wearer") | `PartyState`, `PartyEquipment` | 4 |
| `UI/GameMenu/Controls/StatsPanel`: base stat + weapon bonus preview | `PartyMember.StatsWith(ItemDefinition)` | 4 |
| `Managers/TriggerHandler`: hardcoded 3 goblins | `content/encounters.json` (`EncounterDefinition`) | 7 |
| `SceneManager`: battle background "Forest.png" | `EncounterDefinition.BackgroundFile` | 7 |
| `Managers/PlayerManager` (movement, collision, polygons) | `Exploration/PlayerWalker`, `Geometry/Polygon` | P2 |
| `MapManager` (walkability, tile trigger lookup, animated frames on the wall clock) | `Maps/Walkability`; frame timing on game time -> P3 | P2 |
| `Managers/GameManager` (object events, warps, arrival search, encounter roll with static `Random`, trigger re-arm) | `Exploration/*`, `Events/EventRun`, `Exploration/RandomEncounters` (`IRandom`) | P2 |
| `CutsceneManager.StartIntroCutscene` (content in C#) | `content/cutscenes/intro.json` | P2 |
| `SceneManager`/`GameFlowManager`/`GameStateManager` (static), `GameLoopManager`, `CameraManager`, `DialogManager`, `InputManager`, `InputRouter`, `MapRenderer`, `UI/*`, `Utils/*` | Game shell (scene stack, MonoGame fixed step, camera, dialogue box) | P3 |
| `SceneManager.StartNewGameAsync`: `"start.json"`, `(10, 3)` | `content/new-game.json` start location fields | P2 |

**`MapEditor/`**: `UndoManager`, `IEditorCommand`, `Commands/*` (including the flood fill in `FillTileCommand`), windows and controls -> P4 (Editor, MVVM). No game rules live there. Flood fill can become a Core `Maps/TileFloodFill` when the editor port needs it.

## Slices

- [x] **1a. Solution skeleton**
  - **Deviations:**
    - xunit.v3 4.x only runs on Microsoft.Testing.Platform. `global.json` selects it, the two VSTest packages were dropped, and the test command is now `dotnet test --solution Jrpg.slnx` in CLAUDE.md, the skills, the agents and the stop hook (developer approved, option A). The hook only tests `Jrpg.slnx`; the legacy solution has no tests.
    - `Jrpg.Data.Tests` and `Jrpg.Simulations` pass `--ignore-exit-code 8` (zero tests) until slices 3b and 7.
    - `InternalsVisibleTo` is deferred to the first slice with an internal Core type.
    - Review added a second guard test, `CoreAssembly_TargetPlatform_IsPlatformNeutral`, because the compiler drops unused references and a `net10.0-windows` + `UseWPF` Core would pass the first one. Known gap: an engine reference that comes in transitively through a package isn't detected; Core has no packages, so it isn't a risk yet.
  - **Goal:** `dotnet build`, `test` and `format --verify-no-changes` on `Jrpg.slnx` pass with Core, Data and the three test projects. One guard test proves Core stays engine-free.
  - **Files:**
    - `Jrpg.slnx`: the new projects only. Legacy projects stay in `JRPG.sln`.
    - `Directory.Packages.props`: central versions for the approved packages.
    - `src/Jrpg.Core/Jrpg.Core.csproj` (with `InternalsVisibleTo` per the visibility rule), `src/Jrpg.Data/Jrpg.Data.csproj`
    - `tests/Jrpg.Core.Tests/Jrpg.Core.Tests.csproj`, `tests/Jrpg.Data.Tests/Jrpg.Data.Tests.csproj`, `tests/Jrpg.Simulations/Jrpg.Simulations.csproj`
    - `tests/Jrpg.Core.Tests/Architecture/CoreDependencyTests.cs`
    - `CLAUDE.md`: add `tests/Jrpg.Data.Tests` to the architecture list.
  - **Tests:** `CoreAssembly_ReferencedAssemblies_ContainNoUiOrEngineAssembly` loads `Jrpg.Core` and fails on any reference whose name starts with `PresentationCore`, `PresentationFramework`, `WindowsBase`, `System.Windows`, `MonoGame` or `Microsoft.Xna`.

- [ ] **1b. Game window**
  - **Goal:** `src/Jrpg.Game` opens a black window with MonoGame and closes cleanly.
  - **Files:** `src/Jrpg.Game/Jrpg.Game.csproj`, `Program.cs`, `JrpgGame.cs` (`internal sealed class JrpgGame : Game`: MonoGame host, fixed 60 Hz update step, clears the screen to black). Added to `Jrpg.slnx`.
  - **Tests:** none (no behavior). The 1a architecture test keeps guarding Core.
  - **Verify by hand:** `dotnet run --project src/Jrpg.Game` opens a black window and closes cleanly.

- [ ] **2. Vitals and level progression**
  - **Goal:** A party member gains experience and levels up level by level, reporting one step per level (what the victory screen animates). Placeholder: a level-up only raises the level; stat growth waits for the class design.
  - **New types (Core):**
    - `Stats/Vitals`: current and maximum HP/MP; every change is clamped. `Vitals(int hp, int maxHp, int mp, int maxMp)`, `Hp`, `MaxHp`, `Mp`, `MaxMp`, `IsAlive`, `int TakeDamage(int amount)` (returns the HP actually lost), `int RestoreHp(int amount)` (returns the HP actually restored; 0 for a fallen member, heals don't revive), `int Revive(int hp)` (fallen members only), `bool TrySpendMp(int cost)`, `int RestoreMp(int amount)`.
    - `Stats/CombatStats`: `readonly record struct CombatStats(int Attack, int Defense)`. The offensive and defensive numbers used by damage.
    - `Progression/ExperienceTable`: `record ExperienceTable(IReadOnlyList<int> ExperienceToNextLevel)`. Index `n` = experience needed to go from level `n + 1` to `n + 2`; the max level is one past the last entry. `int ExperienceToReachNext(int level)`, `bool IsMaxLevel(int level)`. Data, not a formula: per-class curves later are just more tables. Loaded from content in slice 4.
    - `Progression/LevelProgressStep`: `record LevelProgressStep(int Level, int FromExperience, int ToExperience, int ExperienceToNextLevel, bool IsLevelUp)`. One level's worth of an experience gain.
    - `Party/PartyMemberDefinition`: content record (`Id`, `Name`, `ClassId`, `PortraitFile`, `BattleSpriteFile`, `AttackEffectFile`, `Level`, `Experience`, `Hp`, `MaxHp`, `Mp`, `MaxMp`, `Attack`, `Defense`). Who a member is and their starting values. The threshold comes from the table, not the definition.
    - `Party/PartyMember`: a recruited member's runtime state (level, experience, base stats, vitals). `static PartyMember Recruit(PartyMemberDefinition definition)`, `Id`, `Definition`, `Level`, `Experience`, `BaseStats`, `Vitals`, `IReadOnlyList<LevelProgressStep> GainExperience(int amount, ExperienceTable table)`. This changes state and returns what happened, which `.claude/rules/core.md` allows ("prefer returning results").
  - **Tests (`tests/Jrpg.Core.Tests/Stats`, `/Progression`; builder `Builders/PartyMemberBuilder`):**
    - `Vitals_TakeDamage_MoreThanHp_ClampsToZeroAndReturnsHpLost`
    - `Vitals_RestoreHp_AboveMax_ClampsToMaxAndReturnsHpRestored`
    - `Vitals_RestoreHp_Fallen_ReturnsZeroAndStaysFallen`
    - `Vitals_Revive_Fallen_RestoresHp`
    - `Vitals_Revive_Living_ReturnsZero`
    - `Vitals_TrySpendMp_NotEnoughMp_ReturnsFalseAndKeepsMp`
    - `ExperienceTable_ExperienceToReachNext_Level1_ReturnsFirstEntry`
    - `PartyMember_GainExperience_BelowThreshold_ReturnsOneStepWithoutLevelUp`
    - `PartyMember_GainExperience_ExactThreshold_LevelsUpWithZeroCarryOver`
    - `PartyMember_GainExperience_CrossesTwoThresholds_ReturnsOneStepPerLevel`
    - `PartyMember_GainExperience_AtMaxLevel_ReturnsNoSteps`
    - `PartyMember_GainExperience_Zero_ReturnsNoSteps`
  - **Also:** in `CoreDependencyTests`, replace `Assembly.Load(new AssemblyName("Jrpg.Core"))` with `typeof(Vitals).Assembly` now that Core has a type.

- [ ] **ADR: content schema mapping** (before 3a). Deserialize Core definition records directly from JSON, or use separate DTOs in Jrpg.Data mapped to Core types. Decides how much JSON shapes Core's public records.

- [ ] **3a. Items and inventory**
  - **Goal:** Items are defined as records and an `Inventory` holds quantities, enforcing each item's max stack.
  - **New types (Core):**
    - `Effects/EffectKind`: `Damage`, `RestoreHp`, `RestoreMp`, `Revive`.
    - `Effects/Targeting`: `SingleEnemy`, `AllEnemies`, `SingleAlly`, `Self`, `AllAllies`, `SingleFallenAlly`, `AllFallenAllies`.
    - `Effects/Effect`: `record Effect(EffectKind Kind, int Power, Targeting Targeting)`. What a skill or consumable does and to whom.
    - `Items/EquipmentSlot`: `Weapon`, `Armor`.
    - `Items/EquipmentStats`: `record EquipmentStats(EquipmentSlot Slot, int AttackBonus, int DefenseBonus, IReadOnlyList<string> AllowedClassIds)` + `bool CanBeWornBy(string classId)`. What wearing an item gives and who may wear it.
    - `Items/ItemDefinition`: content record (`Id`, `Name`, `Description`, `IconFile`, `MaxStack`, `IsUsableFromMenu`, `Effect? Effect`, `EquipmentStats? Equipment`). One item type.
    - `Items/InventoryEntry`: `record InventoryEntry(string ItemId, int Quantity)`.
    - `Items/Inventory`: owned, unworn item quantities in pickup order. `int Add(ItemDefinition item, int quantity)` fills up to `MaxStack` and returns how many didn't fit (throws `ArgumentOutOfRangeException` when the quantity is 0 or less), `bool TryRemove(string itemId, int quantity)`, `int QuantityOf(string itemId)`, `IReadOnlyList<InventoryEntry> Entries`.
  - **Tests (`tests/Jrpg.Core.Tests/Items`; builder `Builders/ItemDefinitionBuilder`):** `Inventory_Add_ExistingItem_IncreasesQuantity`, `Inventory_Add_BeyondMaxStack_FillsToMaxAndReturnsLeftover`, `Inventory_Add_NonPositiveQuantity_Throws`, `Inventory_TryRemove_LastCopy_RemovesEntry`, `Inventory_TryRemove_MoreThanOwned_ReturnsFalseAndKeepsQuantity`, `EquipmentStats_CanBeWornBy_ClassNotAllowed_ReturnsFalse`

- [ ] **3b. Content loading pipeline**
  - **Goal:** `content/items.json` loads and validates into a `ContentCatalog`. This sets up the pipeline every later slice extends.
  - **New types (Core):** `Content/ContentCatalog`: record of `IReadOnlyDictionary<string, …>` per definition type, all loaded content by id. Starts with `Items` and gains a property per slice.
  - **New types (Data):**
    - `Content/ContentJsonOptions`: the one `JsonSerializerOptions` for content (camelCase, string enums, `RespectNullableAnnotations`, `RespectRequiredConstructorParameters`, unknown members rejected).
    - `Content/ContentFileReader`: reads one content file and wraps parse errors with the file name. `T Read<T>(string path)`.
    - `Content/ContentValidationException`: message names the file, the id and the field.
    - `Items/ItemCatalogValidator`: checks duplicate ids, empty names, `MaxStack >= 1`, `Power >= 0`, bonuses `>= 0`, that no item has both an effect and equipment stats, and that a menu-usable item never targets enemies.
    - `Content/ContentCatalogLoader`: `static ContentCatalog Load(string contentRoot)`. Loads every content file under the content root and runs its validator.
  - **Test builder:** `tests/Jrpg.Core.Tests/Builders/ContentCatalogBuilder` (defaults for every property), so adding a catalog property doesn't break existing tests.
  - **Content:** `content/items.json`: the 12 legacy items with snake_case ids. Descriptions now match effects (Elixir: "Restores 100 HP."). Antidote is dropped until status effects exist. Revive Feather becomes `Revive` 15, `AllFallenAllies`.
  - **Also:** remove `--ignore-exit-code 8` from `Jrpg.Data.Tests.csproj`.
  - **Tests (Data):** `ContentCatalogLoader_Load_RepositoryContent_Succeeds` (the "all of content/ must load" test required by `content.md`), `ItemCatalogValidator_Validate_DuplicateId_ThrowsNamingTheId`, `ItemCatalogValidator_Validate_MaxStackZero_ThrowsNamingTheField`, `ItemCatalogValidator_Validate_MenuItemTargetingEnemies_ThrowsNamingTheItem`, `ContentFileReader_Read_MalformedJson_ThrowsNamingTheFile`, `ContentFileReader_Read_MissingRequiredField_ThrowsNamingTheFile`

- [ ] **ADR: equipment ownership model** (before 4). Records the developer's choice of the move model over the legacy "worn copies stay counted" model, and its effect on menus, saves and future shops.

- [ ] **4. Party roster and equipment**
  - **Goal:** Members are recruited from content, the active party enforces size and leader rules, and members equip weapons and armor from the inventory, which changes their total stats.
  - **New types (Core):**
    - `Party/ClassDefinition`: `record ClassDefinition(string Id, string Name)`. A class that equipment can be restricted to.
    - `Party/PartyRules`: `record PartyRules(int MaxActiveMembers)`. Design limits of the party.
    - `Content/GameRules`: `record GameRules(PartyRules Party, ExperienceTable Experience)`. The global rules from `content/rules.json`. `ContentCatalog` gains `Rules`.
    - `Party/PartyState`: recruited members and the ordered active party, with the size and leader rules enforced. The leader is always first in the active order. `PartyState(IReadOnlyList<PartyMember> members, IReadOnlyList<PartyMember> activeOrder, PartyRules rules)` (the leader is `activeOrder[0]`), `Members`, `ActiveMembers`, `Leader`, `IsFull`, `PartyMember MemberById(string id)`, `bool TryAddToActive(PartyMember member)`, `bool TryMoveToReserve(PartyMember member)` (false for the leader), `void Reorder(IReadOnlyList<PartyMember> newOrder)`. Throws `ArgumentException` if the set of members differs or the leader isn't first. Legacy silently filtered.
    - `Party/PartyEquipment`: moves equipment between the inventory and party members. `PartyEquipment(Inventory inventory)`, `bool TryEquip(PartyMember member, ItemDefinition item)` (false if the item isn't equipment, the class isn't allowed, no copy is in the inventory, or the replaced item can't fit back in the inventory; on success the copy leaves the inventory and the replaced item returns to it), `bool TryUnequip(PartyMember member, EquipmentSlot slot)` (false if the slot is empty or the inventory stack is full).
    - `PartyMember` gains `Weapon`, `Armor` (`ItemDefinition?`), `CombatStats TotalStats` (base + bonuses), `CombatStats StatsWith(ItemDefinition item)` (the stat preview: total stats if `item` replaced what's in its slot), and `internal` `Wear`/`TakeOff` used only by `PartyEquipment`.
  - **New types (Data):** `Party/ClassCatalogValidator`, `Party/PartyMemberCatalogValidator` (unknown class id, `Hp <= MaxHp`, level >= 1, level within the experience table), `Content/GameRulesValidator` (`MaxActiveMembers >= 1`, every table entry >= 1). `ItemCatalogValidator` also checks `AllowedClassIds` against the classes.
  - **Content:** `content/classes.json` (Warrior, Mage, Thief, Healer, Knight, Ranger), `content/party-members.json` (the 7 legacy members, test values), `content/rules.json` (`maxActiveMembers` 4, a placeholder experience table).
  - **Tests:**
    - Core: `PartyState_TryAddToActive_PartyFull_ReturnsFalse`, `PartyState_TryMoveToReserve_Leader_ReturnsFalse`, `PartyState_Reorder_DifferentMembers_Throws`, `PartyState_Reorder_LeaderNotFirst_Throws`, `PartyEquipment_TryEquip_ClassNotAllowed_ReturnsFalse`, `PartyEquipment_TryEquip_NoCopyInInventory_ReturnsFalse`, `PartyEquipment_TryEquip_Success_RemovesCopyFromInventory`, `PartyEquipment_TryEquip_SlotOccupied_ReturnsPreviousItemToInventory`, `PartyEquipment_TryEquip_AfterUnequipFromOtherWearer_Succeeds` (the legacy "take it from another wearer" flow), `PartyEquipment_TryUnequip_InventoryStackFull_ReturnsFalseAndKeepsItem`, `PartyMember_TotalStats_WithWeaponAndArmor_AddsBonuses`, `PartyMember_StatsWith_OtherWeapon_ReplacesCurrentWeaponBonus`
    - Data: `PartyMemberCatalogValidator_Validate_UnknownClassId_ThrowsNamingTheMember`, `ItemCatalogValidator_Validate_UnknownAllowedClass_ThrowsNamingTheItem`, `GameRulesValidator_Validate_ZeroMaxActiveMembers_ThrowsNamingTheField`

- [ ] **5a. Combatants, turn order and damage**
  - **Goal:** Combatants for both sides are built from party members and enemy definitions, take turns in a seeded shuffled order, and hits use the damage formula.
  - **New types (Core):**
    - `Randomness/IRandom`: the seam for all randomness in Core. `int Next(int maxExclusive)`.
    - `Randomness/SeededRandom`: `IRandom` over `System.Random` with a required seed.
    - `Battle/BattleSide`: `Party`, `Enemies`.
    - `Battle/EnemyDefinition`: content record (`Id`, `Name`, `PortraitFile`, `BattleSpriteFile`, `AttackEffectFile`, `IsSpriteFlipped`, `MaxHp`, `MaxMp`, `Attack`, `Defense`, `ExperienceReward`). One enemy type.
    - `Battle/Combatant`: one participant in a battle (side, stats, vitals). `static Combatant FromPartyMember(PartyMember member)` (shares the member's `Vitals`, stats = `TotalStats`), `static Combatant FromEnemy(EnemyDefinition enemy)` (fresh full `Vitals`), `Name`, `Side`, `Stats`, `Vitals`, `IsAlive`.
    - `Battle/TurnOrder`: a fixed cycle of combatants, shuffled once at battle start, skipping the defeated (placeholder for a future speed-based order). `TurnOrder(IReadOnlyList<Combatant> combatants, IRandom random)` (Fisher-Yates), `Current`, `void Advance()`, `IReadOnlyList<Combatant> Upcoming` (living only, for the turn bar).
    - `Battle/DamageFormula`: static pure formula (placeholder). `int Calculate(int power, int defense)` = `max(MinimumDamage, power - defense)`, with `MinimumDamage = 1`. Comment: ported from the demo as a placeholder; a minimum of 1 so every hit counts.
  - **Tests (`tests/Jrpg.Core.Tests/Battle`; builders `Builders/CombatantBuilder`, `Builders/EnemyDefinitionBuilder`, `Randomness/FixedSequenceRandom` test double):**
    - `SeededRandom_Next_SameSeed_ReturnsSameSequence`
    - `TurnOrder_Constructor_SameSeed_ProducesSameOrder`
    - `TurnOrder_Advance_NextCombatantDefeated_SkipsIt`
    - `DamageFormula_Calculate_DefenseAbovePower_ReturnsMinimumDamage`
    - `Combatant_FromPartyMember_DamageInBattle_PersistsOnMember`
    - `Combatant_FromEnemy_StartsAtFullHp`

- [ ] **ADR: battle command flow** (before 5b). `Execute` validates, applies and ends the turn, returning `TurnResult` with the actor for the renderer to animate. The alternative is separate execute and advance steps driven by the UI, as in legacy. Enemy tactics stay outside `BattleState`.

- [ ] **5b. Battle commands: attack, defend, flee**
  - **Goal:** A battle runs headless with a fixed seed through typed commands, with enemy target choice and victory, defeat or escape.
  - **New types (Core):**
    - `Battle/BattleCommand` (abstract record), `Battle/AttackCommand(Combatant Target)`, `Battle/DefendCommand` (placeholder: no effect), `Battle/FleeCommand` (placeholder: always succeeds; an escape chance comes later).
    - `Battle/EffectOutcome`: `record EffectOutcome(Combatant Target, EffectKind Kind, int Amount)`. What one effect actually did to one target.
    - `Battle/TurnResult`: `record TurnResult(Combatant Actor, IReadOnlyList<EffectOutcome> Outcomes)`.
    - `Battle/BattlePhase`: `PartyTurn`, `EnemyTurn`, `Victory`, `Defeat`, `Escaped`.
    - `Battle/BattleState`: a battle in progress (whose turn it is, executing commands, the outcome). `BattleState(IReadOnlyList<Combatant> combatants, IRandom random)`, `Phase`, `CurrentActor`, `Combatants`, `IReadOnlyList<Combatant> LivingOn(BattleSide side)`, `TurnResult Execute(BattleCommand command)` (throws `InvalidOperationException` once the battle is over or for `FleeCommand` on an enemy turn, `ArgumentException` for a dead or same-side attack target).
    - `Battle/EnemyTactics`: chooses the command for an enemy's turn: attack a random living party member. `EnemyTactics(IRandom random)`, `BattleCommand ChooseCommand(BattleState battle)`.
  - **Tests:**
    - `BattleState_Execute_Attack_ReducesTargetHpByFormula`
    - `BattleState_Execute_Attack_EndsTurnAndReportsActor`
    - `BattleState_Execute_LastEnemyDefeated_PhaseIsVictory`
    - `BattleState_Execute_LastPartyMemberDefeated_PhaseIsDefeat`
    - `BattleState_Execute_AttackOnDefeatedTarget_Throws`
    - `BattleState_Execute_AfterBattleEnded_Throws`
    - `BattleState_Execute_Defend_ReturnsNoOutcomesAndEndsTurn`
    - `BattleState_Execute_Flee_PhaseIsEscaped`
    - `BattleState_Execute_FleeOnEnemyTurn_Throws`
    - `EnemyTactics_ChooseCommand_FixedSeed_TargetsLivingPartyMember`

- [ ] **6. Skills and items in battle**
  - **Goal:** Party combatants use skills (spending MP) and consumables (spending inventory copies). Targets are checked against the effect's targeting rule, and outcomes report the amounts actually applied.
  - **New types (Core):**
    - `Skills/SkillDefinition`: content record (`Id`, `Name`, `IconFile`, `MpCost`, `Effect`). One skill. MP is the only skill resource for now; class resources come with the class design.
    - `Effects/TargetRelation`: `Self`, `Ally`, `Enemy`. How a candidate relates to the user.
    - `Effects/TargetRules`: which targets an effect may hit. `static bool CanTarget(Targeting targeting, TargetRelation relation, Vitals candidate)` (fallen-ally targetings only accept fallen candidates; every other targeting only accepts living ones; `Self` only accepts the user), `static bool HitsEveryValidTarget(Targeting targeting)`.
    - `Effects/EffectResolver`: applies an effect's power to one target's vitals and returns the amount actually applied. `static int Apply(Effect effect, Vitals target, int targetDefense)`. Damage goes through `DamageFormula` (placeholder: defense applies to skills and items too). `RestoreHp`, `RestoreMp` and `Revive` go through `Vitals`.
    - `Battle/SkillCommand(SkillDefinition Skill, IReadOnlyList<Combatant> Targets)`, `Battle/ItemCommand(ItemDefinition Item, IReadOnlyList<Combatant> Targets)`.
    - `BattleState` gains an `Inventory` constructor parameter, `bool CanUse(SkillDefinition skill)` (enough MP), and `bool CanUse(ItemDefinition item)` (owned, consumable). Executing a skill without enough MP throws; the UI greys it out.
    - `PartyMember` and `Combatant` gain `IReadOnlyList<SkillDefinition> Skills`. `PartyMember.Recruit` takes the resolved skills.
  - **New types (Data):** `Skills/SkillCatalogValidator` (`MpCost >= 0`, duplicate ids). `PartyMemberDefinition` gains `SkillIds`, checked against skills.
  - **Content:** `content/skills.json` (Fire Blast, Fireball, Heal, Recovery, Heal All; Recovery restores MP). Sarxian's `skillIds` in `party-members.json`.
  - **Tests:**
    - `TargetRules_CanTarget_SingleEnemyOnAlly_ReturnsFalse`
    - `TargetRules_CanTarget_SelfOnOtherAlly_ReturnsFalse`
    - `TargetRules_CanTarget_SingleAllyOnFallenAlly_ReturnsFalse`
    - `TargetRules_CanTarget_SingleFallenAllyOnLivingAlly_ReturnsFalse`
    - `EffectResolver_Apply_Damage_ReturnsDamageAfterDefense`
    - `EffectResolver_Apply_RestoreHpOnFullHpTarget_ReturnsZero`
    - `EffectResolver_Apply_RestoreMp_RestoresMpNotHp`
    - `EffectResolver_Apply_ReviveOnFallenTarget_RestoresHp`
    - `BattleState_Execute_SkillCommand_SpendsMpAndHitsEveryTarget`
    - `BattleState_Execute_SkillCommandWithoutEnoughMp_Throws`
    - `BattleState_Execute_ItemCommand_ConsumesOneCopy`
    - `BattleState_Execute_ItemCommandOnInvalidTarget_Throws`
    - Data: `SkillCatalogValidator_Validate_NegativeMpCost_ThrowsNamingTheSkill`, `PartyMemberCatalogValidator_Validate_UnknownSkillId_ThrowsNamingTheMember`

- [ ] **7. Encounters, rewards and the first simulation**
  - **Goal:** A battle is set up from the active party and a content encounter (the legacy goblin trio), and victory grants experience to the living members. A headless simulation runs many seeded battles and prints a summary.
  - **New types (Core):**
    - `Battle/EncounterDefinition`: content record (`Id`, `IReadOnlyList<string> EnemyIds`, `BackgroundFile`). A group of enemies fought together.
    - `Progression/ExperienceGain`: `record ExperienceGain(PartyMember Member, int Amount, IReadOnlyList<LevelProgressStep> Steps)`.
    - `Battle/BattleRewards`: experience earned from defeated enemies and how it's split. Only living active members receive a share; fallen members get nothing and the remainder of the integer division is dropped (developer decision). `static int TotalExperience(IReadOnlyList<EnemyDefinition> enemies)`, `static int SharePerMember(int totalExperience, int recipientCount)` (pure), `static IReadOnlyList<ExperienceGain> GrantExperience(IReadOnlyList<EnemyDefinition> defeated, IReadOnlyList<PartyMember> activeMembers, ExperienceTable table)`.
    - `BattleState.Start(PartyState party, IReadOnlyList<EnemyDefinition> enemies, Inventory inventory, IRandom random)`: static creation from the active party and an encounter's enemies.
  - **New types (Data):** `Battle/EnemyCatalogValidator` (stats >= 0, `MaxHp >= 1`), `Battle/EncounterCatalogValidator` (unknown enemy id, empty encounter).
  - **Content:** `content/enemies.json` (`goblin`: 40 HP, 10 MP, Attack 20, Defense 2, 40 EXP; placeholder values), `content/encounters.json` (`goblin_trio`: 3 goblins, `Forest.png`).
  - **Simulation:** `tests/Jrpg.Simulations/Battle/GoblinTrioSimulation.cs` with the simulation-only party policy `Battle/AttackFirstLivingEnemyPolicy`. It runs 1,000 seeds over real content (leader alone vs `goblin_trio`, as in the legacy new game) and prints win rate, average turns, and average HP left. No balance target yet: balance isn't designed, and with attack-only play the leader is expected to lose almost every battle (legacy play relied on Fireball). The numbers are a baseline, not a goal.
  - **Tests:**
    - Core: `BattleRewards_SharePerMember_ThreeWaySplit_DropsRemainder`, `BattleRewards_GrantExperience_TwoLivingMembers_EachGainsShare`, `BattleRewards_GrantExperience_FallenMember_GainsNothing`, `BattleState_Start_PartyAndEnemies_CreatesCombatantsOnBothSides`
    - Data: `EncounterCatalogValidator_Validate_UnknownEnemyId_ThrowsNamingTheEncounter`
    - Simulation: `GoblinTrio_AttackFirstLivingEnemy_EveryBattleEndsWithinTurnLimit`
  - **Also:** remove `--ignore-exit-code 8` from `Jrpg.Simulations.csproj`.
  - **Verify by hand:** `dotnet test --project tests/Jrpg.Simulations --output Detailed` prints the summary table.

- [ ] **8. Game session replaces GameSystems**
  - **Goal:** `GameSession.StartNew(catalog)` builds the new-game state (recruited members, the active party with the leader first, the starting inventory) from `content/new-game.json`. Consumables can be used from the menu on party members. Nothing static remains.
  - **New types (Core):**
    - `Session/NewGameDefinition`: content record (`RecruitedMemberIds`, `ActiveMemberIds` in order with the leader first, `IReadOnlyList<InventoryEntry> StartingItems`). The state a new playthrough starts from.
    - `Session/GameSession`: all mutable state of one playthrough, which is what a save will capture in P2. `static GameSession StartNew(ContentCatalog catalog)`, `Party` (`PartyState`), `Inventory`, `Equipment` (`PartyEquipment`).
    - `Items/FieldItemUse`: uses a consumable from the menu on party members. `FieldItemUse(Inventory inventory)`, `bool CanUse(ItemDefinition item)` (owned, `IsUsableFromMenu`, has an effect), `IReadOnlyList<int> Use(ItemDefinition item, PartyMember user, IReadOnlyList<PartyMember> targets)` (amount applied per target). Uses `Effects/TargetRules` and `Effects/EffectResolver`, the same code as battle, without creating combatants. `Self` items only target the user.
    - `ContentCatalog` gains `NewGame`.
  - **New types (Data):** `Session/NewGameValidator` (unknown member or item ids, active members must be recruited, active count within `PartyRules`, quantities >= 1 and within `MaxStack`).
  - **Content:** `content/new-game.json` (all 7 recruited, only Sarxian active, the legacy starting stacks minus Antidote). Test values, to be replaced when the real opening is designed.
  - **Tests:**
    - Core: `GameSession_StartNew_ActiveMembers_FollowDefinitionOrder`, `GameSession_StartNew_StartingItems_FillInventory`, `FieldItemUse_Use_RestoreHpOnLivingMember_RestoresHpAndConsumesOneCopy`, `FieldItemUse_Use_ItemNotUsableFromMenu_Throws`, `FieldItemUse_Use_FallenAllyItemOnLivingMember_Throws`, `FieldItemUse_Use_SelfItemOnOtherMember_Throws`
    - Data: `NewGameValidator_Validate_UnknownItemId_ThrowsNamingTheId`, `NewGameValidator_Validate_TooManyActiveMembers_ThrowsNamingTheField`, `GameSession_StartNew_RepositoryContent_LeaderIsFirstActiveMember`

## Follow-up plans (to be detailed with /plan-feature before starting)
- **Part 2: world and exploration core.** (a) Map model + legacy map/tile/object/tilesheet readers in Data; JSON moves from `Assets/` to `content/maps`, `content/tiles`, `content/objects`. (b) Walkability, `PlayerWalker` movement and polygon collision. (c) `MapEvent` + `EventStep` records + `EventRun` state machine + `WorldState` + `InteractionFinder`, with tile triggers unified into events. (d) Warps (`WarpArrival` nearest-walkable search) and `RandomEncounters` (`IRandom`, rolled per step, rate set per map). (e) Save/load v1 (`SaveGame`, `ISaveStore`, `SaveSlotSummary`, `PlayTime`); legacy saves are not loaded. (f) Cutscene scripts + intro content.
- **Part 3: rendering in Jrpg.Game.** Scene stack, battle screen, map renderer with `DrawOrderKey`, dialogue box, menus (Octopath-style direction). Decide the MonoGame content pipeline here.
- **Part 4: editor port.** Create `src/Jrpg.Editor`. Map editor on MVVM: `EditHistory` (was `UndoManager`), edit commands, flood fill, tile-group object creation, content editors validating through Jrpg.Data. Decide `CommunityToolkit.Mvvm` here.
- **Class design (no plan yet, developer to design).** Class resources replacing MP (Rage, Faith…), stat growth on level-up, experience curves (one shared table or one per class), damage, Defend, escape chance, speed-based turn order. Each replaces a placeholder listed in the guiding rule.

## Decisions (developer, 2026-10-17)
1. **Packages:** approved as listed under "Packages". Content pipeline deferred to part 3, MVVM toolkit to part 4.
2. **Data tests:** add `tests/Jrpg.Data.Tests` and list it in CLAUDE.md.
3. **Content ids:** snake_case.
4. **Starting content:** the demo values are test data, to be replaced later.
5. **Level-up:** placeholder. The level goes up; stat growth waits for the class design.
6. **Experience curve:** a threshold table in content. Per-class curves stay possible as extra tables.
7. **Damage:** legacy formula as a placeholder.
8. **Defend:** placeholder, no effect.
9. **Run:** placeholder, always succeeds. An escape chance (or removing Run) comes later.
10. **Skill without enough MP:** rejected. MP stays minimal; classes will get their own resources.
11. **Healing fallen members:** ordinary heals don't revive. Revive Feather targets fallen allies with a `Revive` effect.
12. **Item descriptions:** descriptions are rewritten to match current effects; Antidote is dropped until status effects exist. All values will be tuned later.
13. **Experience split:** only living members get a share; the remainder is dropped.
14. **Turn order:** shuffled cycle as a placeholder; to be improved later.
15. **Stack size:** `MaxStack` (99) is kept and enforced. Adding fills to the max and reports the leftover.
16. **Party rules:** max 4 active members; the leader stays first.
17. **Goblin stats:** kept for now; revised with battle and class design.
18. **Simulation target:** none. The simulation only checks that every battle ends.
19. **Menu "Self" items:** target only the member using them.
20. **Part 2:** encounters roll per step with a per-map rate; tile triggers and events are unified; legacy saves don't need to load; a found item is added to the inventory. Still open for part 2: dialogue defaults (`"???"` / `Fighter.png`), location names (legacy maps file names to place names in code), number of save slots (legacy has 20).
21. **Equipment model:** move model (equipping takes the copy out of the inventory).

## Risks
- **Scope.** The legacy logic is about twice what this plan holds. This plan stops at a playable headless battle. Part 2 must be planned before the Game can show a map.
- **Placeholders hardening into design.** Placeholder rules get tests, and tests make code feel settled. Each placeholder is named in the guiding rule and in its slice so the class design plan can find and replace them.
- **Shared `Vitals`.** Aliasing between `PartyMember` and `Combatant` is deliberate (battle damage persists), but a future "preview" or "simulate" feature could mutate real party state by accident. Simulations must build fresh members from content, never reuse a session's members.
- **Move model and full stacks.** Unequipping or swapping can fail when the returning item's stack is at 99. `TryEquip`/`TryUnequip` report it; the menu must show a message (P3).
- **Determinism.** Legacy randomness used a `Guid` shuffle, `new Random()` and a static `Random`. Its timing used `DateTime.Now` and `Process.StartTime`. All of that goes through `IRandom` or game-loop delta. Seeded tests depend on the exact Fisher-Yates loop and the order of `IRandom` calls, so changing either re-baselines every seeded expectation.
- **Analyzers with warnings as errors.** CA1724 (type matches namespace) is avoided by the naming above. Watch CA1716 (reserved words in names), CA1002/CA2227 on content records, and the MonoGame project template against `EnforceCodeStyleInBuild`.
- **Strict JSON.** `RespectRequiredConstructorParameters` and `RespectNullableAnnotations` (.NET 9+) make missing fields fail at load time, but the error text must still name the file (`ContentFileReader` wraps it). Range checks stay in validators.
- **Behavior changes from legacy, deliberate and tested.** Outcomes report actual amounts. `RestoreMp` restores MP. Heals don't revive. `Reorder` with a different set of members, or without the leader first, throws instead of silently filtering. An unaffordable skill is rejected. Fallen members get no experience. Each is visible through its test name.

## ADRs
Each is a checkbox step in the slice list, written and accepted before the slice that depends on it.
- **Content schema mapping** (before 3a).
- **Equipment ownership model** (before 4). Outcome already chosen by the developer: move model.
- **Battle command flow** (before 5b).
- *(Part 2)* **Legacy map/tile/object format.** Read the legacy formats as they are or convert once to a new schema. Also: save format versioning. (Unifying tile triggers with events is already decided.)
