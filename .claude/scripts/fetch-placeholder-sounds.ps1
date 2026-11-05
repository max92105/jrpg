# Downloads each entry of content/audio/sound-sources.json from freesound.org and converts it into its named WAV, then
# rewrites content/audio/CREDITS.md from the whole manifest (ADR 0036). Placeholders only: the previews are the 128 kbps
# MP3s freesound serves without a login.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/fetch-placeholder-sounds.ps1
#   ... -Only effects/PlaceholderHit.wav   converts that entry alone; the credits are rewritten from the whole manifest
#
# ffmpeg comes from `winget install Gyan.FFmpeg` (decision 428): only this script needs it, the WAVs it writes are
# committed. A swap reruns one file (-Only), so two PCs with different ffmpeg builds don't rewrite every WAV.
param([string]$Only)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$audio = Join-Path $root 'content\audio'
$cache = Join-Path $env:TEMP 'jrpg-freesound'
$rate = 44100
$deeds = @{
    'CC0-1.0' = 'https://creativecommons.org/publicdomain/zero/1.0/'
    'CC-BY-3.0' = 'https://creativecommons.org/licenses/by/3.0/'
    'CC-BY-4.0' = 'https://creativecommons.org/licenses/by/4.0/'
}

# A terminal opened before winget installed ffmpeg doesn't have it on its PATH yet, so winget's own folder is looked
# in too.
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
if (-not $ffmpeg) {
    $installed = Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Packages\Gyan.FFmpeg*\*\bin\ffmpeg.exe'
    $ffmpeg = (Get-ChildItem $installed -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
}
if (-not $ffmpeg) {
    throw 'ffmpeg is missing: run `winget install Gyan.FFmpeg`, then open a new terminal.'
}

$manifest = Get-Content (Join-Path $audio 'sound-sources.json') -Raw | ConvertFrom-Json

# The script is the only writer of the WAVs, so it refuses what decision 427 and the (id, start) key forbid.
$keys = @{}
foreach ($sound in $manifest.sounds) {
    if (-not $deeds.ContainsKey($sound.license)) {
        throw "$($sound.file): license '$($sound.license)' isn't allowed (CC0-1.0, CC-BY-3.0 or CC-BY-4.0)."
    }
    if ([double]$sound.maxSeconds -le 0) { throw "$($sound.file): maxSeconds is missing (the brief's length)." }
    $key = "$($sound.freesoundId) from $([double]$sound.startSeconds) s"
    if ($keys.ContainsKey($key)) { throw "$($sound.file) and $($keys[$key]) both use freesound $key." }
    $keys[$key] = $sound.file
}

# Windows PowerShell turns a native command's stderr lines into errors under 'Stop', and ffmpeg logs everything there.
function Invoke-Ffmpeg([string[]]$arguments) {
    $ErrorActionPreference = 'Continue'
    $output = & $ffmpeg -hide_banner -nostats -y @arguments 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) { throw "ffmpeg failed:`n$($output -join "`n")" }
    return $output
}

function Get-Preview($sound) {
    $path = Join-Path $cache "$($sound.freesoundId).mp3"
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Force $cache | Out-Null
        # A broken download stays a .part, so a rerun fetches it again rather than converting a cut MP3 without a word.
        & curl.exe -fsSL -A 'jrpg-placeholder-fetch' -o "$path.part" $sound.preview
        if ($LASTEXITCODE -ne 0) { throw "$($sound.file): the download of $($sound.preview) failed." }
        Move-Item -Force "$path.part" $path
    }
    return $path
}

# The pass asks for a float encoder: with the null muxer's 16-bit one, the graph mixes in 16 bits and clips at 0 dB
# before astats, and a stereo take whose mono mix peaks above that (0.7 a side) comes out 2 or 3 dB too loud.
function Measure-Peak($sound, [string]$source, [string]$filters) {
    $stats = 'astats=measure_perchannel=none:measure_overall=Peak_level'
    $measured = Invoke-Ffmpeg @('-i', $source, '-af', "$filters,$stats", '-c:a', 'pcm_f32le', '-f', 'null', '-') |
        Select-String 'Peak level dB: (-?[\d.]+)'
    if (-not $measured) { throw "$($sound.file): ffmpeg measured no peak in $source." }
    return [double]$measured.Matches[0].Groups[1].Value
}

function Convert-Sound($sound) {
    $source = Get-Preview $sound
    $start = [double]$sound.startSeconds
    $mono = "aformat=channel_layouts=mono,aresample=$rate,atrim=start=$start,asetpts=PTS-STARTPTS"
    $peak = Measure-Peak $sound $source $mono

    # ffmpeg drops the MP3's encoder delay itself, so the trim is for an uploader's lead-in; relative to the peak, a
    # quiet take keeps its attack and a noisy one still lands on its beat. The fade out is a reversed fade in, since the
    # length after the trim isn't known here; 1 ms in unless the start offset cut into the sound.
    $fadeIn = if ($start -gt 0) { 0.005 } else { 0.001 }
    $fadeOut = if ($sound.fadeOutSeconds) { [double]$sound.fadeOutSeconds } else { 0.01 }
    $shaped = @(
        $mono,
        "silenceremove=start_periods=1:start_threshold=$($peak - 40)dB:detection=peak",
        'asetpts=PTS-STARTPTS',
        "atrim=end_sample=$([int]([double]$sound.maxSeconds * $rate))",
        "afade=t=in:d=$fadeIn",
        "areverse,afade=t=in:d=$fadeOut,areverse"
    ) -join ','
    # The gain comes from the shaped sound's peak: a hit whose loudest moment falls in the fade in or past the cap would
    # land under the manifest's level if it came from the source's.
    $gain = [double]$manifest.peakDb - (Measure-Peak $sound $source $shaped) + [double]$sound.gainDb
    $filters = "$shaped,volume=${gain}dB"
    $target = Join-Path $audio $sound.file
    # bitexact keeps ffmpeg from adding a LIST chunk: a plain RIFF with fmt and data, which the game reads.
    Invoke-Ffmpeg @('-i', $source, '-af', $filters, '-ac', '1', '-ar', "$rate", '-c:a', 'pcm_s16le',
        '-fflags', '+bitexact', '-flags:a', '+bitexact', '-map_metadata', '-1', $target) | Out-Null
    & perl (Join-Path $root '.claude\scripts\new-placeholder-tune.pl') measure $target
}

function Write-Credits {
    $rows = $manifest.sounds | Sort-Object file | ForEach-Object {
        $name = "$($_.name)".Replace('|', '\|')
        '| `{0}` | [{1}]({2}) | {3} | [{4}]({5}) | trimmed, resampled to mono, level-adjusted | {6} |' -f
            $_.file, $name, $_.page, $_.author, $_.license, $deeds[$_.license], $_.fetched
    }
    $lines = @(
        '# Audio credits',
        '',
        'Written by `.claude/scripts/fetch-placeholder-sounds.ps1` from `sound-sources.json`: edit that file and rerun',
        'the script, never this one. Every sound below is a placeholder from [freesound.org](https://freesound.org/)',
        '(ADR 0036), modified as the table says. The victory and defeat jingles are rendered by',
        '`.claude/scripts/new-placeholder-tune.pl` and need no credit; the music is the developer''s own.',
        '',
        '| File | Sound | Author | License | Modified | Fetched |',
        '|---|---|---|---|---|---|'
    ) + $rows
    # UTF-8 without the byte-order mark Set-Content adds, since an author's name may not be ASCII.
    $text = ($lines -join "`r`n") + "`r`n"
    [IO.File]::WriteAllText((Join-Path $audio 'CREDITS.md'), $text, [Text.UTF8Encoding]::new($false))
}

$chosen = if ($Only) { @($manifest.sounds | Where-Object file -eq $Only) } else { @($manifest.sounds) }
if ($chosen.Count -eq 0) { throw "No manifest entry has the file '$Only'." }
foreach ($sound in $chosen) { Convert-Sound $sound }
Write-Credits
