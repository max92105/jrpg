# Writes a placeholder sound: a 16-bit PCM mono 22 050 Hz WAV of one or more sine frequencies. Placeholders only: no music
# is composed here (the music plan's design.md, "Tunables and placeholders").
#
#   powershell -NoProfile -File .claude/scripts/new-placeholder-tone.ps1 -Path content/audio/music/PlaceholderTown.wav -Frequencies 220,277,330 -Seconds 4
#
# Each frequency is rounded so frequency x seconds is a whole number of cycles, so a loop meets its start with no seam.
# Effects pass -FadeMilliseconds 5, so they start and end without a click.
param(
    [Parameter(Mandatory)][string]$Path,
    [Parameter(Mandatory)][double[]]$Frequencies,
    [Parameter(Mandatory)][double]$Seconds,
    [double]$Amplitude = 0.3,
    [int]$FadeMilliseconds = 0
)

$rate = 22050
$count = [int][Math]::Round($rate * $Seconds)
$duration = $count / $rate
$whole = @($Frequencies | ForEach-Object { [Math]::Round($_ * $duration) / $duration })
$fadeSamples = [int]($rate * $FadeMilliseconds / 1000)

$stream = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($stream)
$ascii = [System.Text.Encoding]::ASCII
$dataBytes = $count * 2
$writer.Write($ascii.GetBytes('RIFF')); $writer.Write([int](36 + $dataBytes)); $writer.Write($ascii.GetBytes('WAVE'))
$writer.Write($ascii.GetBytes('fmt ')); $writer.Write([int]16); $writer.Write([int16]1); $writer.Write([int16]1)
$writer.Write([int]$rate); $writer.Write([int]($rate * 2)); $writer.Write([int16]2); $writer.Write([int16]16)
$writer.Write($ascii.GetBytes('data')); $writer.Write([int]$dataBytes)
for ($i = 0; $i -lt $count; $i++) {
    $sum = 0.0
    foreach ($frequency in $whole) { $sum += [Math]::Sin(2 * [Math]::PI * $frequency * $i / $rate) }
    $envelope = 1.0
    if ($fadeSamples -gt 0) { $envelope = [Math]::Min(1.0, [Math]::Min($i, $count - 1 - $i) / $fadeSamples) }
    $writer.Write([int16][Math]::Round(32767 * $Amplitude * $envelope * $sum / $whole.Count))
}

$writer.Flush()
$full = [System.IO.Path]::GetFullPath($Path)
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($full)) | Out-Null
[System.IO.File]::WriteAllBytes($full, $stream.ToArray())
$writer.Dispose()
