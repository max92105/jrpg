# FX1. Everything naming an edited effect or status follows it: `CatalogReferences`, Data's checks and the draft step

Data + Editor, about 220 lines (about 130 of them tests). Decisions 974, 975 (row 6's 789). Design: design.md,
sections 2.3 and 2.6 ("Rebinding, catalog-wide"). Builds on SK2 (`CatalogDraft.Skills`, `CatalogProblemsSkillTests`).

## Goal
The draft holds resolved records (decision 789), so an edit to a tiered effect must reach everything naming it: one
Data rule points every skill's and item's effect at the records of the same ids in a catalog; the draft replaces the
tiered effects as one undo step whose do and undo both rebind the catalog; Data's message for an edited list of
effects checks the skills over the rebound catalog (an effect turned into a heal under a damage skill is refused
naming the skill). FX5 reuses all of it for statuses. No page yet (FX2).

## New types
- `Data/Content/CatalogReferences` (public static): points every content reference resolved at load at the catalog's
  records of the same id (decision 789), beside `CatalogReferenceConverter`, so `CatalogProblems` and the editor's
  draft share it.
  - `Rebound(ContentCatalog catalog)`: the catalog `with` `Skills` and `Items` rebuilt: each skill or item whose
    effect's `Tier` or grants' `Status` aren't (by reference) the catalog's records of those ids gets its effect `with`
    them; a record already right stays the same instance. An id with no record throws `InvalidOperationException`
    naming the skill or item and the id (a step removing a record something names is undone after that thing's step:
    a bug, never input).

## Changes
- `Data/Content/CatalogProblems`: `public static string? OfTieredEffects(IReadOnlyList<TieredEffect> effects,
  ContentCatalog content)`: `CheckTieredEffects`, then `CheckSkills` over the skills of
  `CatalogReferences.Rebound(content with { TieredEffects = the edited ones by id })`, sorted by id. Row 9's IU5 adds its
  item check here.
- `Editor/Catalogs/CatalogDraft.Skills`: `SetTieredEffects(IReadOnlyList<TieredEffect> effects)`: nothing when the same
  by content (record equality); else one `CatalogEdit`: do `catalog => CatalogReferences.Rebound(catalog with {
  TieredEffects = after })`, undo the same with `before`. Summary: "everything naming an effect follows it (decision
  975); no undo refusal: catalog steps undo in order".
- `docs/map.md`: the Data Content line names `CatalogReferences`.

## Tests
- `Data.Tests/Content/CatalogReferencesTests` (new; records built in the test over the test world's catalog):
  - `Rebound_TierEdited_SkillHoldsTheNewRecord`.
  - `Rebound_StatusEdited_SkillGrantHoldsTheNewRecord`.
  - `Rebound_StatusEdited_ItemGrantHoldsTheNewRecord` (an item granting a status, built in the test).
  - `Rebound_NothingToRebind_KeepsTheSameInstances`.
  - `Rebound_MissingId_ThrowsNamingTheRecordAndId`.
- `Data.Tests/Content/CatalogProblemsSkillTests`:
  - `OfTieredEffects_TestWorldEffects_IsNull`.
  - `OfTieredEffects_PerPowerZero_NamesTheEffectsFile`.
  - `OfTieredEffects_KindChangedUnderADamageSkill_NamesTheSkillsFileAndSkill`.
- `Editor.Tests/Catalogs/CatalogDraftTieredEffectTests` (new; `SkillFixtures`):
  - `SetTieredEffects_ChangedPerPower_SkillsHoldTheNewRecord`.
  - `SetTieredEffects_Undo_SkillsHoldTheOldRecord`.
  - `SetTieredEffects_ThenSetSkills_UndoBothRedoBoth_SkillsHoldTheNewRecord` (the review's C7: a renamed skill after a
    retuned effect; two undos, two redos: the skill holds the new name and the retuned record).
  - `SetTieredEffects_EqualByContent_RecordsNothing`.
  - `SetTieredEffects_AddedRecord_UndoRemovesIt`.

## Verify by hand
None: no page changes effects yet (FX2).
- **Done, with deviations:** code about 110 lines (`CatalogReferences` 70), tests about 210 (the plan said 130): the
  references tests build their own skill and item (Spark, the Fire Flask) over the test world. `Rebound` rebuilds
  `Skills` and `Items` as frozen dictionaries, as the loader does.
