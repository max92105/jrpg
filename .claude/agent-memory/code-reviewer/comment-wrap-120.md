---
name: comment-wrap-120
description: Recurring - new or edited XML doc/comment lines exceed the 120-column rule in code-quality.md
metadata:
  type: feedback
---

Changed comment lines over 120 columns, missed by verify.ps1 (format does not check it). Seen in slice C1a (PartyEquipment.cs, 198 cols), C1b1 (ClassGrowth.cs summary, 126 cols) and C1c (Vitals.cs RaiseMaxima summary, 121 cols).

**Why:** `.claude/rules/code-quality.md` says to rewrap every changed comment block to 120 (`perl .claude/scripts/reflow-comments.pl <file>`).

**How to apply:** Run `awk 'length>120'` on changed files; awk counts bytes and the trailing CR on CRLF files, so pipe through `tr -d '\r'` and subtract for multi-byte chars (×, −) before flagging. Only comments are under the rule; long code lines are a Suggestion at most.
