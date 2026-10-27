---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice G1b (saves keep all nine slots, format 4). Round 1. Verdict: Fix the Criticals, then commit.

- Critical: `"worn": { "weapon": null }` passes strict STJ (dictionary values), SaveGameRepair.RepairWornItem -> _catalog.Items.TryGetValue(null) throws ArgumentNullException outside UnreadableSaveException; extend FileSaveStore.CheckNoNullEntries + test — open
- Warning: SaveJsonOptions DictionaryKeyPolicy also rewrites LastingAmounts' string keys on write only (STJ doesn't map string keys back on read); a resource id starting uppercase would be dropped on load — open
- Suggestion: no test that an unknown slot name (`"thirdRing"`) makes the save unreadable, a rule the slice states — open
