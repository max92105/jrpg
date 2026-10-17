---
name: scratch-builds-blocked
description: Windows Application Control (WDAC) blocks loading DLLs built under the temp scratchpad, so test runs there fail with FileLoadException 0x800711C7
metadata:
  type: reference
---

On this machine, assemblies built under `%LOCALAPPDATA%\Temp\...\scratchpad` can't be loaded at runtime: "An Application Control policy has blocked this file. (0x800711C7)". Building works; running tests does not.

**Why:** Seen 2026-10-17 while copying the solution to the scratchpad to try a guard test with a failing probe.

**How to apply:** For experiments, only trust build output and the test runner's console format from a scratch copy. Tests that load assemblies by reflection will fail there for environmental reasons. To inspect assembly references, grep the DLL bytes (`grep -a -o`) instead of loading them. `strings` isn't installed in Git Bash.
