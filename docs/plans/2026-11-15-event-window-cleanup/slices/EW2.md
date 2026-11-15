# EW2. One Add step drop-down

Editor, about 220 lines (about 70 of them tests, 12 test calls edited and six tests renamed). Design: section 3 (the
Add step sketch), section 4, key decisions 2, 3 and 4. Decisions 728, 729, 730. Needs EW1 (`EventView`,
`IsObjectEvent`).

Above the 200-line target by the mechanical test edits. Split point if the size check asks: EW2a, the command
(`AddStepCommand` with its `CanExecute`, the 12 edits and six renames, the two cell tests) with the fourteen buttons
bound to it; EW2b, `DropDownChoice`, `StepKindMenu`, `DropDownMenu`, the style file and the button, with the menu's
three tests.

## Goal
The fourteen Add buttons become one **Add step ▾** button beside Remove, Up and Down; its click opens a menu under it
with the kinds under five greyed titles in capitals (decision 729). On a cell's event, Swap object is greyed (decision
728). A chosen entry adds its step after the chosen step, as the buttons did.

## New types
- `Events/DropDownChoice` (`internal sealed record DropDownChoice(string Label, ICommand? Command, object? Parameter)`):
  one entry of an event dialog's drop-down: its words, and the command and parameter it runs; an entry without a
  command is a group's title.
  - `public bool IsTitle => Command is null;`
  - `public static DropDownChoice Title(string label)`.
- `Events/StepKindMenu` (`internal static class`): the Add step drop-down's entries: each step kind under its group's
  title, in the menu's order.
  - `public static IReadOnlyList<DropDownChoice> Choices(ICommand addStep)`: TALK (Dialogue, Voice, Cinematic), WORLD
    (Give item, Swap object, Warp, Move NPC), PARTY (Recruit, Rest), SCREENS (Open Forge, Open shop), QUESTS (Start
    quest, Advance quest, Complete quest); each entry's `Parameter` is its `EventStepKind`.
- `Events/DropDownMenu` (`internal static class`, view code): opens a button's context menu under the button.
  - `public static void Open(Button button)`: the button's `ContextMenu` gets `PlacementTarget` the button,
    `Placement` `PlacementMode.Bottom`, `DataContext` the button's, then `IsOpen = true`.
- `Shell/EditorDropDownStyles.xaml` (resource dictionary, merged after `EditorTemplates.xaml`, whose implicit
  `MenuItem` style it builds on): `DropDownEntry` (`Style`, `MenuItem`, `BasedOn="{StaticResource {x:Type
  MenuItem}}"`): setters in this order: `CommandParameter` from `Parameter`, then `Command` (so the first `CanExecute`
  sees the parameter), `Header` from `Label`, and a `HeaderTemplate` of `<TextBlock Text="{Binding}" />` (the theme's
  `MenuEntry` reads a string header's `_` as an access key); a `DataTrigger` on `IsTitle` disabling the entry (greyed
  by `MenuEntry`'s own trigger, skipped by the arrow keys), semibold, with the 6 px gap above (design, section 1). Its
  comment says why the order and the template.

## Changes
- `Events/EventViewModel.cs` (160 lines, about 125 after): the fourteen `Add...` `[RelayCommand]` methods go;
  `AddStep(EventStepKind kind)` becomes the `[RelayCommand(CanExecute = nameof(CanAddStep))]` (still after the chosen
  step, or last); `private bool CanAddStep(EventStepKind kind) => kind != EventStepKind.SwapObject || IsObjectEvent;`
  `public IReadOnlyList<DropDownChoice> AddStepChoices { get; }`, set in the constructor to
  `StepKindMenu.Choices(AddStepCommand)`. `IsObjectEvent`'s summary adds "Swap object can't be added".
- `Events/EventView.xaml` and `.xaml.cs`: the Add `WrapPanel` and the Remove row become one row: `<Button
  x:Name="AddStepButton" Content="Add step ▾" Click="OnAddStepClicked" ToolTip="Add a step after the chosen one">`
  with a `<Button.ContextMenu>` whose `ItemsSource` is `AddStepChoices` and `ItemContainerStyle` `DropDownEntry`, then
  Remove, Up, Down. `OnAddStepClicked` calls `DropDownMenu.Open(AddStepButton)`. EW1's Swap object visibility binding
  goes with the buttons.
- `Hosting/App.xaml`: merge `Shell/EditorDropDownStyles.xaml` after `EditorTemplates.xaml`.
- `Shell/ControlsHelpWindow.xaml`, Events: "Add step (in the event's steps): choose a kind; the step goes after the
  chosen one. Swap object is greyed in a cell's event: the game only swaps an object from that object's own event."
- Tests calling the removed commands, edited in place (`open.AddWarpCommand.Execute(null)` becomes
  `open.AddStepCommand.Execute(EventStepKind.Warp)`): `EventListViewModelTests` (4), `EventListViewModelShopTests`
  (1), `EventListViewModelQuestTests` (3), `EventEditingViewModelTests` (2; 286 lines, no growth),
  `CellEventViewModelTests` (2). The six names naming a removed command become `AddStepCommand_` plus the same
  scenario: `AddDialogueCommand_StepSelected_InsertsAfterIt`, `AddWarpCommand_AddsAWarpToCellZeroOfNoMapYet`,
  `AddOpenForgeCommand_NoStepChosen_AddsOpenTheForgeLast` (`EventListViewModelTests`),
  `AddOpenShopCommand_NoStepChosen_AddsOpenShopLast` (`EventListViewModelShopTests`),
  `AddAdvanceQuestCommand_NoStepChosen_AddsItLast`, `AddStartAndCompleteQuestCommands_WithTheQuestPicked_RoundTrip`
  (`EventListViewModelQuestTests`; the last becomes `AddStepCommand_StartAndCompleteWithTheQuestPicked_RoundTrip`).
- `docs/map.md`: Events row: `DropDownChoice`, `StepKindMenu` (every step kind once, under its group), `DropDownMenu`,
  and `EventViewModel.AddStepCommand`; Shell row: `EditorDropDownStyles.xaml` (`DropDownEntry`: a drop-down entry's
  words as plain text, its command, a title greyed).

## Tests
- `EventViewModelAddStepTests` (new, `tests/Jrpg.Editor.Tests/Events/`; an `EventViewModel` over a one-step event and
  `StepChoices` built in the test):
  - `AddStepChoices_AnyEvent_ListsTheKindsUnderFiveTitlesInOrder` (asserts the whole `(Label, IsTitle)` sequence).
  - `AddStepChoices_EveryKind_HasOneEntry` (the entries' parameters equal `Enum.GetValues<EventStepKind>()` as a set,
    with no repeat: a later kind left out of the menu fails here).
  - `AddStepChoices_EachEntry_AddsItsKind` (runs every non-title entry's `Command` with its `Parameter`; the step
    selected after each has that `Kind`).
  - `AddStepCommand_SwapObjectOnObjectEvent_CanExecute`.
- `CellEventViewModelTests` (about 100 after EW1, about 120 after):
  - `AddStepCommand_SwapObjectOnCell_CannotExecute`.
  - `AddStepCommand_GiveItemOnCell_CanExecute` (the other side of the rule, and decision 728's new entry).

## Verify by hand
1. The implementer: `open-editor-windows.ps1 -EventWindows`: both windows open; no binding-error line beyond EW0's
   baseline (none naming `DropDownChoice`, `AddStepChoices` or `Parameter`).
2. The developer (F5): **Events** tab, click an NPC or a chest; choose a step, click **Add step ▾**: the menu opens
   under the button with **TALK**, **WORLD**, **PARTY**, **SCREENS**, **QUESTS** greyed over their entries; Down from
   the top skips the titles. Click **Give item**: the step appears after the chosen one, selected, its fields shown.
3. Close with **OK**, click an empty cell: in its window **Add step ▾** shows **Swap object** greyed and **Give item**
   available; add a Give item step, pick `potion`, **OK**; **Ctrl+S**: the map saves (the game takes Give item on a
   cell). **Ctrl+Z** if the change isn't wanted.
4. **F1**: the Events section has the Add step line.

- **Done, with deviations:** `DropDownMenu.Open` doesn't copy the button's data context to the menu (review): the copy
  went stale when another event was chosen, and a right click, which opens the menu without `Open`, then added the
  step to the old event; `PlacementTarget` set before `IsOpen` already hands the menu the button's context. Four of
  the six renamed tests name their kind in the scenario (`AddStepCommand_Warp_AddsAWarpToCellZeroOfNoMapYet`), since
  one command now serves every kind. The smoke script opened both event windows with no binding-error line. Hand
  check steps 2 to 4 are the developer's; step 2 also looks at the 6 px gap above TALK, the menu's first title.
