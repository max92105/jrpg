---
name: plan-checkbox-unticked
description: Slice hand-offs often leave the plan README checkbox unticked, deviations unrecorded, and new shared Ui types missing from docs/map.md (seen C2, M1, U1-U4, E1, E2, P1, P2)
metadata:
  type: feedback
---

Hand-offs for review regularly leave the slice's `- [ ]` in the plan README unticked and mention deviations only in the hand-off message, not in the slice file. New shared types (SectionTitle in U1, MeasuredText in U2, VitalMeter/MemberVitalBars in U3, MemberCardView in U4, EquipmentSheet/StatPreviewText in E1, EquipmentSheetView in E2 (no-Margin decision only in hand-off), PartyCursor in P1, PartyColumnsView/PartyActionChoiceView in P2; also the "Slots static property" decision only in the hand-off) also tend to be left out of the docs/map.md key-types row.

**Why:** DoD step 5 requires the plan file updated with deviations noted; the next reviewer/slice reads the file, not the hand-off. map.md is how later slices find shared parts.

**How to apply:** Grep the README for the slice id and docs/map.md for each new type; raise as Suggestions (not Critical) since the developer commits it.
