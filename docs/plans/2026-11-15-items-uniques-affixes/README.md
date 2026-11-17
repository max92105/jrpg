# Items, uniques and affixes (editor push, roadmap row 9)
Status: approved, in progress

Drafted on 2026-11-15 from roadmap row 9 of [the editor push](../2026-11-15-editor-push/README.md), its design.md
section 3.9, and the developer's words: "for item I want to be able to create base item. Item will have base stats then
they can roll magic, rare, unique. I need to be able to create unique item. I need to be able to create affix. Selling
price buying price."; then, on question 688: "all potion will have same value yes... effect / affix should hold the
value so it apply to everything having effect / affix" (decisions 699, 703). The developer was away while it was
planned: every game-design choice taken without him is marked "(default ...)" and listed in "Defaults to confirm".

Approved on 2026-11-15 by delegation from the developer, with the devil's advocate's review applied: items keep a flat
power and the tiered items become an optional slice on his word (B1, the coordinator's call: row 6's decision 785
stands); the two editors are pages from birth on the mode bar's [page recipe](../2026-11-15-mode-bar/page-recipe.md)
(B2); decision 681 and "uniques drop by id only" are loud defaults (C1); a unique's values are free within the bonus
bound (C2); Unique is chosen at Add (C3); the fixed affix converter is registered on `items.json`'s write (C4); the
Affixes page merged into its list slice (C5).

**Before this plan starts:**
- Row 6 ([skills and statuses model](../2026-11-15-skills-and-statuses-model/README.md)) is done: T2 (`effects.json`,
  `CatalogReferenceConverter<T>`, `ContentJsonOptions.StrictWith`), SW (`Effect.Statuses`), WK
  (`CombatStats.DamageModifierPermille`).
- The mode bar's stage 1 is done ([mode bar](../2026-11-15-mode-bar/README.md), decisions 934, 937, 938): WD1 moved the
  Database menu to `Shell/DatabaseMenu.xaml` (entries dispatched by `Tag`), WD2 added `Workspaces/CatalogDialog` and
  `IEditedCatalog`.
- Row 7 ([skill and status editors](../2026-11-15-skill-and-status-editors/README.md)) is done: the writer writes an
  `Effect` in its layout (decision 973's filters, type-based, so items' effects get them too), converters are
  registered per file (decision 972), and `CatalogReferences.Rebound` already rebinds items' status grants (decision
  975). IU1 names what it adds if row 7 hasn't landed.

## Goal
1. **The writer takes over `items.json` and `affixes.json`** (IU1): relaid once, written sorted by id in the writer's
   layout, the identical-bytes test guarding both.
2. **Uniques (decisions 681, 699):** a unique is its own item record with fixed affixes from the pool; it drops by its
   id, sells and is given as its own piece, is never rolled nor rerolled at the Forge, and saves as any piece, so the
   save format doesn't change.
3. **Database > Items** (a page): base items by category (consumable, equipment, unique, material): icon, stack, price
   and its sale line, the consumable's effect and own strip, the equipment's kind, damage and bonuses, a unique's fixed
   affixes.
4. **Database > Affixes** (a page): the pool's affixes: stat, range, gear kinds, weight and gold value.
5. **The damage sheet's "gear mod"** (decision 704's "gear 33 %"): a "+% damage" stat that gear affixes give, feeding
   row 6's modifiers' sum (IU17).
6. **Prices (question 688):** one sale share for every item, shown beside each buying price (decision 864).

## Order, and why
1. **The writer first (IU1):** every later content change (the placeholder unique) is then written in the writer's
   layout, and the pages' saves can't rewrite whitespace.
2. **Uniques in the game before their form (ADR 0058, IU2-IU4):** the record, its piece and its save rules settle what
   the Items page writes; Core first (IU2), then the file (IU3), then the save repair, the Forge and the piece's name
   (IU4).
3. **The Items page (IU6-IU13):** the developer's first ask. Rules and the catalog step before the page (IU6, IU7, IU8),
   the undo refusal (IU9), then the forms (IU10 and IU11 equipment, IU12 consumable, IU13 unique).
4. **The Affixes page (IU14, IU15):** the pool is already usable from the unique form; the page edits it.
5. **The gear damage modifier last (IU17):** it needs row 6's WK sum and is easiest to check with an affix made in the
   Affixes page.
6. **IU5 is optional and not run** unless the developer answers the open question for tiered items (decision 874).

## Guiding rules
- **Every slice ends green.** The content changes (relay, placeholder unique) land in the slice whose tests need them;
  a hand check never produces content a later slice depends on.
- **Rules in Core:** a unique's piece, the Forge's refusal and the damage bonus are Core rules with tests; the pages
  only type and show (prices through `PriceFormula`, problems through `CatalogProblems`).
- **The record is the truth:** a unique piece is rebuilt from its record wherever it comes from, a save included
  (decision 868); a fixed affix's stat is resolved at load (row 6's decision 789 mechanism).
- **No save format change** (decision 868): `formatVersion` stays; IU2 and IU4 carry the round-trip and repair tests.
- **Tests on repository content never pin today's values** (`.claude/rules/tests.md`): IU3 asserts ids and shapes.
- **Pages from birth** (the mode bar's decision 934): every rule of [page-recipe.md](../2026-11-15-mode-bar/page-recipe.md)
  applies (no OK, Cancel or problem line in the page; `UpdateSourceTrigger=PropertyChanged`; `AncestorType` the page
  type; resources from `EditorTheme.xaml` only; the `Choose` and `ChosenKey` tests).
- **Sizes:** Core and Data slices about 300 lines with tests at about 13 lines each; Editor slices about 200, tests
  counted as about as many lines as the code; raw JSON counted apart.
- **Files near 300 (checked 2026-11-15, each slice recounts):** `Editor/Shell/MainViewModel.cs` 285 (+1 line per
  page, IU8 and IU15); `Data/Content/CatalogProblems.cs` 130 (row 7 adds three entries, this plan about 25 over IU1,
  IU7, IU15; past 250 it gets an `.Items` partial in the slice that would cross); `Editor/Catalogs/CatalogDraft.Database.cs`
  118 (row 7's three steps, then IU7 and IU15 add about 35); `Core/Battle/BattleState.cs` 283 (untouched). Test files
  past 240: `ContentCatalogLoaderTests` 272, `FieldItemUseTests` 276, `GameSessionSaveTests` 270,
  `ItemCatalogValidatorTests` 248: new tests go to the siblings each slice names.
- **Glob before creating:** each slice says which test files are new (none of them exist on 2026-11-15) and which it
  extends.
- **Every page added** joins `.claude/scripts/open-editor-windows.ps1`'s default `-Windows` list in its slice (IU8,
  IU15), after every World-menu entry (the mode bar's decision 933); the hand checks rely on the event window plan's
  EW0: "no binding-error line" in the Debug output.

## Out of scope
- A per-item sell price (decision 864: one share for every item).
- Items naming tiered records: IU5, optional, only on the developer's word (decision 874).
- A base item rolling Unique (decision 875; its extension point in design.md, 2.6).
- Importing icons: the icon box offers the icons folder's PNGs (backlog: an icons kind for `ContentWriter.ImportArtImage`).
- Editing `rules.json` (the sell share, the rarity weights, the Forge's costs): hand-written, as today.
- An affix that never rolls and only uniques carry (weight 0), and an affix raising two stats (design.md 3.9 of the
  editor push): each waits for the developer to ask (extension points in design.md, 2.6).
- Items granting statuses: no item does; the consumable form carries an effect's statuses, hits and range untouched.
- A list filter by category in the Items page: the list is sorted by category and each row says its category.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md). Fifteen
slices run; IU5 is optional and IU16 merged into IU15.

### Phase 1: the writer takes over the files
- [x] **IU1. The writer writes `items.json` and `affixes.json`, relaid once in its layout** (Data + content, about 200
  lines plus the two relaid files) → [slices/IU1.md](slices/IU1.md)

### Phase 2: uniques in the game
- [x] ADR 0058, unique items as records with fixed affixes (amends ADR 0029). Write it from the README's decisions
  864-872 and 875, recording decisions 681, 866 and 875 as defaults to confirm (the options in design.md, section 4); no
  question is asked.
- [x] **IU2. A unique item gives its own piece everywhere: dropped, stocked, given, restored** (Core, about 230 lines)
  → [slices/IU2.md](slices/IU2.md)
- [x] **IU3. `items.json` holds uniques: `isUnique` and `fixedAffixes`, loaded and checked; a placeholder unique**
  (Data + content, about 240 lines) → [slices/IU3.md](slices/IU3.md)
- [x] **IU4. A saved unique follows its record, the Forge never rerolls one, and a unique is named alone** (Data + Core
  + Game, about 200 lines) → [slices/IU4.md](slices/IU4.md)

### Optional: items name tiered effects (not run without the developer's word)
- [ ] **IU5 (optional, only on the developer's word). A potion or a bomb names its tiered effect and acts with its
  user's Power** (Core + Data + content + Game, about 250 lines) → [slices/IU5.md](slices/IU5.md)

### Phase 3: the Items page
- [x] **IU6. One item as the Items page types it: the general fields and the sale line** (Editor, about 220 lines) →
  [slices/IU6.md](slices/IU6.md)
- [x] **IU7. The Items list and its undo step: add by category, unique included, duplicate, remove while new** (Editor
  + Data, about 220 lines) → [slices/IU7.md](slices/IU7.md)
- [x] **IU8. Database > Items opens the Items page** (Editor, about 190 lines) → [slices/IU8.md](slices/IU8.md)
- [x] **IU9. An undo removing an item a map gives is refused** (Editor, about 120 lines) →
  [slices/IU9.md](slices/IU9.md)
- [ ] **IU10. The equipment form: kind, damage and damage type** (Editor, about 170 lines) →
  [slices/IU10.md](slices/IU10.md)
- [ ] **IU11. The equipment form's bonus rows** (Editor, about 180 lines) → [slices/IU11.md](slices/IU11.md)
- [ ] **IU12. The consumable form: effect kind, targeting, power, damage type, own strip** (Editor, about 210 lines) →
  [slices/IU12.md](slices/IU12.md)
- [ ] **IU13. The unique form: the fixed affix rows and the piece's price** (Editor, about 210 lines) →
  [slices/IU13.md](slices/IU13.md)

### Phase 4: the Affixes page
- [ ] **IU14. One affix as the Affixes page types it** (Editor, about 190 lines) → [slices/IU14.md](slices/IU14.md)
- [ ] **IU15. The Affixes list, its undo step and Database > Affixes, uniques following their affixes' stats** (Editor
  + Data, about 290 lines; IU16 merged in, decision 885) → [slices/IU15.md](slices/IU15.md)

### Phase 5: the gear damage modifier
- [ ] **IU17. A "+% damage" affix stat raises its wearer's damage through the modifiers' sum** (Core + Data + Game,
  about 230 lines) → [slices/IU17.md](slices/IU17.md)

## Decisions

Numbers 861-890. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **861.** One plan, five phases, fifteen slices run (IU1-IU4, IU6-IU15, IU17), IU5 optional, IU16 merged into IU15;
  ADR 0058 before IU2. It starts after row 6, the mode bar's stage 1 and row 7.

The writer and prices:
- **862.** The writer takes over `items.json` and `affixes.json`: relaid once (load, write, the bytes equal the relaid
  file), sorted by id, one field per line in nested objects. Optional item fields are left out at their defaults
  (`effect`, `equipment`, `price`, `effectFile` null; `isMaterial`, `isUnique` false; `fixedAffixes` empty; an
  equipment's `damage` 0, `damageType` null and empty `bonuses`), and an item's bonuses are written in `StatKind` order,
  so a record the editor writes reads like a hand-written one.
- **863.** What the writer and the Items page check is everything the loader checks that reads `items.json`
  (`CatalogProblems.CheckItems`): the items' own rules, the shops' prices, the drop tables' stacks, the Forge's
  materials (`rules.json` `crafting`) and the new game's starting items.
- **864.** (Question 688, settled by decision 703.) One sale share for every item, `rules.json` `trade.sellPercent`, as
  today (ADR 0031): no per-item sell price. The buying price is the item's (empty: never bought nor sold), a piece's adds
  its affixes' gold values; the Items page shows the sale price read-only beside the price. The share isn't edited in
  this plan.

Uniques (ADR 0058, amending ADR 0029):
- **865.** (default taken while the developer was away; his to overturn: decision 681 was the roadmap's direction to
  confirm, and ADR 0058 records it so) A unique is its own item record: `"isUnique": true`, equipment, and
  `fixedAffixes`, a list of `{ "affixId", "value" }` naming the pool's affixes, in order. A fixed affix's stat is
  resolved at load into `Items/RolledAffix` (`Data/Items/FixedAffixConverter`, row 6's decision 789 mechanism), so
  `affixes.json` is read before `items.json`. Only a unique has fixed affixes; a unique may have none.
- **866.** (default taken while the developer was away; his to overturn; the review's preferred reading of decision 699)
  A fixed value is any nonzero value within the bonus bound (`ItemCatalogValidator.MostBonus`, ±100) on its affix's
  side of zero (positive for an affix whose range is positive, above 0 for an affix-only stat), so a unique can beat a
  good Rare; a unique names an affix at most once; it may name any pool affix, whatever kinds the affix rolls on. The
  affix's range bounds rolls only.
- **867.** A unique's piece is always built from its record (`GearPiece.Unique`): `Rarity.Unique` and its fixed affixes
  in order. A drop, a shop's stock, a map gift, a starting item and a restored save all give it; a drop or a stock of a
  unique draws nothing beyond its entry's chance and quantity.
- **868.** No save format change (`formatVersion` stays): a unique piece saves as any piece (base id, `unique`, affixes).
  On load a piece whose base is unique is rebuilt from its record, so an edited unique reaches every owned copy (ADR
  0029's "a changed base reaches every owned piece"); the repair reports a saved piece that differed from its record. A
  saved `unique` piece whose base is no longer unique (a hand edit of `items.json`) becomes a plain Common piece,
  reported. The pool clamp (decision 360) never touches a unique.
- **869.** The Forge never rerolls a unique (decision 699): `ForgeRefusal.FixedAffixes`, "A unique's affixes are
  fixed", the one new refusal. Upgrade and salvage stay refused by decision 339's rules (Highest rarity; no unique
  salvage row, which `CraftingRulesValidator` already requires).
- **870.** (default taken while the developer was away; his to overturn) A unique may be owned in several copies, and a
  drop table or a shop may list it like any item: no one-of-a-kind rule.
- **871.** A unique's price is its base price plus its fixed affixes' gold values, ADR 0031's formula unchanged; its
  name in a question is its own name, without the rarity word (`PieceName`, IU4).
- **872.** (default taken while the developer was away; his to overturn) One placeholder unique, `chiefs_cleaver`
  (design.md, section 1), is written to `items.json` and listed in no drop table and no shop: the developer places it.

Item effects:
- **873.** (Review B1, the coordinator's call.) Items keep a flat power: row 6's decision 785 stands, and T2's refusal of
  a tier on an item stays. The consumable form edits the effect's power and, for a damage, its damage type.
- **874.** Items naming tiered records are IU5, optional, run only on the developer's word (the open question below).
  If he picks it: an item's damage or heal names a record, its amount the user's Power times the record's `perPower`
  and nothing more; the user is the actor in battle and `FieldItemUse.Use`'s existing `user` from the menu (the target
  for a single-target item, the leader for an item that takes everyone, as `UseOn` passes it today); row 7's
  `CatalogReferences.Rebound` already rebinds items, and IU5 adds its item check to `OfTieredEffects` (row 7's
  decision 975).

Uniques and rolls:
- **875.** (default taken while the developer was away; his to overturn; loud) A base item never rolls Unique; a unique
  drops by its id only (ADR 0030's "Unique never rolls" kept). His "they can roll magic, rare, unique" may mean a base
  rolling a unique: the extension is a `unique` row in `rules.json` `loot` plus an optional `baseItem` on a unique,
  picked when that base rolls Unique; neither needs a save change (design.md, 2.6).
- **876.** (Withdrawn at approval: it held the tiered items' rebuild, which review B1 removed; row 7's
  `CatalogReferences.Rebound` covers items anyway, decision 874.)

The Items page (the mode bar's decision 934: a page from birth on its recipe):
- **877.** Database > Items: every item, sorted by category then id, the ones added this session after; Add consumable,
  Add equipment, Add unique, Add material, Duplicate, Remove (only an item added this session); an id typed while new
  and fixed once in the draft; the problem line (shown by `CatalogDialog`) the chosen item's own boxes first, then
  Data's for the items and what reads them (decision 863); OK applies the session as one undo step
  (`CatalogDraft.SetItems`).
- **878.** (default taken while the developer was away; his to overturn; review C3) An item's category (consumable,
  equipment, unique, material) is chosen by its Add button and read-only after: turning a potion into a sword, or a
  sword into a unique, is a new item (Duplicate keeps the category). The repair's "no longer unique" path (decision 868)
  stays for hand edits.
- **879.** The general fields: name, description, icon (an editable box over the icons folder's PNGs, a missing one
  noted, the picture shown), max stack (consumables and materials; a new equipment or unique gets 99, as every gear
  record has), price (empty: never traded) with "Sells for 50 (50 % share)", usable from the menu (consumables only).
- **880.** The equipment form (equipment and uniques): its kind (`EquipmentKind`), a weapon's damage and damage type
  (only for a weapon), and bonus rows: a stat from those a base item may give (not `StatKinds.AffixOnly`), each once,
  and its value.
- **881.** The consumable form: the effect's kind (damage, restoreHp, revive, rest), its targeting (those Data allows an
  item), its power (HP for a damage, a heal or a revive; none for a rest), a damage's damage type; the item's own strip
  (`effectFile`, an editable box over the effects images, empty plays its user's strip). An effect's statuses, hits and
  range are carried from the record, not edited.
- **882.** The unique form (uniques only): the fixed affix rows (the pool's affix by id with its stat and roll range
  shown for reference, its value), added after the chosen row, removed and moved; the piece's whole price and its sale
  line.
- **883.** An undo removing an item a map's event gives is refused, naming the map (as for shops, decision 581).

The Affixes page:
- **884.** Database > Affixes: one affix as typed (id while new; stat from every `StatKind`; min, max, weight and gold
  value; one box per equipment kind); Add, Duplicate, Remove (only an affix added this session); the problem line the
  affix's own boxes first, then Data's for the pool (`CatalogProblems.OfAffixes`; a unique's values don't depend on the
  range, decision 866); OK is one undo step (`CatalogDraft.SetAffixes`) that also re-resolves every unique's fixed
  affixes, so a stat changed on an affix reaches the uniques naming it.
- **885.** (Review C5.) The Affixes page's XAML is under about 80 lines (no OK, Cancel or problem line), so the page
  lands with its list and undo step in IU15; IU16 is merged there. New records start from the tunables in design.md,
  section 1.

The gear damage modifier:
- **886.** It lands here (IU17): `StatKind.DamageBonus`, a percent stat, whose sum over the worn gear joins row 6's
  `CombatStats.DamageModifierPermille` (WK, decision 786), raising tiered skills and the basic attack, never heals or
  items; the Status page lists it with the affix stats.
- **887.** (default taken while the developer was away; his to overturn) The damage bonus is affix-only
  (`StatKinds.AffixOnly`): base items can't give it, uniques give it through a fixed affix. No pool affix gets it in
  this plan: the developer makes one in the Affixes page.
- **888.** No weapon gives Power today (decision 696's "weapon gives power" is content: a weapon's Power bonus, the
  pool's `power` affix, which already rolls on weapons, or a unique's fixed affix; the placeholder unique has one). No
  repository weapon changes (default taken while the developer was away; his to overturn).

Structure:
- **889.** The Items page lives in Editor `Items/` (a new folder), the Affixes page in `Loot/` beside the Drop tables
  page; the Core and Data changes stay in their feature folders (design.md, 2.3).
- **890.** (Review B2.) Both editors are pages on the mode bar's recipe: `Items/ItemCatalogPage` and
  `Loot/AffixCatalogPage`, their catalog view models implementing `Workspaces/IEditedCatalog`, their editing view models
  `ICatalogEditing<T>` once the mode bar's WS2 has added it; an entry each in `Shell/DatabaseMenu.xaml` with its `Tag`
  ("Items", "Affixes"), an `OpenInDialog` opener in `MainWindow.Database.cs` (from WS5, the recipe's page lines).
  Needs WD1 and WD2.

## Defaults to confirm
Taken while the developer was away, each his to overturn; the loud ones first:
1. **875. A base never rolls Unique; uniques drop by id only.** "They can roll magic, rare, unique" may mean a base
   rolling a unique: the extension (a `unique` rarity row and a `baseItem` on a unique) needs no save change.
2. **865 (decision 681). A unique is its own item record** with fixed affixes, not a template over a base item.
3. **866. A unique's values are free within ±100** on their affix's side of zero, not bound by the affix's roll range.
4. **887. The damage bonus is affix-only,** and no affix in the pool gives it yet.
5. **878. An item's category, unique included, is fixed once added.**
6. **870. Uniques may be owned several times.**
7. **872, 888.** The placeholder unique (placed nowhere); no repository weapon gains Power.

## Open design questions (developer decides)
- **Flat or tiered items (IU5, decision 874).** Today, and in this plan, an item keeps its own flat power: a Potion
  heals 15 whoever uses it, a Hi-Potion 50, an Elixir 100, a Bomb deals 15 and Bomb All 30. Tiered (IU5): a Potion
  names `heal_1` and heals its user's Power x 2: 24 to 32 at level 1, about 4x that by level 20 (Power about 50: 100);
  an Elixir names a new `heal_3` (x 9: 108 to 144 at level 1); Bombs 24 to 32 and 48 to 64; a mage heals and bombs for
  less than a warrior (Power 12 against 16 at level 1). Say "tiered" to run IU5.
- For later: an affix only uniques carry; icon import; a per-shop or per-item sell price, should one share ever feel
  wrong.

## Notes for other rows
- **Row 7 (skill and status editors):** IU1 relies on its `Effect` filters (decision 973) and per-file converters
  (decision 972); its `CatalogReferences.Rebound` covers items (decision 975), so this plan adds no rebinding unless IU5
  runs.
- **Row 11 (the mode bar's stage 3):** the Items and Affixes pages get their `CatalogPage.Of` and `Register` lines with
  the others (the recipe's step 4); no conversion slice.

## ADRs needed
- **0058, unique items as records with fixed affixes** (amends ADR 0029, before IU2): decisions 864-872 and 875, with
  681, 866 and 875 written as defaults to confirm; options and costs in design.md, section 4.
- ADR 0030 gets an amendment line in IU17 (the damage bonus joins the affix-only stats). ADR 0056 gets one only if IU5
  runs. Neither takes a number.
