---
name: verify
description: Verify the working tree is commit-ready - build with zero warnings, all tests green, formatting clean. Use before every commit and when asked whether the code is ready.
---

Check that the working tree meets the definition of done. Run in order and stop at the first failure:

1. `dotnet build Jrpg.slnx --nologo -v q` — must succeed with zero warnings.
2. `dotnet test Jrpg.slnx --nologo -v q` — all tests pass. Tests named `..._Bug` are known bugs: report them, they don't block if I've acknowledged them.
3. `dotnet format Jrpg.slnx --verify-no-changes` — if it fails, run `dotnet format Jrpg.slnx`, then re-run step 1.
4. Check no file under `src/Jrpg.Core/` contains `using Microsoft.Xna` or `using System.Windows`.
5. Check no new file under `src/` declares a type ending in `Manager`, `Helper`, `Utils` or `Util`, and no folder under `src/` is named `Models`, `Enums`, `Managers` or `Utils`. Report any hit by file; it fails unless I've approved that name.

Report one line per step: pass/fail. On failure, show the relevant error lines (not the whole log) and fix the cause if it's in code you just changed; otherwise ask me.
