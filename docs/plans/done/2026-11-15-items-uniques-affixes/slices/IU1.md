# IU1. The writer writes `items.json` and `affixes.json`, relaid once in its layout

Data + content, about 200 lines (about 110 of them tests), plus the two relaid files (raw JSON, counted apart). Needs
rows 6 and 7 (README, "Before this plan starts"). Design: design.md, sections 2.1 and 3.1; decisions 862, 863.

## Goal
`ContentWriter` checks and writes `items.json` and `affixes.json` with the other catalog files, only when their content
changed (`CatalogFileStamps`); both repository files are relaid once in the writer's layout, so saving the editor with
no item or affix change writes them byte for byte. No page yet: the writer's tests are its users.

## Changes
- `Data/Content/ContentWriter.Catalogs`: `CheckCatalogs` calls `CatalogProblems.CheckItems(SortedItems(catalog),
  catalog)` and `CatalogProblems.CheckAffixes(SortedAffixes(catalog))`; `SerializedCatalogs` adds
  `ContentCatalogLoader.ItemsFile` and `AffixesFile`; `SortedItems`, `SortedAffixes` (by id, ordinal).
- `Data/Content/ContentCatalogLoader`: `AffixesFile` becomes `internal` (the writer names it).
- `Data/Content/CatalogProblems`:
  - `internal static void CheckItems(IReadOnlyList<ItemDefinition> items, ContentCatalog content)`: "the checks the
    loader makes that read `items.json`" (decision 863): `ItemCatalogValidator.Validate`, then
    `CraftingRulesValidator.Validate(RulesFile, content.Rules.Crafting, itemsById)`, `ShopCatalogValidator` and
    `DropTableCatalogValidator` over the content's shops and tables against the edited items, and the new game's
    starting items through `NewGameValidator` with the edited items. The slice reads the loader for the exact list and
    keeps its order.
  - `internal static void CheckAffixes(IReadOnlyList<AffixDefinition> affixes)`: `AffixCatalogValidator.Validate`.
  - The public `OfItems` and `OfAffixes` come with their first callers (IU7, IU15).
- `Data/Content/AbsentWhenNullFilter`: `(typeof(ItemDefinition), "effect")`, `"equipment"`, `"price"`,
  `"effectFile"`; `(typeof(EquipmentStats), "damageType")`, each with its reason comment (ADR 0031: an item nobody
  trades has no price; decision 678: an item plays its user's strip without one; only a weapon has a type).
- `Data/Content/AbsentWhenFalseFilter`: `(typeof(ItemDefinition), "isMaterial")` (decision 339).
- `Data/Content/AbsentWhenZeroFilter`: `(typeof(EquipmentStats), "damage")` (decision 330: only a weapon swings).
- `Data/Content/AbsentWhenEmptyFilter`: `(typeof(EquipmentStats), "bonuses")`, and the skip predicate also takes a
  `StatBonuses` naming no kind (it isn't an `ICollection`).
- `Data/Items/StatBonusesConverter.Write`: writes the kinds in `StatKind` order (a frozen dictionary has none), the
  summary saying why.
- An item's `effect` is written through row 7's type-based `Effect` filters (its decision 973: `statuses` empty,
  `range` 0, `hits` 1, `tier` and `damageType` null left out); items name no tier (decision 873), so no reference
  converter is registered on `items.json`'s write here. If row 7 hasn't landed, this slice adds those filters, about
  40 lines, and row 7's slice drops them.
- **Files (recount at the start):** `CatalogProblems.cs` 130 on 2026-11-15 plus row 7's three entries, +20 here;
  `ContentWriter.Catalogs.cs` 81 plus row 7's, +6; `StatBonusesConverter.cs` 59, +2; the four filter files under 75.
- `content/items.json` and `content/affixes.json`: relaid through the writer (a throwaway test or the editor's save),
  sorted by id; values unchanged, the bomb's `effectFile` kept (design.md, 3.1). Check the diff shows only layout.

## Tests
- `Data.Tests/Content/ContentWriterRepositoryTests` (81 lines): `CatalogFileBytes` adds `items.json` and
  `affixes.json`, so `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes` covers them.
- `Data.Tests/Content/ContentWriterItemTests` (new; a `CopiedContentRoot` over the test world, records changed with
  `with` in the test):
  - `WriteMapCatalogs_ItemPriceChanged_WritesItemsJsonWithTheNewPrice` (loaded back: the price).
  - `WriteMapCatalogs_ShopItemLosesItsPrice_RefusesNamingShopsJson` (decision 863: nothing written).
  - `WriteMapCatalogs_MaterialStackBelowTheForgesCost_RefusesNamingRulesJson` (`CraftingRulesValidator`'s quantity
    within the material's stack).
  - `WriteMapCatalogs_AffixRangeChanged_WritesAffixesJson`.
  - `WriteMapCatalogs_AffixWithNoKind_RefusesNamingAffixesJson`.
  - `Write_PlainArmor_LeavesOutEveryDefaultField` (no `effect`, `price`, `effectFile`, `isMaterial`, `damage`,
    `damageType`; an armor built in the test).
- `Data.Tests/Items/StatBonusesConverterTests`: `Write_BonusesBuiltOutOfKindOrder_WritesThemInKindOrder` (a
  dictionary built with `fireResistance` before `constitution`).

## Verify by hand
- Run the editor, change nothing, Save all: `git status` shows no content change.
- Database > Shops..., change nothing that touches items, Save all: `items.json` and `affixes.json` unchanged.
- **Done, with deviations:** `RulesFile` became `internal` too (`CheckItems` names it); `items.json` is written with
  row 7's reference converters, as an item's effect may grant a status (`..._ItemGrantingAStatus_WritesTheStatusId`);
  the relay escapes a description's plus sign, as `skills.json` has it.
