---
name: wording-slices-wrap
description: Comment-rewording slices leave lines past the 120-char wrap or one-word orphan tails; dropping a source name can make sibling summaries indistinguishable or leave demo-only words (hover)
metadata:
  type: feedback
---

When a slice only rewords comments (demo retirement W1-W4), the cut leaves doc lines over the file's 120-char wrap or short first lines, and removing the old source name (e.g. `Brush.Highlight` vs `Brush.Highlight.Selected`) can leave two sibling summaries saying the same thing.

**Why:** format check does not enforce comment wrap; the old name was the only thing telling siblings apart. Seen in W1 (wrap), W2 (wrap + UiTheme Highlight/SelectedRow), W3 (a 176-char joined line and ~15 short first lines), W4 (a reflow script fixed >120 but left ~7 one-word tails like "// all."; "when hovered" kept from a mouse-driven demo in a no-mouse game).

**How to apply:** in wording diffs, awk changed comment lines for >120 AND <25 chars (orphan tails); compare sibling summaries side by side; check words describing demo-only behavior (hover, click) against the code; grep each value's users to propose the distinguishing role.
