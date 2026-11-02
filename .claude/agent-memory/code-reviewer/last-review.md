---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice S4 (sounds on the Inventory, Equipment, Party, Status and Quests pages). Round 1.
Verdict: Fix the Criticals, then commit. 2 Critical, 2 Warnings, 3 Suggestions.

- Open (Critical): PartyPage.cs:192, 245, 257 confirm asks have no asserting test, nor the silent refusals at 205 and 253
- Open (Critical): StatusAndQuestsPageSoundTests names two pages; the slice asks one file per page; split into StatusPageSoundTests and QuestsPageSoundTests
- Open (Warning): StatusPage and QuestsPage `_sounds.Clear()` untested; removed, a move's tick replays every frame and all tests pass
- Open (Warning): InventoryPage's `Category` in SelectionNow untested; removed, rail and tab moves at row 0 go silent and all tests pass
- Open (Suggestion): IMenuPage.Sounds default now has no page in src, only the CountingMenuPage test double
- Open (Suggestion): EquipmentPage.UpdateChoice `?.SelectedRow ?? 0` guards a closed choice Update already ruled out
- Open (Suggestion): InventoryPageSoundTests rely on the list opening on the potion; move to it by id as the bomb test does
- Note: hand check is the developer's; asked to decide Confirm on the rail (a move tick) and Remove all with nothing worn (a confirm sound)
- Fixed (from S3): ExplorationScene Sounds no longer sits under Update's summary
- Fixed (from S3): SavePage confirm on a written slot has a test; LoadPage.Sounds forward has a test
- Dropped (from S3): SystemPageTests fixed presses, now in docs/plans/backlog.md
