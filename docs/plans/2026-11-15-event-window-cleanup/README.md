# Event window cleanup
Status: approved

Roadmap row 3 of [the editor push](../2026-11-15-editor-push/README.md) (its design.md, section 3.2). The developer:
"maybe the event window is a lot cluttered and hard to use at the moment." The roadmap's direction: the presets and
the 14 step kinds become grouped drop-downs, the conditions fold under a one-line summary, the step list sits beside
its fields, and keys add, remove and move; the cell event window gets the same parts; no file format change.

Drafted while the developer was away: the roadmap's "start by asking him which part hurts most" became a default
(decision 726), and every other choice he would make is listed under "Defaults to confirm". Approved on 2026-11-15
under the developer's delegation, with the devil's advocate's review applied: underscores kept in menu entries and the
summary (a text template, not an access-key header); the warp row fitted to the narrower fields column, the windows
given minimum sizes; no binding to a property the cell window lacks; plain buttons opening a context menu instead of a
`Menu`; the layout before the fold (EW4, EW5 swapped); one method building the condition words for `Label` and the
summary; and EW0, which lets the implementer open both event windows and read binding errors without the developer.

- Numbers: decisions 726-736. No ADR (see "ADRs needed").
- Editor only (`Events/`, one new style file in `Shell/`, `Hosting/App` for the Debug binding trace and the style
  file's merge, the F1 list) and the smoke script. No Core, Data, content or save change.

## Goal
The event dialogs (object and cell) stop stacking everything in one column: one Preset drop-down instead of nine
buttons and a quest box, one Add step drop-down instead of fourteen buttons, the steps listed beside the chosen step's
fields with room for both, the conditions folded under a line that says them, and the list buttons reachable from the
keyboard. Every slice leaves both windows working, and the implementer can check each one without the developer.

## Order, and why
0. **EW0, a way to check the windows.** They open from a map click, so the smoke script can't reach them, and a
   binding error never shows outside Visual Studio. EW0 traces binding errors to the editor's error output in Debug
   and gives the script an `-EventWindows` mode; every later slice runs it.
1. **EW1, one event view for both windows.** The chosen event's XAML is written twice today (`ObjectEventsWindow`,
   `CellEventWindow`); sharing it first means every later slice edits it once, and the cell window gets each part for
   free.
2. **EW2, Add step.** The biggest clutter: fourteen buttons wrapping to three rows under the fields. Brings the
   drop-down (`DropDownChoice`, `DropDownMenu`, the `DropDownEntry` style), which EW3 reuses.
3. **EW3, Preset.** Nine buttons and a quest box wrapping to four rows over the event list; reuses EW2's drop-down.
4. **EW4, steps beside their fields.** Needs only EW2's single row of step buttons; with the buttons gone, the list and
   the fields can share the height.
5. **EW5, conditions folded.** On the final layout, so whether the fold should start closed is judged where it lives.
6. **EW6, keys.** Last, on the final layout's two lists.

## Guiding rules
- **No format change:** the same `MapEvent` records come out of `EventViewModel.ToEvent()` as before; the existing tests
  of `ToEvent`, `ToEdit`, `Label` and `Problem` pass unchanged (only their calls to the removed `Add...Command`s and
  the quest giver's `PresetQuestId` change, EW2 and EW3).
- **Every slice leaves both windows usable and is checked by the implementer:** each slice runs
  `open-editor-windows.ps1 -EventWindows` (EW0): both windows open, and the trace at the end has no binding-error line
  (`System.Windows.Data Error`). The look is left to the developer's own run.
- **Ids keep their underscores:** a menu entry's or the summary's text is drawn by a `TextBlock`, never as a header
  that reads `_` as an access key; each hand check names an id with an underscore.
- **No binding to a property one window lacks:** `EventView` binds the window's lists through `AncestorType=Window`;
  a list only the object window has is bound only while the event is an object's (EW1), since a collapsed row still
  evaluates its bindings.
- **Sizes:** Editor slices about 200 lines, tests counted at about 10 lines each; layout-only slices (EW4, EW6's view
  code) have no unit test and the script's captures instead.
- **Files near 300** (checked 2026-11-15): `Shell/EditorTemplates.xaml` 297 (untouched: the new style goes in its own
  file, as `EditorTreeTemplates.xaml` did); `Shell/MainWindow.xaml` 294 (untouched);
  `tests/.../Events/EventEditingViewModelTests.cs` 286 (two calls edited in place, EW2);
  `tests/.../Events/EventListViewModelTests.cs` 258 (calls edited in place, no new test: new tests go in siblings);
  `Events/EventStepViewModel.cs` 260 (untouched, decision 735).
- **Test files, globbed on 2026-11-15:** none of `EventViewModelTests`, `EventViewModelAddStepTests`,
  `EventViewModelConditionSummaryTests`, `EventListViewModelPresetTests` exists; each slice names the one it creates.
  `CellEventViewModelTests` (92) is extended by EW1 and EW2.

## Out of scope
- New step kinds, new conditions, or any change to what an event can do.
- A live problem line or a greyed OK (the problem still shows when OK is pressed, as today); backlog line below.
- Numbering the steps in the list, duplicating a step or an event, copying steps between events: backlog.
- A right-click Add step menu on the step list (decision 730).
- The Events tab's help text in `MainWindow.xaml` (294 lines; it still reads right).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: checking, the shared view and the drop-downs
- [x] **EW0. Binding errors on the editor's error output in Debug; `open-editor-windows.ps1 -EventWindows` opens both
  event windows by map clicks** (Editor + script, about 70 lines) → [slices/EW0.md](slices/EW0.md)
- [x] **EW1. One event view for both windows: the chosen event's conditions, steps and fields in `EventView`; the cell
  window offers Give item** (Editor, about 210 lines, most of them moved) → [slices/EW1.md](slices/EW1.md)
- [x] **EW2. One Add step drop-down, grouped under five titles; Swap object greyed on a cell** (Editor, about 220
  lines) → [slices/EW2.md](slices/EW2.md)
- [ ] **EW3. One Preset drop-down, a quest giver entry per quest; the quest box goes** (Editor, about 160 lines) →
  [slices/EW3.md](slices/EW3.md)

### Phase 2: room and keys
- [ ] **EW4. The step list beside the chosen step's fields; bigger windows with minimum sizes; the warp's buttons on
  their own line** (Editor, about 120 lines, XAML only) → [slices/EW4.md](slices/EW4.md)
- [ ] **EW5. The conditions folded under a one-line summary** (Editor, about 110 lines) →
  [slices/EW5.md](slices/EW5.md)
- [ ] **EW6. Keys for the lists: Insert, Delete, Alt+Up, Alt+Down** (Editor + script, about 110 lines, view code) →
  [slices/EW6.md](slices/EW6.md)

## Decisions
- **726.** (Default taken while the developer was away; his to overturn.) The plan does all five parts the roadmap
  named, in the order above; the roadmap's "ask him which hurts most first" was not asked. After EW0's checking aid,
  the order starts with the shared view (it halves every later XAML edit), then the biggest clutter.
- **727.** Both windows show the chosen event through one view, `Events/EventView`; the rows only an object's event
  has (the prompt, "only while the object is") show only there, read from `EventViewModel.IsObjectEvent`, as today.
- **728.** (Default taken while the developer was away; his to overturn.) A cell's event offers every step kind the
  game takes on a cell: Give item joins (the game accepts it; the cell window simply lacked its button), and Swap object
  shows greyed in Add step, F1 saying it only works in an object's event (`MapEventValidator` refuses it on a cell).
- **729.** One Add step drop-down with five groups under greyed titles in capitals, as the roadmap's design 3.2 grouped
  them: TALK (Dialogue, Voice, Cinematic), WORLD (Give item, Swap object, Warp, Move NPC), PARTY (Recruit, Rest),
  SCREENS (Open Forge, Open shop), QUESTS (Start quest, Advance quest, Complete quest). A new step still goes after
  the chosen one.
- **730.** A drop-down is a plain button ("Add step ▾", "Preset ▾") whose click opens its `ContextMenu` under it
  (`Events/DropDownMenu.Open`); the menu's entries are a view-model list of `DropDownChoice`s, a title being an entry
  with no command (greyed, skipped by the keyboard); a test checks that every `EventStepKind` has exactly one Add step
  entry, so a later step kind can't be left out. The step list gets no right-click Add step: a right click doesn't
  choose the row, so "after the chosen step" would add after another one.
- **731.** (Default taken while the developer was away; his to overturn.) One Preset drop-down: OBJECTS (Chest, Door,
  Door you open, Sign), PEOPLE (Talk, Forge master, Shopkeeper), QUEST GIVER (one entry per quest, "Quest giver:
  placeholder_errand", in id order; one greyed "Quest giver (no quest yet)" when the draft has none). The quest box
  beside the presets goes. A preset still replaces every event at once, and Cancel still brings them back. A list
  taller than the screen scrolls (the context menu's arrows).
- **732.** (Default taken while the developer was away; his to overturn.) The conditions fold under a one-line
  summary: the event list's words without the step count, then ", once" (`Press "Open" if door_close when errand is
  active, once`); folded each time a window opens; opened or folded, it stays so while another event is chosen.
- **733.** (Default taken while the developer was away; his to overturn.) The step list sits left of the chosen step's
  fields; the object window opens at 1040 x 620 (was 820 x 560) and the cell window at 760 x 560 (was 560 x 520), each
  its minimum size; the dialogue's lines box grows from 90 to 160 px; a warp step's Pick... and Go to move to a line of
  their own under the map and cell boxes, so the row fits the fields column.
- **734.** (Default taken while the developer was away; his to overturn.) Keys: in the step list, Insert opens Add step,
  Delete removes the chosen step, Alt+Up and Alt+Down move it; in the event list, Insert adds an event, Delete removes
  the chosen one, Alt+Up and Alt+Down move it. The buttons stay, their tooltips naming the keys; F1 lists them.
- **735.** `Events/EventStepViewModel.cs` (260 lines) stays whole: no slice grows it, so the roadmap's "moves `Label`
  and `Problem` out first" is dropped (a file under 300 isn't split on a forecast).
- **736.** A Debug editor writes WPF's binding errors (warning level and up) to its error output, where the smoke
  script's trace shows them; `open-editor-windows.ps1 -EventWindows` opens the Events tab, clicks the map until a
  cell's and an object's event window have each opened, saves a capture of each and closes it as Cancel (nothing
  applied). Release builds trace nothing.

## Defaults to confirm
- **726**, the order, and doing all five parts.
- **728**, Give item offered on a cell; Swap object greyed there, not hidden, with no tooltip (F1 says why).
- **731**, one Quest giver entry per quest in the Preset drop-down, the quest box gone.
- **732**, the summary's words, and the fold starting closed: judge it on the side-by-side layout (EW4), which lands
  before it.
- **733**, the window sizes and minimums, the lines box height, the warp's buttons on their own line.
- **734**, the keys.

## Open design questions (developer decides)
- None open: the choices he would make are the defaults above.

## ADRs needed
- None. No format, save or undo rule changes; the one real alternative (the drop-downs' entries written in XAML or
  listed by the view model) is an implementation choice, decided in design.md ("Key decisions") as decision 730.

## Backlog lines this plan adds
- Event dialogs: a live problem line and an OK greyed through `CanApply` (`.claude/rules/editor.md`), as the Database
  windows have.
- Event dialogs: numbered steps ("2. Dialogue: ...") to match the problem line's "step 2"; duplicate a step or an event.
- The theme's `Expander` keeps WPF's default arrow (off-theme in the dark look; `TileBrowserView` and EW5's fold).
