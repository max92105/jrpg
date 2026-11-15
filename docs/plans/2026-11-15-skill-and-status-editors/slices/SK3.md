# SK3. The skill list: Add, Duplicate, Remove, who uses a skill, the strip's pace, `IEditedCatalog`

Editor, about 230 lines (about 120 of them tests). Decisions 977, 978, 982. Design: design.md, section 2.4; the mode
bar's [page recipe](../../2026-11-15-mode-bar/page-recipe.md), steps 1, 2 and 7. Builds on SK2.

## Goal
The Skills page's state without its page: every skill in id order then the added ones; Add (an empty id, name and
sound, the first damage tier by id on a single enemy), Duplicate (the chosen one copied after it, its id to type),
Remove only an added one; the chosen skill's "Learned by" and "Used by" lines and its strip's pace line; the problem
line (the chosen skill's own, then another's named by id, then Data's for the whole list); the members its host reads
(`IEditedCatalog`); the session applied as one draft step.

## New types
- `Editor/Skills/SkillChoices` (internal sealed record): what the page's boxes offer, read when it opens: `Icons` (the
  icons folder's images), `Sounds` (the audio effects folder's files), `EffectImages` (the effects folder's images),
  `StripPaces` (`IReadOnlyDictionary<string, EffectStripDefinition>`, the draft's effect strip records). SK5, SK6 and
  SK7 add the resources, tiers and statuses with their first box.
- `Editor/Skills/SkillUsers` (internal static): who uses a skill. `LearnedBy(string skillId, ContentCatalog content)`
  ("Learned by: Mage, Warlock", or "Learned by: no class") and `UsedBy(string skillId, ContentCatalog content)` ("Used
  by: Goblin Chief", or empty: enemies whose tactics or phases name it).
- `Editor/Skills/SkillCatalogViewModel` (internal sealed partial, `ObservableObject`, implements
  `Workspaces/IEditedCatalog`): every skill on the page.
  - `SkillCatalogViewModel(ContentCatalog content, SkillChoices choices)`; `Skills`
    (`ObservableCollection<SkillViewModel>`), `SelectedSkill`, `Choices`.
  - `AddSkillCommand`, `DuplicateSkillCommand` (greyed with no choice), `RemoveSkillCommand` (greyed unless the chosen
    one `IsNew`; `Events/ListOrder.Remove`).
  - `LearnedByText`, `UsedByText` for the chosen skill (its id as typed; empty for a new one).
  - `StripPaceText` (decision 982): the chosen skill's strip as typed, read from `Choices.StripPaces` ("80 ms, hit on
    frame 3 (Database > Effect strips)"; for a strip with no record, the unlisted pace with "unlisted"; empty for none),
    raised when the strip box changes.
  - `Problem` (as `ShopCatalogViewModel`'s: `Catalogs/RecordListProblem` for an id twice or another skill's own
    problem, then `CatalogProblems.OfSkills` over the typed list), raised from each skill's `Changed` only.
  - `ChosenKey` (the chosen skill's typed id; null with none), `Choose(string key)` (an unknown key keeps the choice).
  - `ToSkills()`: every skill's record, throws on a `Problem`.
- `Editor/Skills/SkillEditingViewModel` (internal sealed): `SkillEditingViewModel(CatalogDraft catalog,
  Func<SkillChoices> readChoices)`; `Start()` and `Apply(SkillCatalogViewModel edited)` (`CatalogDraft.SetSkills`), the
  `ICatalogEditing<SkillCatalogViewModel>` shape: it declares the interface if the mode bar's WS2 has landed, else WS2
  adds the declaration with the other editing view models (page recipe, step 2).

## Changes
- `Editor/Skills/SkillViewModel`: `static New(TieredEffect? firstDamageTier)` (an effect of that tier's kind and type,
  power 0, `singleEnemy`; a flat damage of power 1 when the content has no damage tier) and `static DuplicateOf(
  SkillViewModel chosen)` (a new skill from its typed record, id empty).

## Tests
- `Editor.Tests/Skills/SkillCatalogViewModelTests` (new; `SkillFixtures`):
  - `Skills_RepositoryOrder_IdOrderThenAdded`.
  - `AddSkill_EmptyCatalogOfTiers_StartsAFlatDamage` and `AddSkill_WithTiers_NamesTheFirstDamageTierById`.
  - `AddSkill_TypedIdNameAndSound_HasNoProblem`.
  - `DuplicateSkill_Chosen_CopiesItAfterWithAnEmptyId`.
  - `RemoveSkill_SavedSkill_Greyed` and `RemoveSkill_AddedSkill_RemovesIt`.
  - `Problem_IdTwice_NamesIt`.
  - `Problem_ClassCantPayAnEditedSkill_IsDataMessage` (a cost edited through the record in the fixture: `classes.json`
    named).
  - `StripPaceText_ListedStrip_ReadsItsPaceAndHit`, `StripPaceText_UnlistedStrip_SaysUnlisted`.
  - `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`, `ChosenKey_NewSkillTyped_IsItsTypedId`.
  - `ToSkills_Problem_Throws`.
- `Editor.Tests/Skills/SkillUsersTests` (new; classes and enemies built with `with` in the test):
  - `LearnedBy_TwoClasses_NamesBothInClassOrder`, `LearnedBy_NoClass_SaysSo`.
  - `UsedBy_EnemyPhaseNamesIt_NamesTheEnemy`, `UsedBy_NoEnemy_IsEmpty`.
- `Editor.Tests/Skills/SkillEditingViewModelTests` (new): `Apply_EditedName_IsOneStep`.

## Verify by hand
None: the page comes in SK4.
