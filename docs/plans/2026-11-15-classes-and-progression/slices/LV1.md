# LV1. A class's skill entry may name a level: the file shape, its checks, every level 1

Core + Data + content, about 240 lines (about 150 of them tests, about 40 of them changed test sites). Needs CL3 and ADR
0062. Design: section 5; decisions 959, 965, 970.

## Goal
`classes.json` lists a class's skills as entries, each a skill id with an optional level: `"skills": [ { "skillId":
"fire_blast" }, { "skillId": "meteor", "level": 10 } ]`, the level left out at 1. Data refuses a null entry, a level
below 1, and a level past the experience table's max (it could never be learned), naming the class and the skill. The
repository's file is converted with every level 1, and nothing changes in play: this slice reads the levels but no rule
uses them yet (LV2).

## New types
- `Core/Party/ClassSkill` (public sealed record): one skill of a class with the level it is learned at.
  `ClassSkill(string SkillId)` with `public int Level { get; init; } = 1;` (a sealed record, so a missing level reads
  1, never `default`'s 0).
- `Data/Party/ClassSkillLevelCheck` (internal static): every class skill's level within the experience table, once both
  are read. `Check(string fileName, IReadOnlyList<ClassDefinition> classes, ExperienceTable table)`: "classes.json:
  class 'mage': skills lists 'meteor' at level 30, past the max level 21."

## Changes
- `Core/Party/ClassDefinition`: `public IReadOnlyList<ClassSkill> Skills { get; init; } = [];` (in menu order, ADR
  0020); `SkillIds` becomes computed, `[.. Skills.Select(skill => skill.SkillId)]`, every entry's id whatever its level
  (left out of the file by `ComputedPropertyFilter`), so its readers (`PartyMember`, Data's checks, row 7's balance
  table and "Learned by" line) don't change (decision 970). Its summary names ADR 0062.
- `Data/Party/ClassCatalogValidator`: a null entry in `skills` ("skills must not contain a null entry", before the id
  checks that read `SkillIds`); a level below 1 ("skills lists 'meteor' at level 0; levels start at 1").
- `Data/Content/ContentCatalogLoader`: `ClassSkillLevelCheck.Check(ClassesFile, classes, rules.Experience)` right after
  the rules are read (the classes come first), as the crafting check runs after the items.
- `Data/Content/CatalogProblems.Party.cs`: `internal static void CheckClassSkillLevels(classes, ExperienceTable)`;
  `OfClasses` runs it after `CheckClasses` with `content.Rules.Experience`; `ContentWriter.CheckCatalogs` calls it once.
  XP1's `CheckRules` runs it with the edited table (XP2's line).
- `Data/Content/AbsentWhenOneFilter`: `(ClassSkill, "level")` (ADR 0062: most skills are known from level 1).
- `Editor/Catalogs/CatalogDraft.Party.cs`: `IsSameClass` compares `Skills` by sequence instead of `SkillIds`, so a
  change of a level alone is a step.
- `content/classes.json`: each `"skillIds": [...]` becomes `"skills"` with one `{ "skillId": ... }` object per id, in
  the writer's layout (nine classes, edited by hand; the identical-bytes test proves the layout).
- Test sites that set `SkillIds` (grep `SkillIds =` and `SkillIds:` in `tests/` before starting; on 2026-11-15
  `Core.Tests/Builders/ClassBuilder`, `PartyMemberTests`, `ClassCatalogValidatorTests`, `ContentCatalogLoaderTests`,
  `StatusLayoutTests`, `CommandMenuSkillTests`, plus row 7's `SkillFixtures` and balance tests): set `Skills` instead,
  through `ClassBuilder.WithSkills(params string[] ids)` where a builder exists.
- `docs/map.md`: the Core `Party` row names `ClassSkill`; the Data validators' line names `ClassSkillLevelCheck`.

## Tests
- `Core.Tests/Party/ClassDefinitionTests` (new): `SkillIds_EntriesAtSeveralLevels_ListsEveryIdInOrder` (levels 5, 1,
  3: the ids in entry order, not by level).
- `Data.Tests/Party/ClassCatalogValidatorTests` (its invalid-classes theory, if it has one, else named tests): rows for
  a null entry and level 0; `Validate_SkillAtLevelOne_IsAccepted` (the bound's accepted side).
- `Data.Tests/Party/ClassSkillLevelCheckTests` (new; a class and a table built in the test):
  `Check_LevelAtMax_IsAccepted`; `Check_LevelPastMax_NamesClassAndSkill`.
- `Data.Tests/Content/ContentWriterClassTests` (CL1's file):
  `WriteMapCatalogs_SkillAtLevelOne_LeavesLevelOut`; `WriteMapCatalogs_SkillAtLevelTen_WritesIt` (read back equal);
  `WriteMapCatalogs_SkillPastMaxLevel_WritesNothing`.
- `Data.Tests/Content/ContentCatalogLoaderClassSkillTests` (new; a copied content root whose `classes.json` gets a
  level past the table): `Load_SkillPastMaxLevel_Refuses`.
- `Editor.Tests/Catalogs/CatalogDraftClassTests` (CL3's file): `SetClasses_OnlyALevelChanged_RecordsOneStep`.

## Verify by hand
1. `git diff content/classes.json`: `skillIds` lists became `skills` entries with the same ids in the same order, no
   `level` written.
2. Run the game, choose Omer: the battle's Skills menu and the Status page list the mage's skills as before.
3. Type `"level": 99` on one entry by hand, run the game: it stops at startup naming `classes.json`, the class, the
   skill and the max level 21. Undo the edit.
- **Note from CL3:** `Catalogs/CatalogDraftClassTests` comes with CL3b, split from CL3 on 2026-11-17.
- **Done, with deviations:** decision 1012: the class validator's existing messages say `skills`, the file's field,
  not `skillIds` (seven asserted messages follow). The null entry and level 0 are a theory,
  `Validate_NullEntryOrLevelBelowOne_Throws`. `ClassSkillLevelCheck` goes through `ContentListValidator` for the
  "class 'mage':" prefix. `classes.json` relaid by a script into the writer's layout (480 to 526 lines, same ids in the
  same order, no `level`); the simulations' output identical; saves untouched. Sizes: about 100 lines of Core, Data and
  Editor (`ClassSkill` 10, `ClassSkillLevelCheck` 25) and about 190 of tests (`ContentWriterClassTests` 236,
  `ClassCatalogValidatorTests` 289, about 40 changed `SkillIds` sites). After review, one test beyond the list:
  `CatalogProblemsPartyTests.OfClasses_SkillPastMaxLevel_NamesClassAndSkill`, so `OfClasses` keeps the level check.
