# SK5. A skill's costs as resource rows

Editor, about 200 lines (about 100 of them tests). Decision 979. Design: design.md, section 2.4. Builds on SK4.

## Goal
The Skills page's Costs section: Changes rows (a resource and an amount: negative spends, positive gains) and Needs
rows (a resource and its minimum), each added after the chosen row and removed; the resource box offers every class's
resources labelled with their classes; a summary line ("Spends 2 mana, gains 1 focus; needs 1 faith"). Data's rules
(no 0 amount, no resource twice, a requirement of at least 1, a class able to pay) show on the problem line.

## New types
- `Editor/Skills/ResourceChoice` (internal sealed record): one resource the cost boxes offer: `Id`, `Label` ("Mana
  (mage, warlock)"); `static IReadOnlyList<ResourceChoice> AllIn(ContentCatalog content)`: every class's resources by
  id in the order the classes list them, each once, its label the first class's resource name and every class having
  it.
- `Editor/Skills/ResourceChangeRow` (internal sealed partial, `ObservableObject`): one change as typed: `ResourceId`,
  `AmountText` (`Catalogs/TypedNumber`); `Problem` (the box not a whole number, a resource not picked); `ToChange()`.
- `Editor/Skills/ResourceNeedRow` (internal sealed partial, `ObservableObject`): one requirement as typed:
  `ResourceId`, `AtLeastText`; `Problem`; `ToRequirement()`.
- `Editor/Skills/SkillCosts` (internal sealed partial, `ObservableObject`): a skill's two row lists.
  - `Changes`, `Needs` (`ObservableCollection`), `SelectedChange`, `SelectedNeed`; `AddChangeCommand` (a row on the
    first offered resource, amount -1, after the chosen one), `RemoveChangeCommand`, `AddNeedCommand` (at least 1),
    `RemoveNeedCommand`.
  - `SummaryText` ("Spends 2 mana, gains 1 focus; needs 1 faith"; "Free" with no row), raised on a typed field.
  - `Problem` (the first row's own, "Changes row 2: ..."); `ToChanges()`, `ToRequirements()`.
  - `event EventHandler? Changed` for typed fields and list changes (`Catalogs/ListItemWatch.Follow`).

## Changes
- `Editor/Skills/SkillChoices`: `IReadOnlyList<ResourceChoice> Resources` (from `ResourceChoice.AllIn` of the draft
  when the page opens).
- `Editor/Skills/SkillViewModel`: `Costs` (a `SkillCosts` from the record); `Problem` after the id reads
  `Costs.Problem`; `ToSkill` sets `ResourceChanges` and `Requirements` from the rows; `Changed` relays `Costs.Changed`.
- `Editor/Skills/SkillCatalogPage.xaml`: a "Costs" section band under Quick (typed boxes `PropertyChanged`): the Changes rows (resource combo by
  `SelectedValue`, amount box) with Add and Remove, the Needs rows likewise, the summary line. A row is chosen by a
  `PreviewGotKeyboardFocus` `EventSetter` (the drop table window's way). About +45 lines.

## Tests
- `Editor.Tests/Skills/SkillCostsTests` (new; `SkillFixtures`):
  - `From_SkillWithCostAndNeed_FillsBothLists`.
  - `SummaryText_SpendAndGainAndNeed_ReadsAllThree`, `SummaryText_NoRow_IsFree`.
  - `AddChange_ChosenRow_InsertsAfterItWithMinusOne`.
  - `Problem_AmountNotANumber_NamesTheRow`.
  - `ToChanges_TypedRows_InOrder`; `ToRequirements_TypedRows_InOrder`.
  - `Changed_AmountTyped_Raised`; `Changed_RowRemoved_Raised`.
- `Editor.Tests/Skills/ResourceChoiceTests` (new; classes built with `with` in the test):
  `AllIn_ResourceOfTwoClasses_ListedOnceNamingBoth`, `AllIn_ClassesOrder_Kept`.
- `Editor.Tests/Skills/SkillCatalogViewModelTests`: `Problem_ZeroAmountTyped_IsDataMessage` (Data's "has a 0 amount
  for 'mana'").

## Verify by hand
1. Database > Skills..., `fire_blast`: Changes holds mana -2; the summary reads "Spends 2 mana".
2. Change -2 to -3, OK, Save all: `git diff content/skills.json` shows the amount only; Ctrl+Z, Save all.
3. On `power_strike` (a fighter's), add a Changes row and pick Mana: the problem line names `classes.json` (the fighter
   has no mana), OK greys; Remove the row.
4. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line.
