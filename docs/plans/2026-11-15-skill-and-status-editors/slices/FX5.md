# FX5. The statuses' list, New tier, Data's check and the draft step

Data + Editor, about 210 lines (about 110 of them tests). Decisions 975, 985. Design: design.md, sections 2.3, 2.4
and 2.6 ("Rebinding, catalog-wide"). Builds on FX1 (`CatalogReferences`), FX2 (`Catalogs/TierIds`) and FX4.

## Goal
The Statuses page's state without its page: every status in id order then the added ones; Add, Duplicate, New tier
(decision 985: `poison_2` after `poison_1`, the family kept), Remove only an added one; the families the Family box
offers; the problem line (the chosen one's own, then another's by id, then Data's: a family whose parts differ, an
amount out of range, then what names the statuses over the rebound catalog); the session applied as one step that
rebinds the catalog, so every skill and item granting an edited status holds the new record.

## New types
- `Editor/Statuses/StatusCatalogViewModel` (internal sealed partial, `ObservableObject`, implements
  `Workspaces/IEditedCatalog`): `Statuses`, `SelectedStatus`, `Families` (every typed family key, sorted, for the
  Family box), `AddStatusCommand`, `DuplicateStatusCommand` (its id to type), `NewTierCommand` (a copy after the chosen
  one, `IdText` = `TierIds.Next(chosen id, every typed id)`, its family the chosen one's family key, its texts and
  parts copied), `RemoveStatusCommand` (an added one only), `Problem` (`Catalogs/RecordListProblem`, then
  `CatalogProblems.OfStatuses`), `ChosenKey`, `Choose`, `ToStatuses()`.
- `Editor/Statuses/StatusEditingViewModel` (internal sealed): `Start()`, `Apply(edited)` through
  `CatalogDraft.SetStatuses`; the `ICatalogEditing<StatusCatalogViewModel>` shape (declared from the mode bar's WS2).

## Changes
- `Data/Content/CatalogProblems`: `public static string? OfStatuses(IReadOnlyList<StatusDefinition> statuses,
  ContentCatalog content)`: `CheckStatuses` (SK1), then `CheckSkills` over the skills of
  `CatalogReferences.Rebound(content with { Statuses = the edited ones by id })`. Row 9's IU5 adds its item check here.
- `Editor/Catalogs/CatalogDraft.Skills`: `SetStatuses(IReadOnlyList<StatusDefinition> statuses)`: as
  `SetTieredEffects`, do `catalog => CatalogReferences.Rebound(catalog with { Statuses = after })` and undo the same
  with `before`; nothing when the same by content (parts compared in order by `StatusDefinition`'s equality, row 6's
  SR1).

## Tests
- `Editor.Tests/Statuses/StatusCatalogViewModelTests` (new; `SkillFixtures`):
  - `NewTier_Poison1_AddsPoison2InItsFamilyAfterIt`.
  - `NewTier_StatusWithoutFamily_GivesBothTheSameFamilyKey` (the copy's family is the chosen id).
  - `Families_TypedFamily_Offered`.
  - `RemoveStatus_SavedStatus_Greyed`.
  - `Problem_FamilyPartsDiffer_IsDataMessage` (a part kind changed on a new tier).
  - `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`, `ChosenKey_NewStatusTyped_IsItsTypedId`.
- `Data.Tests/Content/CatalogProblemsSkillTests`:
  - `OfStatuses_RepositoryStatuses_IsNull`.
  - `OfStatuses_DamageTickZero_NamesTheFileAndStatus`.
  - `OfStatuses_SkillGrantingTheEditedStatusBreaksARule_IsTheSkillsMessage` (the review's C7). Today no Data rule reads
    a granted status's content (grants are checked by id, turns and chance), so the test builds the case the check
    exists for: content with a skill granting `poison_1` and breaking a rule of its own (its chance 0), and an edited
    status list that changes `poison_1`'s amount; the message names `skills.json` and the skill, which proves the
    statuses' check runs the skills over the rebound catalog. When a rule ties a skill or an item to a status's parts
    (row 8's triggers, row 9's IU5), its own row joins this test file.
- `Editor.Tests/Catalogs/CatalogDraftStatusTests` (new; `SkillFixtures`):
  - `SetStatuses_ChangedAmount_GrantsHoldTheNewRecord`.
  - `SetStatuses_Undo_GrantsHoldTheOldRecord`.
  - `SetStatuses_EqualByContent_RecordsNothing`.

## Verify by hand
None: the page comes in FX6.
