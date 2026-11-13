# EN1. Enemies save from the editor

Data + Editor, about 230 lines (120 of them tests). Design: "Data changes", "Content format". Decisions 586, 587, 591. Builds on SH1 (`CatalogDraft.Database.cs`) and plan 1's CW1 and SI1 (`CatalogProblems`).

## Goal
`enemies.json` round-trips through the writer: resistances written in whole percents, every default-valued field left out (decision 586), the file relaid once; Ctrl+S writes it, when changed, checked against the skills and drop tables; the draft replaces the enemies as one undo step; `CatalogProblems.OfEnemies` gives Data's message for an edited list.

## New types
- `Data/Content/AbsentWhenFalseFilter` (internal static): leaves a flag off by default out of a file while false (`EnemyDefinition.isBoss`), on `OptionalFieldFilter.SkipWhen`.
- `Data/Content/AbsentWhenEmptyFilter` (internal static): leaves a list out while empty (`EnemyDefinition.tactics`, `phases`), on the same loop.

## Changes
- `Data/Battle/ResistancesConverter.Write`: an object of whole percents by type in `DamageType`'s order, a type at 0 left out (`StatFormula.WholePercentOf`; exact, since content percents load as permille x 10).
- `Data/Content/AbsentWhenNullFilter`: rows for `EnemyDefinition.dropTableId`, `EnemyDefinition.gold`, `EnemyAction.skillId` (null is the basic attack, decision 412) and `EnemyAction.when` (null is always).
- `Data/Content/AbsentWhenZeroFilter`: a row for `TacticCondition.percent` (only the kinds that read a percent carry one).
- `Data/Content/ContentJsonOptions`: the two new modifiers registered.
- `Data/Content/ContentWriter.CheckedCatalogWrites`: validates (with `EnemyReferences` from the drop tables and skills) and writes `enemies.json` when changed; `EnemiesFile` internal.
- `Data/Content/CatalogProblems.OfEnemies(IReadOnlyList<EnemyDefinition>, ContentCatalog)`.
- `content/enemies.json`: relaid once by the writer (a throwaway test, deleted after; its commit holds only the relay).
- `Editor/Catalogs/CatalogDraft.Database.cs`: `SetEnemies` (equal by content: `Tactics` and `Phases` compared element by element, as `IsSameObject` does).
- No undo refusal: only encounters name an enemy, and the editor doesn't edit them.

## Tests
- `ResistancesConverterTests`: `Write_MixedResistances_WritesPercentsInTypeOrder`; `Write_None_WritesEmptyObject`; `RoundTrip_EveryType_ReadsBackTheSame` (theory over `DamageType`).
- `ContentWriterEnemyTests` (new sibling): `WriteMapCatalogs_RepositoryEnemies_LoadsTheSameAfterRelay`; `WriteMapCatalogs_RelaidEnemies_HoldNoDefaultValuedField` (the written text has no `"skillId": null`, `"when": null`, `"percent": 0`, `"isBoss": false`, `"tactics": []`, `"phases": []`, `"dropTableId": null` or `"gold": null`); `WriteMapCatalogs_BossEnemy_WritesIsBossTrue`; `WriteMapCatalogs_SkillActionWithCondition_WritesSkillWhenAndPercent`; `WriteMapCatalogs_UnknownTacticSkill_WritesNothing`.
- `ContentWriterRepositoryTests`: the identical-bytes list gains `enemies.json`.
- `CatalogProblemsTests`: `OfEnemies_RepositoryEnemies_IsNull`; `OfEnemies_AccuracyTooHigh_NamesEnemyAndField`.
- `CatalogDraftEnemyTests` (new): `SetEnemies_ChangedEnemy_IsOneUndoStep`; `SetEnemies_EqualByContent_RecordsNothing` (fresh lists, same values).

## Verify by hand
None: no window changes enemies yet. Battles play as before; `git diff content/enemies.json` in the relay commit shows only layout.

- **Done, with deviations:** `AbsentWhenEmptyFilter` tests the value as an `ICollection` with no entries; the boss test writes a boss and a non-boss of the tests' own and reads both back from the JSON; `CatalogDraftEnemyTests` also pins a tactic weight change as a step; the relay commit holds only `content/enemies.json` (a throwaway test, deleted); `enemies.json` joined the identical-bytes list. The review: `CatalogProblems.FirstProblem` shares the try block of the three `OfX`, the null and zero filters say their two reasons, a phase tactic change is pinned, and the writer skips a type whose whole percent is 0.
