# CL1. `classes.json` is written by the editor, in the writer's layout

Data + content, about 200 lines (about 110 of them tests) plus the relaid `content/classes.json`. Design: sections 3
(Data changes), 6; decision 944.

## Goal
Save all writes `classes.json` like every other catalog: sorted by id, one field per line, checked first as the loader
checks it, only when it changed (the stamps, decision 559). A class the editor writes reads like a hand-written one: a
resource's absent rules, its `false` lasting flag, a zero max or start and its zero gains are left out. The repository's
file is relaid once; nothing edits classes yet (CL3).

## Changes
- **Recount first:** `Data/Content/CatalogProblems.cs` was 130 lines on 2026-11-15 and row 7 adds its skill, tier and
  status entries (about 190). This slice starts the partial `Data/Content/CatalogProblems.Party.cs` (classes here;
  members PM1; new game NG1; `CatalogProblems.Rules.cs` is XP1's) and moves nothing else.
- `CatalogProblems.Party.cs`:
  - `public static string? OfClasses(IReadOnlyList<ClassDefinition> classes, ContentCatalog content)`: the first
    problem in the classes, a class's own rule or a skill it names (unknown, using a resource it lacks, a requirement
    above a max); null when the loader would take them.
  - `internal static void CheckClasses(IReadOnlyList<ClassDefinition> classes, IReadOnlyDictionary<string,
    SkillDefinition> skills)`: `ClassCatalogValidator.Validate(ClassesFile, classes, new ClassReferences(skills))`,
    extracted from row 7's `CheckSkills`, which now calls it with the catalog's classes sorted by id. The classes are
    checked once per save: through `CheckSkills` in `CheckCatalogs`, with no second call (its comment says so).
- `Data/Content/ContentWriter.Catalogs.cs`: `classes.json` in `SerializedCatalogs`; `SortedClasses` (ordinal by id),
  which `CheckSkills`' call shares. The class summary's list of files gains "classes".
- `Data/Content/AbsentWhenNullFilter.cs`: `ResourceDefinition` `maxFromSpeed`, `backfire`, `strain`, `dominance`,
  `willScaling` (comment: ADR 0019, each rule is for some resources only).
- `Data/Content/AbsentWhenFalseFilter.cs`: `ResourceDefinition` `isLasting` (ADR 0024: most resources start afresh).
- `Data/Content/AbsentWhenZeroFilter.cs`: `ResourceDefinition` `max` and `startsAt` (a speed resource has neither; a
  start of 0 is the default) and the eight `ResourceGains` fields (a resource gains on a few triggers only).
- `content/classes.json`: relaid once by the writer (a throwaway test, deleted after; its commit holds only the relay,
  and `git diff` shows layout and id order only).
- `ContentWriter`'s `WriteMapCatalogs` summary: the list of catalogs gains the classes.
- `docs/map.md`, Data `Content` row: `CatalogProblems.OfClasses` and the `CatalogProblems.Party` partial; the writer's
  files gain `classes.json`.

## Tests
- `Content/ContentWriterClassTests` (new, test-world copy through `CopiedContentRoot("writer-classes", "test-world")`;
  classes built in the test, `new ClassDefinition(...)` with one resource, and `with` over it):
  - `WriteMapCatalogs_Classes_LoadTheSameAfterRelay` (load, write, load; each class compared with its skill ids and
    resources by content, since records compare lists by reference).
  - `WriteMapCatalogs_RelaidClasses_HoldNoDefaultValuedField` (no `"isLasting": false`, `"backfire": null`,
    `"strain": null`, `"dominance": null`, `"maxFromSpeed": null`, `"willScaling": null`, `"perTurn": 0`).
  - `WriteMapCatalogs_SpeedResource_LeavesMaxAndStartOut`.
  - `WriteMapCatalogs_LastingResourceWithStrain_WritesThem` (`isLasting` true and the strain read back equal).
  - `WriteMapCatalogs_ClassNamingUnknownSkill_WritesNothing` (the loader's message, the file's bytes unchanged).
  - `WriteMapCatalogs_SkillUsingForeignResource_WritesNothing` (Data's "a resource ... doesn't have" message).
- `Content/CatalogProblemsPartyTests` (new; classes built in the test): `OfClasses_ValidClasses_IsNull`;
  `OfClasses_EmptyDescription_NamesTheClass`.
- Row 7's `ContentWriterSkillTests.WriteMapCatalogs_SkillItsClassCantPay_WritesNothing` passes unchanged (the guard on
  the extraction).
- `Content/ContentWriterRepositoryTests` (81 lines): `CatalogFileBytes` gains `classes.json` (after the relay).

## Verify by hand
1. `git diff content/classes.json` in the relay commit: the same classes, numbers, skills and resources, laid out one
   field per line in id order.
2. Run the game, choose Omer (mage): the Status page shows Mana and Clarity with their colors and the mage's skills, as
   before the relay.
3. Run the editor, change any map, Save all: `git status` shows no change to `classes.json`.
- **Done, with deviations:** `classes.json` joins the writer's catalogs and stamps, relaid once (85 lines to 480, id
  order, one field per line; the simulations' lines identical, only the per-class theories' order follows the file).
  Decision 1011: a resource gaining on no trigger also leaves `gains` out (`AbsentWhenEmptyFilter`, as the druid's
  Growth and Decay were hand-written), with its own test. `CheckSkills` sorts the classes itself, as `CheckItems` sorts
  the shops, instead of sharing the writer's private `SortedClasses`. Three tests beyond the list, for the two sorts
  and the skills passed: `WriteMapCatalogs_ClassesAddedOutOfOrder_WritesThemSortedById`,
  `WriteMapCatalogs_TwoClassesRefused_NamesTheFirstById` and `OfClasses_UnknownSkill_NamesIt`. The foreign-resource test builds its own skill (`power_strike` with a focus
  cost) rather than pinning what a repository skill uses. `CatalogDraft.SetSkills`' summary reworded here ("no page
  edits what they name"), since the writer now writes `classes.json`. Sizes: about 96 lines of Data
  (`CatalogProblems.Party.cs` 33, `CatalogProblems.cs` 273) and 233 of tests (`ContentWriterClassTests` 197,
  `CatalogProblemsPartyTests` 35), above the 110 planned for tests.
