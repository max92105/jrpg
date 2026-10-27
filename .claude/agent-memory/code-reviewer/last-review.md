---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice E2 (Equipment page look: SheetRowLook, EquipmentChangesView, Changes/Gives caption). Round 1. Verdict: Fix the Criticals.

- Critical: slot labels and comparison line labels drawn through SheetRowLook.DrawLabel are mixed case ("Weapon", "Fire"), not spaced caps as the slice goal and the new summaries claim; uppercase once in EquipmentLabels.From and EquipmentChangesView.Measure — open
- Warning: StatusParametersView.Measure is now a private forwarding wrapper with one caller (line 64); call _rows.Measure directly — open
- Suggestion: caption passed as a message-id string and matched by ==; any other id silently draws "Changes" — open
- Suggestion: MessageIds.EquipmentChanges summary still says "or what a worn item gives" (Gives has its own id now) — open
