---
name: stale-size-comments
description: Sizing or drawing-moves leave comments/summaries in OTHER spots stating the old size or place (SZ1, SZ2, M4 InventoryPage summary)
metadata:
  type: feedback
---

Values-only sizing passes update the comments next to the changed constants but miss comments in neighbouring types
that quote the same size (SZ1: UiTheme row/inset, SquareShrink icon sizes; SZ2: FloatingNumber.StackSpacing "40 px").
Same when a drawing moves: M4 moved the description from under the list to under the selected row, but
InventoryPage's class summary still said "description under the list".

**Why:** a comment stating a wrong size or place is a misleading comment, which the review treats as Critical.

**How to apply:** for each changed constant's old value, grep `"<old> px"` and `<old>` in comments across the feature
folder and `Ui/`, not only the edited lines; when something is drawn somewhere new, grep the old place's wording
("under the list") and read the class summaries of the touched types. Related: [[tautological-tests]].
