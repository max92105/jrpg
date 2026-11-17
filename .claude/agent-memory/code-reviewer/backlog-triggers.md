---
name: backlog-triggers
description: grep docs/plans/backlog.md for each touched file's name; its lines hold "do X when the next change..." triggers
metadata:
  type: feedback
---

Before reviewing, grep `docs/plans/backlog.md` for the names of the files and test classes the diff touches. Backlog
lines carry conditional triggers ("share them when a third victory test file comes", "the next change that grows it
moves the cards out first"); LV3 hit two at once and neither the slice nor its Done note mentioned them.

**Why:** the third-copy rule and file-size splits are easiest to miss when the trigger lives in the backlog, not the code.

**How to apply:** a met trigger on a copy is a Warning (Growth rule); a met size trigger under 300 lines is a Follow-up.
