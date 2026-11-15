# IU3. `items.json` holds uniques: `isUnique` and `fixedAffixes`, loaded and checked; a placeholder unique

Data + content, about 240 lines (about 150 of them tests), plus the placeholder record. Needs IU1, IU2. Design:
design.md, sections 2.3, 2.4 and 3.2; decisions 865, 866, 872.

## Goal
A record in `items.json` may say `"isUnique": true` and list `fixedAffixes` (`{ "affixId", "value" }`); it loads with
each affix's stat resolved from `affixes.json`, read first. Data refuses a unique that isn't equipment, fixed affixes on
an item that isn't unique, an affix the pool lacks (naming the file, path and id), an affix twice, a value of 0 or past
the bonus bound (±100), a value on the other side of zero from its affix's range (above 0 for an affix-only stat), and
a null entry. The value isn't bound by the affix's roll range (decision 866). The writer writes both fields back, left
out at their defaults. `chiefs_cleaver` is the placeholder unique, placed nowhere.

## New types
- `Data/Items/FixedAffixConverter` (internal sealed, `JsonConverter<RolledAffix>`): reads a unique's fixed affix as a
  `RolledAffix` with the pool's stat, and writes it back as its id and value.
  - `FixedAffixConverter(IReadOnlyDictionary<string, AffixDefinition> pool)`.
  - `Read`: an object of exactly `affixId` (a string) and `value` (a whole number); an unknown id is
    `JsonException("no affix '{id}'.")`, which the reader turns into "items.json at $[3].fixedAffixes[0]: no affix 'x'.";
    another shape names what is wrong.
  - `Write`: `{ "affixId", "value" }`, one field per line as the writer lays nested objects; the pool isn't read (the
    writer's instance holds an empty one).

## Changes
- `Data/Content/ContentCatalogLoader.Load`: `affixes.json` read and validated before `items.json`; `items.json` read
  with `ContentJsonOptions.StrictWith(new FixedAffixConverter(affixesById))`; the dictionary built once and reused for
  the catalog.
- `Data/Content/ContentWriter.Catalogs`: `items.json` serialized with `FixedAffixConverter` registered on that file's
  write only, as row 7 registers its converters on `skills.json`'s (its decision 972); IU1's `SerializedCatalogs` line
  for items takes those options.
- `Data/Items/ItemCatalogValidator`: `Validate(fileName, items, pool)` and `FindUniqueProblem` in `FindProblem`'s chain
  after the equipment's: "isUnique needs equipment: a unique is worn (ADR 0058)."; "fixedAffixes are only for a unique
  (isUnique true)."; "fixedAffixes[1] is null."; "fixedAffixes names 'power' twice."; "fixedAffixes[0] (power): value
  must be nonzero and within -100..100 (was 0)." (`MostBonus`, as an item bonus); "fixedAffixes[0] (power): value must
  be above 0, as affix 'power' rolls (was -3)." for an affix whose range is positive or an affix-only stat, and the
  mirror for a negative range (decision 866). The loader and `CatalogProblems.CheckItems` pass the pool
  (`content.Affixes`). `ItemCatalogValidator.cs` is 130 lines on 2026-11-15, about 155 after; past 200 the unique rules
  go to `Items/UniqueItemRules` in this slice.
- `Data/Content/AbsentWhenFalseFilter`: `(typeof(ItemDefinition), "isUnique")`;
  `Data/Content/AbsentWhenEmptyFilter`: `(typeof(ItemDefinition), "fixedAffixes")`.
- `content/items.json`: `chiefs_cleaver` (design.md, 3.2) in id order, in the writer's layout.
- `content/art/...`: nothing (`Weapon.png` exists).

## Tests
- `Data.Tests/Items/FixedAffixConverterTests` (new; a pool built in the test):
  - `Read_KnownAffix_ResolvesItsStat`.
  - `Read_UnknownAffix_ThrowsNamingIt`.
  - `Read_ValueMissing_Throws`.
  - `Read_NumberInsteadOfObject_Throws`.
  - `Write_FixedAffix_WritesItsIdAndValueOnly`.
- `Data.Tests/Items/ItemCatalogValidatorUniqueTests` (new: `ItemCatalogValidatorTests` is at 248; items and a pool
  built in the test): an `InvalidUniques` theory (a unique consumable; a unique material; fixed affixes on a plain
  sword; an affix twice; a value of 0; 101; -3 on a positive affix; 2 on a negative one; -1 on lifesteal; a null
  entry), and `Validate_UniqueValueAtTheBound_Accepted` (100), `Validate_UniqueValueAboveItsAffixsRollRange_Accepted`
  (7 on a 2..5 affix), `Validate_UniqueWithNoFixedAffix_Accepted`,
  `Validate_UniqueNamingAnAffixThatRollsOnOtherKinds_Accepted` (decision 866).
- `Data.Tests/Content/ContentCatalogLoaderUniqueTests` (new: `ContentCatalogLoaderTests` is at 272; `CopiedContentRoot`
  over the repository, one record edited in the test where named):
  - `Load_RepositoryPlaceholderUnique_HoldsFixedAffixesWithTheirStats` (`chiefs_cleaver` is unique; each fixed affix's
    stat is its pool affix's; no value pinned).
  - `Load_UniqueNamingAnUnknownAffix_RefusesNamingFileAndId`.
  - `Load_AffixesFileMissing_RefusesNamingIt`.
- `Data.Tests/Content/ContentWriterItemTests` (IU1's): `WriteMapCatalogs_UniqueAdded_WritesItsFixedAffixesBack` (loaded
  again: equal); IU1's repository bytes test covers the placeholder's layout.

## Verify by hand
- Run the game: it starts. Run the editor, Database > Drop tables, add Chief's Cleaver to the `goblin` table at 100 %,
  OK, Save all; run the game, Title > New game, win a goblin fight: the victory's Items step lists Chief's Cleaver in the
  Unique orange; Menu > Inventory shows it with "+5 Power" and "+3 % crit chance". Revert `content/drop-tables.json`
  (`git checkout`).
- Set its `power` value to 0 in `content/items.json`, run the game: the start stops on "items.json: item
  'chiefs_cleaver': fixedAffixes[0] (power): value must be nonzero and within -100..100 (was 0)."; set it to 9: the
  game starts (above the 2..5 roll range, decision 866); revert.
