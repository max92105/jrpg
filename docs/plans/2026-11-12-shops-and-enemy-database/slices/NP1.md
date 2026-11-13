# NP1. The Quest giver preset

Editor, about 150 lines (80 of them tests). Decision 588. Independent of phase 1.

## Goal
In an object's events dialog, beside the other presets, **Quest giver** with a quest box (the draft's quests, the first chosen): applying it adds two Interact events to the NPC: the first, gated "quest not started", says a line (left to type) and starts the quest; the second, gated "quest active", says a reminder line (left to type). Both gated, so neither shadows the other (ADR 0033's content rule). After the quest is completed the NPC has nothing to say until the developer adds a third event (Completed gate) by hand: the preset's tool tip says so.

## New types
None.

## Changes
- `Editor/Events/EventPresets.QuestGiver(EventIdAllocation target, string questId)`: the two events above; the class summary lists it.
- `Editor/Events/EventListViewModel` (132): `PresetQuestId` (the quest box's value, the first quest by id, empty with none), `QuestIds`, `ApplyQuestGiverPresetCommand` (greyed while no quest exists).
- `Editor/Events/ObjectEventsWindow.xaml` (or the preset row's view): the button, its quest box and the tool tip.

## Tests
- `EventPresetsTests` (146): `QuestGiver_Quest_FirstEventGatedNotStartedStartsIt`; `QuestGiver_Quest_SecondEventGatedActiveTalks`; `QuestGiver_TwoEvents_TakeFreshIds`.
- `EventListViewModelTests`: `ApplyQuestGiverPreset_ChosenQuest_AddsBothEvents`; `ApplyQuestGiverPreset_NoQuest_IsGreyed`.

## Verify by hand
1. Database > Quests...: have a quest (the placeholder errand is enough).
2. Place an NPC, open its events (Events mode, double-click), choose the quest in the Quest giver box, click **Quest giver**: two events appear; type the offer line and the reminder line; OK.
3. Ctrl+S, play: talk to the NPC: the offer line, then "Quest started: ..."; talk again: the reminder.
4. Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** the quest box binds to `Choices.QuestIds` (no separate `QuestIds` member on the list); the list view model tests sit in `EventListViewModelQuestGiverTests` (the existing file is past 250 lines); clearing the box greys the button, tested.
