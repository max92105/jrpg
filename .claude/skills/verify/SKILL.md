---
name: verify
description: Verify the working tree is commit-ready - build with zero warnings, all tests green, formatting clean. Use before every commit and when asked whether the code is ready.
---

Run `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/verify.ps1` (add `-Fix` to apply `dotnet format` and rewrap the changed comments first). It checks, in order: build (zero warnings), all tests, formatting, Core free of `Microsoft.Xna`/`System.Windows`, no `Manager`/`Helper`/`Utils` type names or kind folders, no retired word outside history, changed comments within 120 columns, and three slips a shell edit leaves (a shell quote written literally, a member with two summaries, a changed file crossing 300 lines). It prints one line per step and only the lines that explain a failure; don't run the underlying `dotnet` commands yourself unless you need more detail on one failure.

Tests named `..._Bug` are known bugs: the script lists them without failing; they don't block if I've acknowledged them.

Report the script's lines as they are. On failure, fix the cause if it's in code you just changed; otherwise ask me.
