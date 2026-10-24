---
name: lf-line-endings
description: Edited content/docs files (ui.json, map.md, new-game.json) end up LF instead of CRLF; verify does not catch it
metadata:
  type: feedback
---

Edited JSON/markdown files are often left with LF line endings (git prints "LF will be replaced by CRLF"). Seen in slices C2 and S2 of the new-game-flow plan.

**Why:** the developer's tools (sed edits) drop CR; verify.ps1 and dotnet format don't check non-.cs files.

**How to apply:** run `file <path>` on changed non-.cs files; if LF, raise a Suggestion to run `sed -i 's/\r*$/\r/'`. Never a Warning.
