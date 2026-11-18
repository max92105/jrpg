# IU6. One item as the Items page types it: the general fields and the sale line

Editor, about 220 lines (about 120 of them tests and the shared fixtures). Needs IU1, IU3. Design: design.md, sections
2.3, 2.4 and 5.1; decisions 864, 878, 879.

## Goal
`ItemViewModel` holds one item's general fields as typed (id while new, name, description, icon, max stack, price, usable
from the menu), its category read-only (decision 878), its problem (the typed id, a box that isn't a number), the sale
line read from Core's formula, and the record it makes: its source record `with` the typed fields, so the effect, the
equipment and the fixed affixes are carried untouched until their forms (IU10-IU13). Duplicate copies every field but
the id, the category included; a new item from its category's starting record comes with the list (IU7). Every typed
box raises its change at once (the page's boxes bind on `PropertyChanged`, page-recipe.md step 3).

## New types
- `Editor/Items/ItemCategory` (internal enum: `Consumable`, `Equipment`, `Unique`, `Material`): what an item is to the
  Items page, read from its record.
  - `Editor/Items/ItemCategories` (internal static): `Of(ItemDefinition item)`: `IsMaterial`, then `IsUnique`, then
    `Equipment`, else consumable. IU7 adds the starting records beside it.
- `Editor/Items/ItemViewModel` (internal sealed partial, `ObservableObject`): one item's general fields as typed and the
  record they make.
  - `From(ItemDefinition item, int sellPercent)`, `New(ItemDefinition startingRecord, int sellPercent)`,
    `Duplicate()` (a new item, every field copied, the id empty).
  - `Id` (typed while `IsNew`), `Name`, `Description`, `IconFile`, `MaxStackText`, `PriceText` (empty: never
    traded), `IsUsableFromMenu`, `Category` (no setter), `IsNew`.
  - `Label`: "Unique · Chief's Cleaver (chiefs_cleaver)", "(new unique)" while it has no id.
  - `SaleLine`: "Sells for 50 (50 % share)" through `PriceFormula.SalePrice`; "Never bought nor sold" without a price;
    "" while the price box isn't a number.
  - `ShowsMaxStack` (false for equipment and uniques), `ShowsUsableFromMenu` (consumables only).
  - `Problem`: the typed id while new (`Objects/ObjectIds.ProblemOf`), then "Max stack must be a whole number (was
    'x').", "Price must be a whole number or empty (was 'x')." (`Catalogs/TypedNumber`); null otherwise. Data's checks
    (a name, ranges) come through the list (IU7).
  - `ToItem()`: the source `with` the typed fields; `InvalidOperationException` on a `Problem`.

## Tests
- `Editor.Tests/Items/ItemFixtures` (new, shared with IU7's and IU10-IU13's test files): a consumable, an armor, a
  weapon, a material and a unique built with `with`, and a small `ContentCatalog` over them.
- `Editor.Tests/Items/ItemViewModelTests` (new):
  - `From_Weapon_ShowsItsFieldsAndHidesMaxStack`.
  - `ToItem_NameAndPriceTyped_CarriesTheEquipmentUntouched` (the weapon's damage and bonuses equal the source's).
  - `ToItem_UniqueTyped_CarriesItsFixedAffixes`.
  - `ToItem_PriceEmpty_HasNoPrice`.
  - `SaleLine_Price100AtFiftyPercent_SaysFifty` and `SaleLine_PriceCleared_SaysNeverTraded`.
  - `SaleLine_PriceTyped_RaisesItsChange` (the notification).
  - `Problem_MaxStackNotANumber_NamesTheBox`, `Problem_PriceNotANumber_NamesTheBox`, `Problem_NewWithoutId_NamesTheId`.
  - `Problem_SavedItem_IgnoresTheIdRule` (a saved id with a capital letter isn't re-checked).
  - `Duplicate_Unique_IsANewUniqueWithEveryFieldButTheId`.
- `Editor.Tests/Items/ItemCategoriesTests` (new): `Of_Material_IsMaterial`, `Of_Weapon_IsEquipment`,
  `Of_UniqueWeapon_IsUnique`, `Of_Potion_IsConsumable`.

## Verify by hand
- None: no page yet (IU8). The tests are the check.
- **Done, with deviations:** `ItemFixtures.Catalog` keeps the test world's items beside the five fixtures (its shops,
  tables, Forge and new game name them); `ItemViewModel.Changed` (typed boxes only) added for IU7's relay; a new id is
  checked against no taken id (`ProblemOf(Id, [])`, as `EnemyViewModel`); eleven tests beyond the listed ones.
