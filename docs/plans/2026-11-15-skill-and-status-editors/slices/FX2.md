# FX2. Database > Effects...: the tier ids, the rules and the page

Editor + script, about 290 lines (rules about 110, page XAML about 70, wiring about 10, tests about 100; one slice by
the review's choice, a page being lighter than a window: if `/slice` must split, the page goes second). Decisions 976,
983, 985 (the id rule), 996. Design: design.md, sections 1 (`ShownPower`), 2.4 and 2.7 ("Effects"); the mode bar's
[page recipe](../../2026-11-15-mode-bar/page-recipe.md). Builds on FX1 and SK4.

## Goal
Database > Effects... opens the `effects.json` page: every tiered effect in id order then the added ones, each as
typed (id while new, kind `damage` or `restoreHp`, damage type for a damage, `perPower`), its "Used by" line (the
draft's skills naming it) and "At Power 10: 20 per hit" line (Core's `DamageFormula.TieredAmount`); Add, Duplicate
(the copy's id filled with the next tier, `fire_2` after `fire_1`, still editable), Remove only an added one; the
problem line (an effect's own, in list order, then Data's, skills included); OK applies the session as one
`SetTieredEffects` step, everything naming an effect following it. A new or retuned tier is made without JSON.

## New types
- `Editor/Catalogs/TierIds` (internal static): the id of a record's next tier, shared by Effects and Statuses (FX5).
  `Stem(string id)` (the id without a trailing `_<digits>`: `fire_1` gives `fire`, `stun` gives `stun`);
  `Next(string chosenId, IEnumerable<string> takenIds)`: the stem, an underscore and one more than the highest number
  any taken id of that stem carries (a bare stem counting as 0), so `fire_2` from `fire_1`, `stun_1` from `stun`,
  `fire_3` when `fire_2` exists.
- `Editor/Effects/TieredEffectViewModel` (internal sealed partial, `ObservableObject`): one tiered effect as typed.
  - `IdText` (editable while `IsNew`), `Kind` (from `Kinds`: `damage`, `restoreHp`), `DamageType` (`DamageType?`,
    enabled by `CanTypeDamageType` for a damage; written null for a heal), `PerPowerText`.
  - `UsedByText` ("Used by: fire_blast, fireball", or "Used by no skill"), from the skills given at construction.
  - `AmountText` ("At Power 10: 20 per hit"; empty while `perPower` isn't a whole number), `ShownPower` = 10 (a named
    constant) through `DamageFormula.TieredAmount(ShownPower, perPower, 0)`.
  - `Problem` (the typed id through `Objects/ObjectIds.ProblemOf`, a box not a whole number); `ToEffect()`; `static
    From`, `New`, `DuplicateOf(chosen, string nextId)`; `Changed` for typed fields only.
- `Editor/Effects/TieredEffectCatalogViewModel` (internal sealed partial, `ObservableObject`, implements
  `Workspaces/IEditedCatalog`): `Effects`, `SelectedEffect`, `AddEffectCommand`, `DuplicateEffectCommand` (the copy
  after the chosen one, `IdText` = `TierIds.Next(chosen id, every typed id)`), `RemoveEffectCommand` (an added one
  only), `Problem` (`Catalogs/RecordListProblem`, then `CatalogProblems.OfTieredEffects` over the typed list),
  `ChosenKey`, `Choose`, `ToEffects()`.
- `Editor/Effects/TieredEffectEditingViewModel` (internal sealed): `Start()` from the draft, `Apply(edited)` through
  `CatalogDraft.SetTieredEffects`; the `ICatalogEditing<TieredEffectCatalogViewModel>` shape (declared from the mode
  bar's WS2, as SK3 says).
- `Editor/Effects/TieredEffectCatalogPage` (`UserControl`): the page by the recipe: the list left (id and kind line)
  with Add, Duplicate, Remove; Id, Kind, Damage type, Per Power and the two lines right; typed boxes
  `PropertyChanged`; bindings name `AncestorType=effects:TieredEffectCatalogPage`.

## Changes
- `Editor/Shell/MainViewModel.Database.cs`: `TieredEffectEditing`; `Editor/Shell/MainViewModel.cs`: one constructor
  line (+1).
- `Editor/Shell/MainWindow.Database.cs`: one opener registered under "Effects" through `OpenInDialog`.
- `Editor/Shell/DatabaseMenu.xaml`: "Effects..." (`Tag="Effects"`) beside "Statuses..." and "Effect strips...",
  tooltip "effects.json: fire_1, heal_2...; an image's pace is Effect strips".
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Effects...'` in the default list.
- `docs/map.md`: an Editor `Effects` row.

## Tests
- `Editor.Tests/Catalogs/TierIdsTests` (new): `Next_Fire1_IsFire2`, `Next_StunWithoutNumber_IsStun1`,
  `Next_Fire2Taken_IsFire3`, `Stem_IdEndingInLetters_IsTheId`.
- `Editor.Tests/Effects/TieredEffectViewModelTests` (new; `SkillFixtures`):
  - `From_DamageRecord_FillsEveryBox`.
  - `UsedByText_TwoSkillsNameIt_NamesBoth`, `UsedByText_NoSkill_SaysSo`.
  - `AmountText_PerPowerTwo_ReadsTwentyAtPowerTen`, `AmountText_PerPowerNotANumber_IsEmpty`.
  - `ToEffect_RestoreHp_WritesNoDamageType` (a type chosen before the kind changed).
  - `Problem_NewWithATakenId_NamesIt`.
- `Editor.Tests/Effects/TieredEffectCatalogViewModelTests` (new; `SkillFixtures`):
  - `Effects_Order_IdOrderThenAdded`.
  - `DuplicateEffect_Fire1_AddsFire2AfterIt`.
  - `RemoveEffect_SavedRecord_Greyed`.
  - `Problem_KindChangedUnderASkill_IsDataMessageNamingTheSkill`.
  - `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`, `ChosenKey_NewEffectTyped_IsItsTypedId`.
- `Editor.Tests/Effects/TieredEffectEditingViewModelTests` (new): `Apply_EditedPerPower_SkillsHoldTheNewRecord`.

## Verify by hand
1. Database > Effects...: the records in id order; `fire_1` reads "Used by: fire_blast, fireball" and "At Power 10: 20
   per hit".
2. Per Power 3: the line reads 30; OK, Save all: `git diff content/effects.json` shows `fire_1`'s number only. Run the
   game: Fire Blast hits harder. Ctrl+Z, Save all.
3. Choose `heal_1`, Kind damage: the problem line reads `effects.json`'s "a damage effect needs a damageType." (the
   type box stays empty); pick Damage type fire: it names `skills.json` and a healing skill (Data's message); OK greys
   throughout; Esc.
4. Duplicate `fire_2`: the copy reads `fire_3`, its Per Power 4; OK; Database > Skills..., `fireball`'s Tier box
   offers `fire_3`; Esc, then Ctrl+Z takes `fire_3` out.
5. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Effects...: opened", no `ERROR` or binding-error line.
- **Done (FX2a, the rules):** `Catalogs/TierIds`, `Effects/TieredEffectViewModel`, `TieredEffectCatalogViewModel`,
  `TieredEffectEditingViewModel`, their tests and the map lines; no page, menu entry or shell wiring yet (FX2b).
  Deviations: `DamageFormula` is internal to Core, so the amount line reads Core's new `TieredEffect.AmountAt(power)`
  (one hit before modifiers and target, through `DamageFormula.TieredAmount`; decision 1001, tested in Core's
  `TieredEffectTests`). `DuplicateOf` takes the saved ids as a third parameter, for the copy's id problem. Unnumbered
  choices: Add starts a damage of Core's first type at per-Power 1 (Data's least, a named constant), so only the id is
  left to type; a damage type is kept while the kind is a heal and written empty, and switching back to damage fills
  none (as SK6), so hand check 3 now reads `effects.json`'s message first; the per-Power box's problem says "Per
  Power"; `TierIds.Next` counts the chosen id among the taken ones, counts past int's range in a `long` and ignores a
  number too big for an int. Size: about 335 Editor lines with their doc comments (the plan's rules about 110) and 7
  in Core; tests about 520 (Editor 42 tests, Core 3). The problem line is `RecordListProblem`'s, as on every page: an
  effect's own, in list order, then Data's (the Goal said the chosen one's first; reworded here and in FX5.md).
  Duplicate on an added effect whose id is still empty gives the copy an empty id rather than `_1`.
