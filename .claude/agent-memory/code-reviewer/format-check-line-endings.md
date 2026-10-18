---
name: format-check-line-endings
description: Always run dotnet format --verify-no-changes yourself without a pipe and read its exit code; hand-offs claiming "format clean" were wrong twice (3c, 11e)
metadata:
  type: feedback
---

Run `dotnet format Jrpg.slnx --verify-no-changes` on its own and check `$?` (piping into `tail` hides exit code 2). In slice 3c new LF-only files failed ENDOFLINE although the hand-off said format was clean. In slice 11e (shell-cutover plan) a single LF line inside an otherwise CRLF file (MainViewModel.cs, git warned "LF will be replaced by CRLF") failed ENDOFLINE, again despite a "format clean" hand-off. In slice 3a 14 new LF-only files passed (exit 0), so LF-only is not by itself a failure: trust the unpiped exit code. A `git diff` warning "LF will be replaced by CRLF" on a modified file is a good hint of a mixed-ending line.

**Why:** a failing format check is a Critical under the definition of done; trusting the hand-off or guessing from line endings both mislead.
**How to apply:** every review, run format separately and report its exit code.
