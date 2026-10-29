# The forge: design

Part of [the forge plan](README.md).

## Tunables and placeholders

Every value is a placeholder: the loot simulation prints the drop rates, the developer tunes them.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Salvage, Common | `rules.json` `crafting.salvage.common` | 1 scrap | what a Common piece breaks into |
| Salvage, Magic | `crafting.salvage.magic` | 2 scrap | |
| Salvage, Rare | `crafting.salvage.rare` | 2 scrap, 1 essence | Unique has no row: never salvaged (decision 339) |
| Upgrade to Magic | `crafting.upgradeCosts.magic` | 3 scrap | keyed by the tier reached |
| Upgrade to Rare | `crafting.upgradeCosts.rare` | 5 scrap, 1 essence | Rare is the top: no row above it |
| Reroll | `crafting.rerollCost` | 2 scrap, 1 essence | one cost whatever the tier |
| Goblin Scrap | `drop-tables.json` `goblin` | 20 %, 1-2 | |
| Goblin Essence | `drop-tables.json` `goblin` | 5 %, 1 | decision 371 |
| Material stack | `items.json` `scrap`, `essence` | `maxStack` 99 | as every stack today |
| Material color | `Ui/UiTheme.Loot` `RarityMaterial` | `0xFF5FAFA0`, a muted teal | decision 362 |
| Forge fade | `Crafting/ForgeScene` (F1d) | in 0.15 s, out 0.12 s | the menu's frame and closing fades (`Menus/MenuMotion`) |

## Design overview

### What exists
- `Items/Inventory`: stacks (`InventoryEntry`, capped at `MaxStack`) and owned gear as a list of `GearPiece`s in pickup order; `HasRoomFor(item)` checks room for one more copy; `TryRemoveGear` takes the first equal piece. Worn pieces are on the members, never in `Gear`.
- `Loot/GearRoll` (internal): rolls a base item into a piece (rarity by weight, affix count, affixes by weight, never one twice, decision 359); every draw skipped when only one outcome is possible. After the affix stats plan's A4a: `Roll(item, luckPermille)`, luck touching the rarity only.
- `Session/GameSession.DropRollFor(catalog, random)` (internal static) builds the only `GearRoll`, pool in id order, also used by `tests/Jrpg.Simulations/Loot/LootSimulation.cs:28`.
- `Content/GameRules(Party, Experience, Exploration, Stats, Loot)`: a positional record read from `rules.json`; `Data/Content/GameRulesValidator.Validate(fileName, rules)` sees no items. `ContentCatalogLoader.Load` reads `rules.json` before `items.json`, and calls `DropTableCatalogValidator` with `itemsById` once the items are loaded.
- Events: a waiting step returns `true` from `EventRun.ApplyAndCheckWait`; `Game/Exploration/EventPresenter.Update` returns nothing and shows the current step (`RestStep` keeps a once flag, `_isRestFadeStarted`); only `ExplorationScene.Update` returns requests, and returns null while `_presenter.IsBusy`. `GameFlow.Start` handles `MenuRequested`/`MenuClosed` without a fade (`MenuTransitions`); any other request goes behind the fade, and `ApplyRequestOnceBlack` throws on one it doesn't know. `BattleTransitions` already calls into the session.
- `Data/Content/ContentTypeDiscriminators` names each step type (`"rest"`); `Data/Maps/MapEventSteps` only lists steps for the art and audio checks.
- Editor: `EventStepKind`, `EventStepViewModel.Label`/`From`/`ToStep` per kind (no resources file: labels are C# strings), `EventViewModel.AddX` commands with buttons in `ObjectEventsWindow.xaml` and `CellEventWindow.xaml`, `EventPresets` and `EventListViewModel.ApplyXPreset` commands.
- Game UI: `Menus/InventoryRow` (`Rarity` init) drawn by `InventoryListView.TextColorOf` (rarity color, else greyed when unusable); `Battle/VictoryDropRow` drawn in `UiTheme.RarityColorOf(row.Rarity)` (`VictoryDropsView.cs:103`); `Menus/SheetRowLook` reads its sizes from `StatusSheetView`.

### Core changes
- `Items/ItemDefinition.IsMaterial` (M1, init, false): a material stacks, has no effect, isn't equipment, isn't used from the menu. Material is not a `Rarity` (decision 339); the Game reads `IsMaterial` for the color.
- `Items/Inventory.HasRoomFor(ItemDefinition item, int quantity)` (M2): room for that many more copies (decision 358's whole yield); `Replace(GearPiece old, GearPiece replacement)` (M3a): in place, at the first equal piece; false when none.
- `Crafting/CraftingRules` (M2, M3a, M3b): `Salvage` by rarity, `UpgradeCosts` by the tier reached, `RerollCost`; init properties defaulting to empty, so `GameRules` gains `Crafting { get; init; } = new()` without touching its constructor's call sites.
- `Crafting/ForgeRefusal` (M2, M3a, M3b): why an action is refused: `NoYield` and `StackFull` (M2), `HighestRarity`, `NoAffixLeft`, `ShortOfMaterials` (M3a), `NoAffixes` (M3b).
- `Crafting/ForgeQuote(IReadOnlyList<InventoryEntry> Materials, ForgeRefusal? Refusal)` (M2): one action as offered: the yield or cost, and its refusal (null when it can be done).
- `Crafting/Forge` (M2, M3a, M3b): public, internal constructor (`GearRoll` is internal). `QuoteSalvage`/`TrySalvage`, `QuoteUpgrade`/`TryUpgrade`, `QuoteReroll`/`TryReroll`. A `Try` is false when the piece isn't owned or its quote is refused, and then changes nothing.
- `Loot/GearRoll` (M3a, M3b): the affix draw moves out of `Roll` into a private `DrawAffixes(kind, count, kept)` that `Roll`, `AddAffix` and `Reroll` share; `HasAffixLeftFor(piece)` draws nothing.
- `Session/GameSession`: `Forge` (M2); `GearRollFor(catalog, random)` replaces `DropRollFor` (M3a), one `GearRoll` for the drops and the Forge.
- `Events/OpenForgeStep` (E1): waits, like a dialogue step, until `ContinueEvent`.

### Structure and folder dependencies

Core (every edge checked against the code: `Items` and `Loot` don't reference `Content`, `Session` or `Crafting`):

| Folder | Depends on | New |
|---|---|---|
| `Crafting` | `Items`, `Loot` | `CraftingRules`, `Forge`, `ForgeQuote`, `ForgeRefusal` |
| `Content` | `Crafting` (new edge: `GameRules.Crafting`), `Loot`, ... | |
| `Session` | `Crafting` (new edge: `GameSession.Forge`), `Loot`, ... | |
| `Events` | (none new) | `OpenForgeStep` |

A method that takes `ContentCatalog` stays in `Session` (`GearRollFor`), never in `Crafting` (the loot retro's cycle rule).

Data: `Data/Crafting/CraftingRulesValidator` → Core `Crafting`, `Items`; Data `Content` (`ContentValidationException`, `RangeProblems`); `Data/Content/ContentCatalogLoader` → `Data/Crafting`. The `Content` ↔ feature folder pair is the one `Data/Loot` already has.

Game:

| Folder | Depends on | New |
|---|---|---|
| `Crafting` | `Ui`, `Scenes`, `Input`, `Rendering`; Core `Crafting`, `Items`, `Session` | `ForgeScene`, `ForgeView` (F1d: `ForgeListView`, `ForgeDetailView`), `ForgeList`, `ForgeRow`, `MaterialStock`, `MaterialCount`, `ForgeActionChoice`, `ForgeAction`, `ForgeActionRow`, `MaterialListText`, `ForgeRefusalNames` |
| `Flow` | `Crafting` (new edge) | `ForgeTransitions` |
| `Exploration` | `Scenes` (exists) | |
| `Scenes` | (none new) | `ForgeRequested`, `ForgeClosed` |
| `Ui` | never `Crafting`, `Menus` | `SheetRowLook` moves here from `Menus` (F1c) |

The Game folder is `Crafting/` as Core's (a folder named `Forge` would clash with Core's `Forge` type, CA1724). `Crafting` never references `Menus`: the one piece it shares with the pages, `SheetRowLook`, moves to `Ui/` first.

### Key decisions
- **Quotes, not booleans** (review blocker 3): the screen needs the cost, the yield and the reason; one `ForgeQuote` per action carries all three, worked out in Core and tested there. Refusal order: salvage `NoYield`, then `StackFull`; upgrade `HighestRarity`, then `NoAffixLeft`, then `ShortOfMaterials`; reroll `NoAffixes`, then `ShortOfMaterials`.
- **The piece is replaced in place** (`Inventory.Replace`), so it keeps its row. With two equal pieces, the first one changes (as `TryRemoveGear` takes the first); the screen reads `Gear.IndexOf(old)` before the action and puts the cursor there (F2c), so it always highlights the piece that changed. The Forge lists `Inventory.Gear` in order, so a row index is a gear index.
- **Upgrade adds one affix** (351, clarified): Common to Magic with one, Magic to Rare with a second; the rarity table's affix counts are for drops only.
- **Reroll is a fresh draw** (352): the same count, the same rarity, never one affix twice, from every affix for the kind (the old ones included).
- **The Forge opens and closes without the flow's fade** (370), as the menu does: `ForgeTransitions.Open` pushes it, `Close` pops it and continues the event (`session.Exploration.ContinueEvent()`).

### Extension points
- A worn piece at the Forge (369) is a list change in `ForgeList` and an owned check in `Forge`.
- Unique items: a `crafting.salvage.unique` row and an upgrade rule when the plan exists; `NoYield` already refuses them.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `GameSession.DropRollFor(catalog, random)` | `GameSession.GearRollFor(catalog, random)`; the session builds its `DropRoll` and `Forge` from it; `LootSimulation` too | M3a |
| `GearRoll.Roll`'s affix loop | private `GearRoll.DrawAffixes`, shared with `AddAffix` and `Reroll` | M3a |
| `InventoryListView.TextColorOf` (rarity, else grey when unusable) | the material color first, then as before | M1 |
| `VictoryDropsView.DrawRow`'s `RarityColorOf(row.Rarity)` | the material color for a material stack | M1 |
| `Menus/SheetRowLook` (sizes from `StatusSheetView`) | `Ui/SheetRowLook` owning its sizes | F1c |
| `ForgeView` (plain, F1a-F1c) | `ForgeListView` + `ForgeDetailView` | F1d |

## Risks
- **The affix stats plan's A4a must land first**: M3a edits `GearRoll` and `GameSession` around A4a's luck parameter and the simulation's luck table. If A4a's shape differs, M3a's call-site list changes, not its design.
- **Determinism**: the Forge draws from the session's random, so crafting shifts the draws of later drops and battles; still seeded and repeatable. Refusals draw nothing (every check before the draw).
- **Seeded tests**: M1 adds two entries to the goblin table, so a seeded goblin victory draws twice more. The loot plan's D2 and R3b budgeted fixes and needed none; M1 greps the tests that win against `goblin_trio` and then read the random or the inventory.
- **Event flow**: the Forge pauses the event at its step; the exploration scene doesn't update under it (`SceneStack` updates the top only). Two `openForge` steps in a row would need the presenter's once flag reset between them; it resets whenever the current step isn't an `OpenForgeStep`, so only back-to-back steps would merge (no content does that; E2's preset never makes it).
- **Saving**: `GameSession.Capture` refuses while an event runs, so nothing can save while the Forge is open (by design, ADR 0007); the menu can't open over the Forge either.
- **Save format**: none. A crafted Rare with two affixes, or a reroll's lines, are valid pieces for `GearRepair`.
- **N1's placement**: the Game tests walk set paths on the start map (row 3 right of (10, 3); row 14 left to the forest warp; the door at (5, 14); the chest at (19, 21); the villager at (12, 4); the innkeeper at (30, 26)); the forge master must stand off them.
- **CA1812** (an internal class nothing in its assembly builds): F1a's `ForgeScene` is first built by E1's `SceneFactory.CreateForge`. If the analyzer flags it at F1a, `CreateForge` lands in F1a instead.
