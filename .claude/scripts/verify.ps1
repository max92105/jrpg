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

# Only one run at a time: the line below would kill another run's tests (the stop hook's and a reviewer's met).
$verifyLock = New-Object System.Threading.Mutex($false, 'JrpgVerify')
try { [void]$verifyLock.WaitOne() } catch [System.Threading.AbandonedMutexException] { }

# A test host left running by an interrupted run locks the DLLs and makes the next build fail.
Get-Process -Name 'Jrpg.*.Tests', 'Jrpg.Simulations' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# The files changed since the last commit, modified or new, of the given kinds; deleted ones are left out.
function Get-ChangedFiles([string[]]$patterns) {
    $modified = git diff --name-only HEAD -- @patterns
    $added = git ls-files --others --exclude-standard -- @patterns
    @($modified) + @($added) | Where-Object { $_ -and (Test-Path $_) } | Sort-Object -Unique
}

function Get-ChangedSourceFiles { Get-ChangedFiles @('*.cs') }

if ($Fix) {
    dotnet format $solution 2>&1 | Out-Null
    # Rewraps the changed comment blocks too, so step 7 rarely needs a second run.
    $changedForReflow = @(Get-ChangedSourceFiles)
    if ($changedForReflow) { perl .claude/scripts/reflow-comments.pl @changedForReflow 2>&1 | Out-Null }
    # Files written by an agent's tool come with LF endings; the repository is CRLF (CLAUDE.md), written without a BOM.
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    foreach ($file in Get-ChangedFiles @('*.cs', '*.xaml', '*.md', '*.json', '*.props', '*.csproj', '*.pl', '*.ps1')) {
        $text = [System.IO.File]::ReadAllText($file)
        $normalized = $text -replace "`r?`n", "`r`n"
        if ($normalized -ne $text) { [System.IO.File]::WriteAllText($file, $normalized, $utf8) }
    }
}

# 1. Build: warnings are errors in this repository (Directory.Build.props), so the exit code covers both. MSBuild's own
# copy-retry warnings (a file briefly locked by another build) aren't code warnings and are ignored.
$build = dotnet build $solution --nologo -v q 2>&1 | Out-String
$buildOk = $LASTEXITCODE -eq 0
$buildLines = ($build -split "`r?`n") | Where-Object { $_ -match ': error ' } | Sort-Object -Unique | Select-Object -First 20
# The editor started from Visual Studio locks its own DLLs; the raw copy errors hide that one cause.
if (-not $buildOk -and $build -match 'MSB302[17]:.*Jrpg\.Editor') {
    $buildLines = @('the editor is running (started from Visual Studio) and locks its files: close it and rerun')
}
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

# 7. Comment lines changed since the last commit fit in 120 columns (code-quality.md); older lines are rewrapped when
# next touched. Counted in characters, without the line ending.
$maxCommentColumns = 120
$commentPattern = '^\s*//'
$longComments = @()
$addedLines = @{}
$file = $null
$lineNumber = 0
foreach ($line in git diff -U0 HEAD -- '*.cs') {
    if ($line -match '^\+\+\+ b/(.+)$') { $file = $Matches[1]; $addedLines[$file] = @{}; continue }
    if ($line -match '^@@ -\S+ \+(\d+)') { $lineNumber = [int]$Matches[1]; continue }
    if ($line -match '^\+' -and $line -notmatch '^\+\+\+') {
        $text = $line.Substring(1).TrimEnd("`r")
        if ($text -match $commentPattern -and $text.Length -gt $maxCommentColumns) { $longComments += "${file}:$lineNumber ($($text.Length) columns)" }
        $addedLines[$file][$lineNumber] = $true
        $lineNumber++
    }
}
foreach ($newFile in git ls-files --others --exclude-standard -- '*.cs') {
    $number = 0
    $addedLines[$newFile] = @{}
    foreach ($text in [IO.File]::ReadAllLines((Join-Path $repositoryRoot $newFile))) {
        $number++
        if ($text -match $commentPattern -and $text.Length -gt $maxCommentColumns) { $longComments += "${newFile}:$number ($($text.Length) columns)" }
        $addedLines[$newFile][$number] = $true
    }
}
Report "changed comments fit in $maxCommentColumns columns (perl .claude/scripts/reflow-comments.pl <file>)" (-not $longComments) ($longComments | Select-Object -First 20)

# 7b. A changed comment block of two or more lines doesn't end on a one- or two-word line (code-quality.md); the -Fix
# rewrap left two in the levels plan's SZ3. A line of prose only: an XML tag line or a list item doesn't count.
function Get-CommentProse([string]$text) {
    if ($text -match '^\s*(///?)\s?(.*)$' -and $Matches[2] -notmatch '^\s*(<|-\s|$)') { return @($Matches[1], $Matches[2].Trim()) }
    return $null
}
$shortLastLines = @()
foreach ($changed in $addedLines.Keys) {
    $path = Join-Path $repositoryRoot $changed
    if (-not (Test-Path $path)) { continue }
    $lines = [IO.File]::ReadAllLines($path)
    foreach ($number in $addedLines[$changed].Keys) {
        $prose = Get-CommentProse $lines[$number - 1]
        $previous = if ($number -ge 2) { Get-CommentProse $lines[$number - 2] } else { $null }
        $next = if ($number -lt $lines.Count) { Get-CommentProse $lines[$number] } else { $null }
        $isLastOfBlock = $prose -and $previous -and $previous[0] -eq $prose[0] -and -not ($next -and $next[0] -eq $prose[0])
        if ($isLastOfBlock -and @($prose[1] -split '\s+').Count -le 2) { $shortLastLines += "${changed}:$number ('$($prose[1])')" }
    }
}
Report 'changed comments end on more than two words (perl .claude/scripts/reflow-comments.pl <file>, then reword)' (-not $shortLastLines) ($shortLastLines | Select-Object -First 20)

# 8-11. Slips a shell edit leaves behind, caught before the review. The quote is the shell escape '"'"' written literally
# (a heredoc perl script, see the class-mechanics retro); a doubled summary is a member inserted between another's doc
# comment and its declaration; a broken doc line is a /// line that lost a slash (`/// //` or a lone `//` inside a
# /// block, twice in the loot plan); a file crossing 300 lines is a split the slice should have made (code-quality.md).
$changedSources = @(Get-ChangedSourceFiles)
$shellQuote = "'" + '"' + "'" + '"' + "'"
$quoteHits = @()
$doubledSummaries = @()
$brokenDocLines = @()
$grownFiles = @()
foreach ($source in $changedSources) {
    $lines = [IO.File]::ReadAllLines((Join-Path $repositoryRoot $source))
    $previousDocLine = $null
    for ($number = 1; $number -le $lines.Length; $number++) {
        $text = $lines[$number - 1]
        if ($text.Contains($shellQuote) -or $text.Contains('\x27')) { $quoteHits += "${source}:$number" }
        $insideDocBlock = $number -gt 1 -and $number -lt $lines.Length -and $lines[$number - 2] -match '^\s*///' -and $lines[$number] -match '^\s*///'
        if ($text -match '^\s*/// //' -or ($insideDocBlock -and $text -match '^\s*//(?!/)')) { $brokenDocLines += "${source}:$number" }
        if ($text -match '^\s*///') {
            if ($text -match '^\s*///\s*<summary>' -and $previousDocLine -match '</summary>\s*$') { $doubledSummaries += "${source}:$number" }
            $previousDocLine = $text
        }
        elseif ($text.Trim()) { $previousDocLine = $null }
    }

    $lineLimit = 300
    $headLines = git show "HEAD:$source" 2>$null
    $before = if ($LASTEXITCODE -eq 0) { @($headLines).Count } else { 0 }
    if ($before -le $lineLimit -and $lines.Length -gt $lineLimit) { $grownFiles += "${source} ($before -> $($lines.Length) lines)" }
}
Report 'changed files hold no shell quote or \x27 escape written literally' (-not $quoteHits) ($quoteHits | Select-Object -First 10)
Report 'no member with two summaries' (-not $doubledSummaries) ($doubledSummaries | Select-Object -First 10)
Report 'no doc comment line lost a slash' (-not $brokenDocLines) ($brokenDocLines | Select-Object -First 10)

# A view's markup grows with its panels: the same 300-line limit (the composer plan's AE2 reached 309 in MainWindow.xaml).
$lineLimit = 300
foreach ($view in @(Get-ChangedFiles @('*.xaml'))) {
    $viewLines = [IO.File]::ReadAllLines((Join-Path $repositoryRoot $view)).Length
    $headLines = git show "HEAD:$view" 2>$null
    $before = if ($LASTEXITCODE -eq 0) { @($headLines).Count } else { 0 }
    if ($before -le $lineLimit -and $viewLines -gt $lineLimit) { $grownFiles += "${view} ($before -> $viewLines lines)" }
}

Report 'no changed file crossed 300 lines (split it)' (-not $grownFiles) ($grownFiles | Select-Object -First 10)

# 12. A changed method or constructor in src declares at most 4 parameters (code-quality.md; four Draw methods slipped
# past the gold plan's reviews). Positional records and primary constructors are data shapes and are left out, and a
# generic argument's or a tuple type's comma isn't a parameter: nested groups go before the list is cut at its close.
# Multi-line parameter lists are joined up to their closing parenthesis.
$maxParameters = 4
$manyParameters = @()
foreach ($source in $changedSources | Where-Object { $_ -like 'src/*' }) {
    $lines = [IO.File]::ReadAllLines((Join-Path $repositoryRoot $source))
    for ($number = 1; $number -le $lines.Length; $number++) {
        $text = $lines[$number - 1]
        if ($text -notmatch '^\s*(public|internal|private|protected)[^=;{(]*?\b\w+\s*\((.*)$' -or $text -match '\b(record|class|struct|interface|enum|delegate)\b') { continue }
        $parameters = $Matches[2]
        for ($next = $number; ($parameters -replace '[^(]', '').Length -ge ($parameters -replace '[^)]', '').Length -and $next -lt $lines.Length; $next++) { $parameters += ' ' + $lines[$next] }
        while ($parameters -match '<[^<>]*>|\([^()]*\)') { $parameters = $parameters -replace '<[^<>]*>|\([^()]*\)', '' }
        $parameters = $parameters -replace '\).*$', ''
        $count = if ($parameters.Trim()) { ($parameters -split ',').Count } else { 0 }
        if ($count -gt $maxParameters) { $manyParameters += "${source}:$number ($count parameters)" }
    }
}
Report "no changed method takes more than $maxParameters parameters (a parameter object or the constructor takes the extra)" (-not $manyParameters) ($manyParameters | Select-Object -First 10)

if ($failed) { exit 1 }
exit 0
