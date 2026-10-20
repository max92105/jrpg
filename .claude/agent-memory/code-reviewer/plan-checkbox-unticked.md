---
name: plan-checkbox-unticked
description: Slice hand-offs often leave the plan README checkbox unticked and deviations unrecorded in the slice file (seen C2, M1 of menu-cleanup)
metadata:
  type: feedback
---

Hand-offs for review regularly leave the slice's `- [ ]` in the plan README unticked and mention deviations only in the hand-off message, not in the slice file.

**Why:** DoD step 5 requires the plan file updated with deviations noted; the next reviewer/slice reads the file, not the hand-off.

**How to apply:** Grep the README for the slice id and compare the slice's "Changes" bullets against the diff; raise as a Suggestion (not Critical) since the developer commits it.
