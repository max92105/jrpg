# Stop hook: when Claude finishes a turn that changed code, run the definition-of-done checks.
# If one fails, exit 2 so Claude sees the short report and keeps working.

$raw = [Console]::In.ReadToEnd()
try { $payload = $raw | ConvertFrom-Json } catch { $payload = $null }

# Avoid infinite loops: if we already sent Claude back to work once this turn, let it stop.
if ($payload -and $payload.stop_hook_active) { exit 0 }

Set-Location $env:CLAUDE_PROJECT_DIR

# Only run when C#/project/content files changed. Chat-only turns stay fast.
$changed = git status --porcelain -- '*.cs' '*.csproj' '*.props' '*.json' '.editorconfig' 'CodeMetricsConfig.txt' 2>$null
if (-not $changed) { exit 0 }

$report = & powershell -NoProfile -ExecutionPolicy Bypass -File "$env:CLAUDE_PROJECT_DIR\.claude\scripts\verify.ps1" 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) {
    [Console]::Error.WriteLine("Automatic check failed. Fix these before finishing:")
    [Console]::Error.WriteLine($report)
    exit 2
}

exit 0
