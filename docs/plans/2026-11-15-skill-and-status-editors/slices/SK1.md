# SK1. The writer takes over `skills.json`, `effects.json` and `statuses.json`

Data + content, about 190 lines (about 110 of them tests), plus the `skills.json` relay (counted apart). Decisions 972,
973, 974. Design: design.md, sections 2.3 and 2.6 ("Writing references").

**Two commits:** first the relay alone (`content/skills.json` rewritten by this slice's writer in the working tree
through a throwaway test, deleted; the loader reads either layout, so that commit builds and passes alone), then the
code and tests.

## Goal
Ctrl+S writes the three files when the draft changed them, each checked as the loader checks it first: `skills.json`
with its tiers and grants written as ids, every default-valued field left out, refused when a class or an enemy would
no longer accept the skills. `skills.json` is relaid once by the writer; after it the identical-bytes test covers all
three. No page changes them yet (SK4, FX2, FX6).

## Changes
- `Data/Content/ContentFileWriter`: `Serialize<T>(T content, params JsonConverter[] converters)`: the indented options
  copied with the converters added, made read-only; the one-argument `Serialize` passes none.
- `Data/Content/ContentCatalogLoader`: `SkillsFile` and `ClassesFile` become `internal const` (the writer names them).
- `Data/Content/CatalogProblems` (internal checks, the writer their only caller until SK2, FX1, FX5 add the `OfX`):
  - `CheckSkills(IReadOnlyList<SkillDefinition> skills, ContentCatalog content)`: `SkillCatalogValidator`, then
    `ClassCatalogValidator` over `content.Classes` with `ClassReferences` built from the skills (decision 974). The
    enemies' check isn't repeated: `CheckCatalogs` already runs `CheckEnemies` against the catalog's skills. Row 12's
    CL1 extracts `CheckClasses`; this method then calls it.
  - `CheckTieredEffects(IReadOnlyList<TieredEffect> effects)`: `EffectCatalogValidator`.
  - `CheckStatuses(IReadOnlyList<StatusDefinition> statuses)`: `StatusCatalogValidator`.
- `Data/Content/ContentWriter.Catalogs`: `CheckCatalogs` calls the three (before `CheckEnemies`, so a skill's own
  problem is named first); `SerializedCatalogs` gains `effects.json`,
  `statuses.json` and `skills.json`, the last through `Serialize(SortedSkills(catalog), ReferenceConverters(catalog))`
  (a private method making the two `CatalogReferenceConverter`s over the catalog's records); three `Sorted...` lists by
  id, ordinal.
- Filters (decision 973), each row with its one-line reason:
  - `AbsentWhenNullFilter`: `(SkillDefinition, "effectFile")` (decision 710), `(Effect, "damageType")`,
    `(Effect, "tier")`.
  - `AbsentWhenEmptyFilter`: `(SkillDefinition, "resourceChanges")`, `(SkillDefinition, "requirements")`,
    `(Effect, "statuses")`.
  - `AbsentWhenFalseFilter`: `(SkillDefinition, "isQuick")`.
  - `AbsentWhenZeroFilter`: `(Effect, "range")`.
  - `AbsentWhenOneFilter`: `(Effect, "hits")`.
- `content/skills.json` (the first commit): relaid once by the writer: sorted by id, one field per line, `"power": 0` kept beside a tier (decision 792), every grant's `chance`
  written (decision 972), every `effectFile` line kept.

## Tests
- `Data.Tests/Content/ContentWriterSkillTests` (new; glob: no file of that name exists; skills, tiers and statuses
  built in the test over the test world's catalog with `with`):
  - `WriteMapCatalogs_RepositorySkills_LoadsTheSameAfterRelay` (load, write, load: every skill equal).
  - `WriteMapCatalogs_RelaidSkills_HoldNoDefaultValuedField` (the written text has no `"effectFile": null`,
    `"resourceChanges": []`, `"requirements": []`, `"isQuick": false`, `"range": 0`, `"hits": 1`, `"tier": null`,
    `"statuses": []`, `"damageType": null`).
  - `WriteMapCatalogs_TieredSkillWithAGrant_WritesTheIds` (a skill naming a `fire_1` record and a `poison_1` grant: the
    text holds `"tier": "fire_1"` and `"status": "poison_1"`, not the records).
  - `WriteMapCatalogs_SkillItsClassCantPay_WritesNothing` (a class's skill given a resource the class lacks: the save
    throws `ContentValidationException` naming `classes.json`, no file changed).
  - `WriteMapCatalogs_EnemySkillGivenACost_WritesNothing` (an enemy's tactic skill given a resource change: refused
    naming `enemies.json`).
  - `WriteMapCatalogs_ChangedStatus_WritesStatusesFile` and `WriteMapCatalogs_ChangedTieredEffect_WritesEffectsFile`
    (a record's number changed in the catalog: the file is written with it).
- `Data.Tests/Content/ContentWriterRepositoryTests` (81 lines): the identical-bytes test's file list gains
  `skills.json`, `effects.json`, `statuses.json`.
- `Data.Tests/Content/ContentFileWriterTests`: `Serialize_WithConverter_UsesItForItsType` (a record type of the test's
  own with a converter writing a fixed string).

## Verify by hand
None: no page changes skills yet. The game starts and battles play as before; `git diff content/skills.json` in the
relay commit shows only layout and order.
