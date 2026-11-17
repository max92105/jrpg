# Opens editor windows through their menus, as the developer would, and reports each: opened (a screenshot saved under
# -Out), shown as a page (a Database entry, decision 933: the main window's screenshot, the page found by its "Shops
# page" automation name; NPCs > Character sprites too), greyed, ERROR (opening it threw and the editor's
# unexpected-error box caught it, ADR 0050) or CRASHED (a throw before the main window loaded ended the editor). Editor
# tests never open a window, so a XAML error (a bad value, a missing resource) only shows here: the cinematics window's
# int Width threw on open. The last default entry raises the editor's test error to check the net itself. The trace at
# the end is every report the editor wrote to its error output.
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1 -Out <folder>
# Several chosen windows need -Command, as -File passes a list as one string:
#   powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.claude/scripts/open-editor-windows.ps1' -Windows 'Database>Shops','Help>Raise a test error'"
# The World menus hide in Database mode, so a list runs every World-menu entry (Map > Settings...) before the first
# page entry. A third part names a button inside the opened window or the shown page: 'Database>Effect strips>Strip
# from sheet cells...' shows the page, invokes that button, captures the window it opens and closes it before its
# owner (decision 996).
# -EventWindows: instead of the menus, open the Events tab and click the map until a cell's and an object's event
# window have each opened; captures both, moves the object window's first step down with Alt+Down, and closes them
# unchanged (they open from a map click, so no menu reaches them). It moves the mouse and presses keys: nobody should
# be using the machine. -Map <id> opens that map first (the one shown at start may hold no object with events):
# -EventWindows -Map milestone. That run lives in open-event-windows.ps1, beside this script, which dot-sources it.
# Build the editor first (dotnet build src/Jrpg.Editor); the script runs the Debug build, which reads content/.
# Agents: never open a screenshot that can show Mana Seed art (decision 438): the main window, every Database page's
# included (the World half, and Skills, Items and Character sprites show licensed art), the composer or New NPC. Read
# the script's lines for those; the pages' screenshots are the developer's. Look only at windows of generated art or
# none (Strip from sheet cells).
# Take its whole output (into a variable or a file): a pipe that stops early (head, Select-Object -First) ends the
# script before it closes its editor, which then holds the build's files.
param(
    [string]$Out = (Join-Path $env:TEMP 'jrpg-editor-windows'),
    [string[]]$Windows = @(
        'Map>Settings...',
        'NPCs>Character sprites',
        'Database>Quests', 'Database>Shops', 'Database>Drop tables', 'Database>Effect strips',
        'Database>Effect strips>Strip from sheet cells...', 'Database>Battle backgrounds',
        'Database>Skills', 'Database>Effects', 'Database>Statuses', 'Database>Cinematics',
        'Database>Enemies', 'Database>Items', 'Database>Affixes', 'Database>Encounters', 'Database>Classes',
        'Database>Party members',
        'Help>Raise a test error'),
    [switch]$EventWindows,
    [string]$Map
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing, System.Windows.Forms
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
        if ((Test-Window $window) -and $window.Current.NativeWindowHandle -ne $main.Current.NativeWindowHandle) { return $window }
    }
    $owned = $main.FindAll([System.Windows.Automation.TreeScope]::Children,
        (New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Window)))
    foreach ($window in $owned) { if (Test-Window $window) { return $window } }
    return $null
}

# A popup (a tooltip, a drop-down; the Drop tables page shows one) is a window of the process too, but no dialog: it has
# no WindowPattern to close it by.
function Test-Window($element) {
    return $element.GetCurrentPropertyValue($automation::IsWindowPatternAvailableProperty)
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

# The -EventWindows run, kept in its own file beside this one.
. (Join-Path $PSScriptRoot 'open-event-windows.ps1')

# The window a button inside an opened window opens: captured, then closed before its owner.
function Open-Nested([string]$entry, $owner, [string]$buttonName) {
    $button = Find-Named $owner $buttonName
    if (-not $button) { return "$entry`: no '$buttonName' button" }
    $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 3
    # A window over a dialog shows under it; one over the main window (a page's) beside it, as a dialog does.
    $windows = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Window)
    if ($owner.Current.NativeWindowHandle -eq $main.Current.NativeWindowHandle) {
        $nested = @(Find-Dialog $editor $main | Where-Object { $_ })
    }
    else { $nested = @($owner.FindAll([System.Windows.Automation.TreeScope]::Children, $windows)) }
    if ($nested.Count -eq 0) { return "$entry`: no window opened" }
    if ($nested[0].Current.Name -eq 'Unexpected error') {
        [void][EditorBox]::PostMessage([IntPtr]$nested[0].Current.NativeWindowHandle, 0x0111, [IntPtr]6, [IntPtr]::Zero)
        Start-Sleep -Seconds 1
        return "$entry`: ERROR (the editor's unexpected-error box; its report is in the trace below)"
    }
    $file = Join-Path $Out (($buttonName -replace '[^A-Za-z0-9]', '') + '.png')
    Save-Screenshot $nested[0] $file
    $nested[0].GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    Start-Sleep -Seconds 1
    "$entry`: opened, $file"
}

# A Database entry opens no window but shows its page in the main window, found by the page's automation name
# (decision 933); the capture is the main window's, so the developer's (see the header).
function Open-Page([string]$entry, [string]$itemName, [string]$buttonName) {
    if (-not (Find-Named $main "$itemName page" 8)) { return "$entry`: no window opened, no page shown" }
    if ($buttonName) { return Open-Nested $entry $main $buttonName }
    $file = Join-Path $Out (($itemName -replace '[^A-Za-z0-9]', '') + '.png')
    Save-Screenshot $main $file
    "$entry`: shown as a page, $file"
}

# The editor is closed whatever happens here: left open, it would hold its files and the caller's output.
try {
if ($EventWindows) { Open-EventWindows }
foreach ($entry in $(if ($EventWindows) { @() } else { $Windows })) {
    $menuName, $itemName, $buttonName = $entry -split '>', 3
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
    if (-not $dialog) { Open-Page $entry $itemName $buttonName; continue }
    if ($buttonName) { Open-Nested $entry $dialog $buttonName }
    else {
        $file = Join-Path $Out (($itemName -replace '[^A-Za-z0-9]', '') + '.png')
        Save-Screenshot $dialog $file
        "$entry`: opened, $file"
    }
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
