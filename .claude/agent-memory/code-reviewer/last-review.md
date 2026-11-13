---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice NP2 (portrait picker: ArtFolders.ImagesIn, StepChoices.PortraitFiles,
EventEditingViewModel.PortraitFilesLookup, EventStepEditor editable ComboBox). 2026-11-13.
Round 1. Verdict: ready to commit. XAML checked: DataContext.Choices.PortraitFiles resolves on both hosts
(ObjectEventsWindow -> EventListViewModel.Choices, CellEventWindow -> CellEventViewModel.Choices); Text binds
EventStepViewModel.PortraitFile; theme ComboBox template has PART_EditableTextBox + IsEditable trigger.

- NP1 Suggestions not carried (committed slice).
- Suggestion (open): StartEditingAt_PortraitLookup_OffersItsFiles doesn't show the lookup is read per dialog
  (a value cached at init passes); open twice with a lookup whose list changes between.
