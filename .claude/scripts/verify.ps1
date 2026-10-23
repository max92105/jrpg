# Checks the working tree against the definition of done and prints a short report: one line per step, and only the
# lines that explain a failure. Used by /verify, /slice and the Stop hook, so build and test logs never flood the context.
#
#   powershell -NoProfile -File .claude/scripts/verify.ps1          # check only
#   powershell -NoProfile -File .claude/scripts/verify.ps1 -Fix     # run dotnet format first, then check
#
# Exit code: 0 when every step passes, 1 otherwise.

param([switch]$Fix)

$ErrorActionPreference = 'Continue'
Set-Location (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$solution = 'Jrpg.slnx'
$failed = $false

function Report($step, $ok, $details) {
    $state = if ($ok) { 'pass' } else { 'FAIL' }
    Write-Output "$state  $step"
    if (-not $ok -and $details) { $details | ForEach-Object { Write-Output "      $_" } }
    if (-not $ok) { $script:failed = $true }
}

# A test host left running by an interrupted run locks the DLLs and makes the next build fail.
Get-Process -Name 'Jrpg.*.Tests', 'Jrpg.Simulations' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

if ($Fix) { dotnet format $solution 2>&1 | Out-Null }

# 1. Build: warnings are errors in this repository (Directory.Build.props), so the exit code covers both. MSBuild's own
# copy-retry warnings (a file briefly locked by another build) aren't code warnings and are ignored.
$build = dotnet build $solution --nologo -v q 2>&1 | Out-String
$buildOk = $LASTEXITCODE -eq 0
$buildLines = ($build -split "`r?`n") | Where-Object { $_ -match ': error ' } | Sort-Object -Unique | Select-Object -First 20
Report 'build' $buildOk $buildLines

# 2. Tests: names ending in _Bug are known bugs the QA agent wrote; they're reported but don't fail the step.
if (-not $failed) {
    $test = dotnet test --solution $solution --no-build 2>&1 | Out-String
    $lines = $test -split "`r?`n"
    $total = ($lines | Where-Object { $_ -match '^\s*total:' } | Select-Object -Last 1) -replace '\s+', ' '
    $failedTests = $lines | Where-Object { $_ -match '^failed ' }
    $unexpected = $failedTests | Where-Object { $_ -notmatch '_Bug\b' }
    $details = @()
    foreach ($name in $unexpected | Select-Object -First 10) {
        $index = [Array]::IndexOf($lines, $name)
        $details += $name.Trim()
        $details += $lines[($index + 1)..([Math]::Min($index + 4, $lines.Length - 1))] | Where-Object { $_ -match 'Expected|Actual|Exception|Assert' } | ForEach-Object { '  ' + $_.Trim() }
    }

    # A crashed or hung test host fails without naming a test: show the end of the log instead.
    $crashed = $LASTEXITCODE -ne 0 -and -not $failedTests
    if ($crashed) { $details += ($lines | Where-Object { $_ -match 'error|crash|Exception|exit code' } | Select-Object -First 10) }
    Report "tests ($($total.Trim()))" (-not $unexpected -and -not $crashed) $details
    $failedTests | Where-Object { $_ -match '_Bug\b' } | ForEach-Object { Write-Output "      known bug: $($_.Trim())" }
}

# 3. Formatting.
if (-not $failed) {
    $format = dotnet format $solution --verify-no-changes 2>&1 | Out-String
    $formatLines = ($format -split "`r?`n") | Where-Object { $_ -match 'error' } | ForEach-Object { ($_ -split '\[')[0].Trim() } | Select-Object -First 10
    Report 'format (run with -Fix to apply)' ($LASTEXITCODE -eq 0) $formatLines
}

# 4. Core stays free of rendering and UI libraries.
$coreHits = Get-ChildItem src/Jrpg.Core -Recurse -Filter *.cs | Select-String -Pattern 'using (Microsoft\.Xna|System\.Windows)' | ForEach-Object { "$($_.Path | Resolve-Path -Relative):$($_.LineNumber)" }
Report 'Core references no UI library' (-not $coreHits) $coreHits

# 5. Banned names: types ending in Manager/Helper/Utils/Util, folders named by kind.
$typeHits = Get-ChildItem src -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    Select-String -Pattern '\b(class|record|struct|interface|enum)\s+\w+(Manager|Helper|Utils|Util)\b' | ForEach-Object { "$($_.Path | Resolve-Path -Relative):$($_.LineNumber)" }
$folderHits = Get-ChildItem src -Recurse -Directory | Where-Object { $_.Name -in 'Models', 'Enums', 'Managers', 'Utils' } | ForEach-Object { $_.FullName | Resolve-Path -Relative }
Report 'no Manager/Helper/Utils names or kind folders' (-not $typeHits -and -not $folderHits) (@($typeHits) + @($folderHits))

# 6. The retired word stays out of everything but history (decision 136), in text and in file and folder names. The
# pattern is built in pieces so that this script doesn't contain it and needs no exception for itself.
$retired = 'leg' + 'acy'
$checkedRoots = 'src', 'tests', 'content', 'docs', '.claude', 'CLAUDE.md', 'README.md', 'AI-SETUP.md', '.editorconfig',
    '.gitignore', '.gitattributes', 'Directory.Packages.props', 'Jrpg.slnx', 'Jrpg.slnLaunch', 'global.json'
$history = @(
    'docs/plans/done/',                                    # finished plans keep their text (decision 134)
    'docs/plans/2026-10-22-demo-retirement/',              # the plan that retires the word: remove when it moves to done/
    # ADRs written before decision 134 keep their text; listed by name so that ADR 0016 on is checked.
    'docs/decisions/0001-engine-free-architecture.md',
    'docs/decisions/0002-content-schema-mapping.md',
    'docs/decisions/0003-equipment-ownership-model.md',
    'docs/decisions/0004-battle-command-flow.md',
    'docs/decisions/0005-map-content-source-of-truth.md',
    'docs/decisions/0006-map-event-flow.md',
    'docs/decisions/0007-save-format-and-versioning.md',
    'docs/decisions/0008-game-shell-structure.md',
    'docs/decisions/0009-art-location-and-asset-loading.md',
    'docs/decisions/0010-player-facing-text.md',
    'docs/decisions/0011-content-after-the-converter.md',
    'docs/decisions/0012-editor-architecture.md',
    'docs/decisions/0013-battle-presentation-and-playback.md',
    'docs/decisions/0014-editing-shared-catalogs.md',
    'docs/decisions/0015-npcs-on-maps.md'
)

# Binary files can hold the word in metadata nobody wrote (a Photoshop IPTC digest key in a PNG).
$binaryExtensions = '.png', '.jpg', '.webp', '.ttf', '.dll'
$historyPattern = '^(' + (($history | ForEach-Object { [regex]::Escape($_) }) -join '|') + ')'

# Walks a folder without entering bin/ and obj/, whose build output would make this step take seconds, or
# .claude/worktrees/, whose repository copies would report their own history files.
function Get-PathsOutsideCopies($folder) {
    foreach ($entry in [IO.Directory]::EnumerateFileSystemEntries($folder)) {
        $isFolder = [IO.Directory]::Exists($entry)
        if ($isFolder -and [IO.Path]::GetFileName($entry) -in 'bin', 'obj') { continue }
        if ($isFolder -and $entry -eq (Join-Path $repositoryRoot '.claude\worktrees')) { continue }
        $entry
        if ($isFolder) { Get-PathsOutsideCopies $entry }
    }
}

$repositoryRoot = (Get-Location).Path
function Get-RelativePath($path) { $path.Substring($repositoryRoot.Length + 1).Replace('\', '/') }

$checked = foreach ($root in $checkedRoots | Where-Object { Test-Path $_ }) {
    $full = Join-Path $repositoryRoot $root
    $paths = if (Test-Path $full -PathType Container) { Get-PathsOutsideCopies $full } else { $full }
    $paths | Where-Object { (Get-RelativePath $_) -notmatch $historyPattern }
}
$nameHits = $checked | Where-Object { [IO.Path]::GetFileName($_) -match $retired } | ForEach-Object { Get-RelativePath $_ }
$textFiles = $checked | Where-Object { [IO.File]::Exists($_) -and [IO.Path]::GetExtension($_) -notin $binaryExtensions }
$textHits = if ($textFiles) {
    Select-String -LiteralPath $textFiles -Pattern $retired | ForEach-Object { "$(Get-RelativePath $_.Path):$($_.LineNumber)" }
}
Report 'no retired word outside history' (-not $nameHits -and -not $textHits) (@($nameHits) + @($textHits) | Select-Object -First 20)

if ($failed) { exit 1 }
exit 0
