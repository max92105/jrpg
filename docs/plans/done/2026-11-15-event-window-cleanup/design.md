# Event window cleanup: design

The README holds the order, the decisions and the defaults to confirm. This file holds the tunables, what the two
event dialogs look like today and after, the types, the key decisions and the risks.

## 1. Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Object window size | `Events/ObjectEventsWindow.xaml` | 1040 x 620 (was 820 x 560), also its `MinWidth` and `MinHeight` | Room for the event list, the step list and the fields side by side (decision 733). |
| Cell window size | `Events/CellEventWindow.xaml` | 760 x 560 (was 560 x 520), also its minimum | Room for the step list and the fields side by side. |
| Event list width | `Events/ObjectEventsWindow.xaml` | 260 px (was 300) | The Preset button no longer needs the width of four buttons. |
| Step list width | `Events/EventView.xaml` | 280 px | Fits a step label: `EventStepViewModel.LabelLineLength` cuts a dialogue's first line at 30 characters. |
| Fields column | `Events/EventView.xaml` | what is left: about 436 px in the object window at its minimum | Holds every step's fields; the warp row is split to fit it (EW4). |
| Lines box height | `Events/EventStepEditor.xaml` | 160 px (was 90) | A dialogue's lines, now that the fields have the column's height. |
| Group gap | `Shell/EditorDropDownStyles.xaml` | 6 px above each title | What sets a drop-down's groups apart, with the title's greyed semibold capitals. |
| Summary words | `Events/EventViewModel.ConditionSummary` | the event list's words (`Press "Open"`, `Walk into`, `On arrival`, ` if X`, ` while X leads`, the quest gate's ` when ...`), then `, once` | The folded conditions' one line (decision 732). |
| Keys | `Events/EventView.xaml.cs`, `Events/ObjectEventsWindow.xaml.cs` | Insert, Delete, Alt+Up, Alt+Down | Add, remove, move in each list (decision 734). |
| Script click grid | `.claude/scripts/open-editor-windows.ps1` | 24 px, at most 600 clicks | How `-EventWindows` searches the map for a cell and an object (decision 736). |

## 2. What exists (read on 2026-11-15)

- `Events/ObjectEventsWindow.xaml` (128 lines), 820 x 560. Left column, 300 px: "Events", then a `WrapPanel` of eight
  preset buttons and the Quest giver's quest box (`EventListViewModel.PresetQuestId`), wrapping to four rows, the event
  list, then Add, Remove, Up, Down. Right column: the event id, five condition rows (starts when, prompt, only while
  the object is, only while led by, only while quest with kind and stage) and the once box, "Steps, run in order", the
  step list, the chosen step's fields (`EventStepEditor`) docked under it, fourteen Add buttons wrapping to three rows,
  then Remove, Up, Down. With a dialogue step chosen, the fields take about 170 px and the buttons about 90, so the
  step list shrinks to a few rows.
- `Events/CellEventWindow.xaml` (86 lines), 560 x 520: the right column alone, without the prompt and object rows;
  twelve Add buttons (no Give item, no Swap object) and Remove, Up, Down in one wrap. Its view model's summary says it
  "offers dialogue, warp and cinematic steps", which was already stale. `CellEventViewModel` has no `OnlyWhenChoices`.
- `Events/EventViewModel.cs` (160): one `[RelayCommand]` per step kind (`AddDialogueCommand` ...
  `AddCompleteQuestCommand`), each calling `AddStep(kind)`; `Label` (the event list's line) built from the activation,
  the object, leader and quest conditions, the step count and ", once", pinned by many tests.
- `Events/EventListViewModel.cs` (148): eight preset commands, `PresetQuestId` and `HasPresetQuest`.
- `Events/EventStepEditor.xaml` (117): one `StackPanel` per kind, shown by `Is...` flags; its lists come from
  `DataContext.Choices` on the window (`RelativeSource AncestorType=Window`), so it works in any window whose view
  model has `Choices`. The warp row (lines 79-91: map box 160, "at cell (", two 40 px boxes, Pick..., Go to) is about
  460 px wide.
- The editor's menus are "one level deep" (`Shell/EditorTemplates.xaml`, 297 lines): bar headers and plain entries;
  `MenuEntry` draws its header with `RecognizesAccessKey="True"`, so a string header loses its first underscore. A
  `ContextMenu` gets the theme's frame (the map list's right-click menus use it). No drop-down button exists.
- The game refuses only Swap object on a cell (`Data/Maps/MapEventValidator.FindCellEventProblem`), and an interact
  activation there; Give item on a cell is accepted.
- `open-editor-windows.ps1` opens windows from the menu bar only, prints the editor's error output at the end, closes
  its editor in a `finally`, answers the unexpected-error box with `WM_COMMAND` through its `PostMessage` import, and
  needs `-Command` to pass several `-Windows`. The event windows open from a map click, so it can't reach them.
- Tests calling what changes (grep on 2026-11-15): the step commands at 12 sites in 5 files (`EventListViewModelTests`
  4, `EventListViewModelShopTests` 1, `EventListViewModelQuestTests` 3, `EventEditingViewModelTests` 2,
  `CellEventViewModelTests` 2), six test names naming a removed command; `PresetQuestId` and the quest giver's command
  at 4 sites in `EventListViewModelQuestGiverTests`. The other preset commands stay (7 sites untouched). The
  Cinematics tests' `AddCinematicCommand` is the Cinematics window's, not this one.

## 3. Today and after

Today, the object window (a dialogue step chosen):

```
+- Events of chest_3 (chest) -------------------------------------------------------+
| Events                          | Event id: chest_3_0                             |
| Preset [Chest][Door][Door you open]  Starts when the player  [Interact  v]        |
| [Sign][Talk][Forge master]      | Prompt (for interact)   [Open        ]          |
| [Shopkeeper][Quest giver]       | Only while the object is [          v]          |
| [errand        v]               | Only while led by        [          v]          |
| +-----------------------------+ | Only while quest [      v][kind  v][stage   v]  |
| | Press "Open": 2 step(s),once| | [x] Only once per playthrough                   |
| |                             | | Steps, run in order                             |
| |                             | | +---------------------------------------------+ |
| |                             | | | Dialogue: "A chest."   (two rows visible)   | |
| |                             | | +---------------------------------------------+ |
| |                             | | Lines, one message box each                     |
| |                             | | [                                         ]     |
| |                             | | Speaker [      ]  Portrait file [        v]     |
| +-----------------------------+ | Add [Dialogue][Give item][Swap object][Warp]    |
| [Add][Remove][Up][Down]         | [Move NPC][Cinematic][Recruit][Voice][Rest]...  |
|                                 | [Start quest][Advance quest][Complete quest]    |
|                                 | [Remove][Up][Down]                              |
|                                                          [OK] [Cancel]            |
+-----------------------------------------------------------------------------------+
```

After (EW5), the object window, conditions folded:

```
+- Events of chest_3 (chest) ----------------------------------------------------------------------+
| Events            [Preset ▾]   | Event chest_3_0                                                  |
| +----------------------------+ | > Press "Open" if chest_closed, once                             |
| | Press "Open" if chest_close| | Steps, run in order   [Add step ▾] [Remove] [Up] [Down]          |
| |                            | | +--------------------------+ +---------------------------------+ |
| |                            | | | Give item: potion x1     | | Give [1  ] x [potion         v] | |
| |                            | | | Swap object to: chest_op | |                                 | |
| |                            | | |                          | |                                 | |
| |                            | | |                          | |                                 | |
| +----------------------------+ | +--------------------------+ +---------------------------------+ |
| [Add] [Remove] [Up] [Down]     |                                                                  |
| Event chest_3_0, step 1: Choose the item to give.                                                 |
|                                                                               [OK] [Cancel]       |
+---------------------------------------------------------------------------------------------------+
```

A warp step in the fields column (EW4):

```
| Warp to [interior_1      v]  at cell ( [2  ] , [3  ] ) |
|         [Pick...] [Go to]                               |
```

The conditions opened (a click on the summary line; it stays open while another event is chosen):

```
| v Press "Open" if chest_closed, once                         |
|     Starts when the player    [Interact v]                   |
|     Prompt (for interact)     [Open          ]               |
|     Only while the object is  [chest_closed v]               |
|     Only while led by         [           v]                 |
|     Only while quest  [errand v] [active  v] [stage      v]  |
|     [x] Only once per playthrough (remembered in saves)      |
| Steps, run in order   [Add step ▾] [Remove] [Up] [Down]      |
```

The two drop-downs open under their button; titles are greyed semibold capitals with a gap above, and the arrow keys
skip them (the sketch indents the entries to show the groups; the menu sets them apart by the titles and the gap):

```
[Add step ▾]                        [Preset ▾]
+------------------+                +-----------------------------------+
| TALK             |                | OBJECTS                           |
|   Dialogue       |                |   Chest                           |
|   Voice          |                |   Door                            |
|   Cinematic      |                |   Door you open                   |
| WORLD            |                |   Sign                            |
|   Give item      |                | PEOPLE                            |
|   Swap object    |  (greyed on    |   Talk                            |
|   Warp           |   a cell)      |   Forge master                    |
|   Move NPC       |                |   Shopkeeper                      |
| PARTY            |                | QUEST GIVER                       |
|   Recruit        |                |   Quest giver: heist              |
|   Rest           |                |   Quest giver: placeholder_errand |
| SCREENS          |                +-----------------------------------+
|   Open Forge     |                (taller than the screen: it scrolls,
|   Open shop      |                 with the context menu's arrows)
| QUESTS           |
|   Start quest    |
|   Advance quest  |
|   Complete quest |
+------------------+
```

The cell window is the right part alone (`EventView`), with the summary, Add step and the two columns, at 760 x 560.

## 4. Design overview

### What moves where

| Today | After | Slice |
|---|---|---|
| Binding errors seen only in Visual Studio's Output window; event windows out of the smoke script's reach | The Debug editor's error output, printed by the script; `-EventWindows` | EW0 |
| The chosen event's XAML, twice (`ObjectEventsWindow`, `CellEventWindow`) | `Events/EventView.xaml`, a `UserControl` both windows hold | EW1 |
| The prompt and "only while the object is" rows, absent from the cell window's copy | In `EventView`, shown (and the object list bound) while `EventViewModel.IsObjectEvent` | EW1 |
| Fourteen Add buttons (twelve on a cell), `AddDialogueCommand` ... `AddCompleteQuestCommand` | One "Add step ▾" button over `EventViewModel.AddStepChoices`, one `AddStepCommand(EventStepKind)` | EW2 |
| Eight preset buttons and the quest box (`PresetQuestId`) | One "Preset ▾" button over `EventListViewModel.PresetChoices`; `ApplyQuestGiverPresetCommand(string questId)` | EW3 |
| Step list over the fields over the buttons | Step list left, fields right, buttons in the steps' heading row; the warp's buttons on their own line | EW4 |
| Five condition rows and the once box, always open | An `Expander` whose header is a `TextBlock` of `EventViewModel.ConditionSummary` | EW5 |
| Buttons only | The same buttons, and Insert, Delete, Alt+Up, Alt+Down on each list | EW6 |

### New types (all in Jrpg.Editor)

- `Events/EventView` (`UserControl`, EW1): shows the chosen event of a dialog: its conditions, its steps and the chosen
  step's fields.
- `Events/DropDownChoice` (sealed record, EW2): one entry of an event dialog's drop-down: its words, and the command
  and parameter it runs; an entry without a command is a group's title.
- `Events/StepKindMenu` (static, EW2): the Add step drop-down's entries: each step kind under its group's title, in the
  menu's order.
- `Events/DropDownMenu` (static, EW2, view code): opens a button's context menu under the button.
- `Shell/EditorDropDownStyles.xaml` (resource dictionary, EW2): `DropDownEntry`, the style of a drop-down's entry (its
  words drawn as plain text, its command and parameter, a title greyed with a gap above it).
- `Events/ListKeyCommand` (static, EW6, view code): runs a list's command from a key and gives the keyboard back to
  the list's chosen row.

No new Core, Data or Game type. Changed: `Hosting/App.xaml.cs` (the Debug binding trace, EW0), `Hosting/App.xaml` (one
merge line, EW2), `EventViewModel` (`IsObjectEvent` EW1; `AddStepCommand`, `AddStepChoices` EW2; the condition words
and `ConditionSummary` EW5), `CellEventViewModel` (sets `IsObjectEvent = false` EW1; its summary EW2),
`EventListViewModel` (`PresetChoices`, the quest giver's command takes its quest, `PresetQuestId` removed, EW3),
`EventStepEditor.xaml` (lines box and warp row, EW4), `EventView.xaml.cs` and `ObjectEventsWindow.xaml.cs` (the
drop-down clicks EW2 and EW3, the key handlers EW6), `Shell/ControlsHelpWindow.xaml` (the Events lines, EW2, EW3, EW5,
EW6), `.claude/scripts/open-editor-windows.ps1` (`-EventWindows` EW0, its key pass EW6).

### Folder dependencies

| From | To | Why | New? |
|---|---|---|---|
| `Events/EventView.xaml`, `ObjectEventsWindow.xaml` | `Shell/EditorDropDownStyles.xaml` | the keyed style `DropDownEntry`, through `App.xaml`'s merged resources | new (resources only, no code reference) |
| `Events/EventView.xaml` | `Events/EventStepEditor` | holds it, as the windows did | moved |
| `Events/ObjectEventsWindow.xaml`, `CellEventWindow.xaml` | `Events/EventView` | hold it | new, same folder |
| `Events/EventViewModel` | `Events/StepKindMenu`, `Events/DropDownChoice` | builds its Add step entries | new, same folder |
| `Events/EventView.xaml.cs`, `ObjectEventsWindow.xaml.cs` | `Events/DropDownMenu`, `Events/ListKeyCommand` | open a drop-down, run a key | new, same folder |

Nothing in `Shell/` code reads `Events/` types it didn't read before (`MainWindow.EventEditing` still opens the two
windows the same way). `Hosting/App` gains no reference: the trace is WPF's own `PresentationTraceSources`.

### Key decisions

1. **One view, two windows (decision 727).** `EventView`'s `DataContext` is the chosen `EventViewModel`; the lists it
   offers (activations, leaders, quests, kinds) are read from the window's view model through `RelativeSource
   AncestorType=Window`, as the conditions and `EventStepEditor` already do, so both window view models keep their
   same-named properties (`Activations`, `LeaderChoices`, `QuestChoices`, `ConditionKinds`, `Choices`). The object
   window's `OnlyWhenChoices`, which `CellEventViewModel` lacks, is bound only by a `DataTrigger` on `IsObjectEvent`
   (a style `BasedOn` the theme's `ComboBox` style that sets `ItemsSource`), since a collapsed row still evaluates a
   plain binding and logs an error in the cell window. The warp step's routed events still bubble to the window's
   `DockPanel` handlers: `EventView` adds nothing to their path.
2. **`IsObjectEvent` on the event, not a view flag (decisions 727, 728).** An `init` property, true by default;
   `CellEventViewModel` builds its event with it false. It hides the two object-only rows (EW1) and greys Swap object
   (EW2, the command's `CanExecute`), so the rule is tested, not only drawn.
3. **Entries as a view-model list (decision 730).** The alternative, entries written in XAML with
   `CommandParameter="{x:Static ...}"`, is shorter for Add step but can't list one Quest giver per quest without a
   `CompositeCollection` (whose `CollectionContainer` binding has no `DataContext`), and leaves the menu's order and
   completeness untested. As a list, each generated `MenuItem`'s `DataContext` is its `DropDownChoice`, so its bindings
   read the entry itself. A title is `DropDownChoice.Title("WORLD")`: no command, so `DropDownEntry` disables it
   (greyed, skipped by the arrow keys) and gives it the group gap.
4. **A drop-down is a button and its context menu (decision 730).** "Add step ▾" and "Preset ▾" are plain `Button`s
   (the theme's look, nothing new to template) with a `<Button.ContextMenu>` whose `ItemsSource` is the entries and
   `ItemContainerStyle` is `DropDownEntry`. The click calls `DropDownMenu.Open(button)`: the menu's `PlacementTarget`
   the button, `Placement` `Bottom`, its `DataContext` the button's (so it reads the view model however it opened),
   `IsOpen` true; an open context menu takes the keyboard, so EW6's Insert calls the same. `DropDownEntry` writes its
   `CommandParameter` setter before `Command`, so the command's first `CanExecute` sees the parameter, and draws the
   header through a `HeaderTemplate` `TextBlock`, so `placeholder_errand` keeps its underscore (the theme's `MenuEntry`
   reads one `_` as an access key). A `Menu` was the other way: it needed a new top-level template, a role trigger and
   `IsMainMenu="False"` to keep Alt out of it.
5. **The condition words are written once (decision 732).** A private `ConditionWords()` builds `Press "Open" if
   door_close while omer leads when errand is active`; `Label` is `$"{ConditionWords()}: {n} step(s)"` plus ", once",
   byte for byte as today; `ConditionSummary` is the words plus ", once". It is notified with `Label` by each condition
   field and the gate's relay. The `Expander`'s header is an explicit `TextBlock` (trimmed with an ellipsis), not a
   string, for the same underscore reason.
6. **The fold is view state.** The `Expander` is one control in `EventView`; choosing another event changes its
   `DataContext`, not the control, so its `IsExpanded` survives the change, and a new window starts folded (decision
   732). No view-model property; the hand check confirms it.
7. **The implementer checks the windows himself (decision 736).** In a Debug build, `App` sets
   `PresentationTraceSources.DataBindingSource.Switch.Level` to `Warning` with a `ConsoleTraceListener` on the error
   stream, which the script already collects; `-EventWindows` reaches the windows by clicking the map. The look stays
   the developer's to judge.

### Extension points
- A new step kind: `EventStepKind`, `EventStepViewModel`'s switches (`Label`, `Problem`, `ToStep`, `From`),
  `EventStepEditor.xaml`'s fields, and now one line in `StepKindMenu` under its group; EW2's
  `AddStepChoices_EveryKind_HasOneEntry` fails until that line exists.
- A new preset: an `EventPresets` method, a command on `EventListViewModel`, one line in its preset menu.
- A later window with a drop-down (the mode bar row) can use `DropDownMenu` and `DropDownEntry` as they are; move
  `DropDownChoice` and `DropDownMenu` to `Shell/` when a second folder needs them.
- Another window out of the menu bar's reach can get its own switch in the script beside `-EventWindows`.

### Replacement mapping

| Existing code | Replaced by |
|---|---|
| `EventViewModel.AddDialogue` ... `AddCompleteQuest` (14 commands) | `EventViewModel.AddStepCommand` with an `EventStepKind`, entries from `StepKindMenu` |
| `EventListViewModel.PresetQuestId`, `HasPresetQuest` | `EventListViewModel.PresetChoices`, one quest giver entry per quest; `ApplyQuestGiverPresetCommand` takes the quest id |
| The duplicated event XAML in `ObjectEventsWindow.xaml` and `CellEventWindow.xaml` | `EventView.xaml` |
| `EventViewModel.Label`'s inline words | `ConditionWords()`, shared with `ConditionSummary` |
| The condition rows' always-open `StackPanel` | An `Expander` with a `ConditionSummary` `TextBlock` as its header |

## 5. Risks

- **Binding-trace noise (EW0).** The first `-EventWindows` run may list binding errors that exist today in other
  windows or in the event windows themselves; EW0's Done note records that list as the baseline, and later slices
  check they add none.
- **Clicking the map from a script.** The clicks need the editor in front (the script focuses its main window first)
  and an object within the shown part of the map; if 600 clicks reach no object, the script says so and the slice's
  check falls back to the Placed object panel's Edit events... by hand.
- **The context menu.** A `ContextMenu` opened by `IsOpen` reads the `DataContext` it is given, which `DropDownMenu`
  sets. Its default template scrolls a list taller than the screen with arrow buttons at its ends, which may draw in
  the light Windows look; with today's quests both lists fit, so this is noted, not fixed.
- **Underscores.** Any new header or menu text written as a plain string loses its first `_`; the guiding rule and each
  hand check guard it.
- **Test churn.** EW2 edits 12 calls in 5 files, mechanically (`open.AddWarpCommand.Execute(null)` becomes
  `open.AddStepCommand.Execute(EventStepKind.Warp)`), and renames six tests; EW3 rewrites
  `EventListViewModelQuestGiverTests` (4 calls) and deletes `PresetQuestId_Emptied_GreysTheButton` and
  `PresetQuestId_DraftWithQuests_IsTheFirstById` with the property they tested (a removed behavior, named in the
  slice).
- **Keyboard focus after a key (EW6).** Removing or moving an item replaces its row's container, which drops the
  focus, so a second Delete would do nothing; `ListKeyCommand` focuses the chosen row again once the list has laid
  out. An Alt chord arrives as `Key.System` with the arrow in `SystemKey`; the handlers read both.
- **Width.** The fields column is about 436 px at the object window's minimum; a step row wider than that (the warp's,
  before EW4 splits it) would clip, so EW4's fields `ScrollViewer` also scrolls sideways as a guard.
- **Other plans.** No other roadmap row touches `Events/`; a later step kind (none planned) must add its
  `StepKindMenu` line, which the EW2 test enforces.
- **No save, format or determinism impact.** The records written are the same; `MapEventComparison` still decides "no
  change".
