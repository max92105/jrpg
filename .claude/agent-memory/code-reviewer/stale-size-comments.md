---
name: stale-size-comments
description: Sizing, drawing moves, behavior removals or renamed files leave comments/docs stating the old size, place, rule or file name (SZ1, SZ2, M4, M2c, F1)
metadata:
  type: feedback
---

Values-only sizing passes update the comments next to the changed constants but miss comments in neighbouring types
that quote the same size (SZ1: UiTheme row/inset, SquareShrink icon sizes; SZ2: FloatingNumber.StackSpacing "40 px").
Same when a drawing moves: M4 moved the description from under the list to under the selected row, but
InventoryPage's class summary still said "description under the list". Same when a call becomes conditional: M2c made
the sidebar refresh only on pages that show it, but PartySidebar's rewritten summary kept "refreshed every update a page
is open". Same for files: F1 replaced docs/ui-reference/menu-status.webp with .png, leaving the ui-reference README,
export-index and pending slices (Z, the stats plan's L1) pointing at the deleted file.

**Why:** a comment or doc stating a wrong size, place, rule or file misleads the next reader.

**How to apply:** for each changed constant's old value, grep `"<old> px"` and `<old>` in comments across the feature
folder and `Ui/`, not only the edited lines; when something is drawn somewhere new, grep the old place's wording
("under the list"); when a call site gains a condition, reread the callee's summary for "every"/"always"; when a file
is deleted or renamed (`git status` D/??), grep its old name across docs/. Related: [[tautological-tests]].
