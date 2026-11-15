# Opens editor windows through their menus, as the developer would, and reports each: opened (a screenshot saved under
# -Out), greyed, ERROR (opening it threw and the editor's unexpected-error box caught it, ADR 0050) or CRASHED (a throw
# before the main window loaded ended the editor). Editor tests never open a window, so a XAML error (a bad value, a
# missing resource) only shows here: the cinematics window's int Width threw on open. The last default entry raises the
# editor's test error to check the net itself. The trace at the end is every report the editor wrote to its error output.
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1 -Out <folder>
# Several chosen windows need -Command, as -File passes a list as one string:
#   powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.claude/scripts/open-editor-windows.ps1' -Windows 'Database>Shops...','Help>Raise a test error'"
# -EventWindows: instead of the menus, open the Events tab and click the map until a cell's and an object's event
# window have each opened; captures both and closes them unchanged (they open from a map click, so no menu reaches
# them). It moves the mouse: nobody should be using the machine. -Map <id> opens that map first (the one shown at start
# may hold no object with events): -EventWindows -Map milestone.
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
        'Help>Raise a test error'),
    [switch]$EventWindows,
    [string]$Map
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
$repository = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$exe = Join-Path $repository 'src\Jrpg.Editor\bin\Debug\net10.0-windows\Jrpg.Editor.exe'
New-Item -ItemType Directory -Force $Out | Out-Null
$automation = [System.Windows.Automation.AutomationElement]
Add-Type -Name EditorBox -Namespace '' -MemberDefinition @'
[DllImport("user32.dll")] public static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
[DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
'@

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
    # Cut 12 px inside the window's box: its rounded corners and shadow show what lies behind it, which on the map is
    # Mana Seed art no capture may hold (decision 438).
    $box = $element.Current.BoundingRectangle
    $inset = 12
    $bitmap = New-Object System.Drawing.Bitmap ([int]$box.Width - (2 * $inset)), ([int]$box.Height - (2 * $inset))
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen([int]$box.X + $inset, [int]$box.Y + $inset, 0, 0, $bitmap.Size)
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

# Opens a map by its id from the map list (its row reads "Name (id)"), expanding the folders first.
function Open-Map([string]$mapId) {
    $treeItems = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::TreeItem)
    $texts = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Text)
    for ($pass = 0; $pass -lt 3; $pass++) {
        foreach ($item in $main.FindAll([System.Windows.Automation.TreeScope]::Descendants, $treeItems)) {
            # A row's own name is its view model's type: its label is the text inside it.
            $label = $item.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $texts)
            if ($label -and $label.Current.Name -like "*($mapId)") {
                $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
                Start-Sleep -Seconds 2
                return $true
            }
            $pattern = $null
            if ($item.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$pattern)) {
                if ($pattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed) { $pattern.Expand() }
            }
        }
        Start-Sleep -Milliseconds 500
    }
    return $false
}

# The event windows open from a click on the map: the Events tab is chosen, then the map is clicked on a 24 px grid
# until a cell's window and an object's window have each opened. Each is closed as Cancel, so nothing is applied.
function Open-EventWindows {
    [void][EditorBox]::SetForegroundWindow([IntPtr]$main.Current.NativeWindowHandle)
    if ($Map -and -not (Open-Map $Map)) { "Events: no map '$Map' in the map list"; return }
    $tab = Find-Named $main 'Events'
    if (-not $tab) { 'Events: no Events tab'; return }
    $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 500
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $panel = $walker.GetParent($tab).Current.BoundingRectangle
    $window = $main.Current.BoundingRectangle
    # The map lies between the map list on the left and the side panel: under the tool bar, from the panel's top down.
    $trees = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Tree)
    $mapList = $main.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $trees)
    $left = if ($mapList) { [int]$mapList.Current.BoundingRectangle.Right + 16 } else { [int]$window.Left + 280 }
    $right = [int]$panel.Left - 16
    $top = [int]$panel.Top + 8
    $bottom = [int]$panel.Bottom - 8
    $seen = New-Object System.Collections.Generic.HashSet[string]
    $clicks = 0
    for ($y = $top; $y -lt $bottom -and $seen.Count -lt 2 -and $clicks -lt 600; $y += 24) {
        for ($x = $left; $x -lt $right -and $seen.Count -lt 2 -and $clicks -lt 600; $x += 24) {
            $clicks++
            [void][EditorBox]::SetCursorPos($x, $y)
            [EditorBox]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
            [EditorBox]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
            Start-Sleep -Milliseconds 700
            $editor.Refresh()
            if ($editor.HasExited) { "Events: CRASHED (exit code $($editor.ExitCode))"; return }
            $dialog = Find-Dialog $editor $main
            if (-not $dialog) { continue }
            $name = $dialog.Current.Name
            if ($name -eq 'Unexpected error') {
                'Events: ERROR (the editor''s unexpected-error box; its report is in the trace below)'
                [void][EditorBox]::PostMessage([IntPtr]$dialog.Current.NativeWindowHandle, 0x0111, [IntPtr]6, [IntPtr]::Zero)
                Start-Sleep -Seconds 1
                continue
            }
            $kind = $null
            if ($name -like 'Event of cell*') { $kind = 'EventOfCell' }
            elseif ($name -like 'Events of*') { $kind = 'EventsOfObject' }
            if ($kind -and $seen.Add($kind)) {
                $file = Join-Path $Out "$kind.png"
                Save-Screenshot $dialog $file
                "$name`: opened, $file"
            }
            $dialog.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
            Start-Sleep -Milliseconds 700
        }
    }
    foreach ($kind in 'EventOfCell', 'EventsOfObject') {
        if (-not $seen.Contains($kind)) { "$kind`: never reached in $clicks clicks" }
    }
}

# The editor is closed whatever happens here: left open, it would hold its files and the caller's output.
try {
if ($EventWindows) { Open-EventWindows }
foreach ($entry in $(if ($EventWindows) { @() } else { $Windows })) {
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
