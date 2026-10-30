# 0031. Gold and trade

Status: accepted
Date: 2026-10-29

## Context
The party has no money: items come from chests, drops (ADR 0030) and the Forge's salvage. The gold and shops plan adds
gold, shops opened by a shopkeeper NPC (as the Forge is, decision 353) and a priced inn (question 222). Numbers belong
in content (ADR 0019) and rolls draw only when uncertain (ADR 0026). Gear is pieces with rolled affixes (ADR 0029) and
a rolled piece is worth more than a plain one (decision 375), so a shop that sells gear has to say what a roll costs.
The developer answered the plan's questions on 2026-10-29 (decisions 374-385).

## Options
### A. Gold per enemy, prices on items and affixes, shops listing ids, a stock rolled once and kept, selling at a share
- Pros: one price per item whatever the shop; a piece's worth is its base plus its affixes' values, the same formula
  for buying and selling; a shop's gear is a surprise once, then stable until the leader grows; every number is content.
- Cons: the stock is state (in the session and the save); a gold range draws once per defeated enemy.

### B. Gold on drop tables, prices per shop, a separate sell price per item
- Pros: shops can price the same item differently; selling is tuned apart from buying.
- Cons: two price tables to keep in step; gold tied to a table shared by several enemies; a `GoldRange` on the drop
  roll gives `DropRoll` a second job.

### C. A fixed gold amount per enemy, plain Common pieces in shops
- Pros: no draw, no stock state, no save change for stocks.
- Cons: the developer wants a range and rolled shop gear (decisions 376, 385).

## Decision
A.
- **Gold** is `Party/Purse`, 0 to `Purse.MaxGold` (9,999,999: seven digits fit every field); gold past the cap is
  lost, from a battle or a sale, so no trade is refused for a full purse. Saves keep it (format 6, decision 374).
- **Each enemy may carry a gold range** (`enemies.json` `gold: { min, max }`, 0 ≤ min ≤ max ≤ `MaxGold`,
  decision 376). After a victory each defeated enemy with a range draws once, after every item draw, so the item
  drops of a seeded battle don't move; no drop luck applies (decision 349). `BattleSpoils.Gold` reports what the purse
  took.
- **A price is the item's** (`items.json` `price`, optional): an item without one is never bought nor sold; materials
  have none (decision 378). **Each affix carries a gold value** (`affixes.json` `goldValue`, decision 375): a piece's
  price is its base price plus its affixes' values.
- **Shops list item ids** (`shops.json`); every listed item must have a price. **A shop's gear is rolled once and
  kept** (decision 385): on the first opening each gear entry rolls into a piece through the session's shared
  `GearRoll` (seeded, no luck), priced at its full price; the stock lives in the session and the save (shop id, pieces,
  the leader's level at the roll) and rolls again on the next opening once the leader has gained a level. A bought
  piece leaves the stock until then. Stacks are never rolled.
- **Selling pays a share of the price** (`rules.json` `trade.sellPercent`, rounded down); only a gear piece asks to
  confirm (decision 379). **The inn** charges its `RestStep` price and always asks (decision 382).
- Every "can it be done" rule is Core's: `Trade/Shop` and `ItemSales` return a quote per trade with its refusal and
  the most that can be traded, as the Forge's quotes do (ADR 0030's pattern for mechanisms in code, numbers in content).

## Consequences
- The save format changes once (6) for the gold and the shops' stocks; a saved stock piece with an unknown base or
  affix is dropped and reported by the repair.
- A new game that fights and then opens a shop sees its later draws shift by one per defeated enemy with a range.
- Repricing an item or an affix changes every shop at once; a per-shop price would need a new field.
