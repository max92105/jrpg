# Items, uniques and affixes: design

Read with the [README](README.md). Sections: 1 tunables, 2 design overview, 3 the content changes, 4 ADR 0058's
options, 5 the two pages, 6 risks.

## 1. Tunables and placeholders

Every number is content or an editor starting value; "placeholder" means the developer replaces it.

| Name | Where | Value | Meaning |
|---|---|---|---|
| Sell share | `rules.json` `trade.sellPercent` | 50 (unchanged) | What every sale pays of a price (decision 864). |
| Placeholder unique | `items.json` `chiefs_cleaver` | Chief's Cleaver, weapon, damage 28 slashing, price 400, fixed `power` 5 and `crit_chance` 3, `Weapon.png` (placeholder) | Decision 872; placed in no table or shop. |
| Fixed value bound | `ItemCatalogValidator.MostBonus` | ±100 (existing placeholder bound) | Decision 866: a unique's value is free within it, on its affix's side of zero. |
| Forge refusal line | `ui.json` `forge_refused_fixed_affixes` | "A unique's affixes are fixed" | IU4. |
| Damage bonus's name | `ui.json` `stat_damage_bonus` | "Damage bonus" | IU17. |
| New consumable | Items page | `Consumable.png`, max stack 99, usable from the menu, `restoreHp` at one ally, power 10, no price | Decision 885. |
| New equipment | Items page | `Armor.png`, max stack 99, kind `chest`, no bonus, no price | Decision 885. |
| New unique | Items page | the new equipment's record with `isUnique` true and no fixed affix | Decisions 878, 885. |
| New material | Items page | `Item.png`, max stack 99, no price | Decision 885. |
| New affix | Affixes page | stat `power`, min 1, max 1, weight 10, gold value 20, no kind (Data's problem names it until one is ticked) | Decision 885. |
| New fixed affix row | Items page | the chosen pool affix, value its range's min | IU13. |

No Core constant is added. IU5's numbers (optional) are in its file.

## 2. Design overview

### 2.1 What exists (2026-11-15, rows 6 and 7 and the mode bar's stage 1 done)
| Need | Today | This plan |
|---|---|---|
| Base items | `Items/ItemDefinition` (effect or equipment, `IsMaterial`, `Price`, `EffectFile`), `items.json` hand-written (inline objects, `price` before `isUsableFromMenu`; the bomb names `FireExplosion.png`) | Written by the editor (IU1), edited in Database > Items (IU6-IU13) |
| Rarities | `Items/Rarity` (Common, Magic, Rare, Unique reserved), `rules.json` `loot` (ADR 0030); the Game names and colors Unique already (`RarityNames`, `UiTheme.RarityUnique`) | Unique used, by id only (IU2, decision 875) |
| Affixes | `Loot/AffixDefinition` (stat, range, kinds, weight, gold value), `affixes.json` one line per record | Written by the editor (IU1), edited in Database > Affixes (IU14, IU15) |
| Rolling | `Loot/GearRoll` (drops through `DropRoll`, shop stocks through `Session/ShopStocks`, the Forge's upgrade and reroll); `Inventory.Add` makes `GearPiece.Plain` pieces | A unique comes as its own piece, no draw (IU2) |
| Saving a piece | `SavedGear` (base id, rarity, affix ids and values); `ContentCatalog.PieceFrom`; `Data/Saving/GearRepair.RepairAffixes` clamps to the pool (shared with `ShopStockRepair`) | A unique rebuilt from its record (IU2, IU4); no format change |
| The Forge | `Crafting/Forge`: salvage by rarity row (none for Unique), upgrade to Rare at most, reroll any piece with affixes | Reroll refused for a unique (IU4) |
| Naming a piece | `Game/Ui/PieceName.Of`: the rarity first for any rolled piece ("Unique Chief's Cleaver") | A unique by its own name (IU4) |
| Prices | `Trade/Pricing`, `PriceFormula.PieceValue`, `SalePrice`; one `sellPercent` (ADR 0031) | Unchanged; shown in the Items page (decision 864) |
| Item effects | Flat power, landed unscaled (`EffectLanding.LandFromItem`, `FieldItemUse`, decision 321); row 6's T2 refuses a tier on an item (decision 785) | Unchanged (decision 873); tiered items are IU5, optional |
| "+% damage" on gear | Nothing; row 6's WK adds `CombatStats.DamageModifierPermille`, statuses only (decision 786) | `StatKind.DamageBonus` (IU17) |
| No weapon gives Power | Weapons' bonuses: only the Magic Staff's Intelligence; the `power` affix rolls on weapons; the Power Ring gives +5 | Content, unchanged (decision 888) |
| Database editors | Pages on the mode bar's recipe, opened in `Workspaces/CatalogDialog` from `Shell/DatabaseMenu.xaml` (WD1, WD2) | Two new pages (IU8, IU15) |

### 2.2 Core changes, by slice
| Slice | Change |
|---|---|
| IU2 | `ItemDefinition.IsUnique`, `FixedAffixes` (and its equality, fixed affixes in order); `GearPiece.Unique(item)`, `GearPiece.Unrolled(item)`; `Inventory.Add` gives `Unrolled`; `GearRoll.Roll` returns a unique's piece first; `ContentCatalog.PieceFrom` rebuilds a unique from its record. |
| IU4 | `ForgeRefusal.FixedAffixes`; `Forge.QuoteReroll` refuses a unique. |
| IU5 (optional) | `Effects/ItemEffectScaling`; `EffectLanding.LandFromItem` and `FieldItemUse.Use`/`HpAfterUse` land through it with the user's Power. |
| IU17 | `StatKind.DamageBonus` in `StatKinds.AffixOnly`; `StatSheet.DamageBonusPermille`; `Party/MemberSheet` sums it from gear; `Battle/MemberBattleStats` copies it into `CombatStats.DamageModifierPermille`. |

### 2.3 Structure and folder dependencies
New types (one responsibility each):
- Data `Items/FixedAffixConverter` (IU3): reads a unique's `{ affixId, value }` as a `RolledAffix` with the pool's
  stat, and writes it back without the stat.
- Editor `Items/ItemCategory` (IU6): what an item is to the Items page (consumable, equipment, unique, material), read
  from its record; `Items/ItemCategories` beside it (`Of`, from IU7 `StartingRecord`).
- Editor `Items/ItemViewModel` (IU6): one item's general fields as typed, and the record they make.
- Editor `Items/ItemCatalogViewModel` (IU7; `IEditedCatalog` from IU8): every item as the page lists, adds and removes
  them, and the problem line.
- Editor `Items/ItemEditingViewModel` (IU7): starts the page's session from the draft and applies it as one undo step.
- Editor `Items/ItemCatalogPage` (IU8): the page.
- Editor `Items/ItemEquipmentViewModel` (IU10, rows from IU11): an equipment's kind, damage, type and bonuses as typed.
- Editor `Items/StatBonusRow` (IU11): one bonus as typed, a stat and its value.
- Editor `Items/ItemEffectViewModel` (IU12): a consumable's effect as typed: kind, targeting, power, damage type.
- Editor `Items/UniqueAffixesViewModel` (IU13): a unique's fixed affix rows as typed and the price they make.
- Editor `Items/FixedAffixRow` (IU13): one fixed affix of a unique as typed: the affix and its value.
- Editor `Items/AffixChoice` (IU13): one pool affix the row box offers, with its stat and roll range.
- Editor `Loot/AffixViewModel` (IU14): one affix as typed and the record it makes.
- Editor `Loot/EquipmentKindToggle` (IU14): one equipment kind's box on an affix.
- Editor `Loot/AffixCatalogViewModel` (IU15): every affix as the page lists, adds and removes them, and the problem line.
- Editor `Loot/AffixEditingViewModel` (IU15): starts the page's session from the draft and applies it as one undo step.
- Editor `Loot/AffixCatalogPage` (IU15): the page.
- Core `Effects/ItemEffectScaling` (IU5, optional): an item's effect as its user lands it, a tiered amount from the
  user's Power alone.

| From | To | Why | New? |
|---|---|---|---|
| Core `Items` | `Stats`, `Effects` | `RolledAffix.Stat`, `ItemDefinition.Effect` | existing; `Items` still never reads `Loot` |
| Core `Loot` | `Items` | `GearRoll` reads `IsUnique`, makes `GearPiece.Unique` | existing |
| Core `Crafting` | `Items`, `Loot` | the refusal reads `Base.IsUnique` | existing |
| Core `Content` | `Items`, `Loot` | `PieceFrom` | existing |
| Data `Items` | Core `Items`, `Loot`, `Stats`; Data `Content` | the converter, the unique rules | Core `Loot` new for `Data/Items` (`Data/Loot` already reads `Data/Items`; no cycle between Data folders is enforced, and none forms: `Data/Items` reads no `Data/Loot` type) |
| Data `Content` | Data `Items`, `Loot`, `Crafting`, `Trade` | `CatalogProblems.CheckItems`, `OfItems`, `OfAffixes` | existing folders |
| Data `Saving` | Core `Items` | the unique repair | existing |
| Game `Ui` | Core `Items` | `PieceName` reads `Rarity.Unique` | existing |
| Editor `Items` | Core `Items`, `Effects`, `Loot`, `Stats`, `Trade`; Editor `Catalogs`, `Objects` (id rule), `Events` (`ListOrder`), `Workspaces` (`IEditedCatalog`) | the forms and the page | new folder; only `Shell` reads it |
| Editor `Loot` | Core `Loot`, `Items`, `Stats`; Editor `Catalogs`, `Objects`, `Workspaces` | the Affixes page | existing |
| Editor `Shell` | Editor `Items`, `Loot` | the openers | existing pattern |

No cycle: Core `Items` stays below `Loot` (the fixed affix is a `RolledAffix`, which lives in `Items`).

### 2.4 Key decisions (README numbers)
- **A unique is an item record (865-867):** the record holds `IsUnique` and `FixedAffixes` as resolved `RolledAffix`
  lines, so `GearPiece.Unique(item)` needs no catalog and `Inventory.Add`, `GearRoll.Roll` and `PieceFrom` all build the
  same piece. `GearPiece.Unrolled(item)` is the one entry for "a piece as an item gives it with nothing rolled": the
  unique's, else `Plain`. `GearPiece.Plain` and `Rolled` keep their meaning and their 144 test uses.
- **Free fixed values (866):** a unique's value is checked against the bonus bound and its affix's sign, never its
  roll range, so the Affixes page can narrow a range without touching any unique, and nothing cross-checks the two
  files.
- **The record is the truth for a saved unique (868):** `PieceFrom` and the repair read the record, never the saved
  affixes; the save shape and `formatVersion` don't change.
- **Uniques drop by id (875):** `GearRoll.RollRarity` never yields Unique (no rules row); a unique arrives only where a
  table, a shop, a gift or the new game names its id.
- **Items keep a flat power (873):** nothing in Core or Data changes for item effects; the consumable form edits the
  power as `items.json` holds it.
- **Pages on the recipe (890):** `Start`/`Apply` on an editing view model; a catalog view model with `Problem`
  (`RecordListProblem.FirstOf`, then `CatalogProblems.OfItems` or `OfAffixes`), `ChosenKey`, `Choose`, and `ToItems` or
  `ToAffixes` throwing on a problem; each record's view model keeps its source record and builds `source with { ... }`,
  so what no form edits (an effect's statuses, hits and range) is carried (as `EnemyViewModel.ToEnemy`).
- **Uniques follow their affixes' stats in the draft (884):** `CatalogDraft.SetAffixes` replaces the pool and every
  unique's fixed affixes with the pool's stats in one `CatalogEdit`; an undo restores both.

### 2.5 IU5's two shapes, if the developer picks tiered items (decision 874)
- **The user's Power (IU5 as written):** `LandFromItem` has its actor; `FieldItemUse.Use` already has a `user` (the
  target for one, the leader for all, as `UseOn` passes it), and `HpAfterUse` takes the same user, so the preview and
  the use agree. No rules field.
- **One fixed item Power in `rules.json`** (`items.power`, 10: Potion 20, Hi-Potion 40, Elixir 90): keeps decision 321
  ("an item isn't raised by its user") whole, but needs `GameRules` to reach `BattleState`, which takes no rules today
  (`BattleState.Start`, 12 test call sites over 10 files, `BattleState.cs` at 283 lines), and a third constructor
  argument for `FieldItemUse`. About one more slice.

### 2.6 Extension points
- **A base rolling Unique (decision 875's alternative):** a `unique` row in `rules.json` `loot` (`LootRulesValidator`
  refuses one today) and an optional `baseItem` on a unique record; when `GearRoll.RollRarity` lands on Unique for a
  base, it gives one of the uniques naming that base (a weighted pick, or a reroll to Rare when none does). The piece is
  still the unique's own, so no save change.
- **An affix only uniques carry:** allow `weight` 0 in `AffixCatalogValidator` and skip weight 0 in
  `GearRoll.CandidatesFor` (`WeightedPick` refuses an all-zero list); content only after that.
- **Fixed values bound by the roll range** (decision 866's alternative): a range rule in the unique check plus the
  pool cross-check in `OfAffixes`; the repair already reads the record.
- **An icons import:** an `icons` kind for `ContentWriter.ImportArtImage` and an Import... beside the icon box.
- **Items granting statuses:** the consumable form reuses row 7's status rows.
- **Tiered items:** IU5 (2.5).

### 2.7 Replacement mapping
| Today | Becomes | Slice |
|---|---|---|
| `items.json`, `affixes.json` hand-written | written by `ContentWriter` | IU1 |
| `Inventory.Add` making `GearPiece.Plain` | `GearPiece.Unrolled` (a unique's piece, else plain) | IU2 |
| `GearRoll.Roll` rolling every base | a unique's piece first, no draw | IU2 |
| `PieceFrom` from the saved affixes | a unique from its record | IU2 |
| `GearRepair.RepairAffixes` clamping every piece | a unique's record taken, others clamped | IU4 |
| `Forge.QuoteReroll` refusing only a piece without affixes | refusing a unique first | IU4 |
| `PieceName.Of` putting the rarity first on a unique | the unique's own name | IU4 |
| Hand-editing `items.json`, `affixes.json` | Database > Items, Database > Affixes | IU6-IU15 |

## 3. The content changes

### 3.1 The relay (IU1)
`items.json` keeps its records and values; what moves: records sorted by id (`amber_amulet` first, `wooden_shield`
last), every nested object one field per line, `price` after `isMaterial` (the record's order: `id`, `name`,
`description`, `iconFile`, `maxStack`, `isUsableFromMenu`, `effect`, `equipment`, `isMaterial`, `price`,
`effectFile`), an equipment's fields `kind`, `damage`, `bonuses`, `damageType`. The bomb's `"effectFile":
"FireExplosion.png"` (row 2) is kept. `affixes.json` becomes one field per line with `kinds` one per line, sorted by id.

### 3.2 The placeholder unique (IU3)
```json
  {
    "id": "chiefs_cleaver",
    "name": "Chief's Cleaver",
    "description": "A notched goblin blade that still bites. Placeholder: the developer writes the real unique.",
    "iconFile": "Weapon.png",
    "maxStack": 99,
    "isUsableFromMenu": false,
    "equipment": {
      "kind": "weapon",
      "damage": 28,
      "damageType": "slashing"
    },
    "price": 400,
    "isUnique": true,
    "fixedAffixes": [
      {
        "affixId": "power",
        "value": 5
      },
      {
        "affixId": "crit_chance",
        "value": 3
      }
    ]
  },
```
(the exact layout is the writer's; IU3's identical-bytes test decides it). Its price in a shop: 400 + 20 + 20 = 440;
sold: 220.

## 4. ADR 0058's options, honestly

**A. A unique is its own item record with fixed affixes (chosen; decision 681, the roadmap's direction, and 699;
recorded as a default to confirm).** `isUnique` and `fixedAffixes` on `items.json`; the piece is `Unique` with the
record's affixes; uniques drop by id (decision 875).
- Pros: no save change (a unique piece saves as any piece); the record is the truth, so an edited unique reaches every
  owned copy; drops, shops, gifts and starting items name it by id with no new mechanism; the Items page edits it beside
  the base items. Three Core slices (IU2-IU4) and one form (IU13).
- Cons: a unique is a whole item (name, icon, damage) even when it differs from a base item by its affixes only; the
  saved affixes are redundant (kept so the shape doesn't change); a base never rolls one (decision 875) until the
  extension in 2.6.

**B. A unique as a template naming a base item (`uniques.json`: a base id, a name, fixed affixes).**
- Pros: shares the base's numbers; the list of base items stays short; a base rolling Unique picks a template naturally.
- Cons: a piece must say which template it is: `SavedGear` gains a unique id, a format bump (ADR 0022), a repair case
  for a removed template; every reader of a piece's name and icon checks the template first (about 15 Game sites); a
  second Database page. About four more slices.

**C. Values rolled once on the drop within the affix ranges, then fixed (the action RPG way).**
- Pros: two copies of a unique can differ, a reason to farm.
- Cons: against decision 699 ("on a unique they are fixed"); an edited unique can't reach owned copies.

**D. A drop entry carrying fixed affixes (no record).**
- Cons: a shop or a map gift can't give it; nothing to edit by name. Refused.

## 5. The two pages

Both follow [page-recipe.md](../2026-11-15-mode-bar/page-recipe.md): a `UserControl` hosted in `CatalogDialog` (which
shows the title, the problem line, OK and Cancel at the main window's size), typed boxes on `PropertyChanged`, bindings
to the page's DataContext through `AncestorType` the page type.

### 5.1 Database > Items (IU8, then the forms)
- **Left (300 px):** the list, each row "Equipment · Bronze Sword (bronze_sword)"; under it Add consumable, Add
  equipment, Add unique, Add material, Duplicate, Remove.
- **Right, General:** Id (read-only once in the draft), Name, Description (two lines), Icon (an editable box and the
  picture at 2x, a note when the file is missing), Max stack (hidden for equipment and uniques), Price and "Sells for
  50 (50 % share)", Usable from the menu (consumables).
- **Right, by category:** Equipment and Unique: Kind, Damage and Damage type (a weapon), the bonus rows with Add bonus
  and Remove; a unique adds its fixed affix rows with their roll-range notes and the piece's price line (IU13).
  Consumable: Effect, Target, Power, Damage type (a damage), Own strip. Material: nothing more.

### 5.2 Database > Affixes (IU15)
The list left (each row "power: Power 2..5"), Add, Duplicate, Remove; right: Id, Stat, Min, Max, Weight, Gold value,
then "Rolls on" with one box per equipment kind (eight, two columns). About 65 lines of XAML.

## 6. Risks
- **Row 7's writer work (IU1):** the relay needs an `Effect` written in the writer's layout (row 7's decision 973). If
  row 7 hasn't landed or wrote it differently, IU1 adds the `Effect` filters itself (named in its file, about 40 lines
  more) and the identical-bytes test shows any other gap.
- **`StatBonuses` order:** `StatBonuses.Values` is a frozen dictionary with no order; IU1 writes bonuses in `StatKind`
  order, which every repository record already follows (checked 2026-11-15), so the relay changes no bonus line.
- **Saves (IU2, IU4):** no format change; a save holding a piece of an item later made unique by hand is rebuilt and
  reported; covered by round-trip and repair tests.
- **Determinism:** a unique's drop draws its entry's chance and quantity only; IU2 pins it with a `FixedSequenceRandom`
  holding no extra value. Shop stocks listing a unique draw one draw fewer than a rolled base would.
- **Resolved references in the draft:** a unique holds `RolledAffix` lines with stats; `SetAffixes` re-resolves them
  (IU15). A record compared by reference would look changed: `ItemDefinition` compares its fixed affixes in order (IU2).
- **Balance (decision 866):** a unique's line may exceed any roll (a Power +30 ring); that is the point, and his to
  tune.
- **Hot files (each slice recounts):** `MainViewModel.cs` 285 (+2 over the plan); `CombinedHistory.cs` 221 (+15 in
  IU9); `CatalogProblems.cs` 130 (row 7's entries, then about 25 here); `CatalogDraft.Database.cs` 118 (row 7's steps,
  then about 35 here); `ItemCatalogValidator.cs` 130 (+25 in IU3); `GearRepair.cs` 136 (+25 in IU4).
- **The Status page (IU17):** a fifth affix stat needs a third row in the Affixes section; IU17 writes the arithmetic
  and checks the page against 1080 with an F12 capture.
- **Save-format discipline:** if any slice finds it must change `SavedGear`, it stops: that is ADR 0058's option B, the
  developer's call, with ADR 0022's bump and an old-format refusal test.
