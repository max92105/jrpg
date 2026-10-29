# Gold, shops and the inn
Status: draft

Open question 222 (paying for rest, from the class mechanics plan) answered as a feature on 2026-10-29: the developer wants gold, shops and a priced inn. Follows [the forge plan](../done/2026-10-27-forge/README.md), whose Forge screen (opened by an `openForge` step from a forge master NPC) is the model for the Shop screen a shopkeeper opens.

Numbers: local only. Open questions are Q1, Q2... below; decisions this draft takes are listed unnumbered under "Decisions (proposed, numbered at approval)". The real numbers come at approval.

## Goal
1. **Gold**: the party owns gold (`Party/Purse`), kept by saves (format 6, ADR 0022); defeated enemies drop it (a range on their drop table, rolled after the items) and the victory screen shows it; the menu header shows what is owned; a new game starts with some.
2. **The inn**: the existing `RestStep` gains an optional price; a priced rest asks "Rest for 20 G?", Yes pays and rests, and without the gold a message says so and nothing changes.
3. **Shops**: prices on items (`items.json` `price`), shop inventories in `shops.json` (a name and the item ids it sells), the sell share in `rules.json` `trade`; Core's `Shop` (buying) and `ItemSales` (selling) quote every action with its refusal (short of gold, stack full, not for sale, purse full), as the Forge's quotes do.
4. **At a shopkeeper**: an `openShop` event step names a shop and opens the Shop screen over the map (editor support, a Shopkeeper preset, one placeholder shopkeeper on the start map); Buy and Sell tabs in the game's look (`docs/ui-reference/shop-sell.webp`), a refused row greyed with its reason.

## Order, and why
1. **Phase 1: gold (G1, G2, G3).** The purse and its save shape first (the loot retro: the save shape lands once, before the slice that makes the data appear in play), then gold from battles and a new game, then where the player sees it.
2. **Phase 2: the inn (X1, I1, I2a, I2b, I3).** The smallest gold sink, and it only needs Phase 1. `ExplorationState` (296 lines) is split first, as the backlog asks; the Core rule, then the Game's question (after moving the rest's showing out of the 258-line `EventPresenter`), then the editor and the innkeeper's price (content with a new field only once the editor keeps it).
3. **Phase 3: shops in Core (S1, S2, S3).** Buying, selling, then the content and its checks.
4. **Phase 4: the shop on the map (H1, E1, E2, N1).** A bare Shop screen, the step that opens it, the editor support, then the shopkeeper: from N1 on every shop slice is checked by hand (the forge review's blocker 4).
5. **Phase 5: what the Shop shows (B1, B2, L1, L2a, L2b).** The Buy list, the Sell list and the tabs, the shared overlay fade, then the look.
6. **Phase 6: trading (T1, T2).** Buying, then selling (a piece behind a prompt).

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0030's pattern): prices, the sell share, gold ranges, starting gold and the inn's price are content; every number is a placeholder (design.md, "Tunables").
- **Every "can it be done" rule is Core's** (the forge review): `Trade/Shop` and `Trade/ItemSales` return a `TradeQuote` (gold and `TradeRefusal?`) per action; `Exploration/InnStay` says what a rest costs and whether it can be paid. The screens format them and never decide.
- **A refusal changes nothing**: every check runs before gold or an item moves.
- **Pure presenter + thin view** (`docs/map.md`); one selection look (decision 252), every list through `SelectionMotion`; shared pieces in `Ui/` (gold text, the overlay fade, a rolled piece's name), never `Trade` → `Crafting` or `Menus`.
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x; Editor slices aim at about 200 lines.
- **Files near 300 lines:** split first or add sibling test files. Split in this plan: `ExplorationState` (296, X1), `EventPresenter` (258, I2a). New test files beside `EventPresenterTests` (305), `GameFlowTests` (321), `EventRunTests` (269), `ContentCatalogLoaderTests` (272), `GameSessionSaveTests` (270), `EventStepViewModelTests` (260), `EventListViewModelTests` (258), `ItemCatalogValidatorTests` (248). Watched: `VictoryScreenView` 263, `GameFlow` 254, `GameSession` 245, `SaveGameRepair` 221, `ForgeScene` 233, `Ui/MessageIds` 648 (constants; backlog says it can wait).
- **Save format change:** one, in G1 (format 5 → 6, gold); an older save is refused (ADR 0022). **No NuGet package.**

## Out of scope (follow-ups)
- **The party's stat preview in the shop** (the reference's right column, Q9): the detail column shows the item's description, owned count and a piece's affixes.
- **Category tabs in the shop** (the reference's All / Items / Weapons rail, Q10) and a **quantity picker** (Q3).
- **Selling worn gear**: only owned items and pieces are sold, as the Forge works owned pieces only (decision 369).
- **Gold in the save list** (`Saving/SaveSlotList`) and **gold from map events** (a `giveGold` step): no caller yet.
- **Unique items** in shops (decision 339): no Unique exists.
- **Story and maps**: the developer places the NPCs; N1's shopkeeper is a placeholder he moves.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: gold
- [ ] **G1. The party's purse, kept by saves (format 6)** (Core + Data, about 210 lines) → [slices/G1.md](slices/G1.md)
- [ ] **ADR: gold and trade** (where gold drops are defined, prices on items with shop listings, the sell share; from the developer's answers to Q1, Q2, Q4), before G2
- [ ] **G2. Enemies drop gold into the purse; a new game starts with some** (Core + Data + content, about 235 lines) → [slices/G2.md](slices/G2.md)
- [ ] **G3. Gold on the victory screen and in the menu header** (Game + text, about 130 lines; adds no view type) → [slices/G3.md](slices/G3.md)

### Phase 2: the inn
- [ ] **X1. Split `ExplorationState`: starting events into `EventStarter`** (Core refactor, about 120 lines moved, tests unchanged) → [slices/X1.md](slices/X1.md)
- [ ] **I1. A priced rest waits for payment: `InnStay`** (Core + Data, about 190 lines) → [slices/I1.md](slices/I1.md)
- [ ] **I2a. Move the rest's showing into `RestPlayback`** (Game refactor, about 110 lines, tests unchanged) → [slices/I2a.md](slices/I2a.md)
- [ ] **I2b. The inn asks with its price; Yes pays and rests; short of gold, a message says so** (Game + text, about 220 lines; draws an existing view) → [slices/I2b.md](slices/I2b.md)
- [ ] **I3. The editor's rest step takes a price; the innkeeper charges** (Editor + content, about 150 lines) → [slices/I3.md](slices/I3.md)

### Phase 3: shops in Core
- [ ] **S1. Buying in Core: prices on items, `Shop`, its quotes and refusals** (Core, about 210 lines) → [slices/S1.md](slices/S1.md)
- [ ] **S2. Selling in Core: the sell share, `ItemSales`** (Core, about 230 lines) → [slices/S2.md](slices/S2.md)
- [ ] **S3. Shops, prices and the sell share in content, checked by the loader** (Core + Data + content, about 245 lines) → [slices/S3.md](slices/S3.md)

### Phase 4: the shop on the map
- [ ] **H1. A Shop screen that opens over the map and Cancel closes** (Game + text, raw 90, about 145 lines at 1.6x) → [slices/H1.md](slices/H1.md)
- [ ] **E1. The `openShop` event step opens the named shop and waits for it** (Core + Data + Game, about 240 lines) → [slices/E1.md](slices/E1.md)
- [ ] **E2. The editor shows and adds the `openShop` step; the Shopkeeper preset** (Editor, about 190 lines) → [slices/E2.md](slices/E2.md)
- [ ] **N1. A shopkeeper stands on the start map** (content + tests, about 70 lines) → [slices/N1.md](slices/N1.md)

### Phase 5: what the Shop shows
- [ ] **B1. The Buy tab lists the stock with prices and owned counts** (Game + text, about 175 lines) → [slices/B1.md](slices/B1.md)
- [ ] **B2. The Sell tab lists what can be sold; Left and Right switch tabs** (Game + text, about 210 lines) → [slices/B2.md](slices/B2.md)
- [ ] **L1. One overlay fade in `Ui/` for the Forge and the Shop** (Game refactor, about 110 lines) → [slices/L1.md](slices/L1.md)
- [ ] **L2a. The Shop's frame and list column: header with gold, tabs, column titles, footer, fade** (Game, raw 125, about 200 lines at 1.6x) → [slices/L2a.md](slices/L2a.md)
- [ ] **L2b. The Shop's detail column: the highlighted item's description, owned count, a piece's affixes** (Game, raw 95, about 150 lines at 1.6x) → [slices/L2b.md](slices/L2b.md)

### Phase 6: trading
- [ ] **T1. Buying at the Shop: Confirm buys one; a refused row greyed with its reason** (Game + text, about 190 lines) → [slices/T1.md](slices/T1.md)
- [ ] **T2. Selling at the Shop: Confirm sells one; a piece asks first** (Game + text, about 210 lines) → [slices/T2.md](slices/T2.md)

## Decisions (proposed, numbered at approval)
From earlier plans: 103 (gold will exist; the victory view takes it as an addition), 252 (one selection look), 257 (the inn's fade), 339 (Unique reserved), 353 (an NPC opens a screen through an event step), 358 (nothing is destroyed for less than it yields), 369 (owned pieces only), 370 (a screen over the map fades itself in, refused actions greyed with their reason).

Planner's picks, each marked in its slice; the developer confirms or changes them at approval:
- **Gold is the party's**: a `Party/Purse` on `PartyState`, never negative, at most `Purse.MaxGold` (9,999,999, a fixed technical limit like a stack's cap: seven digits fit every gold field). Battle gold past the cap is lost, as drops past a stack's limit are; a sale that would pass it is refused (decision 358's spirit).
- **Gold drops are a range on the drop table** (`drop-tables.json` `gold: { min, max }`), one roll per defeated enemy whose table has one, after every item draw, so today's seeded drops don't change (Q2).
- **A price is the item's** (`items.json` `price`, optional): one price whatever the shop; an item without one is never bought or sold. A shop lists item ids (`shops.json`); every listed item must have a price (checked at load).
- **Selling pays `price × sellPercent / 100`, rounded down** (`rules.json` `trade.sellPercent`, placeholder 50); a sale worth 0 is refused as not for sale. A rolled piece sells as a plain one (Q1).
- **One item per Confirm**, buying or selling (Q3). Buying gear gives a plain Common piece (Q11).
- **Refusal order** (shown greyed with the first reason): buying `StackFull` then `ShortOfGold` (a full stack can't take it whatever the gold); selling `NotForSale` then `PurseFull`.
- **Selling a gear piece asks first** ("Sell the Magic Bronze Sword for 25 G?", the Forge's salvage prompt); buying and selling one copy of a stack go at once, with a notice (Q5).
- **The Sell tab lists stacks in pickup order, then pieces in pickup order**; an unpriced one (Scrap, Essence today) is listed greyed "Not for sale" (decision 370's convention).
- **The inn always asks**, even when the gold is short; Yes then shows "You don't have enough gold." and the event continues unrested. A free rest (no price) is unchanged: no question (Q8).
- **The shop opens like the Forge**: no fade to black, it fades itself in (decision 370), through one shared `Ui/OverlayFade` (the backlog's third-copy extraction of `ForgeMotion`).
- **Gold shows as "{amount} G"** (`ui.json`; Q6) in the menu header left of the map name, in the Shop's header, and as a reward row on the victory screen when the battle gave any.
- **`EventPresenter` takes a `Ui/ScreenParts`** (the text and the shared textures, one parameter) so it can draw the inn's `ConfirmPromptView` without a fifth constructor parameter (I2b). The alternative, an inn scene opened through the flow like the Forge, costs a request pair, a transitions class and two `GameFlow` cases for one prompt.
- **The victory view grows by one reward row only** (about 3 lines in `VictoryScreenView`, 263): the backlog's `VictoryCardsView` split waits for a change that grows the cards.

## Open design questions (developer decides)
- **Q1. Does a rolled piece sell for more than a plain one?** Placeholder: no, every piece sells at its base price's share. Alternatives: a multiplier per rarity in `rules.json` `trade`, or a bonus per affix.
- **Q2. Where does gold drop from, and does drop luck raise it?** Placeholder: a range on the drop table, untouched by luck. Alternative: a range on each enemy (`enemies.json`, beside `experienceReward`); the stone beetle shares the goblin table today, so it would drop the same gold.
- **Q3. Buy and sell one at a time, or a quantity picker** (the reference's quantity window)? Placeholder: one per Confirm.
- **Q4. What can be sold?** Placeholder: anything owned with a price; materials have none, so they can't be. Alternative: materials sell too (a price on Scrap and Essence).
- **Q5. Which trades ask to confirm?** Placeholder: selling a gear piece only.
- **Q6. The currency's name and look**: placeholder "G" after the amount, no icon (the reference draws a coin icon before it).
- **Q7. The placeholder numbers** (design.md, "Tunables"): starting gold 100, goblin table 6 to 12 gold, prices, the 50 % sell share, the inn's 20. To tune once fights and prices can be compared.
- **Q8. The inn**: does it charge when nobody needs rest (placeholder: yes, as the free rest rests anyway), and does it ask when the gold is short (placeholder: yes, then says so)?
- **Q9. The party's stat preview in the shop** (the reference's right column, as the Equipment page compares): this plan or a follow-up? Placeholder: a follow-up.
- **Q10. Category tabs in the shop** (All, Items, Weapons, Armor, as the reference and our Inventory rail): this plan or a follow-up? Placeholder: a follow-up; the lists are short.
- **Q11. Does a bought piece roll** (rarity and affixes, as a drop does) or come plain? Placeholder: plain Common, as starting items and map gifts (decision 344).

## ADRs needed
- **ADR needed: gold and trade.** Real alternatives: gold ranges on drop tables or on enemies (Q2); prices on items with shops listing ids, or prices per shop in `shops.json`; selling at a share of the price, or a separate sell price. Written from the README's entries once the developer answers Q1, Q2 and Q4 (a checkbox step before G2). The save format change follows ADR 0022 and needs no ADR; the `openShop` step follows ADR 0006 and the forge plan's `openForge`.
