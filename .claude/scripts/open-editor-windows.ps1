# Opens editor windows through their menus, as the developer would, and reports each: opened (a screenshot saved under
# -Out), greyed, ERROR (opening it threw and the editor's unexpected-error box caught it, ADR 0050) or CRASHED (a throw
# before the main window loaded ended the editor). Editor tests never open a window, so a XAML error (a bad value, a
# missing resource) only shows here: the cinematics window's int Width threw on open. The last default entry raises the
# editor's test error to check the net itself. The trace at the end is every report the editor wrote to its error output.
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1 -Out <folder>
# Several chosen windows need -Command, as -File passes a list as one string:
#   powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.claude/scripts/open-editor-windows.ps1' -Windows 'Database>Shops...','Help>Raise a test error'"
# Build the editor first (dotnet build src/Jrpg.Editor); the script runs the Debug build, which reads content/.
# Agents: never open a screenshot that can show Mana Seed art (decision 438): the main window (its tile and object
# panels), Character sprites, the composer or New NPC. Read the script's lines for those; look only at windows of
# generated art (Effect strips, Cinematics, Enemies, Shops, Drop tables, Quests).
# Take its whole output (into a variable or a file): a pipe that stops early (head, Select-Object -First) ends the
# script before it closes its editor, which then holds the build's files.
param(
    [string]$Out = (Join-Path $env:TEMP 'jrpg-editor-windows'),
    [string[]]$Windows = @(
        'Database>Quests...', 'Database>Shops...', 'Database>Drop tables...', 'Database>Effect strips...',
        'Database>Cinematics...', 'Database>Enemies...', 'NPCs>Character sprites...', 'Map>Settings...',
        'Help>Raise a test error')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
$repository = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$exe = Join-Path $repository 'src\Jrpg.Editor\bin\Debug\net10.0-windows\Jrpg.Editor.exe'
New-Item -ItemType Directory -Force $Out | Out-Null
$automation = [System.Windows.Automation.AutomationElement]
Add-Type -Name EditorBox -Namespace '' -MemberDefinition '[DllImport("user32.dll")] public static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);'

function Find-Named($root, [string]$name, [int]$tries = 20) {
    $condition = New-Object System.Windows.Automation.PropertyCondition ($automation::NameProperty), $name
    for ($i = 0; $i -lt $tries; $i++) {
        $found = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        if ($found) { return $found }
        Start-Sleep -Milliseconds 250
    }
    return $null
}

# The window the process opened over its main one, if any.
function Find-Dialog($process, $main) {
    $condition = New-Object System.Windows.Automation.PropertyCondition ($automation::ProcessIdProperty), $process.Id
    $all = $automation::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $all) {
        if ($window.Current.NativeWindowHandle -ne $main.Current.NativeWindowHandle) { return $window }
    }
    $owned = $main.FindAll([System.Windows.Automation.TreeScope]::Children,
        (New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Window)))
    if ($owned.Count -gt 0) { return $owned[0] }
    return $null
}

function Save-Screenshot($element, [string]$path) {
    $box = $element.Current.BoundingRectangle
    $bitmap = New-Object System.Drawing.Bitmap ([int]$box.Width), ([int]$box.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen([int]$box.X, [int]$box.Y, 0, 0, $bitmap.Size)
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}

$start = New-Object System.Diagnostics.ProcessStartInfo $exe
$start.WorkingDirectory = $repository
$start.RedirectStandardError = $true
$start.UseShellExecute = $false
$editor = [System.Diagnostics.Process]::Start($start)
$errors = $editor.StandardError.ReadToEndAsync()
$main = $null
for ($i = 0; $i -lt 60 -and -not $main; $i++) {
    Start-Sleep -Milliseconds 500
    $editor.Refresh()
    if ($editor.MainWindowHandle -ne 0) { $main = $automation::FromHandle($editor.MainWindowHandle) }
}
if (-not $main) { $editor.Kill(); throw 'The editor showed no window within 30 s.' }
Start-Sleep -Seconds 3

# The editor is closed whatever happens here: left open, it would hold its files and the caller's output.
try {
foreach ($entry in $Windows) {
    $menuName, $itemName = $entry -split '>', 2
    $menuElement = Find-Named $main $menuName
    if (-not $menuElement) { "$entry`: no '$menuName' menu"; continue }
    $menuElement.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Start-Sleep -Milliseconds 400
    $itemElement = Find-Named $main $itemName
    if (-not $itemElement) { "$entry`: no '$itemName' entry"; continue }
    if (-not $itemElement.Current.IsEnabled) {
        "$entry`: greyed"
        $menuElement.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse()
        continue
    }
    $itemElement.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 3
    $editor.Refresh()
    if ($editor.HasExited) { "$entry`: CRASHED (exit code $($editor.ExitCode))"; break }
    $dialog = Find-Dialog $editor $main
    $raisesTheTestError = $itemName -eq 'Raise a test error'
    if ($dialog -and $dialog.Current.Name -eq 'Unexpected error') {
        if ($raisesTheTestError) { "$entry`: the net caught it" }
        else { "$entry`: ERROR (the editor's unexpected-error box; its report is in the trace below)" }
        # Yes, sent as the box's own command (WM_COMMAND, IDYES): the button's caption follows the system's language.
        [void][EditorBox]::PostMessage([IntPtr]$dialog.Current.NativeWindowHandle, 0x0111, [IntPtr]6, [IntPtr]::Zero)
        Start-Sleep -Seconds 1
        continue
    }
    if ($raisesTheTestError) { "$entry`: THE NET DID NOT CATCH IT"; continue }
    if (-not $dialog) { "$entry`: no window opened"; continue }
    $file = Join-Path $Out (($itemName -replace '[^A-Za-z0-9]', '') + '.png')
    Save-Screenshot $dialog $file
    "$entry`: opened, $file"
    $dialog.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    Start-Sleep -Seconds 1
}
}
finally {
    $editor.Refresh()
    if (-not $editor.HasExited) { $editor.Kill() }
}
Start-Sleep -Milliseconds 500
$trace = $errors.Result
if ($trace) { '--- the editor''s error output'; $trace }
