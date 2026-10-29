# Starts the game, presses keys, saves the game's own capture of its screen, then closes the game it started. For look
# changes that tests can't see.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/play-game.ps1 -Out <png> -Steps "69:100,87:400"
#
# Each step is "virtual key:milliseconds held". E (69) confirms, WASD (87, 65, 83, 68) moves, Esc (27) opens the menu,
# Backspace (8) cancels. Arrow keys and Enter don't register: simulated keys reach MonoGame through their scan codes,
# and those two need flags this script doesn't send. Run `dotnet build` first; the game starts with --no-build.
#
# The developer may be working on this machine: keys would land in whatever window is in front (once a screenshot
# caught a private note). So the script stops, pressing nothing, when the game isn't the foreground window. The picture
# is the game's Debug capture key (F12), never a copy of the screen, which comes out black for the borderless
# full-screen game anyway.

# -Content runs the game on another content folder (its --content), such as a copy set up so a battle comes quickly.
# For a battle capture, copy content/ to the scratchpad and in the copy give the enemies 1 HP and 0 dodge, set
# rules.json stats.baseAccuracy to 200 and the drop table's chances to 100: otherwise a goblin survives a missed hit
# and the steps run out (loot plan, D3 and P1).
param([Parameter(Mandatory)][string]$Out, [string]$Steps = '', [int]$StartSeconds = 6, [string]$Content = '')

Add-Type @"
using System; using System.Runtime.InteropServices;
public static class SimulatedKeys {
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint code, uint type);
}
"@

$captureKey = 123

function Stop-UnlessGameInFront($game, [string]$when) {
    if ([SimulatedKeys]::GetForegroundWindow() -ne $game.MainWindowHandle) {
        Stop-Process -Id $game.Id
        Write-Error "The game was not the foreground window $when; nothing was captured."
        exit 1
    }
}

function Send-Key([int]$key, [int]$held) {
    $scan = [byte][SimulatedKeys]::MapVirtualKey([uint32]$key, 0)
    [SimulatedKeys]::keybd_event([byte]$key, $scan, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds $held
    [SimulatedKeys]::keybd_event([byte]$key, $scan, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}

Set-Location (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$running = @(Get-Process Jrpg.Game -ErrorAction SilentlyContinue).Id
$gameArguments = @('run', '--project', 'src/Jrpg.Game', '--no-build')
if ($Content) { $gameArguments += @('--', '--content', "`"$Content`"") }
Start-Process dotnet -ArgumentList $gameArguments -WindowStyle Hidden
Start-Sleep -Seconds $StartSeconds

# Only the game this script started is driven and closed, never one the developer has open.
$game = Get-Process Jrpg.Game -ErrorAction SilentlyContinue | Where-Object { $running -notcontains $_.Id } | Select-Object -First 1
if (-not $game) { Write-Error 'The game did not start (build it first?)'; exit 1 }
[SimulatedKeys]::SetForegroundWindow($game.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 300
Stop-UnlessGameInFront $game 'before the keys'

foreach ($step in $Steps.Split(',', [StringSplitOptions]::RemoveEmptyEntries)) {
    $key, $held = $step.Split(':')
    Send-Key ([int]$key) ([int]$held)
}

# The Debug build saves captures in the repository's docs/captures/; the file takes a moment to be written.
Stop-UnlessGameInFront $game 'at the screenshot'
$pressedAt = Get-Date
Send-Key $captureKey 100
Start-Sleep -Seconds 2
Stop-Process -Id $game.Id
$capture = Get-ChildItem docs/captures -Filter *.png -File |
    Where-Object { $_.LastWriteTime -ge $pressedAt } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $capture) { Write-Error 'The game saved no capture (a Release build has no capture key).'; exit 1 }
Move-Item $capture.FullName $Out -Force
