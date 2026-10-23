# Starts the game, presses keys, saves a screenshot of the game's window, then closes the game it started. For look
# changes that tests can't see.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/play-game.ps1 -Out <png> -Steps "69:100,87:400"
#
# Each step is "virtual key:milliseconds held". E (69) confirms, WASD (87, 65, 83, 68) moves, Esc (27) opens the menu,
# Backspace (8) cancels. Arrow keys and Enter don't register: simulated keys reach MonoGame through their scan codes,
# and those two need flags this script doesn't send. Run `dotnet build` first; the game starts with --no-build.
#
# The developer may be working on this machine: keys and the screenshot would land in whatever window is in front
# (once it caught a private note). So the script stops, pressing and capturing nothing, when the game isn't the
# foreground window, and the picture holds only the game's window, never the rest of the screen.

param([Parameter(Mandatory)][string]$Out, [string]$Steps = '', [int]$StartSeconds = 6)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class SimulatedKeys {
  [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window, out Rect area);
  [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint code, uint type);
}
"@

function Stop-UnlessGameInFront($game, [string]$when) {
    if ([SimulatedKeys]::GetForegroundWindow() -ne $game.MainWindowHandle) {
        Stop-Process -Id $game.Id
        Write-Error "The game was not the foreground window $when; nothing was captured."
        exit 1
    }
}

Set-Location (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$running = @(Get-Process Jrpg.Game -ErrorAction SilentlyContinue).Id
Start-Process dotnet -ArgumentList 'run', '--project', 'src/Jrpg.Game', '--no-build' -WindowStyle Hidden
Start-Sleep -Seconds $StartSeconds

# Only the game this script started is driven and closed, never one the developer has open.
$game = Get-Process Jrpg.Game -ErrorAction SilentlyContinue | Where-Object { $running -notcontains $_.Id } | Select-Object -First 1
if (-not $game) { Write-Error 'The game did not start (build it first?)'; exit 1 }
[SimulatedKeys]::SetForegroundWindow($game.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 300
Stop-UnlessGameInFront $game 'before the keys'

foreach ($step in $Steps.Split(',', [StringSplitOptions]::RemoveEmptyEntries)) {
    $key, $held = $step.Split(':')
    $scan = [byte][SimulatedKeys]::MapVirtualKey([uint32]$key, 0)
    [SimulatedKeys]::keybd_event([byte][int]$key, $scan, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds ([int]$held)
    [SimulatedKeys]::keybd_event([byte][int]$key, $scan, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}

Stop-UnlessGameInFront $game 'at the screenshot'
$area = New-Object SimulatedKeys+Rect
[SimulatedKeys]::GetWindowRect($game.MainWindowHandle, [ref]$area) | Out-Null
$width = $area.Right - $area.Left
$height = $area.Bottom - $area.Top
$bitmap = New-Object System.Drawing.Bitmap $width, $height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($area.Left, $area.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
$bitmap.Save($Out)
$graphics.Dispose()
$bitmap.Dispose()
Stop-Process -Id $game.Id
