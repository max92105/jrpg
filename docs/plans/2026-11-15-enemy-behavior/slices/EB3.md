# EB3. One rule as typed, and what its boxes offer

Editor, about 280 lines (about 180 of code, 13 tests at about 10 lines), above the editor's 200 since the review folded
the choices in. **Split point:** the three choice records and `EnemyCatalogViewModel.SkillChoices` with their four
tests (about 90 lines) as a first commit, then the row. Needs EB2 and the mode bar's WS7. Design: design.md, sections
2.3, 3.1 and 3.2 (the words), 4.2 (the boxes and the sentence).

## Goal
One rule of an enemy, as the Tactics tab will type it: its When choice and number, its skill, its target, its weight;
the numbered sentence the list shows; the problem of a box that isn't a number; the action it builds. And what the
boxes offer: the triggers with their words, the targets with theirs, the attack then the draft's skills an enemy can
use, by name.

## New types (Editor, `Battle/`)
- `TacticConditionChoice` (`internal sealed record (TacticConditionKind? Kind, string Label)`): one trigger as the When
  box offers it, with its words.
  - `public static IReadOnlyList<TacticConditionChoice> All`: "always" (null), "its HP is below", "an ally's HP is
    below", "an ally has fallen" (design.md, section 3.1), as `Events/QuestConditionKindChoice.All` does.
  - `public static TacticConditionChoice Of(TacticConditionKind? kind)`: the entry of `All` (`Single`).
- `TacticTargetChoice` (`internal sealed record (TacticTarget? Target, string Label)`): one target rule as the On box
  offers it, with its words: "automatic" (null), "that one", "itself", "the foe with the lowest HP", "the foe with the
  highest HP", "the lowest HP on its side (itself included)"; `All` and `Of` as above.
- `TacticSkillChoice` (`internal sealed record (string Id, string Name)`): one skill as the Use box offers it; the
  attack is `("", "Attack")`, as the Drop table box's "" is none (decision 853).
- `TacticRuleRow` (internal sealed partial, `ObservableObject`): one rule as typed.
  - `public static TacticRuleRow From(EnemyAction action)`; `public static TacticRuleRow New()` (always, the attack,
    automatic, weight 1: decision 859); `public static TacticRuleRow CopyOf(TacticRuleRow row)` (the boxes as typed,
    never `ToAction`, `.claude/rules/editor.md`).
  - Observable: `TacticConditionChoice When`, `string NumberText` (the percent; kept as typed when the kind reads no
    number), `string SkillId` ("" the attack), `TacticTargetChoice On`, `string WeightText`; `int Index` (set by the
    list, Data's `tactics[i]`).
  - `public bool IsNumberShown` (the kind's shape reads a number: `TacticConditionKinds.ShapeOf`), `public string
    NumberUnit` ("%").
  - `public string Sentence`: "2. When an ally's HP is below 50 %: field_dressing on that one, weight 3"; "0. Always:
    attack on automatic, weight 4"; built from the choices' words, the skill's id or "attack" (decision 853).
  - `public string? Problem`: `TypedNumber.ProblemOf("Weight", ...)`, then the number's while shown ("Percent").
  - `public EnemyAction ToAction()` (throws on a problem): `SkillId` "" → null; `When` null → no condition; a kind
    reading no number → percent 0, whatever is typed.
  - `[NotifyPropertyChangedFor]` keeps `Sentence`, `Problem`, `IsNumberShown` and `NumberUnit` raised with the boxes.

## Changes
- `Editor/Battle/EnemyCatalogViewModel` (re-read after WS7): `public IReadOnlyList<TacticSkillChoice> SkillChoices`
  (the attack, then the content's skills with no `EnemySkillUse` refusal, by name, ordinal), `ConditionChoices` (=
  `TacticConditionChoice.All`) and `TargetChoices` (= `TacticTargetChoice.All`), for the tab's boxes to reach as
  `DropTableChoices` is reached. The constructor's `content` param doc names the skills' new reader.
- `docs/map.md`, Editor `Battle` row: the three choices, `TacticRuleRow`.

## Tests
- New `Editor.Tests/Battle/TacticChoiceTests`:
  - `ConditionChoices_EveryCoreKind_IsOfferedOnce` (every `TacticConditionKind` plus null: later kinds fail here until
    their slice adds the words)
  - `TargetChoices_EveryCoreTarget_IsOfferedOnce`
- `Editor.Tests/Battle/EnemyCatalogViewModelTests` (about 190 after WS7, about 210 after; the test world's catalog with
  skills built in the test added through `with`):
  - `SkillChoices_AttackFirstThenSkillsByName` (two skills added in reverse name order)
  - `SkillChoices_SkillWithAClassResource_IsNotOffered`
- New `Editor.Tests/Battle/TacticRuleRowTests` (actions built in the test):
  - `From_ActionWithACondition_ReadsEveryBox`
  - `ToAction_TypedBoxes_BuildsTheAction` (a fixture whose weight, percent and target all differ from `New`'s)
  - `ToAction_AttackRow_HasNoSkillId`
  - `ToAction_KindWithoutNumber_LeavesThePercentAtZero` (`allyFallen` with "40" typed)
  - `Problem_WeightNotANumber_NamesWeight`
  - `Problem_PercentNotANumberWhileShown_NamesPercent`
  - `Problem_PercentNotANumberWhileHidden_IsNull`
  - `Sentence_HealOnThatOne_ReadsWhenUseOnAndWeight` (asserts the whole line)
  - `Sentence_SkillChanged_IsRaised` (the relayed `PropertyChanged`)

## Verify by hand
None: nothing shows yet (EB7 shows the boxes).
- **Done (EB3a, the choices):** the three choice records and `EnemyCatalogViewModel.SkillChoices`,
  `ConditionChoices` and `TargetChoices`, with their four tests. Deviations: the Use box's list is built by
  `TacticSkillChoice.AllIn(content)`, as `Skills/StatusChoice.AllIn` builds the grant boxes', so the record holds the
  attack's words; `ConditionChoices` and `TargetChoices` are auto-properties set to `All` (an expression body is static
  data, which CA1822 refuses). No tie-break on equal names: two skills with one name read alike in the box in either
  order. A skill an enemy names is always in `SkillChoices`: Data refuses an enemy naming a skill it can't use, and the
  Skills page's check (`CatalogProblems.OfSkills`) runs the enemies against the edited skills, so the draft never holds
  one.
