# PreToolUse hook (Edit|Write): blocks edits to files Claude should never touch.
# Exit 2 = block the edit and send the message on stderr back to Claude.

$raw = [Console]::In.ReadToEnd()
try { $payload = $raw | ConvertFrom-Json } catch { exit 0 }

$path = $payload.tool_input.file_path
if (-not $path) { exit 0 }
$path = $path -replace '\\', '/'

$protected = @(
    @{ Pattern = '/.git/';                Reason = 'git internals' },
    @{ Pattern = '/bin/';                 Reason = 'build output' },
    @{ Pattern = '/obj/';                 Reason = 'build output' },
    @{ Pattern = '.claude/settings.json'; Reason = 'AI guardrails (the developer edits these by hand)' },
    @{ Pattern = '.claude/hooks/';        Reason = 'AI guardrails (the developer edits these by hand)' },
    @{ Pattern = 'Directory.Build.props'; Reason = 'build-wide quality settings (ask the developer first)' },
    @{ Pattern = '.editorconfig';         Reason = 'code style settings (ask the developer first)' },
    @{ Pattern = 'CodeMetricsConfig.txt'; Reason = 'complexity limits (ask the developer first)' }
)

foreach ($p in $protected) {
    if ($path -like "*$($p.Pattern)*") {
        [Console]::Error.WriteLine("Blocked: $path is protected ($($p.Reason)). If this change is really needed, explain why and ask the developer to make it.")
        exit 2
    }
}
exit 0
