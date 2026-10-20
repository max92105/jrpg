---
name: verify
description: Verify the working tree is commit-ready - build with zero warnings, all tests green, formatting clean. Use before every commit and when asked whether the code is ready.
---

Run `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/verify.ps1` (add `-Fix` to apply `dotnet format` first). It checks, in order: build (zero warnings), all tests, formatting, Core free of `Microsoft.Xna`/`System.Windows`, and no `Manager`/`Helper`/`Utils` type names or kind folders. It prints one line per step and only the lines that explain a failure; don't run the underlying `dotnet` commands yourself unless you need more detail on one failure.

Tests named `..._Bug` are known bugs: the script lists them without failing; they don't block if I've acknowledged them.

Report the script's lines as they are. On failure, fix the cause if it's in code you just changed; otherwise ask me.
