# The checks on the changed lines' text (steps 7 to 12 of verify.ps1): column limits, comment endings, slips a shell
# edit leaves, files crossing 300 lines, parameter counts. No build needed, so verify.ps1 -TextOnly runs them alone in
# seconds (the shops and enemies retro: about 14 full runs failed only here). Dot-sourced by verify.ps1, which gives
# Report, Get-ChangedFiles, Get-ChangedSourceFiles and $repositoryRoot.

# 7. Comment lines changed since the last commit fit in 120 columns and code lines in 130 (code-quality.md); older
# lines are rewrapped when next touched. Counted in characters, without the line ending.
$maxCommentColumns = 120
$maxCodeColumns = 130
$commentPattern = '^\s*//'
$longComments = @()
$longCode = @()
$addedLines = @{}
$file = $null
$lineNumber = 0
foreach ($line in git diff -U0 HEAD -- '*.cs') {
    if ($line -match '^\+\+\+ b/(.+)$') { $file = $Matches[1]; $addedLines[$file] = @{}; continue }
    if ($line -match '^@@ -\S+ \+(\d+)') { $lineNumber = [int]$Matches[1]; continue }
    if ($line -match '^\+' -and $line -notmatch '^\+\+\+') {
        $text = $line.Substring(1).TrimEnd("`r")
        if ($text -match $commentPattern -and $text.Length -gt $maxCommentColumns) { $longComments += "${file}:$lineNumber ($($text.Length) columns)" }
        elseif ($text -notmatch $commentPattern -and $text.Length -gt $maxCodeColumns) { $longCode += "${file}:$lineNumber ($($text.Length) columns)" }
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
        elseif ($text -notmatch $commentPattern -and $text.Length -gt $maxCodeColumns) { $longCode += "${newFile}:$number ($($text.Length) columns)" }
        $addedLines[$newFile][$number] = $true
    }
}
Report "changed comments fit in $maxCommentColumns columns (perl .claude/scripts/reflow-comments.pl <file>)" (-not $longComments) ($longComments | Select-Object -First 20)
Report "changed code lines fit in $maxCodeColumns columns (split the line where it does two things)" (-not $longCode) ($longCode | Select-Object -First 20)

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

# 7c. A test or doc comment added right under a closing brace has a blank line before it: format leaves the join, and
# the member then reads as glued to the one above (the map editing flows' WP1 and WP2).
$joinedMembers = @()
foreach ($changed in $addedLines.Keys) {
    $path = Join-Path $repositoryRoot $changed
    if (-not (Test-Path $path)) { continue }
    $lines = [IO.File]::ReadAllLines($path)
    foreach ($number in $addedLines[$changed].Keys) {
        if ($number -lt 2 -or $lines[$number - 1] -notmatch '^\s*(\[(Fact|Theory)|///)') { continue }
        if ($lines[$number - 2] -match '^\s*}\s*$') { $joinedMembers += "${changed}:$number" }
    }
}
Report 'changed tests and doc comments have a blank line after the brace before them' (-not $joinedMembers) ($joinedMembers | Select-Object -First 20)

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
