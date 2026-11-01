---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice R1 (ExplorationState split: MapEventStarter, pure Core refactor). Round 1. Verdict: Ready to commit.
verify.ps1 all pass (3186 tests, unchanged).

- Suggestion, open: MapEventStarter.Running() is a noun-named method; RunningOrThrow / RequireRun
- Suggestion, open: map and finder now live in both ExplorationState and MapEventStarter, synced by the warp path's EnteredMap call (accepted deviation; QC/G4 must keep the pair)
- QS suggestions (shared _errand fixture, IsFinished asserts): not in this diff, left to the developer
- Follow-up (carried): Trade/ShopScene 296 lines
