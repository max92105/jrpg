# XP1. `rules.json` is written by the editor, every section in the writer's layout

Data + content, about 140 lines (about 90 of them tests) plus the relaid `content/rules.json`; about 40 more only if the
rarity keys need their converter (the contingency below). Needs ADR 0061, LV1. Design: sections 4, 6, 7 (risks 1, 2);
decision 955.

## Goal
Save all writes `rules.json` like the catalogs: the whole `GameRules` in the writer's layout, checked first as the
loader checks it (every section, the crafting materials against the draft's items, and LV1's class skill levels
against the table), only when it changed. The repository's file is relaid once (about 40 lines become about 110; the
sections in `GameRules`' order). Nothing edits the rules yet (XP2).

## Changes
- `Data/Content/ContentCatalogLoader.cs`: `RulesFile` becomes `internal`.
- `Data/Content/CatalogProblems.Rules.cs` (new partial): `public static string? OfRules(GameRules rules, ContentCatalog
  content)` and `internal static void CheckRules(GameRules rules, ContentCatalog content)`:
  `GameRulesValidator.Validate(RulesFile, rules)`, `CraftingRulesValidator.Validate(RulesFile, rules.Crafting,
  content.Items)`, then `CheckClassSkillLevels(content.Classes, rules.Experience)` (LV1), so a shortened table that
  strands a class skill is refused here too.
- `Data/Content/ContentWriter.Catalogs.cs`: `CheckRules(catalog.Rules, catalog)` in `CheckCatalogs`; `rules.json` in
  `SerializedCatalogs` (one object, no sort).
- `content/rules.json`: relaid once by the writer (a throwaway test, deleted after; the commit holds only the relay:
  `git diff` shows the same numbers, `walkSpeed` 3.0 written as 3).
- `docs/map.md`, Data `Content` row: `rules.json` written whole (ADR 0061); `CatalogProblems.OfRules`.

**Contingency (not in the estimate):** if the relay test shows `"Common"` keys (System.Text.Json writing an enum
dictionary key by its C# name), add `Data/Loot/RarityKeyConverter`: a `Rarity` read and written as a property name in
the files' spelling (`ContentJsonOptions.EnumName`), registered in `ContentJsonOptions.Strict`, with
`Loot/RarityKeyConverterTests` (`Write_Rare_WritesLowerCase`, `Read_UnknownRarity_ThrowsWithTheFilesMessage`). Never
`DictionaryKeyPolicy`: it would also rename the string keys other files hold. The Done note says whether it was needed.

## Tests
- `Content/ContentWriterRulesTests` (new; test-world copy):
  - `WriteMapCatalogs_Rules_LoadTheSameAfterRelay` (each section compared by content: the experience list, the rarity
    table and the crafting lists by sequence, the rest by value).
  - `WriteMapCatalogs_RelaidRules_SpellRaritiesAsTheFileDoes` (the written text holds `"common"`, not `"Common"`).
  - `WriteMapCatalogs_EmptyExperienceTable_WritesNothing` (the loader's message, the file's bytes unchanged).
  - `WriteMapCatalogs_CraftingNamingRemovedItem_WritesNothing` (the items without the material).
  - `WriteMapCatalogs_TableShorterThanASkillLevel_WritesNothing` (a class built in the test learning a skill at level 5,
    a table of three entries).
- `Content/CatalogProblemsRulesTests` (new): `OfRules_ValidRules_IsNull`; `OfRules_ZeroExperienceEntry_NamesIt`.
- `Content/ContentWriterRepositoryTests`: `CatalogFileBytes` gains `rules.json` (after the relay).

## Verify by hand
1. `git diff content/rules.json` in the relay commit: every number the same, one field per line, sections reordered.
2. Run the game: walk (same speed), win a battle (the bar fills by the same table), open the Forge (same costs).
3. Edit a map in the editor, Save all: `git status` shows no change to `rules.json`.
- **Done, with deviations:** `rules.json` joins the writer's catalogs and stamps (one record, `GameRules`' order),
  relaid once (39 lines to 127, the experience list one entry per line; every section the same by value, the
  simulations' lines identical); no rarity converter needed (the keys come out `"common"`). Decision 1020: `CheckRules`
  runs the sections' rules and the class skill levels (replacing the writer's own `CheckClassSkillLevels` call), not
  the Forge's materials, which `CheckItems` already checks on every save; `OfRules` runs both, the loader's order.
  `RulesFile` was already `internal`. The classes' sort is shared, `CatalogProblems.ClassesById` (third copy). Beyond
  the list: `WriteMapCatalogs_UnchangedRules_LeavesTheFile`, `OfRules_TableShorterThanASkillLevel_NamesTheClass`,
  `OfRules_CraftingNamingRemovedItem_NamesIt`; `ContentCatalogLoaderCraftingTests` edits the file through `JsonNode`
  (its text replace no longer matched the new layout). Saves untouched. Sizes: Data `CatalogProblems.Rules.cs` 38,
  `ContentWriter.Catalogs.cs` 160; tests `ContentWriterRulesTests` 135, `CatalogProblemsRulesTests` 55.
  After review: with both a rules problem and an item problem, Save all names the item's first (`CheckItems` runs before
  `CheckRules`) where the loader names the rules'; the problem line (`OfRules`) keeps the loader's order.
