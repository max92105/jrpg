# GS1. Moves only: `MessageIds` in partial files by feature

Game, about 770 lines moved, about 30 new (eight file heads), no behavior change. Builds on decision 552 and design.md, "Risks" (GS1's size).

## Goal
`Ui/MessageIds.cs` (767 lines, 272 constants) becomes one `internal static partial class MessageIds` over eight files by the screen that shows each id, so a plan adding ids opens the file of its screen. No call site changes: every id stays `MessageIds.X`.

## Files and what each holds
Constants keep their order inside each file; the line numbers are today's.

| File | Holds | From (today's lines) | About |
|---|---|---|---|
| `Ui/MessageIds.cs` | the class summary; startup; the title, the leader choice, the cutscene skip; the menu's title, System, the Yes, No and OK every dialog shares | `StartupFailed`; `GameTitle` … `CinematicSkip`; `MenuTitle` … `NoticeOk` (7, 28-83) | 75 |
| `Ui/MessageIds.Field.cs` | what the map's events say: found items, recruits, rests, the inn, quest notices | `ItemFound` … `MemberAlreadyInParty`; `InnQuestion`, `InnShortOfGold`; `QuestStarted` … `QuestCompleted` (10-26, 724-729, 760-767) | 45 |
| `Ui/MessageIds.StatusPage.cs` | the Status page's labels | `MenuStatus` … `StatusCrowdControl`, `StatusEquipment` … `StatusLastingAmount` (85-139, 156-171) | 80 |
| `Ui/MessageIds.Stats.cs` | names several screens share: damage types, stats, affixes, rarities, slots | `DamageTypeBlunt` … `DamageTypeDark`, `StatHp` … `SlotEmpty` (140-155, 172-262) | 115 |
| `Ui/MessageIds.Menus.cs` | the menu pages: Save and Load, Inventory, Equipment, Party, Options, the menu hints, Quests | `SystemSave` … `TextSpeedInstant`; `MenuHintMain` … `EquipmentTakeHint`; `MenuQuests` … `QuestsObjective` (264-403, 560-597, 730-758) | 210 |
| `Ui/MessageIds.Battle.cs` | the battle screen, its statuses and its outcomes | `BattleAttack` … `BattleLevelUp` (405-559) | 160 |
| `Ui/MessageIds.Crafting.cs` | the Forge, and the rolled piece's name it shares with the Shop | `ForgeTitle` … `ForgeRerolled` (599-668) | 75 |
| `Ui/MessageIds.Trade.cs` | the Shop | `ShopHint` … `ShopSoldCount` (670-723) | 60 |

The table is the plan; where a constant's summary shows it belongs elsewhere (a battle status name a menu also shows, say), it goes by the rule of decision 552 (the screen that shows it, or a shared file), and the Done note names it.

The `<summary>` stays on `MessageIds.cs` only, as on `UiTheme.cs` beside `UiTheme.Battle.cs` and the others: it says where a new id goes (decision 552). Each of the seven other files starts with a `//` comment naming the screens whose ids it holds.

The five comment lines over 120 columns are rewrapped (decision 541): today's lines 15, 350, 440, 443 and 592.

## Changes beside the move
- `docs/map.md`, Ui row: `UiMessages`/`MessageIds` (partial by screen: `.Field`, `.Stats`, `.StatusPage`, `.Menus`, `.Battle`, `.Crafting`, `.Trade`).
- `docs/plans/backlog.md`: delete the `MessageIds` 767 line; the new-game flow's "comment lines over 120 columns" line drops `MessageIds` and keeps the Core files.

## Tests
None new, no test moves. The guard: the `public const string` count, 272 summed over the eight files (`Grep "public const string"` in count mode), recorded in the Done note with each file's first and last constant and its line count; and the runner's totals per project, equal before and after.

## Verify by hand
None beyond the build: a constant left out fails the build at the call site that names it, and a constant doubled across two files fails it at the declaration. Start the game once and open the title and the menu to see the text read as before.

- **Done, with deviations:** as the table says, by today's line ranges; every constant went where the table put it (none moved by the summary rule). The guard: 272 `public const string` over the eight files, 20 + 11 + 26 + 49 + 76 + 50 + 22 + 18; each file's first and last constant and length: `MessageIds.cs` 71 lines (`StartupFailed` … `NoticeOk`), `.Field` 42 (`ItemFound` … `QuestCompleted`), `.StatusPage` 77 (`MenuStatus` … `StatusLastingAmount`), `.Stats` 112 (`DamageTypeBlunt` … `SlotEmpty`), `.Menus` 217 (`SystemSave` … `QuestsObjective`), `.Battle` 164 (`BattleAttack` … `BattleLevelUp`), `.Crafting` 74 (`ForgeTitle` … `ForgeRerolled`), `.Trade` 61 (`ShopHint` … `ShopSoldCount`). The five comments over 120 columns were rewrapped by the fix pass. The runner's total is 4610 before and after; no test moved. The two backlog lines changed as planned.
