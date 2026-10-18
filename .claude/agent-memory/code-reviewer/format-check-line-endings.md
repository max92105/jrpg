---
name: format-check-line-endings
description: Always run dotnet format --verify-no-changes yourself without a pipe; new files written LF-only fail ENDOFLINE even when the developer reports format clean
metadata:
  type: feedback
---

Run `dotnet format Jrpg.slnx --verify-no-changes` on its own and check `$?` (piping into `tail` hides exit code 2). New files are sometimes written LF-only and fail ENDOFLINE (slice 3c: EventConversion.cs + its tests) although the hand-off said format was clean.

**Why:** a failing format check is a Critical under the definition of done; trusting the hand-off missed nothing only because the output was read.
**How to apply:** every review, run format separately; `file <new files>` shows missing CRLF quickly.
