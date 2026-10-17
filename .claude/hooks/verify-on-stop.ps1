# Stop hook: when Claude finishes a turn that changed code, build and test.
# If either fails, exit 2 so Claude sees the errors and keeps working.

$raw = [Console]::In.ReadToEnd()
try { $payload = $raw | ConvertFrom-Json } catch { $payload = $null }

# Avoid infinite loops: if we already sent Claude back to work once this turn, let it stop.
if ($payload -and $payload.stop_hook_active) { exit 0 }

Set-Location $env:CLAUDE_PROJECT_DIR

# The legacy demo (JRPG.sln) and the new structure (Jrpg.slnx) live side by side during the migration.
$solutions = @('JRPG.sln', 'Jrpg.slnx') | Where-Object { Test-Path $_ }
if (-not $solutions) { exit 0 }

# Only run when C#/project/content files changed. Chat-only turns stay fast.
$changed = git status --porcelain -- '*.cs' '*.csproj' '*.props' '*.json' '*.xaml' '.editorconfig' 'CodeMetricsConfig.txt' 2>$null
if (-not $changed) { exit 0 }

function Fail($step, $output) {
    $lines = $output -split "`r?`n" | Where-Object { $_ -match 'error|failed|Failed|warning' } | Select-Object -First 40
    if (-not $lines) { $lines = ($output -split "`r?`n") | Select-Object -Last 40 }
    [Console]::Error.WriteLine("Automatic check failed at '$step'. Fix these before finishing:")
    [Console]::Error.WriteLine(($lines -join "`n"))
    exit 2
}

foreach ($solution in $solutions) {
    $build = & dotnet build $solution --nologo -v q 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { Fail "dotnet build $solution" $build }

    # Only the new solution has test projects. global.json selects Microsoft.Testing.Platform, which needs --solution.
    if ($solution -ne 'Jrpg.slnx') { continue }
    $test = & dotnet test --solution $solution --no-build 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        # Known bugs written by the QA agent are allowed to fail; anything else is not.
        $failedNames = ($test -split "`r?`n") | Where-Object { $_ -match '^\s*Failed\s' }
        $unexpected = $failedNames | Where-Object { $_ -notmatch '_Bug\b' }
        if ($unexpected -or -not $failedNames) { Fail "dotnet test $solution" $test }
    }
}

exit 0
