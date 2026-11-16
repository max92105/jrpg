# 0058. Unique items as records with fixed affixes

Status: accepted
Date: 2026-11-16

Amends ADR 0029: Unique is no longer reserved; a unique is its own item record with fixed affixes, and its piece is
built from that record wherever it comes from, a save included.

## Context
ADR 0029 made a gear piece a value (a base item, a rarity, its rolled affixes) and reserved Unique: it never rolls
(ADR 0030), `Inventory.Add` makes plain pieces, and the Game already names and colors the rarity. The developer wants
to "create unique item" beside base items and affixes, and "affix should hold their values": on a unique they are fixed,
never rerolled (decision 699); prices keep one sale share for every item (question 688, decision 703). The roadmap's
direction to confirm was a unique as its own item record with fixed affixes, dropped by its id and saved as any piece
(decision 681). Constraints: no save format change unless the developer picks one (ADR 0022); content is the only
source of truth (ADR 0011); a drop draws nothing it doesn't need (determinism); the Items page edits uniques beside the
base items. The developer delegated the plan's approval, so its game-design choices were taken while he was away and
are marked below.

## Options
### 1. What a unique is
**A. Its own item record with fixed affixes (chosen).** `isUnique` and `fixedAffixes` on an `items.json` record; its
piece is Unique with the record's affixes; it drops by its id.
- Pros: no save change (a unique piece saves as any piece); the record is the truth, so an edited unique reaches every
  owned copy; drops, shops, gifts and starting items name it by id with no new mechanism; the Items page edits it
  beside the base items. Three Core slices (IU2-IU4) and one form (IU13).
- Cons: a unique is a whole item (name, icon, damage) even when it differs from a base item by its affixes only; the
  saved affixes are redundant (kept so the shape doesn't change); a base never rolls one (decision 875) until the
  extension in option 3B.

**B. A template naming a base item (`uniques.json`: a base id, a name, fixed affixes).**
- Pros: shares the base's numbers; the list of base items stays short; a base rolling Unique picks a template
  naturally.
- Cons: a piece must say which template it is: `SavedGear` gains a unique id, a format bump (ADR 0022) and a repair
  case for a removed template; every reader of a piece's name and icon checks the template first (about 15 Game
  sites); a second Database page. About four more slices.

**C. Values rolled once on the drop within the affix ranges, then fixed (the action RPG way).**
- Pros: two copies of a unique can differ, a reason to farm.
- Cons: against decision 699 ("on a unique they are fixed"); an edited unique can't reach owned copies.

**D. A drop entry carrying fixed affixes (no record).**
- Cons: a shop or a map gift can't give it; nothing to edit by name. Refused.

### 2. What bounds a fixed value (decision 866)
**A. Any nonzero value within the bonus bound, on its affix's side of zero (chosen).**
- Pros: a unique can beat a good Rare; the Affixes page can narrow a range without touching any unique, and nothing
  cross-checks the two files.
- Cons: a unique's line may exceed any roll (a Power +30 ring); that is the point, and his to tune.

**B. Bound by the affix's roll range.**
- Pros: a unique stays within what the pool rolls.
- Cons: a range rule in the unique check plus the pool cross-check in `CatalogProblems.OfAffixes`, so narrowing a
  range can refuse a unique.

### 3. How a unique arrives (decision 875)
**A. By its id only (chosen).** `GearRoll.RollRarity` never yields Unique (no rules row); a unique arrives only where a
table, a shop, a gift or the new game names its id.
- Pros: ADR 0030's "Unique never rolls" kept; no new rules row and no pick among uniques.
- Cons: the developer's "they can roll magic, rare, unique" may mean a base rolling a unique, which this doesn't do.

**B. A base rolling Unique.** A `unique` row in `rules.json` `loot` (`LootRulesValidator` refuses one today) and an
optional `baseItem` on a unique record; when a base rolls Unique, it gives one of the uniques naming that base (a
weighted pick, or a reroll to Rare when none does).
- Pros: matches the other reading of his words; the piece is still the unique's own, so no save change.
- Cons: a rules row, a field and a pick to build now, before he has asked for them.

## Decision
Option A on each question (decisions 864-872 and 875).
- **A unique is its own item record (865).** (default taken while the developer was away; his to overturn: decision
  681 was the roadmap's direction to confirm, recorded here so) `"isUnique": true`, equipment, and `fixedAffixes`, a
  list of `{ "affixId", "value" }` naming the pool's affixes, in order. A fixed affix's stat is resolved at load into
  `Items/RolledAffix` (`Data/Items/FixedAffixConverter`, row 6's decision 789 mechanism), so `affixes.json` is read
  before `items.json`. Only a unique has fixed affixes; a unique may have none.
- **Fixed values (866).** (default taken while the developer was away; his to overturn; the review's preferred reading
  of decision 699) A fixed value is any nonzero value within the bonus bound (`ItemCatalogValidator.MostBonus`, ±100)
  on its affix's side of zero (positive for an affix whose range is positive, above 0 for an affix-only stat), so a
  unique can beat a good Rare; a unique names an affix at most once; it may name any pool affix, whatever kinds the
  affix rolls on. The affix's range bounds rolls only.
- **The piece (867)** is always built from the record (`GearPiece.Unique`): `Rarity.Unique` and its fixed affixes in
  order. A drop, a shop's stock, a map gift, a starting item and a restored save all give it; a drop or a stock of a
  unique draws nothing beyond its entry's chance and quantity.
- **Saves (868).** No format change (`formatVersion` stays): a unique piece saves as any piece (base id, `unique`,
  affixes). On load a piece whose base is unique is rebuilt from its record, so an edited unique reaches every owned
  copy (ADR 0029's "a changed base reaches every owned piece"); the repair reports a saved piece that differed from its
  record. A saved `unique` piece whose base is no longer unique (a hand edit of `items.json`) becomes a plain Common
  piece, reported. The pool clamp (decision 360) never touches a unique.
- **The Forge (869)** never rerolls a unique (decision 699): `ForgeRefusal.FixedAffixes`, "A unique's affixes are
  fixed", the one new refusal. Upgrade and salvage stay refused by decision 339's rules (Highest rarity; no unique
  salvage row, which `CraftingRulesValidator` already requires).
- **Copies (870).** (default taken while the developer was away; his to overturn) A unique may be owned in several
  copies, and a drop table or a shop may list it like any item: no one-of-a-kind rule.
- **Price and name (871).** A unique's price is its base price plus its fixed affixes' gold values, ADR 0031's formula
  unchanged; its name in a question is its own name, without the rarity word (`PieceName`).
- **Prices (864).** (Question 688, settled by decision 703.) One sale share for every item, `rules.json`
  `trade.sellPercent`, as today (ADR 0031): no per-item sell price. The buying price is the item's (empty: never bought
  nor sold), a piece's adds its affixes' gold values; the Items page shows the sale price read-only beside the price.
  The share isn't edited in this plan.
- **The placeholder (872).** (default taken while the developer was away; his to overturn) One placeholder unique,
  `chiefs_cleaver`, is written to `items.json` and listed in no drop table and no shop: the developer places it.
- **By id only (875).** (default taken while the developer was away; his to overturn; loud) A base item never rolls
  Unique; a unique drops by its id only (ADR 0030's "Unique never rolls" kept). His "they can roll magic, rare, unique"
  may mean a base rolling a unique: the extension is option 3B, and neither of its parts needs a save change.

## Consequences
- No save format change: saves made today keep loading, and a unique piece saves as any piece; the saved affixes are
  redundant, since the record is the truth.
- An edited unique reaches every owned copy on the next load, and the repair reports each copy it changed.
- Drops, shops, map gifts and the new game give a unique by its id with no new mechanism; a unique's drop or stock
  draws one draw fewer than a rolled base would, pinned by a test.
- `ItemDefinition` compares its fixed affixes in order, so a record rebuilt with the same affixes isn't seen as
  changed; the Affixes page re-resolves every unique's fixed affixes when it changes an affix's stat (decision 884).
- A unique is a whole item record even when it differs from a base by its affixes only.
- If a later slice finds it must change `SavedGear`, it stops: that is option 1B, the developer's call, with ADR 0022's
  bump and an old-format refusal test.
- Later work: a base rolling Unique (option 3B), fixed values bound by the roll range (option 2B), an affix only
  uniques carry (weight 0 allowed in `AffixCatalogValidator`, skipped in `GearRoll.CandidatesFor`).
