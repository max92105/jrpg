---
name: scratch-builds-blocked
description: WDAC blocks loading DLLs built under the temp scratchpad (FileLoadException 0x800711C7); `dotnet fsi` scripts referencing repo bin DLLs do work for behavior probes
metadata:
  type: reference
---

On this machine, assemblies built under `%LOCALAPPDATA%\Temp\...\scratchpad` can't be loaded at runtime: "An Application Control policy has blocked this file. (0x800711C7)". Building works; running tests does not.

**Why:** Seen 2026-10-17 while copying the solution to the scratchpad to try a guard test with a failing probe.

**How to apply:** For experiments, only trust build output and the test runner's console format from a scratch copy. To inspect assembly references, grep the DLL bytes (`grep -a -o`) instead of loading them. `strings` isn't installed in Git Bash.

What does work (verified 2026-10-17, slice 3b review): an F# script in the scratchpad run with `dotnet fsi probe.fsx`, using `#r "C:/Git/jrpg/src/Jrpg.Core/bin/Debug/net10.0/Jrpg.Core.dll"`. Good for probing System.Text.Json/runtime behavior against real Core types without editing the repo.
