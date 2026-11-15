# EB7. The Tactics tab

Editor, about 190 lines (about 160 of XAML in a new user control, about 3 lines on the page, an
`InitializeComponent`-only code-behind). Needs EB6 and the mode bar's WS7. **Re-read the Enemies page first** (WS7's
`Battle/EnemyCatalogPage.xaml` and its host in `Workspaces/`, as the mode bar plan left them): the tab goes on that
page. Design: design.md, section 4.2 (the layout), decisions 849-852.

## Goal
The Enemies page gets a third tab, Tactics, where the developer writes "when / use / on" rules: the lists on the left
(from the start and each phase, a phase's threshold, Add phase, Remove phase, the "Uses:" line), the chosen list's
numbered sentences on the right with Add, Remove, Up and Down, and the chosen rule's boxes under them.

## New types (Editor, `Battle/`)
- `EnemyTacticsView` (`UserControl`, `x:ClassModifier="internal"`): the Tactics tab, bound to an
  `EnemyTacticsViewModel`.
  - Left column (180 px): a `ListBox` of `Lists` (`DisplayMemberPath="Label"`, `SelectedItem` two-way), under it the
    threshold box (`ThresholdText`, enabled by `SelectedList.IsPhase`), Add phase and Remove phase, then
    `SkillsUsedLine` wrapped in the muted brush.
  - Right column: a `ListBox` of `SelectedList.Rules` (`DisplayMemberPath="Sentence"`), Add, Remove, Up and Down bound
    to the list's commands; under them a panel with the `EnabledWhenChosen` style bound to `SelectedList.SelectedRule`:
    When (`ComboBox`, `ItemsSource` the page view model's `ConditionChoices`, `DisplayMemberPath="Label"`,
    `SelectedItem` `When`), the number box and its unit (visible by `IsNumberShown`), Use (`ComboBox` over
    `SkillChoices`, `DisplayMemberPath="Name"`, `SelectedValuePath="Id"`, `SelectedValue` `SkillId`), On (over
    `TargetChoices`, `SelectedItem` `On`), Weight. The page view model is reached as the page reaches
    `DropTableChoices` today (re-read: after WS7 it is the page's `DataContext`, not a window's).
  - Every keyed or local style is `BasedOn` the theme's (`.claude/rules/editor.md`); the page's `FieldLabel` and
    `NumberBox` styles are reached through the resource lookup up the tree (no copy) if the page defines them as
    resources; otherwise the control declares its own, `BasedOn` the theme's.

## Changes
- The Enemies page's XAML: a third `TabItem Header="Tactics"` holding `<local:EnemyTacticsView DataContext="{Binding
  Tactics}" />` inside the chosen enemy's panel (about +3 lines).
- `docs/map.md`, Editor `Battle` row: the Enemies page's Tactics tab (`EnemyTacticsView`).
- `.claude/scripts/open-editor-windows.ps1`: no new entry (the tab is on the Enemies page, already in the default list
  as WS7 left it).

## Tests
None new: the rules are tested headless in EB3-EB6. Run `.claude/scripts/open-editor-windows.ps1 -Windows
'Database>Enemies'` (the entry as WS7 named it): no `ERROR` line and no binding-error line (the event window plan's
EW0).

## Verify by hand
The Enemies page shows no Mana Seed art (its battlers are the gnoll GIF strips and placeholders), so an agent may look
at its capture.
1. Run the editor, Database > Enemies, choose `goblin_chief`, open Tactics. The left list shows "From the start" and
   "Below 50 % HP"; the right list reads "0. Always: attack on automatic, weight 4", "1. Always: cleave on automatic,
   weight 2", "2. When an ally's HP is below 50 %: field_dressing on automatic, weight 3".
2. Choose rule 2, set On to "that one": the sentence ends "on that one, weight 3". For this heal, "that one" after "an
   ally's HP is below" heals what automatic already healed (the lowest share among the allies below 50 %); "the lowest
   HP on its side" widens it to every ally and the Chief itself. Set On to "the foe with the lowest HP": the problem
   line names "tactics[2] (field_dressing).on" and applying is refused; set it back to "that one".
3. Choose rule 1, click Add: a rule 2 "Always: attack on automatic, weight 1" appears and is chosen; set When to "its
   HP is below", type 30, Use "Crushing Blow", On "the foe with the lowest HP"; click Up: it becomes rule 1.
4. Choose "From the start", click Add phase: "Below 25 % HP" appears chosen with one attack rule; Remove phase removes
   it; Remove phase is grey on "From the start".
5. The "Uses:" line reads the skills the rules name.
6. Apply, then File > Save all; `git diff content/enemies.json` shows the Chief's new rule with `"on": "lowestHpFoe"`
   and `"on": "subject"`, nothing else changed. Ctrl+Z, Save all again: the diff is empty.
7. Both columns fit the page at the editor's usual size without a horizontal scroll bar; if not, the Done note says what
   was narrowed.
