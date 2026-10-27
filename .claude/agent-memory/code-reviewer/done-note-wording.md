---
name: done-note-wording
description: Slice Done notes open with "Done, with deviations: as planned." — self-contradicting lead (seen C2 and PD1, plan escape-menu-third-pass)
metadata:
  type: feedback
---

Slice Done notes in `docs/plans/**/slices/*.md` keep opening with "Done, with deviations: as planned." and then list the deviations (or none).

**Why:** the lead contradicts itself; a reader can't tell at a glance whether the slice deviated. Seen in C2 and PD1.

**How to apply:** one Suggestion line per review: "Done: as planned." when nothing deviated, "Done, with deviations:" followed by the list otherwise. Never more than a Suggestion. Related: [[stale-size-comments]].
