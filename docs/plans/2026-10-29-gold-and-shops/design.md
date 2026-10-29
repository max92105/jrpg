# Gold, shops and the inn: design

Part of [the gold and shops plan](README.md).

## Tunables and placeholders

Every value is a placeholder (Q7): the developer tunes them once fights and prices can be compared.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Max gold | `Core/Party/Purse.MaxGold` (constant) | 9,999,999 | a fixed technical limit: seven digits fit every gold field |
| Starting gold | `new-game.json` `startingGold` | 100 | enough to try the shop and the inn on a new game |
| Goblin gold | `drop-tables.json` `goblin.gold` | 6 to 12 | one roll per defeated enemy using the table (the stone beetle too) |
| Sell share | `rules.json` `trade.sellPercent` | 50 | a sale pays this share of the price, rounded down |
| Consumable prices | `items.json` `price` | potion 10, hi_potion 40, elixir 120, camp_kit 60, bomb 30, bomb_all 80, revive_feather 50 | |
| Gear prices | `items.json` `price` | bronze_sword 50, iron_sword 120, magic_staff 90, leather_armor 60, chain_mail 150, wizard_robe 110, wooden_shield 40, leather_cap 30, leather_gloves 30, leather_boots 30, amber_amulet 100, power_ring, sage_ring, swift_ring 120 each | |
| Material prices | `items.json` | none | Scrap and Essence can't be sold (Q4) |
| General store | `shops.json` `general_store` ("General Store") | potion, hi_potion, revive_feather, camp_kit, bronze_sword, leather_cap, leather_boots, wooden_shield, leather_armor | |
| Inn price | `map-layouts/start.json`, the innkeeper's `rest` step | 20 | |
| Currency | `text/ui.json` `gold_amount` | "{amount} G" | Q6 |
| Shop fade | `Ui/OverlayFade` (L1, moved from `Crafting/ForgeMotion`) | in 0.15 s, out 0.12 s | the Forge's and the menu's times (decision 370) |

## Design overview

### What exists
- `Party/PartyState`: members, active order, reserve; built by `GameSession.StartNew` and `Restore`. `Items/Inventory`: stacks (`Add` returns what didn't fit, `HasRoomFor(item, quantity)`, `TryRemove`) and owned pieces (`TryRemoveGear`, `GearCountOf`). No gold anywhere (the demo's `SaveData.Gold` was dropped, decision 103).
- `Session/GameSession` (245): `FinishBattle` returns `BattleSpoils(Rewards, Drops)` (4 test sites build one, all in `VictoryScreenTests`); `SpoilsOf` grants experience, then `_dropRoll.Roll(tables, luck)`, then `Keep` adds what fits. `Capture`/`Restore` map `SaveGame` (required init properties; 4 object initializers in Data tests: `SaveGameFixtures` (2), `SaveGameRepairWornTests`, `SaveGameRepairGearTests`).
- `Data/Saving`: `SaveFile.CurrentFormatVersion = 5`; `FileSaveStore` refuses older and newer formats and null entries (`FileSaveStoreFormatTests`: `Write_File_HoldsTheFormatFiveEnvelope`, `Read_FormatFour_ThrowsOlder`).
- `Loot/DropTable(Id, Entries)`, `DropRoll.Roll` (draws only when uncertain, `RangeRoll.Within` skips the draw on a single value), `Data/Loot/DropTableCatalogValidator`.
- Events: `RestStep` (no fields) rests every member at once in `EventRun.ApplyAndCheckWait` and waits; `Game/Exploration/EventPresenter.ShowRest` fades (decision 257) and shows `event_party_rests`. `OpenForgeStep` waits; `EventPresenter.FollowForgeStep` sets `IsOpeningForge` once per step (4 uses in `EventPresenterForgeTests`); `ExplorationScene` returns `ForgeRequested`; `GameFlow.Start` handles `ForgeRequested`/`ForgeClosed` through `ForgeTransitions` with `Session ?? throw` (four such `??` today).
- `Core/Exploration/ExplorationState` is 296 lines; `EventRun(triggered, map, context, party)` is at four parameters; `ExplorationContext(Catalog, Inventory, World, Random)`.
- `Game/Exploration/EventPresenter` is 258 lines, constructor `(ExplorationState, ScreenText, PlayerSettings, VoiceChannel)` at four (16 `new EventPresenter(` sites, 15 in tests).
- Data: `MapContentReferences` (3 sites) feeds `MapEventValidator`; `ContentTypeDiscriminators` names step types. Editor: `StepChoices(ItemIds, ObjectIds, MapIds, MemberIds)` (3 sites), `EventStepViewModel` (Label/Problem/From/ToStep per kind), `EventPresets`, `EventListViewModel.Apply...Preset`.
- Game UI: `Menus/MenuHeader` (title left; map name and play time right, measured on change); `Battle/VictoryScreenView.RewardRows(VictoryRewards)` returns the reward rows the view loops over (one today: EXP); `Crafting/ForgeScene`, `ForgeList`, `ForgeView`/`ForgeListView`/`ForgeDetailView`, `ForgeMotion` (the third copy of the overlay fade, backlog); `Ui/ConfirmPrompt`/`ConfirmPromptView(text, ui)`, `Ui/NumberText.Of(int)` (no separators), `Ui/AffixText`, `Ui/PointedBarShape` (the victory's Items tabs).

### Core changes
- `Party/Purse` (G1): the party's gold. `int Gold`, `const int MaxGold`, `bool HasRoomFor(int amount)`, `int Add(int amount)` (returns what didn't fit, as `Inventory.Add`), `bool CanSpend(int amount)`, `bool TrySpend(int amount)`. `PartyState.Purse { get; } = new()`.
- `Saving/SaveGame.Gold` (G1, required); `GameSession.Capture` writes it, `Restore` adds it to the purse.
- `Loot/GoldRange(int Min, int Max)` and `DropTable.Gold` (G2, init, null drops none); `DropRoll.RollGold(tables)`; `BattleSpoils(Rewards, Drops, int Gold)` (what went in); `Content/NewGameDefinition.StartingGold` (init, 0).
- `Events/RestStep.Price` (I1, `int?` init, null for a free rest); `Exploration/InnStay` (a priced rest waiting for its answer); `ExplorationState.CurrentInnStay`; `Exploration/EventStarter` (X1, moved out of `ExplorationState`).
- `Items/ItemDefinition.Price` (S1, `int?` init). `Trade/TradeQuote(int Gold, TradeRefusal? Refusal)`, `Trade/TradeRefusal`, `Trade/Shop` (S1), `Trade/TradeRules` + `GameRules.Trade` (S2, init, default refuses every sale), `Trade/PriceFormula`, `Trade/ItemSales` + `GameSession.Sales` (S2), `Trade/ShopDefinition` + `ContentCatalog.Shops` + `GameSession.ShopFor(shopId)` (S3).
- `Events/OpenShopStep(string ShopId)` (E1).

### Structure and folder dependencies

Core (each edge checked against today's `using`s: `Party` uses no `Content`, `Session` or `Exploration`; `Items` and `Loot` use no `Party`, `Trade` or `Content`):

| Folder | Depends on | New |
|---|---|---|
| `Party` | `Items`, `Stats`, ... (none new) | `Purse` |
| `Trade` (new) | `Items`, `Party` (`Purse`) | `ShopDefinition`, `Shop`, `ItemSales`, `TradeQuote`, `TradeRefusal`, `TradeRules`, `PriceFormula` |
| `Loot` | (none new) | `GoldRange` |
| `Content` | `Trade` (new edge: `GameRules.Trade`, `ContentCatalog.Shops`), `Loot`, ... | |
| `Session` | `Trade` (new edge: `Sales`, `ShopFor`), `Party`, `Loot`, ... | |
| `Exploration` | `Party` (exists), `Events` | `InnStay`, `EventStarter` |
| `Events` | (none new) | `OpenShopStep` |
| `Saving` | (none new: gold is an `int`) | |

`Trade` never references `Content` or `Session`: `ShopFor(shopId)` takes the catalog, so it lives in `Session` (the loot retro's cycle rule). The folder is `Trade`, not `Shop` (a folder named after its `Shop` type clashes, CA1724).

Data: `Data/Trade/ShopCatalogValidator` → Core `Trade`, `Items`; Data `Content` (`ContentValidationException`). `ContentCatalogLoader` → `Data/Trade` (the `Content` ↔ feature folder pair `Data/Loot` and `Data/Crafting` already have). `Data/Saving` reads `Purse.MaxGold` (Core `Party`).

Game:

| Folder | Depends on | New |
|---|---|---|
| `Trade` (new) | `Ui`, `Scenes`, `Input`, `Rendering`; Core `Trade`, `Items`, `Party`, `Session` | `ShopScene`, `ShopView`, `ShopListView`, `ShopDetailView`, `StockList`, `StockRow`, `SaleList`, `SaleRow`, `ShopTab`, `TradeRefusalNames` |
| `Flow` | `Trade` (new edge) | `ShopTransitions` |
| `Exploration` | `Ui` (exists) | `RestPlayback` |
| `Scenes` | (none new) | `ShopRequested`, `ShopClosed` |
| `Ui` | never `Trade`, `Crafting`, `Menus`, `Battle` | `GoldText`, `OverlayFade` (from `Crafting/ForgeMotion`), `PieceName` (from `ForgeScene.NameOf`), `ScreenParts` |

`Trade` never references `Crafting` or `Menus`: what it shares with them (the fade, a rolled piece's name, gold text) moves to `Ui/` first, in the slice that makes the second user.

### Key decisions
- **Quotes, not booleans** (the forge review's blocker 3): `TradeQuote` carries the gold an action costs or gives and its refusal; the screen greys and explains from it. Refusal order: buying `StackFull`, then `ShortOfGold`; selling `NotForSale`, then `PurseFull`.
- **`Shop` buys, `ItemSales` sells.** Buying depends on one shop's stock; selling is the same at every shop (one share, every priced item), so it lives on the session (`GameSession.Sales`) and the Shop screen uses both. Each constructor stays at four parameters or fewer: `Shop(ShopDefinition, IReadOnlyList<ItemDefinition> stock, Inventory, Purse)`, `ItemSales(Inventory, Purse, TradeRules)`.
- **Gold rolls after the items** (`DropRoll.RollGold` after `Roll`), so every seeded drop stays as it is; only draws after a won battle shift.
- **What went in**: `BattleSpoils.Gold` is the gold the purse took (past the cap it is lost), as `Drops` lists only the copies that fit.
- **The inn's rule is Core's**: reaching a priced rest doesn't rest; `InnStay.TryPay()` pays and rests (false when short or already paid, changing nothing); continuing without paying leaves the party as it was. A free rest still rests at once (decision 257's flow unchanged).
- **One opening-step signal**: `EventPresenter.IsOpeningForge` becomes `OpeningStep` (the Forge or shop step on the one update it first shows); `ExplorationScene` maps it to `ForgeRequested` or `ShopRequested(shopId)`.
- **`GameFlow.Start` stays under CA1502**: E1 replaces the four `Session ?? throw` with one `RunningSession` property before adding the two shop cases (each `??` counts as a branch).

### Extension points
- A quantity picker (Q3): `Shop.QuoteBuy(item, quantity)` beside the one-copy quote; the screen's prompt grows a count.
- Rarity-priced sales (Q1): one function in `PriceFormula` and a `rules.json` `trade` table.
- Category tabs (Q10): `StockList` and `SaleList` filter as `Menus/InventoryFilter` does.
- Gold from events: a `giveGold` step with `Purse.Add`.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `ExplorationState`'s event starting (cell, touch, arrival, `Start`) | `Exploration/EventStarter` | X1 |
| `EventPresenter.ShowRest` and `_restFade`, `_isRestFadeStarted` | `Exploration/RestPlayback` | I2a |
| `EventPresenter(…, ScreenText text, …)` | `EventPresenter(…, ScreenParts parts, …)` | I2b |
| `EventPresenter.IsOpeningForge` | `EventPresenter.OpeningStep` | E1 |
| `GameFlow.Start`'s four `Session ?? throw` | `RunningSession` | E1 |
| `Crafting/ForgeMotion` | `Ui/OverlayFade` (the Forge and the Shop) | L1 |
| `ForgeScene.NameOf` and `forge_rolled_piece_name` | `Ui/PieceName.Of` and `rolled_piece_name` | T2 |
| `VictoryScreenView.RewardRows(VictoryRewards)` | `RewardRows(BattleSpoils)`: EXP, then gold when any | G3 |
| `FileSaveStoreFormatTests.Read_FormatFour_ThrowsOlder` | `Read_FormatFive_ThrowsOlder` | G1 |

## Risks
- **Save format**: G1 bumps to 6 and refuses format 5 (ADR 0022): the developer's existing saves stop loading. Gold outside 0 to `MaxGold` is refused as unreadable (Core never writes it).
- **Determinism**: gold draws after the items; a seeded test that wins against the goblins and then draws again (a second battle, the Forge) sees different numbers. G2 greps the tests that win against `goblin_trio` and then read the random or the inventory before budgeting fixes (the loot and forge plans budgeted fixes and needed none).
- **`ExplorationState` split (X1)**: a refactor of the event-starting rules (decision 38's touch arming, ADR 0016's arrival); the guard is the unchanged Core exploration tests. If the split doesn't fit one slice, I1 can add its three lines (296 → 299) and the split waits; X1 says so.
- **`EventPresenter` parameter change (I2b)**: 16 constructor sites change mechanically (`new ScreenParts(text, new UiTextures())` in tests).
- **Event flow**: the Shop pauses its event as the Forge does; nothing saves while it is open (`Capture` refuses during an event). A priced rest's question is asked inside the event, so the menu can't open over it.
- **Seeded Game tests on the start map**: N1's shopkeeper must stand off the paths the tests walk (the forge plan's list, plus the forge master at (25, 14)).
- **CA1812**: H1's `ShopScene` is first built by E1's `SceneFactory.CreateShop`; if the analyzer flags it at H1, `CreateShop` lands in H1.
- **Content written by the editor**: a `RestStep` price added by hand before I3 would be dropped by an editor save; I3 lands the field and the innkeeper's price together.
