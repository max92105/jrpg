# SK2. A skill's texts and files as typed, and the draft's skills step

Data + Editor, about 200 lines (about 110 of them tests). Decisions 974, 975 (the step's shape), 977. Design:
design.md, sections 2.3, 2.4 and 2.6 ("Checks across files"). Builds on SK1.

## Goal
The rules the Skills page needs before it has a page: one skill as typed (its id while new, name, description,
icon, sound, own strip, quick flag), handed back as a record that keeps what no box edits yet (costs, effect); Data's
message for an edited list of skills, the classes and enemies checked with it; the draft replacing the skills as one
undo step.

## New types
- `Editor/Skills/SkillViewModel` (internal sealed partial, `ObservableObject`): one skill as typed.
  - Typed: `IdText` (editable while `IsNew`), `Name`, `Description`, `IconFile`, `SoundFile`, `EffectFile` (empty for
    none), `IsQuick`; `IsNew`; `Id` (the typed id trimmed).
  - `static SkillViewModel From(SkillDefinition skill)`; the loaded record kept for what no box edits.
  - `Problem`: the typed id first (`Objects/ObjectIds.ProblemOf` against the other skills' ids, passed in), else null;
    Data's message is the list's (SK3).
  - `ToSkill()`: the loaded record `with` the typed fields (strip trimmed, none when empty); throws
    `InvalidOperationException` on a `Problem`.
  - `event EventHandler? Changed`: raised for typed fields only, the list's relay (`.claude/rules/editor.md`).
- `Editor/Catalogs/CatalogDraft.Skills` (partial, new file): `SetSkills(IReadOnlyList<SkillDefinition> skills)`: one
  `CatalogEdit`, nothing when the same by content (`HasSameRecords` with a private `IsSameSkill`: the record with the
  lists swapped in compared, then `ResourceChanges` and `Requirements` in order; `Effect` compares its statuses itself,
  row 6's MS). No undo refusal: classes and enemies name skills but the editor writes neither (the summary says so).
- `Editor.Tests/Skills/SkillFixtures` (static, test): the test world's catalog loaded once; a `fire_1` tier, a
  `poison_1` status and a `bolt` skill naming both with a mana cost; `CatalogWith(skills, tiers, statuses)` over the
  catalog with `with`. Shared by every `Skills/`, `Effects/` and `Statuses/` test file of this plan and by the
  `CatalogDraft...` tests of SK2, FX1 and FX5.

## Changes
- `Data/Content/CatalogProblems`: `public static string? OfSkills(IReadOnlyList<SkillDefinition> skills,
  ContentCatalog content)`: `CheckSkills` (SK1: skills and classes), then `CheckEnemies` over the content's enemies with
  the edited skills in the catalog (`content with { Skills = ... }`), in one `FirstProblem`; the summary names the
  classes' and enemies' checks.

## Tests
- `Data.Tests/Content/CatalogProblemsSkillTests` (new: `CatalogProblemsTests` is 160 lines and FX1 and FX5 add to this
  sibling; skills built in the test):
  - `OfSkills_RepositorySkills_IsNull`.
  - `OfSkills_EmptyName_NamesTheFileAndSkill`.
  - `OfSkills_ClassSkillWithAForeignResource_NamesClassesFile`.
  - `OfSkills_EnemySkillWithACost_NamesEnemiesFile`.
- `Editor.Tests/Catalogs/CatalogDraftSkillTests` (new; `SkillFixtures`):
  - `SetSkills_ChangedName_IsOneUndoStep`.
  - `SetSkills_EqualByContentWithFreshLists_RecordsNothing`.
  - `SetSkills_AddedSkill_UndoRemovesIt`.
- `Editor.Tests/Skills/SkillViewModelTests` (new; `SkillFixtures`):
  - `From_Skill_FillsEveryBox`.
  - `ToSkill_EditedTexts_KeepsTheEffectAndCosts`.
  - `ToSkill_EmptyStrip_WritesNone`.
  - `ToSkill_StripWithSpaces_TrimsIt`.
  - `Problem_NewSkillWithATakenId_NamesIt`.
  - `Changed_TypedName_Raised` and `Changed_ProblemNotice_NotRaised`.

## Verify by hand
None: no page yet (SK4).
- **Done, with deviations:** a new skill comes from `SkillViewModel.ForNew(start, takenIds)` (the test needed one; SK3's
  `New` and `DuplicateOf` can build on it); two more draft tests (`CostChanged`, `RequirementChanged`) and two more view
  model tests; the Data test named `OfSkills_TestWorldSkills_IsNull`. About 477 lines (`wc -l`): 166 source, 311 tests.
