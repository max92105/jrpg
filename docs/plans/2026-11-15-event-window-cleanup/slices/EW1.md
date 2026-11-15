# EW1. One event view for both windows

Editor, about 210 lines, most of them moved XAML (about 40 new: the property, its uses, the object list's style, the
tests). Design: section 4, "What moves where" and key decisions 1 and 2. Decisions 727, 728. Needs EW0 (the check).

## Goal
The chosen event's part of the dialogs (its id, conditions, steps, the chosen step's fields and the step buttons) is
written once, in `EventView`, and both windows hold it. The layout stays the object window's stacked one; the cell
window now shows its buttons as the object window does, and gains the Give item button (decision 728). Nothing else
looks different.

## New types
- `Events/EventView` (`UserControl`, `EventView.xaml` + `EventView.xaml.cs` with only `InitializeComponent`): shows the
  chosen event of a dialog: its conditions, its steps and the chosen step's fields. Its `DataContext` is an
  `EventViewModel`.

## Changes
- `Events/EventView.xaml` (new, about 110 lines): the object window's right `DockPanel` (lines 45-126 today) moved
  whole: the id line, the five condition rows and the once box, "Steps, run in order", the Add buttons, Remove, Up,
  Down, the `EventStepEditor` and the step list. The window lists stay read through `RelativeSource
  AncestorType=Window` (key decision 1), except the object list:
  - the prompt row and the "Only while the object is" row: `Visibility="{Binding IsObjectEvent, Converter={StaticResource
    VisibleWhenTrue}}"` (a `BooleanToVisibilityConverter` in the control's resources, as `EventStepEditor` declares
    its own). The Swap object button reads it the same way.
  - the "Only while the object is" box: no `ItemsSource` attribute; a style in the control's resources, `BasedOn` the
    theme's `ComboBox` style, with a `DataTrigger` on `IsObjectEvent` = `True` setting `ItemsSource` to
    `DataContext.OnlyWhenChoices` of the window. `CellEventViewModel` has no `OnlyWhenChoices`, and a collapsed row
    still evaluates a plain binding, which would log a binding error in every cell window. Its comment says so.
  - the activation box's tooltip, shared: "Touch: walks into it. Arrive: arrives on the map, such as an intro." (the
    cell window's own tooltip, reworded to fit an object too).
- `Events/ObjectEventsWindow.xaml`: the right `DockPanel` becomes
  `<local:EventView Margin="12,0,0,0" DataContext="{Binding SelectedEvent}" />`.
- `Events/CellEventWindow.xaml`: its event `DockPanel` becomes `<local:EventView DataContext="{Binding Event}" />`.
- `Events/EventViewModel.cs`: `public bool IsObjectEvent { get; init; } = true;` with a summary: false for a cell's
  event, which has no prompt, no object to wait for, and can't swap an object (the game refuses it).
- `Events/CellEventViewModel.cs`: builds its event with `IsObjectEvent = false`; its class summary's "It offers
  dialogue, warp and cinematic steps (the game refuses swapping an object from a cell)" becomes "It offers every step
  but Swap object, which the game refuses on a cell (decision 728)".
- `docs/map.md`, Events row: add `EventView` (the chosen event's conditions, steps and fields, shared by the object
  and cell dialogs; the object-only rows and list read `EventViewModel.IsObjectEvent`).

## Tests
- `CellEventViewModelTests` (92 lines, about 100 after):
  - `Constructor_CellWithEvent_EventIsNotAnObjectEvent` (built over the file's `_exit`).
- `EventViewModelTests` (new, `tests/Jrpg.Editor.Tests/Events/`; the placed object and `StepChoices` built in the
  test):
  - `IsObjectEvent_EventsOfTheObjectDialog_AreTrue` (the object's own event, and one added by `AddEventCommand`).
  - `IsObjectEvent_PresetEvents_AreTrue` (after `ApplyDoorYouOpenPresetCommand`: both its events).
- Every existing Events test passes unchanged.

## Verify by hand
1. The implementer: `dotnet build src/Jrpg.Editor`, then `open-editor-windows.ps1 -EventWindows`: both windows open;
   the trace has no `System.Windows.Data Error` line beyond EW0's baseline (none naming `OnlyWhenChoices` in
   particular); the cell capture shows Give item and no Swap object, no prompt or object row.
2. The developer, in his own run (F5): click the **Events** tab, then a chest on the map; choose its event: the prompt
   and "Only while the object is" rows show, and the object box lists ids with underscores whole (`chest_open`).
   Click **Give item**, pick an item, **OK**; reopen: the step is there.
3. Click an empty cell: the buttons row shows **Give item** and no **Swap object**. Click **Warp**, **Pick...**, pick a
   cell, **OK** twice: the cyan square appears. Reopen, choose the warp step, **Go to**: the editor shows the
   destination map (the routed event still reaches the window).

- **Done:** as planned (`EventView.xaml` 96 lines). Run with `-EventWindows -Map milestone`: both windows open, no
  binding-error line; a wrong binding added on purpose did show in the trace (the check works), then was removed. The
  cell capture shows Give item, no Swap object, no prompt or object row. Steps 2 and 3 are the developer's.
