# Gold, shops and the inn: design

Part of [the gold and shops plan](README.md).

## Tunables and placeholders

Every value is a placeholder (decision 381): the developer tunes them once fights and prices can be compared.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Max gold | `Core/Party/Purse.MaxGold` (constant) | 9,999,999 | seven digits fit every gold field; gold past it is lost |
| Starting gold | `new-game.json` `startingGold` | 100 | enough to try the shop and the inn on a new game |
| Goblin gold | `enemies.json` `goblin.gold` | 6 to 12 | one draw per defeated goblin (decision 376) |
| Stone beetle gold | `enemies.json` `stone_beetle.gold` | 6 to 12 | the practice enemy, as the goblin |
| Training dummy gold | `enemies.json` | none | no draw |
| Sell share | `rules.json` `trade.sellPercent` | 50 | a sale pays this share of the price, rounded down |
| Consumable prices | `items.json` `price` | potion 10, hi_potion 40, elixir 120, camp_kit 60, bomb 30, bomb_all 80, revive_feather 50 | |
| Gear prices | `items.json` `price` | bronze_sword 50, iron_sword 120, magic_staff 90, leather_armor 60, chain_mail 150, wizard_robe 110, wooden_shield 40, leather_cap 30, leather_gloves 30, leather_boots 30, amber_amulet 100, power_ring, sage_ring, swift_ring 120 each | a piece's base price |
| Material prices | `items.json` | none | Scrap and Essence can't be sold (decision 378) |
| Affix values | `affixes.json` `goldValue` | 20 each | added to a rolled piece's price (decision 375) |
| General store | `shops.json` `general_store` ("General Store") | potion, hi_potion, revive_feather, camp_kit, bronze_sword, leather_cap, leather_boots, wooden_shield, leather_armor | the gear entries roll into pieces (decision 385) |
| Inn price | `map-layouts/start.json`, the innkeeper's `rest` step | 20 | |
| Coin icon | `content/art/icons/Gold.png` | a copy of `Item.png` | until the developer draws one (decision 380) |
| Quantity steps | `Trade/QuantityPicker` | 1 on Up and Down, 10 on Left and Right | decision 377 |
| Overlay fade | `Ui/OverlayFade` (L1, from `Crafting/ForgeMotion`) | in 0.15 s, out 0.12 s | the Forge's and the menu's times (decision 370) |

## Design overview

### What exists
- `Party/PartyState`: members, active order, reserve, the fixed leader (decision 107). `Items/Inventory`: stacks (`Add` returns what didn't fit, `HasRoomFor(item, quantity)`, `TryRemove`, `QuantityOf`) and owned pieces (`TryRemoveGear`, `GearCountOf`). No gold anywhere (the demo's `SaveData.Gold` was dropped, decision 103).
- `Session/GameSession` (245): `FinishBattle` returns `BattleSpoils(Rewards, Drops)` (4 test sites build one, all in `VictoryScreenTests`); `SpoilsOf` grants experience, then `_dropRoll.Roll(tables, luck)`, then `Keep`. One `GearRoll` from `GearRollFor(catalog, random)` (pool in id order) serves the drops and the Forge; `GearRoll.Roll(item, luckPermille)`. `Capture`/`Restore` map `SaveGame` (required init properties; 4 object initializers in Data tests: `SaveGameFixtures` (2), `SaveGameRepairWornTests`, `SaveGameRepairGearTests`).
- `Data/Saving`: `SaveFile.CurrentFormatVersion = 5`; `FileSaveStore` refuses older and newer formats and null entries; `SaveGameRepair` with `GearRepair` (drops a piece whose base is gone, clamps affix values, decision 360) and `WorldStateRepair`.
- `Battle/EnemyDefinition` (`ExperienceReward`, `DropTableId`); `Battle` and `Loot` reference each other in neither direction. `Loot/AffixDefinition` (3 `new AffixDefinition` sites in tests; 23 affixes in content). `Randomness/RangeRoll.Within` draws only when the range holds more than one value.
- Events: `RestStep` (no fields) rests every member at once in `EventRun.ApplyAndCheckWait`; `EventPresenter.ShowRest` fades (decision 257). `OpenForgeStep` waits; `EventPresenter.IsOpeningForge` (4 uses in `EventPresenterForgeTests`); `ExplorationScene` returns `ForgeRequested`; `GameFlow.Start` handles `ForgeRequested`/`ForgeClosed` through `ForgeTransitions`, with four `Session ?? throw`. `ExplorationState` is 296 lines; `EventRun` and `EventPresenter` constructors are at four parameters (16 `new EventPresenter(` sites, 15 in tests).
- Data: `MapContentReferences` (5 positional members; 5 construction sites: `ContentCatalogLoader`, `ContentWriter.ContentReferencesOf`, `MapLayoutValidatorTests`, `MapFolderReaderTests`, `MapEventValidatorTests`) feeds `MapEventValidator.FindStepProblem`. Editor: `StepChoices` (4 positional members, 3 sites), `EventStepViewModel`, `EventPresets`, `EventListViewModel.Apply...Preset`.
- Game UI: `Menus/MenuHeader` (title; map name and play time at the right); `Battle/VictoryScreenView.RewardRows(VictoryRewards)`; the Forge (`ForgeScene`, `ForgeList`, `ForgeView`/`ForgeListView`/`ForgeDetailView`, `ForgeMotion`, `ForgeClosed`); `Menus/CategoryRail`/`CategoryRailView`/`InventoryFilter`/`InventoryCategory`, `Menus/EquipmentComparison` (`ShowChoice`, `ShowWorn`) and `EquipmentChangesView`; `Ui/ConfirmPrompt`/`ConfirmPromptView`, `HeaderBar`, `FooterBar`, `ColumnPanelView`, `ColumnTitles`, `NumberText`, `AffixText`, `PointedBarShape`, `UiArtFiles.Icons` (checked by `UiArtFilesTests`). `GameButton.PreviousTab`/`NextTab` have controller bindings only.

### Core changes
- `Party/Purse` (G1) on `PartyState.Purse`.
- `Saving/SaveGame.Gold` and `ShopStocks` (G2), `Trade/SavedShopStock` (G2).
- `Battle/GoldRange` and `EnemyDefinition.Gold` (G3); `BattleSpoils(Rewards, Drops, Gold)`; `Content/NewGameDefinition.StartingGold` (G3).
- `Items/ItemDefinition.Price` (S1); `Trade/ShopDefinition`, `ShopStock`, `Shop`, `TradeQuote`, `TradeRefusal` (S1); `Loot/AffixDefinition.GoldValue`, `Trade/Pricing`, `Trade/PriceFormula` (S2); `Trade/TradeRules`, `GameRules.Trade`, `Trade/ItemSales`, `GameSession.Sales` (S3); `ContentCatalog.Shops` (S4); `Session/ShopStocks`, `GameSession.ShopFor` (S5); stocks in `Capture`/`Restore` (S6).
- `Events/OpenShopStep(ShopId)` (E1). `Events/RestStep.Price`, `Exploration/InnStay`, `ExplorationState.CurrentInnStay` (I1).

### Structure and folder dependencies

Core (each edge checked against today's `using`s):

| Folder | Depends on | New |
|---|---|---|
| `Party` | `Items`, `Stats`, ... (none new) | `Purse` |
| `Trade` (new) | `Items`, `Party` (`Purse`) | `SavedShopStock`, `ShopDefinition`, `ShopStock`, `Shop`, `ItemSales`, `TradeQuote`, `TradeRefusal`, `TradeRules`, `Pricing`, `PriceFormula` |
| `Battle` | (none new) | `GoldRange` |
| `Loot` | (none new) | (`AffixDefinition.GoldValue`) |
| `Saving` | `Trade` (new edge: `SavedShopStock`) | |
| `Content` | `Trade` (new edge: `GameRules.Trade`, `ContentCatalog.Shops`) | |
| `Session` | `Trade` (new edge), `Loot`, `Party`, ... | `ShopStocks` |
| `Exploration` | `Party` (exists), `Events` | `InnStay` |
| `Events` | (none new) | `OpenShopStep` |

`Trade` never references `Content`, `Session` or `Loot`: rolling stocks and building `Pricing` take the catalog and the `GearRoll`, so they live in `Session` (the loot retro's cycle rule); `Pricing` takes the affixes' values as a plain id-to-gold map. The gold range is `Battle`'s (the enemy's), and the session rolls it: `Loot` doesn't gain a `Battle` edge. The folder is `Trade`, not `Shop` (CA1724).

Data: `Data/Trade/ShopCatalogValidator` → Core `Trade`, `Items`; Data `Content`. The gold bounds (`Data/Items`, `Data/Battle`, `Data/Loot`, `Data/Content`, `Data/Saving`, `Data/Maps`) read `Purse.MaxGold` from Core `Party`. `Data/Saving/ShopStockRepair` → Core `Trade`, `Content`; Data `Saving` (`GearRepair`'s affix rule). `ContentCatalogLoader` → `Data/Trade` (the `Content` ↔ feature folder pair `Data/Loot` and `Data/Crafting` already have).

Game:

| Folder | Depends on | New |
|---|---|---|
| `Trade` (new) | `Ui`, `Scenes`, `Input`, `Rendering`; `Menus` (the frame's `MenuHeader.Height` and `MenuLayout` as `Crafting` reads them, `CategoryRail`/`CategoryRailView`/`InventoryFilter`, `EquipmentComparison`/`EquipmentChangesView`); Core `Trade`, `Items`, `Party`, `Session` | `ShopScene`, `ShopView`, `ShopListView`, `ShopDetailView`, `StockList`, `StockRow`, `SaleList`, `SaleRow`, `ShopTab`, `QuantityPicker`, `TradeRefusalNames` |
| `Menus` | never `Trade` | (`EquipmentComparison.ShowPiece`, `InventoryFilter.Shows`) |
| `Flow` | `Trade` (new edge), `Crafting` | `EventScreenTransitions` (replaces `ForgeTransitions`) |
| `Scenes` | (none new) | `ShopRequested`, `EventScreenClosed` (replaces `ForgeClosed`) |
| `Exploration` | `Ui` (exists) | `RestPlayback`, `EventLook` |
| `Ui` | never `Trade`, `Crafting`, `Menus`, `Battle` | `OverlayFade` (from `Crafting/ForgeMotion`), `GoldAmountView`, `PieceName` (from `ForgeScene.NameOf`); `ConfirmPrompt.Price` |

`Trade` never references `Crafting`: what the Forge and the Shop share (the fade, the close request, a rolled piece's name) moves to `Ui/` or `Scenes/` in the slice that makes the second user.

### Key decisions
- **Quotes, not booleans** (the forge review's blocker 3): `TradeQuote(Gold, Refusal?)` carries what a trade costs or gives and why it's refused; `Shop.MostBuyable` and `ItemSales.MostSellable` are the picker's limits. Refusal order: buying `StackFull`, then `ShortOfGold`; selling `NotForSale`.
- **`Shop` buys, `ItemSales` sells.** Buying depends on one shop's stock; selling is the same at every shop, so it lives on the session. `Shop(ShopStock, Inventory, Purse, Pricing)`, `ItemSales(Inventory, Purse, Pricing)`: four parameters at most.
- **What a thing is worth** is `Pricing`'s: an item's price; a piece's base price plus its affixes' gold values (decision 375); a sale's share of either (`PriceFormula`, one `long`, rounded down once).
- **The stock** (decision 385): `Session/ShopStocks` keeps every opened shop's `ShopStock` by id; `GameSession.ShopFor(id)` rolls a missing or stale one (the leader's level above the one it was rolled at), each gear entry of the list in order through the shared `GearRoll` with no luck; stacks are resolved, never rolled. A bought piece leaves the stock. Saved as `SavedShopStock(ShopId, RolledAtLevel, Pieces)`.
- **Gold rolls after the items**, one `RangeRoll.Within` per defeated enemy with a range, so every seeded drop stays as it is; `BattleSpoils.Gold` is what the purse took (past the cap it is lost).
- **The inn's rule is Core's**: a priced rest doesn't rest on arrival; `InnStay.TryPay()` pays and rests (false and nothing changed when short or already paid); continuing without paying leaves the party as it was. A free rest still rests at once.
- **One opening-step signal and one way back**: `EventPresenter.IsOpeningForge` becomes `OpeningStep`; `ExplorationScene` maps it to `ForgeRequested` or `ShopRequested(shopId)`; both screens return `EventScreenClosed`, and `EventScreenTransitions` (`OpenForge`, `OpenShop`, `Close`) pops and continues the event. `GameFlow.Start` uses one `RunningSession` helper instead of four `Session ?? throw` (CA1502).
- **The Shop's controls**: the tabs first (Left and Right choose, Confirm enters the list); in the list Up and Down move, the tab buttons change the category, Left and Right switch the previewed member on a gear row, Confirm buys or sells (a stack through the picker: Up and Down by one, Left and Right by ten); Cancel goes back one level: the prompt, the picker, the list, then the Shop.

### Extension points
- The whole party's preview (the reference): `L2b`'s one-member preview repeated per active member.
- A shop-specific price: `Pricing` takes a shop's markup.
- Gold from events: a `giveGold` step with `Purse.Add`.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `VictoryScreenView.RewardRows(VictoryRewards)` | `RewardRows(BattleSpoils)`: EXP, then gold when any | G4 |
| `FileSaveStoreFormatTests.Read_FormatFour_ThrowsOlder` | `Read_FormatFive_ThrowsOlder` | G2 |
| `Crafting/ForgeMotion` | `Ui/OverlayFade` (the Forge and the Shop) | L1 |
| `Scenes/ForgeClosed` | `Scenes/EventScreenClosed` | L1 |
| `Flow/ForgeTransitions` | `Flow/EventScreenTransitions` (`OpenForge`, `OpenShop`, `Close`) | E1 |
| `EventPresenter.IsOpeningForge` | `EventPresenter.OpeningStep` | E1 |
| `GameFlow.Start`'s four `Session ?? throw` | `RunningSession` | E1 |
| `InventoryFilter.Keeps` (private) | `InventoryFilter.Shows` (internal, the Shop's second user) | B3 |
| `ForgeScene.NameOf` and `forge_rolled_piece_name` | `Ui/PieceName.Of` and `rolled_piece_name` | T3 |
| `EventPresenter.ShowRest` and `_restFade`, `_isRestFadeStarted` | `Exploration/RestPlayback` | I2a |
| `EventPresenter(…, ScreenText text, …)` | `EventPresenter(…, EventLook look, …)` | I2b |

## Risks
- **Save format**: G2 bumps to 6 and refuses format 5 (ADR 0022): the developer's saves stop loading. Gold outside 0 to `MaxGold` is refused as unreadable. `ShopStocks` is in the shape from G2 but written empty until S6 (nothing rolls a stock before S5).
- **Determinism**: gold draws after the items; a stock rolls when a shop first opens. A seeded test that wins against the goblins and then draws again sees different numbers: G3 greps the tests that win against `goblin_trio` and then read the random or the inventory before budgeting fixes (the loot and forge plans budgeted fixes and needed none).
- **`ExplorationState` at 299 after I1**: any more lines there wait for the quests plan's split (its R1).
- **`EventPresenter` parameter change (I2b)**: 16 constructor sites change mechanically.
- **Event flow**: the Shop pauses its event as the Forge does; nothing saves while it is open (`Capture` refuses during an event), so a stock rolled at opening is saved at the next save after the event ends.
- **The detail column's height** (L2a, L2b): name, description, owned, affixes, the member's preview and the reason line must fit 848 px; L2b writes the arithmetic.
- **Seeded Game tests on the start map**: N1's shopkeeper must stand off the paths the tests walk.
- **CA1812**: H1's `ShopScene` is first built by E1's `SceneFactory.CreateShop`; if the analyzer flags it at H1, `CreateShop` lands in H1.
- **Content written by the editor**: a `RestStep` price added by hand before I3 would be dropped by an editor save; I3 lands the field and the innkeeper's price together. `ContentWriter.ContentReferencesOf` must pass the shop ids (E1), or the editor's map save refuses a map with an `openShop` step.
