# EW5. The conditions folded under a one-line summary

Editor, about 110 lines (about 50 of them tests). Design: section 1 ("Summary words"), section 3 (the opened sketch),
key decisions 5 and 6. Decision 732. Needs EW4 (the side-by-side layout the fold is judged on).

## Goal
The five condition rows and the once box fold under one line that says them: the event list's words without the
step count, then ", once" (`Press "Open" if door_close when errand is active, once`). A click on the line opens or
folds them; folded when a window opens, kept as set while another event is chosen. Both windows (a cell's line never
has a prompt or an object).

## New types
None.

## Changes
- `Events/EventViewModel.cs` (about 125 lines after EW2, about 135 after):
  - a private `ConditionWords()` holding what `Label` builds today before the step count: the start (`Walk into`, `On
    arrival`, `Press "{Prompt}"`), ` if {OnlyWhenObjectId}`, ` while {OnlyWhenLeaderId} leads`, `QuestGate.Label`.
  - `Label` becomes `$"{ConditionWords()}: {Steps.Count} step(s){(IsOnce ? ", once" : "")}"`, byte for byte as today
    (every `Label` test passes unchanged and guards it).
  - `public string ConditionSummary => $"{ConditionWords()}{(IsOnce ? ", once" : "")}";` with a one-line summary and
    its example.
  - `nameof(ConditionSummary)` joins `nameof(Label)` in the `[NotifyPropertyChangedFor]` of `Activation`, `Prompt`,
    `IsOnce`, `OnlyWhenObjectId` and `OnlyWhenLeaderId`; the quest gate's relay raises both. The steps' relay stays
    `Label`'s alone.
- `Events/EventView.xaml`: the condition rows and the once box move into an `Expander` (`IsExpanded="False"`) under the
  id line, its header an explicit `<Expander.Header><TextBlock Text="{Binding ConditionSummary}"
  TextTrimming="CharacterEllipsis" /></Expander.Header>`: a string header would read the first `_` of an id as an
  access key and drop it (its comment says so). Tooltip: "Click to open or fold when the event runs."
- `Shell/ControlsHelpWindow.xaml`, Events: "The chosen event's conditions (what starts it, the prompt, the object,
  leader and quest it waits for, once) fold under one line that says them; click the line to open or fold them."
- `docs/map.md`, Events row: `EventViewModel.ConditionSummary` (the folded conditions' line: the list's words and
  "once").

## Tests
- `EventViewModelConditionSummaryTests` (new, `tests/Jrpg.Editor.Tests/Events/`; events and `StepChoices` with quest
  stages built in the test). The words themselves are `Label`'s, already tested; these test the join and the
  notifications:
  - `ConditionSummary_Once_IsTheWordsThenOnce` (`Press "Open" if door_close, once`: an id with an underscore).
  - `ConditionSummary_NotOnce_IsTheWordsAlone` (an arrival event for one leader and a quest: `On arrival while omer
    leads when errand is active`, no step count).
  - `ConditionSummary_QuestGateChanged_IsRaised` (the relay arm: `QuestGate.QuestId` set, `PropertyChanged` names
    `ConditionSummary`).
  - `ConditionSummary_ConditionFieldChanged_IsRaised` (a theory, one row per attribute arm: `Activation`, `Prompt`,
    `IsOnce`, `OnlyWhenObjectId`, `OnlyWhenLeaderId`).
- Every `Label` test passes unchanged.

## Verify by hand
1. The implementer: `open-editor-windows.ps1 -EventWindows`: both captures show one summary line and no condition
   rows; no binding-error line beyond EW0's baseline (none naming `ConditionSummary`).
2. The developer (F5): **Events** tab, click a door; apply **Preset ▾** > **Door you open** if it has no events. Under
   "Event ..." one line reads `Press "Open" if door_close` (the door's own id, its underscore whole). Click it: the
   rows open. Tick **Only once per playthrough**: the line ends with `, once` at once, and the event list's line too.
3. Choose the second event: the rows stay open and show its conditions; its line reads `Walk into if door_open`. Fold,
   choose the first event: still folded.
4. **Cancel**, click the door again: the conditions start folded.
5. Click an empty cell with an event: its line reads `Walk into` (or `On arrival ...`); open it: no prompt or object
   row.
6. **F1**: the line about the folded conditions.

- **Done, with deviations:** the tooltip sits on the header line rather than on the `Expander`, whose tooltip would
  also cover the rows inside it; `Label` and `ConditionSummary` share a private `OnceWords` with `ConditionWords()`.
  The header's tooltip repeats the line (review: a narrow column trims its end). EW3's Preset tooltip gains the
  old Quest giver button's hint, the third event (EW3 and EW4 review follow-up). The smoke script opened both windows with no binding-error line; the look is the developer's hand check, steps 2
  to 6.
