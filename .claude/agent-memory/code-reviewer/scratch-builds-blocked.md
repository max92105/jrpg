---
name: scratch-builds-blocked
description: WDAC blocks scratch DLLs; fsi + repo DLL for Core probes; fsi IL walk for MonoGame calls; PowerShell 5.1 -STA for WPF probes; no Python, use sed+awk+diff to verify moved code
metadata:
  type: reference
---

On this machine, assemblies built under `%LOCALAPPDATA%\Temp\...\scratchpad` can't be loaded at runtime: "An Application Control policy has blocked this file. (0x800711C7)". Building works; running tests does not.

**Why:** Seen 2026-10-17 while copying the solution to the scratchpad to try a guard test with a failing probe.

**How to apply:** For experiments, only trust build output and the test runner's console format from a scratch copy. To inspect assembly references, grep the DLL bytes (`grep -a -o`) instead of loading them. `strings` isn't installed in Git Bash.

What does work (verified 2026-10-17, slice 3b review): an F# script in the scratchpad run with `dotnet fsi probe.fsx`, using `#r "C:/Git/jrpg/src/Jrpg.Core/bin/Debug/net10.0/Jrpg.Core.dll"`. Good for probing System.Text.Json/runtime behavior against real Core types without editing the repo.

WPF probes (verified 2026-10-22, E13b review): `dotnet fsi` can't `#r` WindowsDesktop WPF DLLs (WindowsBase type-forward error). Windows PowerShell 5.1 with `-STA` works: Add-Type PresentationFramework, `[Reflection.Assembly]::Load("PresentationFramework.Aero2, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")`, then `[System.Windows.Application]::LoadComponent([Uri]"/PresentationFramework.Aero2;component/themes/aero2.normalcolor.xaml")` gives the theme dictionary; read a style's Setters, or `template.LoadContent()` and walk LogicalTreeHelper to find hardcoded brushes.

No Python on this machine (`python` opens the Store stub). For "moved code unchanged" checks (seen X1 review, 2026-10-23): sed the HEAD file with the renames, split each file into one file per test method with awk (start at `    [Fact`, end at `    }`), then `diff -r` the two folders. Strip `\r` first.

MonoGame behavior probes (verified 2026-10-24, A1b review): `dotnet fsi` can `Assembly.LoadFrom` the nuget `MonoGame.Framework.dll` and walk a method's IL (`GetMethodBody().GetILAsByteArray()`, opcodes 0x28/0x6F/0x73 + `Module.ResolveMethod(token)`) to list what it calls. Found `SoundEffect.MasterVolume = 1f` is a no-op (early return when equal) and only SoundEffect ctors, DynamicSoundEffectInstance and Microphone init the audio device.
