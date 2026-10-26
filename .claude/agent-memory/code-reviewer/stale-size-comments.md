---
name: stale-size-comments
description: Sizing/values-only slices leave comments in OTHER files stating the old pixel sizes (SZ1 UiTheme/SquareShrink, SZ2 FloatingNumber.cs)
metadata:
  type: feedback
---

Values-only sizing passes update the comments next to the changed constants but miss comments in neighbouring types
that quote the same size (SZ1: UiTheme row/inset, SquareShrink icon sizes; SZ2: FloatingNumber.StackSpacing "40 px").

**Why:** a comment stating a wrong size is a misleading comment, which the review treats as Critical.

**How to apply:** for each changed constant's old value, grep `"<old> px"` and `<old>` in comments across the feature
folder and `Ui/`, not only the edited files. Related: [[tautological-tests]].
