# Makes the placeholder animated battle background (decision 910): four 960 x 540 frames of the generated forest,
# hand-laid in a 2 x 2 grid (1920 x 1080), a slow brightness pulse the record plays at 200 ms a frame. The editor's
# writer would lay four such frames in one row; this grid exercises the game's column rule instead (decision 897).
# Run by hand when the look changes:
#   powershell -NoProfile -ExecutionPolicy Bypass -File art-src/backgrounds/New-PlaceholderAnimatedBackground.ps1 -Force
# System.Drawing is this tool's only: the game reads the PNG (ADR 0009).
param([switch] $Force)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repository = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$backgrounds = Join-Path $repository 'content\art\backgrounds'
$sourcePath = Join-Path $backgrounds 'Forest.png'
$outputPath = Join-Path $backgrounds 'ForestPulse.png'

$frameWidth = 960
$frameHeight = 540
$columns = 2
# Frames left to right, then top to bottom: darker to 80 %, then back.
$brightnesses = @(1.0, 0.9, 0.8, 0.9)

if ((Test-Path $outputPath) -and -not $Force) {
    throw "$outputPath exists; pass -Force to replace it."
}

# The largest 16:9 area about the source's center, so the shrink keeps the picture's proportions.
function Get-CenteredCrop([System.Drawing.Image] $image) {
    $width = [Math]::Min($image.Width, [int][Math]::Floor($image.Height * 16 / 9))
    $height = [int][Math]::Round($width * 9 / 16)
    $left = [int](($image.Width - $width) / 2)
    $top = [int](($image.Height - $height) / 2)
    return New-Object System.Drawing.Rectangle $left, $top, $width, $height
}

# High-quality bicubic averages the source pixels each target pixel covers when it shrinks (decision 904: painted art
# keeps its detail as color); the flipped tiling keeps the edges from blending with transparent black.
function Write-Frame($graphics, [System.Drawing.Image] $source, $crop, [int] $frame) {
    $brightness = [single]$brightnesses[$frame]
    $matrix = New-Object System.Drawing.Imaging.ColorMatrix
    $matrix.Matrix00 = $brightness
    $matrix.Matrix11 = $brightness
    $matrix.Matrix22 = $brightness
    $attributes = New-Object System.Drawing.Imaging.ImageAttributes
    $attributes.SetColorMatrix($matrix)
    $attributes.SetWrapMode([System.Drawing.Drawing2D.WrapMode]::TileFlipXY)
    $left = ($frame % $columns) * $frameWidth
    $top = [int][Math]::Floor($frame / $columns) * $frameHeight
    $target = New-Object System.Drawing.Rectangle $left, $top, $frameWidth, $frameHeight
    $graphics.DrawImage(
        $source, $target, $crop.X, $crop.Y, $crop.Width, $crop.Height, [System.Drawing.GraphicsUnit]::Pixel, $attributes)
    $attributes.Dispose()
}

$source = [System.Drawing.Image]::FromFile($sourcePath)
$crop = Get-CenteredCrop $source
$rows = [int][Math]::Ceiling($brightnesses.Count / $columns)
$sheetWidth = $frameWidth * $columns
$sheetHeight = $frameHeight * $rows
# Opaque, as Forest.png is: a background has no transparency to keep.
$sheet = New-Object System.Drawing.Bitmap $sheetWidth, $sheetHeight, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$graphics = [System.Drawing.Graphics]::FromImage($sheet)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
for ($frame = 0; $frame -lt $brightnesses.Count; $frame++) {
    Write-Frame $graphics $source $crop $frame
}

$sheet.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$sheet.Dispose()
$source.Dispose()
Write-Output "Wrote ForestPulse.png ($sheetWidth x $sheetHeight, $($brightnesses.Count) frames of $frameWidth x $frameHeight)."
