---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11j-1 (Event presets)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 819 tests pass, format exit 0)

- open (Critical): tests/Jrpg.Editor.Tests/Events/EventPresetsTests.cs:28 Door_TouchWarpOnlyWhenOpen tests DoorYouOpen (misleading next to Door_WalkedInto_Warps); rename DoorYouOpen_...
- open (Warning): selection after a preset only pinned when the first event has the problem; DoorYouOpen (problem in event 2, warp) untested at VM level
- open (Suggestion): fallback guess = object itself makes chest swap to itself / DoorYouOpen warp while closed, no Problem flags it (legacy same)
- open (Suggestion): PresetTarget also serves AddEvent; name is preset-specific
- open (Suggestion): GuessOpenedObject_ChestFindsChestOpened duplicates the theory's ("chest","chest_opened") row
- verified: ids skip both current and dialog-open event ids; legacy prompts/IsOnce/OnlyWhen match; Dialogue empty-lines Problem notified via LinesText; validator refuses empty ids on save
