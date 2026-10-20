---
name: plan-checkbox-unticked
description: Slice hand-offs often leave the plan README checkbox unticked, deviations unrecorded, and new shared Ui types missing from docs/map.md (seen C2, M1, U1, U2)
metadata:
  type: feedback
---

Hand-offs for review regularly leave the slice's `- [ ]` in the plan README unticked and mention deviations only in the hand-off message, not in the slice file. New shared types (SectionTitle in U1, MeasuredText in U2) also tend to be left out of the docs/map.md key-types row.

**Why:** DoD step 5 requires the plan file updated with deviations noted; the next reviewer/slice reads the file, not the hand-off. map.md is how later slices find shared parts.

**How to apply:** Grep the README for the slice id and docs/map.md for each new type; raise as Suggestions (not Critical) since the developer commits it.
