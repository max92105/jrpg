# Renders the battle's damage digits (decision 653) from the OFL serif the game ships, Cormorant Garamond: ten cells
# side by side, "0" to "9", slanted, heavy, white fading to a pale lavender-grey, a thin dark edge baked in. White, so
# the game tints criticals, heals and Poison. The critical strip is the same digits 1.4x bigger, so no art is scaled.
# Run by hand when the look changes:
#   powershell -NoProfile -ExecutionPolicy Bypass -File art-src/digits/New-DigitStrips.ps1
# System.Drawing is this tool's only: the game reads the PNGs (ADR 0009).

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repository = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$fontPath = Join-Path $repository 'content\art\fonts\titles\CormorantGaramond-SemiBold.ttf'
$outputFolder = Join-Path $repository 'content\art\fonts'

# The spec's values for the normal strip; the critical one multiplies every size by $criticalScale.
$cellWidth = 40
$cellHeight = 52
$inkHeight = 39
$baselineRow = 45
$widestInk = 36
$slantDegrees = 10
$heavyStroke = 4
$edgeWidth = 2
$criticalScale = 1.4
$topColor = [System.Drawing.Color]::White
$bottomColor = [System.Drawing.ColorTranslator]::FromHtml('#C9C3DD')
$edgeColor = [System.Drawing.ColorTranslator]::FromHtml('#1A1830')

$fonts = New-Object System.Drawing.Text.PrivateFontCollection
$fonts.AddFontFile($fontPath)
$family = $fonts.Families[0]

# One digit as a path: scaled so its ink with the strokes is the ink height, slanted right, its bottom on the baseline
# row and its middle on the cell's.
function New-DigitPath([string] $digit, [double] $scale) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddString($digit, $family, [int][System.Drawing.FontStyle]::Regular, 200, (New-Object System.Drawing.PointF 0, 0),
        [System.Drawing.StringFormat]::GenericTypographic)
    $outer = (($heavyStroke / 2) + $edgeWidth) * $scale
    $bounds = $path.GetBounds()
    $glyphScale = (($inkHeight * $scale) - (2 * $outer)) / $bounds.Height
    $matrix = New-Object System.Drawing.Drawing2D.Matrix
    $matrix.Scale($glyphScale, $glyphScale)
    $matrix.Translate(-$bounds.X, -$bounds.Y)
    $path.Transform($matrix)
    $slant = [Math]::Tan($slantDegrees * [Math]::PI / 180)
    $height = $path.GetBounds().Height
    $shear = New-Object System.Drawing.Drawing2D.Matrix
    $shear.Shear(-$slant, 0)
    $shear.Translate(0, -$height, [System.Drawing.Drawing2D.MatrixOrder]::Prepend)
    $path.Transform($shear)
    $ink = $path.GetBounds()
    # 1.0, not 1: PowerShell would pick Min's Int32 overload and round the ratio to 0 or 1.
    $narrowing = [Math]::Min(1.0, (($widestInk * $scale) - (2 * $outer)) / $ink.Width)
    $place = New-Object System.Drawing.Drawing2D.Matrix
    $place.Translate(($cellWidth * $scale / 2) - ($ink.Width * $narrowing / 2), ($baselineRow * $scale) - $outer)
    $place.Scale($narrowing, 1)
    $place.Translate(-$ink.X, -$ink.Bottom)
    $path.Transform($place)
    return , $path
}

function Write-Strip([string] $fileName, [double] $scale) {
    $width = [int][Math]::Round($cellWidth * $scale) * 10
    $cell = [int][Math]::Round($cellWidth * $scale)
    $height = [int][Math]::Round($cellHeight * $scale)
    $image = New-Object System.Drawing.Bitmap $width, $height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($image)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)
    for ($digit = 0; $digit -le 9; $digit++) {
        $path = New-DigitPath ([string]$digit) $scale
        $shift = New-Object System.Drawing.Drawing2D.Matrix
        $shift.Translate($digit * $cell, 0)
        $path.Transform($shift)
        # The gradient spans the heavy stroke's reach past the outline: a brush tiles past its own ends.
        $bounds = $path.GetBounds()
        $reach = $heavyStroke * $scale / 2
        $fill = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
            (New-Object System.Drawing.PointF 0, ($bounds.Top - $reach)),
            (New-Object System.Drawing.PointF 0, ($bounds.Bottom + $reach + 1)),
            $topColor,
            $bottomColor)
        $blend = New-Object System.Drawing.Drawing2D.ColorBlend 3
        $blend.Colors = @($topColor, $topColor, $bottomColor)
        $blend.Positions = @(0.0, 0.5, 1.0)
        $fill.InterpolationColors = $blend
        $edge = New-Object System.Drawing.Pen $edgeColor, (($heavyStroke + (2 * $edgeWidth)) * $scale)
        $edge.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
        $heavy = New-Object System.Drawing.Pen $fill, ($heavyStroke * $scale)
        $heavy.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
        $graphics.DrawPath($edge, $path)
        $graphics.DrawPath($heavy, $path)
        $graphics.FillPath($fill, $path)
        $edge.Dispose(); $heavy.Dispose(); $fill.Dispose(); $path.Dispose()
    }

    $image.Save((Join-Path $outputFolder $fileName), [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $image.Dispose()
    Write-Output "Wrote $fileName ($width x $height)."
}

Write-Strip 'DamageDigits.png' 1
Write-Strip 'CriticalDigits.png' $criticalScale
