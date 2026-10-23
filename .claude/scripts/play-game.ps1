# Starts the game, presses keys, saves a screenshot of the screen, then closes the game it started. For look changes
# that tests can't see.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/play-game.ps1 -Out <png> -Steps "69:100,87:400"
#
# Each step is "virtual key:milliseconds held". E (69) confirms, WASD (87, 65, 83, 68) moves, Esc (27) opens the menu,
# Backspace (8) cancels. Arrow keys and Enter don't register: simulated keys reach MonoGame through their scan codes,
# and those two need flags this script doesn't send. Run `dotnet build` first; the game starts with --no-build.

param([Parameter(Mandatory)][string]$Out, [string]$Steps = '', [int]$StartSeconds = 6)

Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class SimulatedKeys {
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
  [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint code, uint type);
}
"@

Set-Location (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$running = @(Get-Process Jrpg.Game -ErrorAction SilentlyContinue).Id
Start-Process dotnet -ArgumentList 'run', '--project', 'src/Jrpg.Game', '--no-build' -WindowStyle Hidden
Start-Sleep -Seconds $StartSeconds

# Only the game this script started is driven and closed, never one the developer has open.
$game = Get-Process Jrpg.Game -ErrorAction SilentlyContinue | Where-Object { $running -notcontains $_.Id } | Select-Object -First 1
if (-not $game) { Write-Error 'The game did not start (build it first?)'; exit 1 }
[SimulatedKeys]::SetForegroundWindow($game.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 300

foreach ($step in $Steps.Split(',', [StringSplitOptions]::RemoveEmptyEntries)) {
    $key, $held = $step.Split(':')
    $scan = [byte][SimulatedKeys]::MapVirtualKey([uint32]$key, 0)
    [SimulatedKeys]::keybd_event([byte][int]$key, $scan, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds ([int]$held)
    [SimulatedKeys]::keybd_event([byte][int]$key, $scan, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}

$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bitmap.Save($Out)
$graphics.Dispose()
$bitmap.Dispose()
Stop-Process -Id $game.Id
