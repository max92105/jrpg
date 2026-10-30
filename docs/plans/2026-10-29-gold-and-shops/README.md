# Gold, shops and the inn
Status: approved

Open question 222 (paying for rest, from the class mechanics plan) answered as a feature on 2026-10-29: the developer wants gold, shops and a priced inn. Follows [the forge plan](../done/2026-10-27-forge/README.md), whose Forge screen (opened by an `openForge` step from a forge master NPC) is the model for the Shop screen a shopkeeper opens.

- Numbers: decisions 374-385, ADR 0031. Runs before [the quests plan](../2026-10-29-quests/README.md) (decision 374): saves are format 6 here, 7 there.
- Approved on 2026-10-29 with the developer's answers (374-385) and the devil's advocate's edits: the inn moves after trading so the shop shows early; no `ExplorationState` split (I1's three lines keep it at 299; the quests plan splits it); the Shop's frame and fade come from the Forge from its first slice; one transitions class and one close request for the screens an event opens; new reference sets as `init` members; CA1502 budgets named in the slices; both bounds tested for every number; no `PurseFull` refusal (gold past the cap is lost, one rule).

## Goal
1. **Gold**: the party owns gold (`Party/Purse`), kept by saves (format 6, ADR 0022); each defeated enemy drops a range of it (`enemies.json`, decision 376), shown on the victory screen; the menu header shows what is owned, as a number with a coin icon (decision 380); a new game starts with some.
2. **Shops**: prices on items (`items.json` `price`), a gold value on each affix (`affixes.json` `goldValue`, decision 375), shop inventories in `shops.json`, the sell share in `rules.json` `trade`. A shop's gear is rolled once into pieces like drops and kept, saved, and rerolled once the leader has gained a level (decision 385). Core's `Shop` (buying) and `ItemSales` (selling) quote every trade with its refusal, as the Forge's quotes do.
3. **At a shopkeeper**: an `openShop` event step names a shop and opens the Shop screen over the map (editor support, a Shopkeeper preset, one placeholder shopkeeper on the start map): Buy and Sell tabs, category tabs (decision 384), a quantity picker (decision 377), the detail column with a member's stat preview (decision 383), in the game's look (`docs/ui-reference/shop-sell.webp`).
4. **The inn**: the existing `RestStep` gains an optional price; a priced rest always asks with its price; Yes pays and rests, and without the gold the refusal shows and nothing changes (decision 382).

## Order, and why
1. **Phase 1: gold (G1-G4).** The purse, then the format-6 save shape for everything this plan saves (gold and the shops' stocks) before either appears in play (the loot retro), then gold from battles and a new game, then where the player sees it.
2. **Phase 2: shops in Core (S1-S6).** Buying stacks, then what a rolled piece is worth and buying one, selling, the content and its checks, the rolled stocks, then their saving and repair.
3. **Phase 3: the shop on the map (L1, H1, E1, E2, N1).** What the Forge and the Shop share moves to `Ui/` and `Scenes/` first; the Shop screen in the menu's frame from its first slice; the step that opens it, the editor support, then the shopkeeper: from N1 on every shop slice is checked by hand (the forge review's blocker 4).
4. **Phase 4: what the Shop shows (B1, B2, B3, L2a, L2b).** The Buy list, the tabs and the Sell list, the categories, the detail column, the stat preview.
5. **Phase 5: trading (T1, T2, T3).** Buying a piece, buying stacks with the quantity picker, selling.
6. **Phase 6: the inn (I1, I2a, I2b, I3).** Last, so the shop shows early (the review); it reuses T3's priced prompt.

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0030's pattern): prices, affix values, the sell share, gold ranges, starting gold and the inn's price are content; every number is a placeholder (design.md, "Tunables"; decision 381).
- **Every "can it be done" rule is Core's** (the forge review): `Shop` and `ItemSales` return a `TradeQuote` per trade and say the most that can be bought or sold (the picker's limit); `InnStay` says whether a rest can be paid. The screens format them and never decide.
- **A refusal changes nothing**: every check runs before gold, an item or a stock piece moves. **One overflow rule**: gold past `Purse.MaxGold` is lost, from a battle or a sale.
- **Pure presenter + thin view** (`docs/map.md`); one selection look (decision 252), every list through `SelectionMotion`. `Trade` reads `Menus` for the menu's frame (`MenuHeader.Height`, `MenuLayout`), the categories (`CategoryRail`, `InventoryFilter`) and the comparison (`EquipmentComparison`, `EquipmentChangesView`), as `Crafting` reads the frame; `Menus` never reads `Trade`. What the Forge and the Shop both draw moves to `Ui/` (design.md, "Structure").
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x; Editor slices aim at about 200 lines.
- **Files near 300 lines:** new test files beside `EventPresenterTests` (305), `GameFlowTests` (321), `EventRunTests` (269), `ContentCatalogLoaderTests` (272), `GameSessionSaveTests` (270), `EventStepViewModelTests` (260), `EventListViewModelTests` (258), `ItemCatalogValidatorTests` (248). Watched: `ExplorationState` 296 (I1 takes it to 299, no more), `EventPresenter` 258 (I2a moves the rest out first), `VictoryScreenView` 263 (G4 adds about 8; the backlog's `VictoryCardsView` split stays in the backlog), `GameFlow` 254, `GameSession` 245 (stocks live in `Session/ShopStocks`), `SaveGameRepair` 221, `ForgeScene` 233, `Ui/MessageIds` 648 (constants; the backlog says it can wait).
- **Save format:** one change, in G2 (format 5 → 6: gold and the shops' stocks); an older save is refused (ADR 0022). **No NuGet package.**

## Out of scope (follow-ups)
- **Editing shops in the editor**: a new shop, its stock and prices are edited by hand in `shops.json` and `items.json`; the editor only picks a shop id for an `openShop` step.
- **Selling worn gear**: only owned stacks and pieces are sold, as the Forge works owned pieces only (decision 369).
- **Gold in the save list** (`Saving/SaveSlotList`) and **gold from map events** (a `giveGold` step): no caller yet.
- **The whole party's preview at once** (the reference shows every member): one member at a time here (decision 383).
- **Unique items** in shops (decision 339): no Unique exists.
- **Story and maps**: the developer places the NPCs; N1's shopkeeper is a placeholder he moves.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: gold
- [x] **G1. The party's purse** (Core, about 130 lines) → [slices/G1.md](slices/G1.md)
- [x] **G2. The format-6 save: gold and the shops' stocks** (Core + Data, about 175 lines) → [slices/G2.md](slices/G2.md)
- [x] **ADR 0031: gold and trade** (gold per enemy, prices on items and affixes, shops listing ids, the rolled and kept stock, the sell share): write it from the README's entries (decisions 375, 376, 378, 385), before G3
- [x] **G3. Each enemy drops gold; a new game starts with some** (Core + Data + content, about 215 lines) → [slices/G3.md](slices/G3.md)
- [x] **G4. Gold with its coin icon on the victory screen and in the menu header** (Game + art + text, raw 105, about 170 lines at 1.6x) → [slices/G4.md](slices/G4.md)

### Phase 2: shops in Core
- [x] **S1. Buying stacks in Core: prices, `Shop`, quotes with a quantity** (Core, about 220 lines) → [slices/S1.md](slices/S1.md)
- [x] **S2. What a rolled piece is worth; buying a stock piece** (Core + Data + content, about 225 lines) → [slices/S2.md](slices/S2.md)
- [x] **S3. Selling in Core: the sell share, `ItemSales`** (Core, about 235 lines) → [slices/S3.md](slices/S3.md)
- [x] **S4. Shops, prices and the sell share in content, checked by the loader** (Core + Data + content, about 240 lines) → [slices/S4.md](slices/S4.md)
- [x] **S5. The shops' stocks: rolled on first opening, rerolled after a leader level** (Core, about 160 lines) → [slices/S5.md](slices/S5.md)
- [x] **S6. The shops' stocks saved, restored and repaired** (Core + Data, about 180 lines) → [slices/S6.md](slices/S6.md)

### Phase 3: the shop on the map
- [x] **L1. One overlay fade and one close request for the screens an event opens** (Game refactor, about 120 lines) → [slices/L1.md](slices/L1.md)
- [x] **H1. A Shop screen in the menu's frame that fades in over the map and Cancel closes** (Game + text, raw 120, about 190 lines at 1.6x) → [slices/H1.md](slices/H1.md)
- [x] **E1. The `openShop` event step opens the named shop and waits for it** (Core + Data + Game, about 230 lines) → [slices/E1.md](slices/E1.md)
- [x] **E2. The editor shows and adds the `openShop` step; the Shopkeeper preset** (Editor, about 190 lines) → [slices/E2.md](slices/E2.md)
- [x] **N1. A shopkeeper stands on the start map** (content + tests, about 70 lines) → [slices/N1.md](slices/N1.md)

### Phase 4: what the Shop shows
- [x] **B1. The Buy list: the stock's stacks and pieces with owned counts and prices** (Game, raw 150, about 240 lines at 1.6x) → [slices/B1.md](slices/B1.md)
- [x] **B2. The Buy and Sell tabs; the Sell list** (Game + text, about 230 lines; extends `ShopListView`) → [slices/B2.md](slices/B2.md)
- [x] **B3. Category tabs on both lists, and the column titles** (Game + text, about 190 lines) → [slices/B3.md](slices/B3.md)
- [ ] **L2a. The detail column: name, description, owned count, a piece's affixes** (Game + text, raw 110, about 175 lines at 1.6x) → [slices/L2a.md](slices/L2a.md)
- [ ] **L2b. A member's stat preview for gear; Left and Right switch the member** (Game + text, about 200 lines; reuses `EquipmentChangesView`) → [slices/L2b.md](slices/L2b.md)

### Phase 5: trading
- [ ] **T1. Buying a stock piece; a refused row's reason; the notice** (Game + text, about 190 lines) → [slices/T1.md](slices/T1.md)
- [ ] **T2. The quantity picker; buying stacks with it** (Game + text, about 220 lines; drawn by `ShopDetailView`) → [slices/T2.md](slices/T2.md)
- [ ] **T3. Selling: stacks with the picker, a piece behind a priced prompt** (Game + text, about 235 lines) → [slices/T3.md](slices/T3.md)

### Phase 6: the inn
- [ ] **I1. A priced rest waits for payment: `InnStay`** (Core + Data, about 190 lines) → [slices/I1.md](slices/I1.md)
- [ ] **I2a. Move the rest's showing into `RestPlayback`** (Game refactor, about 110 lines, tests unchanged) → [slices/I2a.md](slices/I2a.md)
- [ ] **I2b. The inn asks with its price; Yes pays and rests; short of gold, the refusal shows** (Game + text, about 220 lines) → [slices/I2b.md](slices/I2b.md)
- [ ] **I3. The editor's rest step takes a price; the innkeeper charges** (Editor + content, about 150 lines) → [slices/I3.md](slices/I3.md)

## Decisions
From earlier plans: 103 (gold will exist; the victory view takes it as an addition), 107 (the leader is fixed), 252 (one selection look), 257 (the inn's fade), 339 (Unique reserved), 344 (plain pieces from outside the drops; decision 385 makes shop pieces rolled), 349 (luck is a drop's), 353 (an NPC opens a screen through an event step), 358 (nothing is destroyed for less than it yields), 360 (a saved affix out of range is clamped), 369 (owned pieces only), 370 (a screen over the map fades itself in; refused actions greyed with their reason).

The developer's answers (2026-10-29):
- **374. Gold and shops runs before quests**: this plan's save format is 6, the quests plan's 7.
- **375. A rolled piece is worth more** (question Q1): each affix carries a gold value (`affixes.json` `goldValue`, validated, placeholder numbers); a piece's price is its base item's price plus its affixes' values; a sale pays the sell share of that price. *Interpretation flagged:* "the base share plus its affixes' values" is read as the share of the whole price (base plus affixes), the same formula as buying at full price; the other reading (half the base plus the full affix values) is a one-line change in `PriceFormula`. The field is `goldValue`, not `value`: saves already use `value` for an affix's rolled stat.
- **376. Gold is a range per enemy** (Q2): `enemies.json` `gold: { "min", "max" }` (validated 0 ≤ min ≤ max ≤ `Purse.MaxGold`), one draw per defeated enemy after the item drops, so item draws don't change; no drop luck; `BattleSpoils.Gold`.
- **377. A quantity picker** (Q3): Confirm on a stack opens it at 1; Up and Down change the count by one (wrapping between 1 and the most), Left and Right by ten (stopping at the ends); the most is Core's: what the gold and the stack's room allow when buying, what is owned when selling. A piece is one of a kind: no picker.
- **378. Anything with a price can be sold** (Q4); materials have no price, so they can't be.
- **379. Only selling a gear piece asks first** (Q5); buying anything and selling stacks go at once, with a notice.
- **380. Gold shows as a number with a coin icon beside it, no currency name** (Q6): a placeholder icon `content/art/icons/Gold.png`, checked like every UI icon by `UiArtFilesTests` (the startup `ArtReferenceCheck` covers content references only). Sentences don't write amounts: a question with a price draws it under the question with the icon (T3), and notices name the item and count while the header shows the gold.
- **381. The placeholder numbers stay** (Q7): starting gold 100, 6 to 12 per goblin, a 50 % sell share, the inn at 20 (design.md, "Tunables").
- **382. The inn always charges and always asks** (Q8), even when nobody needs rest or the gold is short; Yes without the gold shows the refusal.
- **383. A stat preview in this plan** (Q9): the detail column compares one member wearing the highlighted gear, the leader first; Left and Right switch through the active party (the picker uses Up and Down), reusing `EquipmentComparison` and `EquipmentChangesView`.
- **384. Category tabs in this plan** (Q10): the Inventory's categories (`CategoryRail`, `InventoryFilter`) on both lists, changed with the tab buttons (`PreviousTab`/`NextTab`: the shoulders, and Q and R on the keyboard, new bindings).
- **385. A shop's gear is rolled once and kept** (Q11): when a shop is first opened, each gear entry of its list rolls into a piece through the session's shared `GearRoll` (seeded, no luck), priced at its full price (decision 375); stacks are never rolled. The stock is kept in the session and the save (format 6: shop id, the pieces, the leader level it was rolled at) and rolled again on the next opening once the leader has gained a level since. A saved stock piece with an unknown base or affix is dropped and reported. *Interpretations flagged:* "reroll shop when levelup" is read as the leader's level (the leader is fixed, decision 107); a bought piece leaves the stock until the next reroll (planner's pick: a rolled piece is one of a kind).

Planner's picks, each marked in its slice (unnumbered):
- **`Purse.MaxGold` is 9,999,999**: seven digits fit every gold field (the header, the rows, the picker). One overflow rule: gold past it is lost, from a battle or a sale.
- **A price is the item's** (`items.json` `price`, optional): one price whatever the shop; an item without one is never bought or sold. Every item a shop lists must have a price (checked at load).
- **Refusal order**: buying `StackFull`, then `ShortOfGold`; selling `NotForSale` only.
- **The Shop opens on its Buy and Sell tabs** (Left and Right choose, Confirm enters the list, Cancel in the list returns to the tabs, Cancel on the tabs leaves), so Left and Right are free in the list for the member switch (decision 383).
- **The Sell list shows stacks in pickup order, then pieces in pickup order**; an unpriced one is listed greyed "Not for sale" (decision 370).
- **The shop opens like the Forge**: no fade to black, it fades itself in through one shared `Ui/OverlayFade`; both close through one `EventScreenClosed` and one `Flow/EventScreenTransitions`.
- **`EventPresenter` takes an `Exploration/EventLook`** (the text and the shared textures, one parameter) so it can draw the inn's question without a fifth constructor parameter; one user, so it stays in `Exploration/`.
- **The stock rolls without drop luck** (decision 349: luck is a drop's).

## Open design questions (developer decides)
None open. The flagged interpretations in decisions 375 and 385 (the sale formula, the leader's level, a bought piece leaving the stock) are for the developer to confirm.

## ADRs
- **ADR 0031: gold and trade.** Gold ranges on each enemy rather than on drop tables; prices on items and gold values on affixes rather than per shop; shops listing ids; a shop's gear rolled once, kept in the save and rerolled after a leader level; selling at a share of the price. A checkbox step before G3, written from decisions 375, 376, 378 and 385 (nothing left to ask). The save format change follows ADR 0022 and the `openShop` step ADR 0006 and the forge plan's `openForge`: no ADR for either.
