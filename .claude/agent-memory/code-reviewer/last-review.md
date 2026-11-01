---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-quests, slice QV (quest condition validated in content; no shadowing object event). Round 1.
Verdict: Ready to commit.

- Suggestion, open: MapEventValidator.CamelCase<T> is a noun-ish name; call it JsonNameOf (siblings use NameOf)
- Suggestion, open: FindShadowedEvent runs the Skip/FirstOrDefault search before CanAlwaysStart; guard first with `continue`
- Follow-up: enum-to-camelCase name now written in five places (ResistancesConverter, StatBonusesConverter, Crafting/Loot validators, MapEventValidator); one shared Data helper type
- Fixed (from QC): QuestCondition AtStage uses NamedStage(); IsMetBy_Completed_MetOnlyOnceCompleted renamed; Running() gone
- Follow-up (carried): Trade/ShopScene 296 lines
