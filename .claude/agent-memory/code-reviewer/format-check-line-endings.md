---
name: format-check-line-endings
description: Always run dotnet format --verify-no-changes yourself without a pipe and read its exit code; don't infer pass/fail from LF-only files alone
metadata:
  type: feedback
---

Run `dotnet format Jrpg.slnx --verify-no-changes` on its own and check `$?` (piping into `tail` hides exit code 2). In slice 3c new LF-only files failed ENDOFLINE although the hand-off said format was clean. In slice 3a (shell plan) 14 new LF-only files still passed (exit 0), so LF-only is not by itself a failure: trust the unpiped exit code.

**Why:** a failing format check is a Critical under the definition of done; trusting the hand-off or guessing from line endings both mislead.
**How to apply:** every review, run format separately and report its exit code.
