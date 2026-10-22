---
name: wording-slices-wrap
description: Comment-rewording slices leave lines past the 120-char wrap or short orphan lines; and dropping a source name can make two summaries indistinguishable
metadata:
  type: feedback
---

When a slice only rewords comments (demo retirement W1, W2), the cut leaves doc lines over the file's 120-char wrap or short first lines, and removing the old source name (e.g. `Brush.Highlight` vs `Brush.Highlight.Selected`) can leave two sibling summaries saying the same thing.

**Why:** format check does not enforce comment wrap; the old name was the only thing telling siblings apart. Seen in W1 (wrap) and W2 (wrap + UiTheme Highlight/SelectedRow).

**How to apply:** in wording diffs, awk line lengths of changed comment lines, and compare sibling summaries side by side; grep each value's users to propose the distinguishing role.
