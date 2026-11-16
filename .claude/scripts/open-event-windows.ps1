# The -EventWindows run of open-editor-windows.ps1, dot-sourced by it: the event windows open from a click on the map,
# so no menu reaches them. Its functions read that script's $main, $editor, $Out and $Map and its Find-Named,
# Find-Dialog and Save-Screenshot; run open-editor-windows.ps1 -EventWindows, never this file alone.

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

# A list row's own name is its view model's type: its label is the text inside it.
function Get-RowLabels($rows) {
    $texts = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Text)
    foreach ($row in $rows) { $row.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $texts).Current.Name }
}

# Gives the object window's events the Chest preset (two steps) through the Preset button, for an object whose chosen
# event has fewer: the window is closed as Cancel after, so nothing is applied. True when the entry was found and run.
function Invoke-ChestPreset($dialog) {
    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition ($automation::AutomationIdProperty), 'PresetButton'
    $button = $dialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    if (-not $button) { return $false }
    $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 500
    # The open menu is a window of its own, beside the dialog.
    $processCondition = New-Object System.Windows.Automation.PropertyCondition ($automation::ProcessIdProperty), $editor.Id
    $chestCondition = New-Object System.Windows.Automation.AndCondition (
        (New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::MenuItem)),
        (New-Object System.Windows.Automation.PropertyCondition ($automation::NameProperty), 'Chest'))
    foreach ($window in $automation::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $processCondition)) {
        $chest = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $chestCondition)
        if ($chest) {
            $chest.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
            Start-Sleep -Milliseconds 500
            return $true
        }
    }
    return $false
}

# In an object's event window: chooses the step list's first row, presses Alt+Down (decision 734) and says the steps'
# order before and after, and whether the moved step is still chosen; a second capture shows it; then Alt+Up, whose
# order back to the first says the keyboard stayed on the row. An event with fewer
# than two steps gets the Chest preset first; says nothing when that fails too, so the clicks go on to another object.
# Closed as Cancel after.
function Test-StepListKeys($dialog) {
    $listCondition = New-Object System.Windows.Automation.PropertyCondition ($automation::AutomationIdProperty), 'StepList'
    $list = $dialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $listCondition)
    $rowCondition = New-Object System.Windows.Automation.PropertyCondition ($automation::ControlTypeProperty), ([System.Windows.Automation.ControlType]::ListItem)
    if (-not $list) { return 'Step keys: no step list' }
    $rows = $list.FindAll([System.Windows.Automation.TreeScope]::Children, $rowCondition)
    if ($rows.Count -lt 2 -and (Invoke-ChestPreset $dialog)) {
        $rows = $list.FindAll([System.Windows.Automation.TreeScope]::Children, $rowCondition)
    }
    if ($rows.Count -lt 2) { return }
    $before = (Get-RowLabels $rows) -join ' | '
    $rows[0].GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $rows[0].SetFocus()
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}')
    Start-Sleep -Milliseconds 700
    $rows = $list.FindAll([System.Windows.Automation.TreeScope]::Children, $rowCondition)
    $after = (Get-RowLabels $rows) -join ' | '
    $chosen = $rows[1].GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected
    $file = Join-Path $Out 'EventsOfObjectKeys.png'
    Save-Screenshot $dialog $file
    # With no click in between, Alt+Up only reaches the list if the keyboard stayed on the moved row.
    [System.Windows.Forms.SendKeys]::SendWait('%{UP}')
    Start-Sleep -Milliseconds 700
    $back = (Get-RowLabels ($list.FindAll([System.Windows.Automation.TreeScope]::Children, $rowCondition))) -join ' | '
    "Step keys: Alt+Down, before: $before; after: $after; second row chosen: $chosen; then Alt+Up: $back; $file"
}

# The event windows open from a click on the map: the Events tab is chosen, then the map is clicked on a 24 px grid
# until a cell's window and an object's window have each opened, and an object's window shows two steps or more for the
# keys. Each is closed as Cancel, so nothing is applied.
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
    $keysTested = $false
    for ($y = $top; $y -lt $bottom -and ($seen.Count -lt 2 -or -not $keysTested) -and $clicks -lt 600; $y += 24) {
        for ($x = $left; $x -lt $right -and ($seen.Count -lt 2 -or -not $keysTested) -and $clicks -lt 600; $x += 24) {
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
            if ($kind -eq 'EventsOfObject' -and -not $keysTested) {
                $keys = Test-StepListKeys $dialog
                if ($keys) { $keys; $keysTested = $true }
            }
            $dialog.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
            Start-Sleep -Milliseconds 700
        }
    }
    foreach ($kind in 'EventOfCell', 'EventsOfObject') {
        if (-not $seen.Contains($kind)) { "$kind`: never reached in $clicks clicks" }
    }
    if (-not $keysTested) { "Step keys: no object's event with two steps or more in $clicks clicks" }
}
