# EW3. One Preset drop-down

Editor, about 160 lines (about 70 of them tests). Design: section 3 (the Preset sketch), section 4, key decisions 3
and 4. Decision 731. Needs EW2 (`DropDownChoice`, `DropDownMenu`, `DropDownEntry`).

## Goal
The object window's eight preset buttons and the Quest giver's quest box become one **Preset ▾** button on the
"Events" heading row; its menu: OBJECTS (Chest, Door, Door you open, Sign), PEOPLE (Talk, Forge master, Shopkeeper),
QUEST GIVER (one entry per quest, "Quest giver: placeholder_errand", in id order; one greyed "Quest giver (no quest
yet)" when the draft has none). A chosen preset replaces the events as the buttons did; Cancel still brings the old
ones back. A menu taller than the screen scrolls with the context menu's arrows.

## New types
None.

## Changes
- `Events/EventListViewModel.cs` (148 lines, about 160 after):
  - `PresetQuestId` and `HasPresetQuest` go; their summary's decision 588 line moves to the quest giver's command.
  - `ApplyQuestGiverPreset(string? questId)` takes the quest as the command's parameter:
    `[RelayCommand(CanExecute = nameof(IsQuestId))]`, `private static bool IsQuestId(string? questId) =>
    !string.IsNullOrEmpty(questId);`.
  - `public IReadOnlyList<DropDownChoice> PresetChoices { get; }`, set in the constructor by a private
    `PresetMenu(StepChoices choices)`: the titles OBJECTS, PEOPLE, QUEST GIVER, the seven fixed presets with their
    existing commands (no parameter), then one `new($"Quest giver: {id}", ApplyQuestGiverPresetCommand, id)` per
    `choices.QuestIds`, or the greyed `new("Quest giver (no quest yet)", ApplyQuestGiverPresetCommand, "")`. The
    constructor's `choices` param doc names the presets' quests.
- `Events/ObjectEventsWindow.xaml` and `.xaml.cs`: the preset `WrapPanel` goes; the "Events" heading becomes a row with
  the heading left and, right, `<Button x:Name="PresetButton" Content="Preset ▾" Click="OnPresetClicked"
  ToolTip="Replaces the object's events with a usual set; the first field left to fill is shown.">` with a
  `<Button.ContextMenu>` whose `ItemsSource` is `PresetChoices` and `ItemContainerStyle` `DropDownEntry`;
  `OnPresetClicked` calls `DropDownMenu.Open(PresetButton)`. The comment "A preset replaces all the events; the first
  field it leaves empty is shown." stays above it.
- `Shell/ControlsHelpWindow.xaml`, Events: "Presets: Chest, Door, Door you open, Sign, and Talk (an NPC: type its
  speaker and lines)." becomes "Preset ▾ (over the event list): replaces the object's events with a usual set: Objects
  (Chest, Door, Door you open, Sign), People (Talk, Forge master, Shopkeeper), or a Quest giver for one quest; the
  first field left to fill is shown, and Cancel brings the old events back."
- `docs/map.md`, Events row: `EventPresets` is offered by the Preset drop-down (`EventListViewModel.PresetChoices`,
  one quest giver entry per quest).

## Tests
- `EventListViewModelPresetTests` (new, `tests/Jrpg.Editor.Tests/Events/`; the placed object and `StepChoices` with
  two quests, `heist` and `placeholder_errand`, built in the test):
  - `PresetChoices_TwoQuests_ListsTheFixedPresetsThenOneQuestGiverPerQuest` (the whole `(Label, IsTitle)` sequence,
    `Quest giver: placeholder_errand` with its underscore).
  - `PresetChoices_NoQuest_QuestGiverEntryCannotRun` (its one entry's command can't execute with its parameter).
  - `PresetChoices_SecondQuestGiverEntry_AppliesThePresetForItsQuest` (`placeholder_errand`, the second: an entry
    wired to the first quest fails).
  - `PresetChoices_ChestEntry_RunsTheChestPreset` (one event, its steps give an item then swap the object).
  - `PresetChoices_ShopkeeperEntry_RunsTheShopkeeperPreset` (the last of its group: an entry shifted by one fails).
- `EventListViewModelQuestGiverTests` (70 lines, about 50 after), rewritten for the parameter; its header comment says
  the quest comes from the chosen entry:
  - `ApplyQuestGiverPreset_ChosenQuest_AddsBothEvents` now runs `Execute("placeholder_errand")`, asserts unchanged.
  - `ApplyQuestGiverPreset_NoQuest_IsGreyed` becomes `ApplyQuestGiverPresetCommand_EmptyQuest_CannotExecute`.
  - Removed with the property they tested (a removed behavior, not a weakened assert):
    `PresetQuestId_DraftWithQuests_IsTheFirstById` (its "first quest" rule is gone: every quest has its entry, tested
    above) and `PresetQuestId_Emptied_GreysTheButton`.
- The seven fixed preset commands' tests (`EventListViewModelTests`, `EventListViewModelShopTests`) pass unchanged.

## Verify by hand
1. The implementer: `open-editor-windows.ps1 -EventWindows`: the object window's capture shows **Events** and **Preset
   ▾** on one row and no preset buttons; no binding-error line beyond EW0's baseline (none naming `PresetChoices`).
2. The developer (F5): **Events** tab, click an NPC. Click **Preset ▾**: **OBJECTS**, **PEOPLE**, **QUEST GIVER**
   greyed over their entries, one "Quest giver: ..." per quest of Database > Quests, `placeholder_errand` shown with
   its underscore.
3. Click **Quest giver: placeholder_errand**: the list holds the offer and the reminder; **OK** says "Write at least
   one line." for the first dialogue. **Cancel**, reopen: the old events are back.
4. **Preset ▾** > **Shopkeeper**: one event, its shop step to fill. Fill it, **OK**, then **Ctrl+Z**: the old events
   are back.
5. The greyed "Quest giver (no quest yet)" entry is left to `PresetChoices_NoQuest_QuestGiverEntryCannotRun`: the
   repository's draft has quests.
6. **F1**: the Preset line.

- **Done, with deviations:** as planned, plus `ApplyQuestGiverPresetCommand_AQuest_CanExecute` (the rule's other
  side). The smoke script opened both event windows with no binding-error line; its capture isn't opened by an agent
  (it can show the map's sheets at its edges), so the heading row's look is the developer's hand check with steps 2
  to 6.
