---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice NP1 (Quest giver preset: EventPresets.QuestGiver,
EventListViewModel.PresetQuestId / ApplyQuestGiverPresetCommand, ObjectEventsWindow button + quest box). 2026-11-13.
Round 1. Verdict: ready to commit. XAML bindings checked: ApplyQuestGiverPresetCommand, Choices.QuestIds (public
StepChoices Choices), PresetQuestId all match. Probed: a ComboBox with SelectedItem "" over an empty list never writes
null back (no NRE in HasPresetQuest).

- SH3 Suggestions not carried (committed slice).
- Suggestion (open): test PresetQuestId_ClearedByTheBox_GreysTheButton names a UI path the read-only box can't take;
  rename PresetQuestId_Emptied_GreysTheButton.
- Suggestion (open): the Quest giver tool tip doesn't say it replaces the object's events like every preset.
- Suggestion (open): EventListViewModel ctor's `choices` param doc omits the quests (gate and now the preset box).
