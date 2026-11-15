---
name: scratch-builds-blocked
description: fsi + repo DLL for Core probes; fsi IL walk for MonoGame calls; PowerShell 5.1 -STA for WPF probes; no Python, use sed+awk+diff to verify moved code
metadata:
  type: reference
---

Probe recipes for this machine. (The Application Control policy that once blocked scratch DLLs was turned off on 2026-10-25, so scratch builds can run again.)

What does work (verified 2026-10-17, slice 3b review): an F# script in the scratchpad run with `dotnet fsi probe.fsx`, using `#r "C:/git_perso/jrpg/src/Jrpg.Core/bin/Debug/net10.0/Jrpg.Core.dll"`. Good for probing System.Text.Json/runtime behavior against real Core types without editing the repo.

WPF probes (verified 2026-10-22, E13b review): `dotnet fsi` can't `#r` WindowsDesktop WPF DLLs (WindowsBase type-forward error). Windows PowerShell 5.1 with `-STA` works: Add-Type PresentationFramework, `[Reflection.Assembly]::Load("PresentationFramework.Aero2, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")`, then `[System.Windows.Application]::LoadComponent([Uri]"/PresentationFramework.Aero2;component/themes/aero2.normalcolor.xaml")` gives the theme dictionary; read a style's Setters, or `template.LoadContent()` and walk LogicalTreeHelper to find hardcoded brushes.

No Python on this machine (`python` opens the Store stub). For "moved code unchanged" checks (seen X1 review, 2026-10-23): sed the HEAD file with the renames, split each file into one file per test method with awk (start at `    [Fact`, end at `    }`), then `diff -r` the two folders. Strip `\r` first.

MonoGame behavior probes (verified 2026-10-24, A1b review): `dotnet fsi` can `Assembly.LoadFrom` the nuget `MonoGame.Framework.dll` and walk a method's IL (`GetMethodBody().GetILAsByteArray()`, opcodes 0x28/0x6F/0x73 + `Module.ResolveMethod(token)`) to list what it calls. Found `SoundEffect.MasterVolume = 1f` is a no-op (early return when equal) and only SoundEffect ctors, DynamicSoundEffectInstance and Microphone init the audio device.

Allocation probes (verified 2026-11-10, DO1 review): fsi loop with `GC.GetAllocatedBytesForCurrentThread()` around
1000 calls after a 2000-call warm-up. Found `List<T>.Sort(IComparer<T>)` allocates 64 B per call (it wraps
`comparer.Compare` in a delegate); `Sort(Comparison<T>)` with a cached static delegate allocates 0.

Runtime WPF probes against .NET 10 (verified 2026-11-05, WP3 review): a scratch `net10.0-windows` `UseWPF` console
project in the scratchpad (csproj + a UserControl/Window XAML pair + `[STAThread] Main` that `new`s the window without
`Show`) builds in seconds and runs; good for "does this XAML wire/resolve at runtime" questions. Check the `.g.cs`
`Connect` too: members of local types the pass-1 compiler can't resolve go into BAML instead. For behavior needing
containers (TreeView events, context menu bindings, verified MF3 2026-11-10): `XamlReader.Parse` the XAML, `Show()` a
Window at Left=-2000, then `Dispatcher.CurrentDispatcher.Invoke(() => {}, DispatcherPriority.ApplicationIdle)`.

Editor internals without WPF (verified 2026-11-14, MB4 review): fsi `Assembly.LoadFrom` the built
`src/Jrpg.Editor/bin/Debug/net10.0-windows/Jrpg.Editor.dll`, `GetType("Jrpg.Editor.GifImport.FrameScaler")`, Invoke a
static method over a grid of sizes; works for any internal static type that touches no WPF type. Found the area
average's `(x + 1) * step` passing the source edge (53x100 to 192 throws).

WPF binding fact (probed 2026-11-13, NP1 review, PowerShell -STA + XamlReader): a non-editable ComboBox whose bound
SelectedItem ("" or any value) is not in its ItemsSource leaves the source alone on load and close: no null write-back.

WPF layout fact (probed 2026-11-14, CE6 review, PowerShell 5.1 -STA, Measure/Arrange + VisualTreeHelper.GetOffset): an
Image with Stretch="UniformToFill" and default Stretch alignment that overflows its slot is pinned top-left (offset
0,0); only Center alignment centers the crop (offset 0,-45 for 4:3 in 480x270).

Generated PNG probes (verified 2026-11-14, NU2 review): a PowerShell 5.1 script in the scratchpad, `Add-Type
System.Drawing`, `[System.Drawing.Bitmap]::FromFile(...)` and `GetPixel` per column/row prints alpha and colors; a copy of
an art-src script without its last calls (sed) plus Write-Host lines prints its intermediate values. Found PowerShell's
`[Math]::Min(1, 0.92)` picks the Int32 overload (returns 1) and LinearGradientBrush tiles past its rectangle.
