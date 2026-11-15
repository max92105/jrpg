# Opens editor windows through their menus, as the developer would, and reports each: opened (a screenshot saved under
# -Out), greyed, or the editor crashed (its unhandled exception printed). Editor tests never open a window, so a XAML
# error (a bad value, a missing resource) only shows here: the cinematics window's int Width crashed the editor on open.
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1 -Out <folder>
#   ... -Windows 'Database>Effect strips...','NPCs>Character sprites...'
# Build the editor first (dotnet build src/Jrpg.Editor); the script runs the Debug build, which reads content/.
param(
    [string]$Out = (Join-Path $env:TEMP 'jrpg-editor-windows'),
    [string[]]$Windows = @(
        'Database>Quests...', 'Database>Shops...', 'Database>Drop tables...', 'Database>Effect strips...',
        'Database>Cinematics...', 'Database>Enemies...', 'NPCs>Character sprites...', 'Map>Settings...')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
$repository = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$exe = Join-Path $repository 'src\Jrpg.Editor\bin\Debug\net10.0-windows\Jrpg.Editor.exe'
New-Item -ItemType Directory -Force $Out | Out-Null
$automation = [System.Windows.Automation.AutomationElement]

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
    if (-not $dialog) { "$entry`: no window opened"; continue }
    $file = Join-Path $Out (($itemName -replace '[^A-Za-z0-9]', '') + '.png')
    Save-Screenshot $dialog $file
    "$entry`: opened, $file"
    $dialog.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    Start-Sleep -Seconds 1
}

$editor.Refresh()
if (-not $editor.HasExited) { $editor.Kill() }
Start-Sleep -Milliseconds 500
$trace = $errors.Result
if ($trace) { '--- the editor''s unhandled exception'; $trace }
