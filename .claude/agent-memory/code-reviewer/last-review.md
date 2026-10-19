---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M4a (Typewriter and page marker; M4 split into M4a/M4b on 2026-10-19)
Round: 1 (verdict: Ready to commit; build 0 warnings, format exit 0; full run: Game host "error: 1" flake with 876 total, Game-only rerun 108/108 clean)

- open (Suggestion): EventPresenter.ShowPages runs _reading.Update(delta) before Confirm, so a press on the frame the last character lands turns the page instead of completing it; handle Confirm first
- open (Suggestion): DialogueBox.DrawTypedText slices by TextLine.Count (glyphs/codepoints, probed) as a char count; a non-BMP char on a line cuts that line's last char
- open (Suggestion): EventPresenter static _dialogueCharacterTime sits between instance readonly fields; move above them
- verified (fsi probe of FontStashSharp 1.6.2): TextLine.TextStartIndex is a char index; wrap-space is inside Count; '\n' counted in Count and harmless to DrawText
- verified: found-item zero-time path covered by Update_ConfirmOnFoundItem_ContinuesEvent (one Confirm, no extra time)
